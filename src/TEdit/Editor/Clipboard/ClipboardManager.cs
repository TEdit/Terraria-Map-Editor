using System;
using System.Collections.ObjectModel;
using System.Windows;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using XNA = Microsoft.Xna.Framework;
using TEdit.Terraria;
using System.Collections.Generic;
using System.Linq;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Editor.Undo;
using TEdit.ViewModel;

namespace TEdit.Editor.Clipboard;

public partial class ClipboardManager : ReactiveObject
{
    [Reactive]
    private bool _pasteEmpty = true;

    [Reactive]
    private bool _pasteTiles = true;

    [Reactive]
    private bool _pasteWalls = true;

    [Reactive]
    private bool _pasteLiquids = true;

    [Reactive]
    private bool _pasteWires = true;

    [Reactive]
    private bool _pasteSprites = true;

    [Reactive]
    private bool _pasteOverTiles = true;

    private readonly ObservableCollection<ClipboardBufferPreview> _loadedBuffers = new ObservableCollection<ClipboardBufferPreview>();

    [Reactive]
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
        bool onlyCopyFiltered = FilterManager.FilterClipboard;
        var bufferData = ClipboardBuffer.GetSelectionBuffer(
            world, selection,
            onlyCopyFiltered,
            tileFilter:   onlyCopyFiltered ? (id => FilterManager.TileIsNotAllowed(id) && FilterManager.SpriteIsNotAllowed(id)) : null,
            wallFilter:   onlyCopyFiltered ? FilterManager.WallIsNotAllowed : null,
            liquidFilter: onlyCopyFiltered ? (id => FilterManager.LiquidIsNotAllowed((LiquidType)id)) : null,
            wireFilter:   onlyCopyFiltered ? (id => FilterManager.WireIsNotAllowed((FilterManager.WireType)id)) : null
        );

        LoadedBuffers.Add(new ClipboardBufferPreview(bufferData));
        Buffer = new ClipboardBufferPreview(bufferData); // Set the last added buffer as the active one
    }

    public void PasteBufferIntoWorld(World world, Vector2Int32 anchor)
    {
        if (Buffer == null) return;
        if (!PasteTiles && !PasteLiquids && !PasteWalls && !PasteWires && !PasteSprites) return;

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
