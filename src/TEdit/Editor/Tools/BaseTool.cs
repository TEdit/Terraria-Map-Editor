using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Input;
using TEdit.UI;
using TEdit.ViewModel;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public abstract partial class BaseTool : ReactiveObject, ITool
{
    protected WriteableBitmap _preview;
    protected WorldViewModel _wvm;
    private double _previewScale = 1;

    protected BaseTool(WorldViewModel worldViewModel)
    {
        _wvm = worldViewModel;
        _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
        _preview.Clear();
        _preview.SetPixel(0, 0, 127, 0, 90, 255);
    }

    #region ITool Members

    public string Name { get; protected set; }

    public string Title => Properties.Language.ResourceManager.GetString($"tool_{Name.ToLower()}_title") ?? Name;

    public virtual ToolType ToolType { get; protected set; }

    public virtual BitmapImage Icon { get; protected set; }

    public virtual SymbolRegular SymbolIcon { get; protected set; } = SymbolRegular.Empty;

    public virtual ImageSource? VectorIcon { get; protected set; }

    [Reactive]
    private bool _isActive;

    public virtual void MouseDown(TileMouseState e)
    {
    }

    public virtual void MouseMove(TileMouseState e)
    {
    }

    public virtual void MouseUp(TileMouseState e)
    {
    }

    public virtual void MouseWheel(TileMouseState e)
    {
    }

    public double PreviewScale
    {
        get { return _previewScale; }
        protected set { _previewScale = value; }
    }

    public virtual WriteableBitmap PreviewTool()
    {
        return _preview;
    }

    public virtual bool PreviewIsTexture
    {
        get { return false; }
    }

    #endregion

    #region Input Helpers

    /// <summary>
    /// Gets active editor actions based on current mouse state and keyboard modifiers.
    /// </summary>
    protected List<string> GetActiveActions(TileMouseState e)
    {
        var actions = new List<string>();
        var modifiers = Keyboard.Modifiers;

        // Check left button
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            actions.AddRange(App.Input.HandleMouseButton(TEditMouseButton.Left, modifiers, TEdit.Input.InputScope.Editor));
        }

        // Check right button
        if (e.RightButton == MouseButtonState.Pressed)
        {
            actions.AddRange(App.Input.HandleMouseButton(TEditMouseButton.Right, modifiers, TEdit.Input.InputScope.Editor));
        }

        // Check middle button
        if (e.MiddleButton == MouseButtonState.Pressed)
        {
            actions.AddRange(App.Input.HandleMouseButton(TEditMouseButton.Middle, modifiers, TEdit.Input.InputScope.Editor));
        }

        return actions;
    }

    /// <summary>
    /// Checks if a specific action is active based on current input.
    /// </summary>
    protected bool IsActionActive(TileMouseState e, string actionId)
    {
        return GetActiveActions(e).Contains(actionId);
    }

    #endregion
}
