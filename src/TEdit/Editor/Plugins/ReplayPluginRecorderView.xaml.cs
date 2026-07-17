using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Symbol = Wpf.Ui.Controls.SymbolRegular;

namespace TEdit.Editor.Plugins;

public partial class ReplayPluginRecorderView
{
    private readonly ReplayRecorder _recorder = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(50) };

    public ReplayPluginRecorderView()
    {
        InitializeComponent();
        _timer.Tick += Timer_Tick;
        Closed += (_, _) => _timer.Stop();
    }

    private void RecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (_recorder.IsRecording)
            StopRecording();
        else
            StartRecording();
    }

    private void StartRecording()
    {
        RecordIcon.Symbol = Symbol.Stop20;
        RecordButton.Foreground = System.Windows.Media.Brushes.Red;
        _recorder.Start();
        _timer.Start();
    }

    private void StopRecording()
    {
        _timer.Stop();
        _recorder.Stop();

        var title = _recorder.Recording.BaselineWorld.Title;
        var worldName = string.Join("-", title.Split(Path.GetInvalidFileNameChars()));
        var startTime = _recorder.Recording.StartTime.ToString("yyyyMMddHHmm");

        var defaultName = $"{worldName}_{startTime}";
        var defaultPath = Path.Combine(Path.GetTempPath(), $"{defaultName}.TEditReplay");

        var dialog = new SaveFileDialog { FileName = defaultName, Filter = "Replay Files|*.TEditReplay" };
        if (dialog.ShowDialog() == true)
            _recorder.Recording.Save(dialog.FileName);
        else
            _recorder.Recording.Save(defaultPath);

        RecordIcon.Symbol = Symbol.Record20;
        RecordButton.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
        TimerText.Text = "00:00:00 F:0";
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        if (_recorder.IsRecording) return;

        var dialog = new OpenFileDialog { Filter = "Replay Files|*.TEditReplay" };
        if (dialog.ShowDialog() != true) return;

        ReplayFile file;
        try
        {
            file = new ReplayFile();
            file.Load(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open replay file.\n\n{ex.Message}",
                "Replay", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var playerView = new ReplayPluginPlayerView();
        playerView.Load(file);
        playerView.Show();
        Close();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        _recorder.Poll();
        if (_recorder.Recording != null)
        {
            var elapsed = DateTime.UtcNow - _recorder.Recording.StartTime;
            TimerText.Text = $"{elapsed:hh\\:mm\\:ss} F:{_recorder.Recording.Frames.Count}";
        }
    }
}
