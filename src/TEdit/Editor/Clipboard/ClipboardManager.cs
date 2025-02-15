using System;
using System.Collections.ObjectModel;
using System.Windows;
using TEdit.Common.Reactive;
using TEdit.ViewModel;
using XNA = Microsoft.Xna.Framework;
using TEdit.Terraria;
using System.Collections.Generic;
using System.Linq;
using TEdit.Geometry;
using TEdit.Configuration;
using TEdit.Editor.Undo;

namespace TEdit.Editor.Clipboard;

public class ClipboardManager : ObservableObject
{
    private bool _pasteEmpty = true;
    private bool _pasteTiles = true;
    private bool _pasteWalls = true;
    private bool _pasteLiquids = true;
    private bool _pasteWires = true;
    private bool _pasteSprites = true;
    private bool _pasteOverTiles = true;
    private readonly ObservableCollection<ClipboardBufferPreview> _loadedBuffers = new ObservableCollection<ClipboardBufferPreview>();
    private ClipboardBufferPreview _buffer;
    private readonly ISelection _selection;
    private readonly IUndoManager _undo;
    private readonly NotifyTileChanged _notifyTileChanged;

    public ClipboardManager(
        ISelection selection,
        IUndoManager undo,
        NotifyTileChanged? notifyTileChanged)
    {
        _selection = selection;
        _undo = undo;
        _notifyTileChanged = notifyTileChanged;
    }

    public bool PasteEmpty
    {
        get { return _pasteEmpty; }
        set { Set(nameof(PasteEmpty), ref _pasteEmpty, value); }
    }
    public bool PasteTiles
    {
        get { return _pasteTiles; }
        set { Set(nameof(PasteTiles), ref _pasteTiles, value); }
    }
    public bool PasteWalls
    {
        get { return _pasteWalls; }
        set { Set(nameof(PasteWalls), ref _pasteWalls, value); }
    }
    public bool PasteLiquids
    {
        get { return _pasteLiquids; }
        set { Set(nameof(PasteLiquids), ref _pasteLiquids, value); }
    }
    public bool PasteWires
    {
        get { return _pasteWires; }
        set { Set(nameof(PasteWires), ref _pasteWires, value); }
    }
    public bool PasteSprites
    {
        get { return _pasteSprites; }
        set { Set(nameof(PasteSprites), ref _pasteSprites, value); }
    }

    public bool PasteOverTiles
    {
        get { return _pasteOverTiles; }
        set { Set(nameof(PasteOverTiles), ref _pasteOverTiles, value); }
    }

    public ClipboardBufferPreview Buffer
    {
        get { return _buffer; }
        set { Set(nameof(Buffer), ref _buffer, value); }
    }

    public ObservableCollection<ClipboardBufferPreview> LoadedBuffers
    {
        get { return _loadedBuffers; }
    }

    public void ClearBuffers()
    {
        Buffer = null;
        LoadedBuffers.Clear();
    }

    public void Remove(ClipboardBufferPreview item)
    {
        if (LoadedBuffers.Contains(item))
            LoadedBuffers.Remove(item);
    }

    public void Import(string filename)
    {
        var bufferData = ClipboardBuffer.Load(filename);
        var buffer = new ClipboardBufferPreview(bufferData);

        if (buffer != null)
        {
            LoadedBuffers.Add(buffer);
        }
    }

    public void CopySelection(World world, RectangleInt32 selection)
    {
        var bufferData = ClipboardBuffer.GetSelectionBuffer(world, selection);
        LoadedBuffers.Add(new ClipboardBufferPreview(bufferData));
		
        Buffer = new ClipboardBufferPreview(bufferData); // Set the last added buffer as the active one
    }

    public void PasteBufferIntoWorld(World world, Vector2Int32 anchor)
    {
        if (Buffer == null) return;
        if (!PasteTiles && !PasteLiquids && !PasteWalls && !PasteWires) return;

        _selection.IsActive = false; // clear selection when pasting to prevent "unable to use pencil" issue

        ClipboardBuffer buffer = Buffer.Buffer;
        var pasteOptions = new PasteOptions
        {
            PasteEmpty = PasteEmpty,
            PasteLiquids = PasteLiquids,
            PasteWalls = PasteWalls,
            PasteOverTiles = PasteOverTiles,
            PasteSprites = PasteSprites,
            PasteTiles = PasteTiles,
            PasteWires = PasteWires
        };

        buffer.Paste(world, anchor, _undo, pasteOptions);

        /* Heathtech */
        _notifyTileChanged(anchor.X, anchor.Y, buffer.Size.X, buffer.Size.Y);
    }

    

    // Reverse the buffer along the x- or y- axis
    public void Flip(ClipboardBufferPreview bufferPreview, bool flipX, bool rotate)
    {
        int bufferIndex = LoadedBuffers.IndexOf(bufferPreview);

        var flippedBuffer = ClipboardBuffer.Flip(bufferPreview.Buffer, flipX, rotate);
        ClipboardBufferPreview bufferPreviewNew = new ClipboardBufferPreview(flippedBuffer);

        LoadedBuffers.Insert(bufferIndex, bufferPreviewNew);
        LoadedBuffers.RemoveAt(bufferIndex + 1);
        Buffer = bufferPreviewNew;
    }

    public void FlipX(ClipboardBufferPreview buffer)
    {
        Flip(buffer, true, false);
    }
    public void FlipY(ClipboardBufferPreview buffer)
    {
        Flip(buffer, false, false);
    }
    public void Rotate(ClipboardBufferPreview buffer)
    {
        Flip(buffer, false, true);
    }
}
