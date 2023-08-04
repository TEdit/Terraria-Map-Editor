using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using TEdit.Configuration;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria.Editor;
using TEdit.Terraria.Objects;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public class TextStatuePlugin : BasePlugin, INotifyPropertyChanged
{
    Dictionary<char, SpriteItem> _textFrames = new();
    private TileProperty _textStatueTileProperties;
    private Vector2Short _size;

    public TextStatuePlugin(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        Name = "Generate Text Statues";
    }

    private ClipboardBuffer _generatedSchematic;
    public ClipboardBuffer GeneratredSchematic
    {
        get { return _generatedSchematic; }
        set { _generatedSchematic = value; OnPropertyChanged(); }
    }

    public override void Execute()
    {
        var view = new TextStatuePluginView();
        view.Owner = Application.Current.MainWindow;


        if (view.ShowDialog() == true)
        {
            GenerateTextStatues(
                view.ViewModel.TextValue,
                (byte)view.ViewModel.LetterSpacing,
                (byte)view.ViewModel.LineSpacing,
                view.ViewModel.LineLength,
                view.ViewModel.Justification);

            _wvm.SelectedTabIndex = 3;
        }
    }

    IEnumerable<string> SplitToLines(string stringToSplit, int maximumLineLength = 64)
    {
        var words = stringToSplit.Split(' ');
        StringBuilder line = new StringBuilder(400);

        foreach (var word in words)
        {
            if (line.Length + word.Length > maximumLineLength)
            {
                yield return line.ToString();
                line.Clear();
            }
            else
            {
                line.Append(' ');
            }

            line.Append(word);
        }

        yield return line.ToString();
    }

    public void GenerateTextStatues(
        string text,
        byte letterSpacing = 0,
        byte rowSpacing = 1,
        int splitLength = 400,
        TextAlignment justification = TextAlignment.Left)
    {
        if (string.IsNullOrWhiteSpace(text)) { return; }

        Initialize();

        string textUpper = text.ToUpperInvariant();

        var linesSplit = textUpper.Split('\n');
        var lines = new List<string>();

        foreach (var line in linesSplit)
        {
            lines.AddRange(SplitToLines(line, splitLength));
        }


        if (lines == null || lines.Count == 0) { return; }
        int lineCount = lines.Count;
        int maxLineWidth = lines.Max(x => x.Length);

        int height = (_size.Height * lineCount) + (rowSpacing * (lineCount - 1));
        int width = (_size.Width * maxLineWidth) + (letterSpacing * (maxLineWidth - 1));

        Vector2Int32 _generatedSchematicSize = new Vector2Int32(width, height);
        _generatedSchematic = new(_generatedSchematicSize, true);

        int y = 0;
        foreach (var line in lines)
        {
            // justify the text for the line
            var start = justification switch
            {
                TextAlignment.Left => 0,
                TextAlignment.Center => (int)Math.Ceiling((maxLineWidth - line.Length) / 2.0),
                TextAlignment.Right => maxLineWidth - line.Length,
                _ => 0,
            };


            int x = _size.Width * start;

            foreach (char c in line)
            {
                if (_textFrames.TryGetValue(c, out var sprite))
                {
                    sprite.Place(x, y, _generatedSchematic);
                }

                x += _size.Width + letterSpacing;
            }

            y += _size.Height + rowSpacing;
        }


        var bufferRendered = new ClipboardBufferPreview(_generatedSchematic);
        _wvm.Clipboard.LoadedBuffers.Add(bufferRendered);
        _wvm.ClipboardSetActiveCommand.Execute(bufferRendered);
    }

    private void Initialize()
    {
        if (_textStatueTileProperties == null)
        {
            _textStatueTileProperties = WorldConfiguration.TileProperties[337];
            var sprite = WorldConfiguration.Sprites2.FirstOrDefault(x => x.Tile == (uint)_textStatueTileProperties.Id);

            _size = _textStatueTileProperties.FrameSize.FirstOrDefault();

            foreach (var frame in sprite.Styles)
            {
                char c = frame.Name[13]; // [13] - skips the first 13 chars of the string.
                _textFrames[c] = frame;
            }
        }
    }

    public new event PropertyChangedEventHandler PropertyChanged;
    // Create the OnPropertyChanged method to raise the event
    // The calling member's name will be used as the parameter.
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
