using System.Collections.Generic;
using System.Windows.Input;
using TEdit.Configuration;

namespace TEdit.Input;

/// <summary>
/// Main service for handling all input (keyboard, mouse, wheel).
/// Manages action registration, binding resolution, and user customizations.
/// </summary>
public class InputService
{
    private readonly InputBindingRegistry _registry = new();

    public InputBindingRegistry Registry => _registry;

    /// <summary>Initialize the service with default actions and load user customizations.</summary>
    public void Initialize()
    {
        RegisterDefaultActions();
        LoadUserCustomizations();
    }

    /// <summary>Handle keyboard input and return matching action IDs.</summary>
    public List<string> HandleKeyboard(Key key, ModifierKeys modifiers, InputScope scope)
    {
        var binding = InputBinding.Keyboard(key, modifiers);
        return _registry.ResolveInput(binding, scope);
    }

    /// <summary>Handle mouse button input and return matching action IDs.</summary>
    public List<string> HandleMouseButton(TEditMouseButton button, ModifierKeys modifiers, InputScope scope)
    {
        var binding = InputBinding.Mouse(button, modifiers);
        return _registry.ResolveInput(binding, scope);
    }

    /// <summary>Handle mouse wheel input and return matching action IDs.</summary>
    public List<string> HandleMouseWheel(int delta, ModifierKeys modifiers, InputScope scope)
    {
        var direction = delta > 0 ? MouseWheelDirection.Up : MouseWheelDirection.Down;
        var binding = InputBinding.Wheel(direction, modifiers);
        return _registry.ResolveInput(binding, scope);
    }

    /// <summary>Save user customizations to UserSettings.</summary>
    public void SaveUserCustomizations()
    {
        var customizations = _registry.GetUserCustomizations();
        UserSettingsService.Current.InputBindings = customizations;
    }

    private void LoadUserCustomizations()
    {
        var customizations = UserSettingsService.Current.InputBindings;
        if (customizations != null && customizations.Count > 0)
        {
            _registry.LoadUserCustomizations(customizations);
        }
    }

