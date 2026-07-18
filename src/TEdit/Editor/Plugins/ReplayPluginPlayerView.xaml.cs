using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TEdit.ViewModel;
using Symbol = Wpf.Ui.Controls.SymbolRegular;

namespace TEdit.Editor.Plugins;

public partial class ReplayPluginPlayerView
{
    private static readonly double[] Speeds = [0.5, 1.0, 2.0, 4.0, 16.0];
    private static readonly string[] SpeedLabels = ["0.5x", "1x", "2x", "4x", "16x"];
    private static readonly int[] Delays = [10, 50, 100, 250, 500, 1000];
    private static readonly string[] DelayLabels = ["10ms", "50ms", "100ms", "250ms", "500ms", "1000ms"];

    private ReplayPlayer _player;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(10) };
    private bool _seeking;

    public ReplayPluginPlayerView()
    {
        InitializeComponent();
        Closed += (_, _) => _timer.Stop();
    }

    public void Load(ReplayFile file)
    {
        _timer.Tick += TickTimer_Tick;

        _player = new ReplayPlayer(file);

        ViewModelLocator.WorldViewModel.CurrentWorld = file.BaselineWorld;

        ModeCombo.SelectedIndex = 0;
        RateCombo.SelectedIndex = 1;

        UpdateDisplay();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        _player.Seek(0);
        UpdateDisplay();
    }

    private void StepBackButton_Click(object sender, RoutedEventArgs e)
    {
        _player.StepBackward();
        UpdateDisplay();
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_player.IsPlaying)
        {
            _player.Pause();
            _timer.Stop();
            PlayPauseIcon.Symbol = Symbol.Play20;
            PlayPauseButton.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
        }
        else
        {
            _player.Play();
            _timer.Start();
            PlayPauseIcon.Symbol = Symbol.Pause20;
            PlayPauseButton.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private void StepForwardButton_Click(object sender, RoutedEventArgs e)
    {
        _player.StepForward();
        UpdateDisplay();
    }

    private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_seeking || _player == null) return;
        _seeking = true;

        if (ModeCombo.SelectedIndex == 0)
            _player.SeekByTime(ProgressSlider.Value);
        else
            _player.Seek((int)ProgressSlider.Value);

        UpdateDisplay();
        _seeking = false;
    }

    private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_player == null) return;

        RateCombo.Items.Clear();
        if (ModeCombo.SelectedIndex == 0)
        {
            _player.SetMode(PlaybackMode.Speed);
            RateCombo.SelectedIndex = 1;
            foreach (var label in SpeedLabels)
                RateCombo.Items.Add(new ComboBoxItem { Content = label });
        }
        else
        {
            _player.SetMode(PlaybackMode.Delay);
            RateCombo.SelectedIndex = 2;
            foreach (var label in DelayLabels)
                RateCombo.Items.Add(new ComboBoxItem { Content = label });
        }

        UpdateDisplay();
    }

    private void RateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_player == null || RateCombo.SelectedIndex < 0) return;

        if (ModeCombo.SelectedIndex == 0)
            _player.SetSpeed(Speeds[RateCombo.SelectedIndex]);
        else
            _player.SetDelay(Delays[RateCombo.SelectedIndex]);
    }

    private void TickTimer_Tick(object sender, EventArgs e)
    {
        _player.OnTick();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_player.IsPlaying)
        {
            PlayPauseIcon.Symbol = Symbol.Pause20;
            PlayPauseButton.Foreground = System.Windows.Media.Brushes.Red;
        }
        else
        {
            PlayPauseIcon.Symbol = Symbol.Play20;
            PlayPauseButton.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
        }

        if (ModeCombo.SelectedIndex == 0)
        {
            var cur = TimeSpan.FromMilliseconds(_player.CurrentTime);
            var total = TimeSpan.FromMilliseconds(_player.TotalTime);

            ProgressSlider.Value = _player.CurrentTime;
            ProgressSlider.Maximum = _player.TotalTime;

            PositionText.Text = $"{cur:mm\\:ss} / {total:mm\\:ss}";
        }
        else
        {
            ProgressSlider.Value = _player.CurrentIndex;
            ProgressSlider.Maximum = _player.FrameCount;

            PositionText.Text = $"{_player.CurrentIndex} / {_player.FrameCount}";
        }
    }
}
