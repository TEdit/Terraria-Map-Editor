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
            Name = Properties.Language.input_file_new,
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.N, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "file.open",
            Name = Properties.Language.input_file_open,
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.O, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "file.save",
            Name = Properties.Language.input_file_save,
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.S, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "file.saveas",
            Name = Properties.Language.input_file_saveas,
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.S, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "file.saveasversion",
            Name = Properties.Language.input_file_saveasversion,
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.S, ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "file.reload",
            Name = Properties.Language.input_file_reload,
            Category = InputCategory.File,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.F5) }
        });

        // === Editing ===
        Register(new InputAction
        {
            Id = "edit.copy",
            Name = Properties.Language.input_edit_copy,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.C, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "edit.cut",
            Name = Properties.Language.input_edit_cut,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.X, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "edit.paste",
            Name = Properties.Language.input_edit_paste,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.V, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "edit.undo",
            Name = Properties.Language.input_edit_undo,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Z, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "edit.redo",
            Name = Properties.Language.input_edit_redo,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Y, ModifierKeys.Control) }
        });

        // === Paste Layer ===
        Register(new InputAction
        {
            Id = "paste.accept",
            Name = Properties.Language.input_paste_accept,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Enter) }
        });

        Register(new InputAction
        {
            Id = "paste.rotate.cw",
            Name = Properties.Language.input_paste_rotate_cw,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Q, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "paste.rotate.ccw",
            Name = Properties.Language.input_paste_rotate_ccw,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Q, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "paste.flip.h",
            Name = Properties.Language.input_paste_flip_h,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.H, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "paste.flip.v",
            Name = Properties.Language.input_paste_flip_v,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.H, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        // === Selection ===
        Register(new InputAction
        {
            Id = "selection.all",
            Name = Properties.Language.input_selection_all,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.A, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.none",
            Name = Properties.Language.input_selection_none,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.D, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.delete",
            Name = Properties.Language.input_selection_delete,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Delete) }
        });

        // Selection movement (1 tile)
        Register(new InputAction
        {
            Id = "selection.move.up",
            Name = Properties.Language.input_selection_move_up,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up) }
        });

        Register(new InputAction
        {
            Id = "selection.move.down",
            Name = Properties.Language.input_selection_move_down,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down) }
        });

        Register(new InputAction
        {
            Id = "selection.move.left",
            Name = Properties.Language.input_selection_move_left,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left) }
        });

        Register(new InputAction
        {
            Id = "selection.move.right",
            Name = Properties.Language.input_selection_move_right,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right) }
        });

        // Selection fast movement (5 tiles)
        Register(new InputAction
        {
            Id = "selection.move.up.fast",
            Name = Properties.Language.input_selection_move_up_fast,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.move.down.fast",
            Name = Properties.Language.input_selection_move_down_fast,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.move.left.fast",
            Name = Properties.Language.input_selection_move_left_fast,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.move.right.fast",
            Name = Properties.Language.input_selection_move_right_fast,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right, ModifierKeys.Shift) }
        });

        // Selection resizing (1 tile, top-left anchored)
        Register(new InputAction
        {
            Id = "selection.resize.up",
            Name = Properties.Language.input_selection_resize_up,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.down",
            Name = Properties.Language.input_selection_resize_down,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.left",
            Name = Properties.Language.input_selection_resize_left,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.right",
            Name = Properties.Language.input_selection_resize_right,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right, ModifierKeys.Control) }
        });

        // Selection fast resizing (5 tiles)
        Register(new InputAction
        {
            Id = "selection.resize.up.fast",
            Name = Properties.Language.input_selection_resize_up_fast,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.down.fast",
            Name = Properties.Language.input_selection_resize_down_fast,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.left.fast",
            Name = Properties.Language.input_selection_resize_left_fast,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "selection.resize.right.fast",
            Name = Properties.Language.input_selection_resize_right_fast,
            Category = InputCategory.Selection,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right, ModifierKeys.Control | ModifierKeys.Shift) }
        });

        // Selection mouse corner adjustment
        Register(new InputAction
        {
            Id = "selection.adjust.startpoint",
            Name = Properties.Language.input_selection_adjust_startpoint,
            Category = InputCategory.Selection,
            Scope = InputScope.Editor,
            Description = Properties.Language.input_selection_adjust_startpoint_desc,
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "selection.adjust.endpoint",
            Name = Properties.Language.input_selection_adjust_endpoint,
            Category = InputCategory.Selection,
            Scope = InputScope.Editor,
            Description = Properties.Language.input_selection_adjust_endpoint_desc,
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "edit.crop",
            Name = Properties.Language.input_edit_crop,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
        });

        Register(new InputAction
        {
            Id = "edit.expand",
            Name = Properties.Language.input_edit_expand,
            Category = InputCategory.Editing,
            Scope = InputScope.Application,
        });

        Register(new InputAction
        {
            Id = "app.settings",
            Name = Properties.Language.input_app_settings,
            Category = InputCategory.File,
            Scope = InputScope.Application,
        });

        // === Navigation ===
        Register(new InputAction
        {
            Id = "nav.scroll.up",
            Name = Properties.Language.input_nav_scroll_up,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.down",
            Name = Properties.Language.input_nav_scroll_down,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.left",
            Name = Properties.Language.input_nav_scroll_left,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.right",
            Name = Properties.Language.input_nav_scroll_right,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.up.fast",
            Name = Properties.Language.input_nav_scroll_up_fast,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Up, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.down.fast",
            Name = Properties.Language.input_nav_scroll_down_fast,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Down, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.left.fast",
            Name = Properties.Language.input_nav_scroll_left_fast,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Left, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "nav.scroll.right.fast",
            Name = Properties.Language.input_nav_scroll_right_fast,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Right, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "nav.pan",
            Name = Properties.Language.input_nav_pan,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Space) }
        });

        Register(new InputAction
        {
            Id = "nav.zoom.in",
            Name = Properties.Language.input_nav_zoom_in,
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
            Name = Properties.Language.input_nav_zoom_out,
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
            Name = Properties.Language.input_nav_reset,
            Category = InputCategory.Navigation,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Escape) }
        });

        // === Tool Selection ===
        Register(new InputAction
        {
            Id = "tool.arrow",
            Name = Properties.Language.input_tool_arrow,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.A) }
        });

        Register(new InputAction
        {
            Id = "tool.brush",
            Name = Properties.Language.input_tool_brush,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.B) }
        });

        Register(new InputAction
        {
            Id = "tool.pencil",
            Name = Properties.Language.input_tool_pencil,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.E) }
        });

        Register(new InputAction
        {
            Id = "tool.fill",
            Name = Properties.Language.input_tool_fill,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.F) }
        });

        Register(new InputAction
        {
            Id = "tool.picker",
            Name = Properties.Language.input_tool_picker,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.R) }
        });

        Register(new InputAction
        {
            Id = "tool.point",
            Name = Properties.Language.input_tool_point,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.P) }
        });

        Register(new InputAction
        {
            Id = "tool.selection",
            Name = Properties.Language.input_tool_selection,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.S) }
        });

        Register(new InputAction
        {
            Id = "tool.sprite",
            Name = Properties.Language.input_tool_sprite,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.T) }
        });

        Register(new InputAction
        {
            Id = "tool.hammer",
            Name = Properties.Language.input_tool_hammer,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.H) }
        });

        Register(new InputAction
        {
            Id = "tool.morph",
            Name = Properties.Language.input_tool_morph,
            Category = InputCategory.Tools,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.M) }
        });

        // === Toggles ===
        Register(new InputAction
        {
            Id = "toggle.eraser",
            Name = Properties.Language.input_toggle_eraser,
            Category = InputCategory.Toggles,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Z) }
        });

        Register(new InputAction
        {
            Id = "toggle.swap",
            Name = Properties.Language.input_toggle_swap,
            Category = InputCategory.Toggles,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.X) }
        });

        // CAD wire routing actions
        Register(new InputAction
        {
            Id = "editor.wire.modecycle",
            Name = Properties.Language.input_editor_wire_modecycle,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = Properties.Language.input_editor_wire_modecycle_desc,
            DefaultBindings = { InputBinding.Keyboard(Key.W, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.togglehv",
            Name = Properties.Language.input_editor_wire_togglehv,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = Properties.Language.input_editor_wire_togglehv_desc,
            DefaultBindings = { InputBinding.Keyboard(Key.Q, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.color1",
            Name = Properties.Language.input_editor_wire_color1,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = Properties.Language.input_editor_wire_color1_desc,
            DefaultBindings = { InputBinding.Keyboard(Key.D1) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.color2",
            Name = Properties.Language.input_editor_wire_color2,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = Properties.Language.input_editor_wire_color2_desc,
            DefaultBindings = { InputBinding.Keyboard(Key.D2) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.color3",
            Name = Properties.Language.input_editor_wire_color3,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = Properties.Language.input_editor_wire_color3_desc,
            DefaultBindings = { InputBinding.Keyboard(Key.D3) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.color4",
            Name = Properties.Language.input_editor_wire_color4,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Application,
            Description = Properties.Language.input_editor_wire_color4_desc,
            DefaultBindings = { InputBinding.Keyboard(Key.D4) }
        });

        Register(new InputAction
        {
            Id = "editor.wire.trace",
            Name = Properties.Language.input_editor_wire_trace,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = Properties.Language.input_editor_wire_trace_desc,
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Alt) }
        });

        Register(new InputAction
        {
            Id = "toggle.tile",
            Name = Properties.Language.input_toggle_tile,
            Category = InputCategory.Toggles,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.Q) }
        });

        Register(new InputAction
        {
            Id = "toggle.wall",
            Name = Properties.Language.input_toggle_wall,
            Category = InputCategory.Toggles,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.W) }
        });

        // === Editor (Tool) Actions ===
        Register(new InputAction
        {
            Id = "editor.draw",
            Name = Properties.Language.input_editor_draw,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = Properties.Language.input_editor_draw_desc,
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left) }
        });

        Register(new InputAction
        {
            Id = "editor.secondary",
            Name = Properties.Language.input_editor_secondary,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = Properties.Language.input_editor_secondary_desc,
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Right) }
        });

        Register(new InputAction
        {
            Id = "editor.draw.constrain",
            Name = Properties.Language.input_editor_draw_constrain,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = Properties.Language.input_editor_draw_constrain_desc,
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Control) }
        });

        Register(new InputAction
        {
            Id = "editor.draw.line",
            Name = Properties.Language.input_editor_draw_line,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = Properties.Language.input_editor_draw_line_desc,
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Shift) }
        });

        Register(new InputAction
        {
            Id = "editor.pan",
            Name = Properties.Language.input_editor_pan,
            Category = InputCategory.ToolDrawing,
            Scope = InputScope.Editor,
            Description = Properties.Language.input_editor_pan_desc,
            DefaultBindings = { InputBinding.Mouse(TEditMouseButton.Middle) }
        });

        // === Help ===
        Register(new InputAction
        {
            Id = "help.wiki",
            Name = Properties.Language.input_help_wiki,
            Category = InputCategory.Help,
            Scope = InputScope.Application,
            DefaultBindings = { InputBinding.Keyboard(Key.F1) }
        });
    }

    private void Register(InputAction action) => _registry.RegisterAction(action);
}