    private void RegisterDefaultActions()
    {
        // === File Operations ===
        Register(new InputAction
        {
            Id = "file.new",
            Name = "New World",
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.N, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "file.open",
            Name = "Open",
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.O, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "file.save",
            Name = "Save",
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.S, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "file.saveas",
            Name = "Save As",
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.S, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "file.saveasversion",
            Name = "Save As Version",
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.S, ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "file.reload",
            Name = "Reload World",
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.F5) }
        });

        // === Editing ===
        Register(new InputAction
        {
            Id = "edit.copy",
            Name = "Copy",
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.C, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "edit.paste",
            Name = "Paste",
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.V, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "edit.undo",
            Name = "Undo",
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Z, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "edit.redo",
            Name = "Redo",
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Y, ModifierKeys.Control) }
        });

        // === Selection ===
        Register(new InputAction
        {
            Id = "selection.all",
            Name = "Select All",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.A, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.none",
            Name = "Deselect",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.D, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.delete",
            Name = "Delete Selection",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Delete) }
        });

        // Selection movement (1 tile)
        Register(new InputAction
        {
            Id = "selection.move.up",
            Name = "Move Selection Up",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up) }
        });

        Register(new InputAction
        {
            Id = "selection.move.down",
            Name = "Move Selection Down",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down) }
        });

        Register(new InputAction
        {
            Id = "selection.move.left",
            Name = "Move Selection Left",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left) }
        });

        Register(new InputAction
        {
            Id = "selection.move.right",
            Name = "Move Selection Right",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right) }
        });

        // Selection fast movement (5 tiles)
        Register(new InputAction
        {
            Id = "selection.move.up.fast",
            Name = "Move Selection Up (Fast)",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.move.down.fast",
            Name = "Move Selection Down (Fast)",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.move.left.fast",
            Name = "Move Selection Left (Fast)",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.move.right.fast",
            Name = "Move Selection Right (Fast)",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right, ModifierKeys.Shift) }
        });

        // Selection resizing (1 tile, top-left anchored)
        Register(new InputAction
        {
            Id = "selection.resize.up",
            Name = "Shrink Selection Height",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.down",
            Name = "Grow Selection Height",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.left",
            Name = "Shrink Selection Width",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.right",
            Name = "Grow Selection Width",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right, ModifierKeys.Control) }
        });

        // Selection fast resizing (5 tiles)
        Register(new InputAction
        {
            Id = "selection.resize.up.fast",
            Name = "Shrink Selection Height (Fast)",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.down.fast",
            Name = "Grow Selection Height (Fast)",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.left.fast",
            Name = "Shrink Selection Width (Fast)",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.right.fast",
            Name = "Grow Selection Width (Fast)",
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        // Selection mouse corner adjustment
        Register(new InputAction
        {
            Id = "selection.adjust.startpoint",
            Name = "Move Selection Start Point",
            Category = InputCategory.Selection,
            Scope = InputScope.Editor,
            Description = "Ctrl+click/drag to move top-left corner of selection",
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.adjust.endpoint",
            Name = "Move Selection End Point",
            Category = InputCategory.Selection,
            Scope = InputScope.Editor,
            Description = "Shift+click/drag to move bottom-right corner of selection",
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "edit.crop",
            Name = "Crop World",
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
        });

        Register(new InputAction
        {
            Id = "edit.expand",
            Name = "Expand World",
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
        });

        Register(new InputAction
        {
            Id = "app.settings",
            Name = "Settings",
            Category = InputCategory.File,
            Scope = InputScope.Application,
        });

        // === Navigation ===
        Register(new InputAction
        {
            Id = "nav.scroll.up",
            Name = "Scroll Up",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.down",
            Name = "Scroll Down",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.left",
            Name = "Scroll Left",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.right",
            Name = "Scroll Right",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.up.fast",
            Name = "Scroll Up Fast",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.down.fast",
            Name = "Scroll Down Fast",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.left.fast",
            Name = "Scroll Left Fast",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.right.fast",
            Name = "Scroll Right Fast",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "nav.pan",
            Name = "Pan Mode",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Space) }
        });

        Register(new InputAction
        {
            Id = "nav.zoom.in",
            Name = "Zoom In",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings =
            {
                InputBinding.Keyboard(Key.OemPlus, ModifierKeys.Control),
                InputBinding.Wheel(MouseWheelDirection.Up)
            }
        });

        Register(new InputAction
        {
            Id = "nav.zoom.out",
            Name = "Zoom Out",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings =
            {
                InputBinding.Keyboard(Key.OemMinus, ModifierKeys.Control),
                InputBinding.Wheel(MouseWheelDirection.Down)
            }
        });

        Register(new InputAction
        {
            Id = "nav.reset",
            Name = "Reset Tool / Deselect",
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Escape) }
        });

        // === Tool Selection ===
        Register(new InputAction
        {
            Id = "tool.arrow",
            Name = "Arrow Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.A) }
        });

        Register(new InputAction
        {
            Id = "tool.brush",
            Name = "Brush Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.B) }
        });

        Register(new InputAction
        {
            Id = "tool.pencil",
            Name = "Pencil Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.E) }
        });

        Register(new InputAction
        {
            Id = "tool.fill",
            Name = "Fill Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.F) }
        });

        Register(new InputAction
        {
            Id = "tool.picker",
            Name = "Picker Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.R) }
        });

        Register(new InputAction
        {
            Id = "tool.point",
            Name = "Point Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.P) }
        });

        Register(new InputAction
        {
            Id = "tool.selection",
            Name = "Selection Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.S) }
        });

        Register(new InputAction
        {
            Id = "tool.sprite",
            Name = "Sprite Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.T) }
        });

        Register(new InputAction
        {
            Id = "tool.hammer",
            Name = "Hammer Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.H) }
        });

        Register(new InputAction
        {
            Id = "tool.morph",
            Name = "Morph Tool",
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.M) }
        });

        // === Toggles ===
        Register(new InputAction
        {
            Id = "toggle.eraser",
            Name = "Toggle Eraser",
            Category = InputCategory.Toggles,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Z) }
        });

        Register(new InputAction
        {
            Id = "toggle.swap",
            Name = "Swap Colors",
            Category = InputCategory.Toggles,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.X) }
        });

        // CAD wire routing actions
        Register(new InputAction
        {
            Id = "editor.wire.modecycle",
            Name = "Cycle Wire Drawing Mode",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = "Cycle wire drawing mode: Off → Wire90 → Wire45 → Off",
            DefaultBindings = { InputBinding.Keyboard(Key.W, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.togglehv",
            Name = "Toggle Wire H/V Direction",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = "Toggle between horizontal-first, vertical-first, and auto-detect wire routing",
            DefaultBindings = { InputBinding.Keyboard(Key.Q, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.color1",
            Name = "Toggle Wire Red",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = "Toggle red wire active",
            DefaultBindings = { InputBinding.Keyboard(Key.D1) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.color2",
            Name = "Toggle Wire Blue",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = "Toggle blue wire active",
            DefaultBindings = { InputBinding.Keyboard(Key.D2) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.color3",
            Name = "Toggle Wire Green",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = "Toggle green wire active",
            DefaultBindings = { InputBinding.Keyboard(Key.D3) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.color4",
            Name = "Toggle Wire Yellow",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = "Toggle yellow wire active",
            DefaultBindings = { InputBinding.Keyboard(Key.D4) }
        });

        Register(new InputAction
        {
            Id = "toggle.tile",
            Name = "Toggle Tile Style",
            Category = InputCategory.Toggles,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Q) }
        });

        Register(new InputAction
        {
            Id = "toggle.wall",
            Name = "Toggle Wall Style",
            Category = InputCategory.Toggles,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.W) }
        });

        // === Editor (Tool) Actions ===
        Register(new InputAction
        {
            Id = "editor.draw",
            Name = "Draw / Primary Action",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = "Primary drawing action in tools",
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left) }
        });

        Register(new InputAction
        {
            Id = "editor.secondary",
            Name = "Secondary Action",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = "Secondary action in tools (inspect, deselect, pick mask, etc.)",
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Right) }
        });

        Register(new InputAction
        {
            Id = "editor.draw.constrain",
            Name = "Draw Constrained (H/V/45°)",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = "Constrain drawing to horizontal, vertical, or 45° diagonal lines",
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "editor.draw.line",
            Name = "Draw Point-to-Point Line",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = "Draw straight lines between click points",
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "editor.pan",
            Name = "Pan Viewport",
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = "Drag to pan the viewport",
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Middle) }
        });

        // === Help ===
        Register(new InputAction
        {
            Id = "help.wiki",
            Name = "Wiki / Help",
            Category = InputCategory.Help,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.F1) }
        });
    }

    private void Register(InputAction action) => _registry.RegisterAction(action);
}
