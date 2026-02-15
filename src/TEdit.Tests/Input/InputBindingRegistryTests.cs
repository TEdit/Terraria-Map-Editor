using System.Windows.Input;
using TEdit.Input;
using Xunit;
using InputBinding = TEdit.Input.InputBinding;
using InputScope = TEdit.Input.InputScope;

namespace TEdit.Tests.Input;

public class InputBindingRegistryTests
{
    private InputBindingRegistry CreateRegistry()
    {
        return new InputBindingRegistry();
    }

    private InputAction CreateAction(string id, InputScope scope, params InputBinding[] bindings)
    {
        return new InputAction
        {
            Id = id,
            Name = id,
            Category = "Test",
            Scope = scope,
            DefaultBindings = bindings.ToList()
        };
    }

    [Fact]
    public void RegisterAction_AddsAction()
    {
        var registry = CreateRegistry();
        var action = CreateAction("test.action", InputScope.Application, InputBinding.Keyboard(Key.A));

        registry.RegisterAction(action);

        Assert.NotNull(registry.GetAction("test.action"));
        Assert.Single(registry.GetBindings("test.action"));
    }

    [Fact]
    public void GetAction_UnknownId_ReturnsNull()
    {
        var registry = CreateRegistry();

        Assert.Null(registry.GetAction("unknown"));
    }

    [Fact]
    public void GetBindings_ReturnsDefaultBindings()
    {
        var registry = CreateRegistry();
        var binding1 = InputBinding.Keyboard(Key.Z, ModifierKeys.Control);
        var binding2 = InputBinding.Keyboard(Key.Y, ModifierKeys.Control);
        var action = CreateAction("edit.undo", InputScope.Application, binding1, binding2);

        registry.RegisterAction(action);

        var bindings = registry.GetBindings("edit.undo");
        Assert.Equal(2, bindings.Count);
        Assert.Contains(binding1, bindings);
        Assert.Contains(binding2, bindings);
    }

    [Fact]
    public void GetBindings_UnknownAction_ReturnsEmpty()
    {
        var registry = CreateRegistry();

        var bindings = registry.GetBindings("unknown.action");

        Assert.Empty(bindings);
    }

    [Fact]
    public void ResolveInput_FindsMatchingAction()
    {
        var registry = CreateRegistry();
        var binding = InputBinding.Keyboard(Key.C, ModifierKeys.Control);
        var action = CreateAction("edit.copy", InputScope.Application, binding);

        registry.RegisterAction(action);

        var actions = registry.ResolveInput(binding, InputScope.Application);
        Assert.Single(actions);
        Assert.Contains("edit.copy", actions);
    }

    [Fact]
    public void ResolveInput_FiltersOnScope()
    {
        var registry = CreateRegistry();
        var binding = InputBinding.Keyboard(Key.C, ModifierKeys.Control);
        var appAction = CreateAction("app.action", InputScope.Application, binding);
        var editorAction = CreateAction("editor.action", InputScope.Editor, binding);

        registry.RegisterAction(appAction);
        registry.RegisterAction(editorAction);

        var appActions = registry.ResolveInput(binding, InputScope.Application);
        Assert.Single(appActions);
        Assert.Contains("app.action", appActions);

        var editorActions = registry.ResolveInput(binding, InputScope.Editor);
        Assert.Single(editorActions);
        Assert.Contains("editor.action", editorActions);
    }

    [Fact]
    public void ResolveInput_NoScope_ReturnsAllMatches()
    {
        var registry = CreateRegistry();
        var binding = InputBinding.Keyboard(Key.C, ModifierKeys.Control);
        var appAction = CreateAction("app.action", InputScope.Application, binding);
        var editorAction = CreateAction("editor.action", InputScope.Editor, binding);

        registry.RegisterAction(appAction);
        registry.RegisterAction(editorAction);

        var actions = registry.ResolveInput(binding);
        Assert.Equal(2, actions.Count);
        Assert.Contains("app.action", actions);
        Assert.Contains("editor.action", actions);
    }

    [Fact]
    public void ResolveInput_NoMatch_ReturnsEmpty()
    {
        var registry = CreateRegistry();
        var binding = InputBinding.Keyboard(Key.C, ModifierKeys.Control);

        var actions = registry.ResolveInput(binding, InputScope.Application);

        Assert.Empty(actions);
    }

    [Fact]
    public void SetUserBindings_OverridesDefaults()
    {
        var registry = CreateRegistry();
        var defaultBinding = InputBinding.Keyboard(Key.Z, ModifierKeys.Control);
        var userBinding = InputBinding.Keyboard(Key.U, ModifierKeys.Control | ModifierKeys.Shift);
        var action = CreateAction("edit.undo", InputScope.Application, defaultBinding);

        registry.RegisterAction(action);
        registry.SetUserBindings("edit.undo", new List<InputBinding> { userBinding });

        var bindings = registry.GetBindings("edit.undo");
        Assert.Single(bindings);
        Assert.Contains(userBinding, bindings);
        Assert.DoesNotContain(defaultBinding, bindings);
    }

    [Fact]
    public void SetUserBindings_UpdatesReverseLookup()
    {
        var registry = CreateRegistry();
        var defaultBinding = InputBinding.Keyboard(Key.Z, ModifierKeys.Control);
        var userBinding = InputBinding.Keyboard(Key.U, ModifierKeys.Control);
        var action = CreateAction("edit.undo", InputScope.Application, defaultBinding);

        registry.RegisterAction(action);
        registry.SetUserBindings("edit.undo", new List<InputBinding> { userBinding });

        // Old binding should not resolve
        var oldActions = registry.ResolveInput(defaultBinding, InputScope.Application);
        Assert.Empty(oldActions);

        // New binding should resolve
        var newActions = registry.ResolveInput(userBinding, InputScope.Application);
        Assert.Single(newActions);
        Assert.Contains("edit.undo", newActions);
    }

    [Fact]
    public void ResetToDefaults_RestoresOriginalBindings()
    {
        var registry = CreateRegistry();
        var defaultBinding = InputBinding.Keyboard(Key.Z, ModifierKeys.Control);
        var userBinding = InputBinding.Keyboard(Key.U, ModifierKeys.Control);
        var action = CreateAction("edit.undo", InputScope.Application, defaultBinding);

        registry.RegisterAction(action);
        registry.SetUserBindings("edit.undo", new List<InputBinding> { userBinding });
        registry.ResetToDefaults("edit.undo");

        var bindings = registry.GetBindings("edit.undo");
        Assert.Single(bindings);
        Assert.Contains(defaultBinding, bindings);
    }

    [Fact]
    public void ResetToDefaults_RestoresReverseLookup()
    {
        var registry = CreateRegistry();
        var defaultBinding = InputBinding.Keyboard(Key.Z, ModifierKeys.Control);
        var userBinding = InputBinding.Keyboard(Key.U, ModifierKeys.Control);
        var action = CreateAction("edit.undo", InputScope.Application, defaultBinding);

        registry.RegisterAction(action);
        registry.SetUserBindings("edit.undo", new List<InputBinding> { userBinding });
        registry.ResetToDefaults("edit.undo");

        // Default binding should resolve again
        var defaultActions = registry.ResolveInput(defaultBinding, InputScope.Application);
        Assert.Single(defaultActions);
        Assert.Contains("edit.undo", defaultActions);

        // User binding should not resolve
        var userActions = registry.ResolveInput(userBinding, InputScope.Application);
        Assert.Empty(userActions);
    }

    [Fact]
    public void ResetAllToDefaults_ResetsAllCustomizations()
    {
        var registry = CreateRegistry();
        var action1 = CreateAction("action1", InputScope.Application, InputBinding.Keyboard(Key.A));
        var action2 = CreateAction("action2", InputScope.Application, InputBinding.Keyboard(Key.B));

        registry.RegisterAction(action1);
        registry.RegisterAction(action2);
        registry.SetUserBindings("action1", new List<InputBinding> { InputBinding.Keyboard(Key.X) });
        registry.SetUserBindings("action2", new List<InputBinding> { InputBinding.Keyboard(Key.Y) });

        registry.ResetAllToDefaults();

        Assert.False(registry.IsCustomized("action1"));
        Assert.False(registry.IsCustomized("action2"));
    }

    [Fact]
    public void IsCustomized_ReturnsCorrectState()
    {
        var registry = CreateRegistry();
        var action = CreateAction("test.action", InputScope.Application, InputBinding.Keyboard(Key.T));

        registry.RegisterAction(action);

        Assert.False(registry.IsCustomized("test.action"));

        registry.SetUserBindings("test.action", new List<InputBinding> { InputBinding.Keyboard(Key.X) });

        Assert.True(registry.IsCustomized("test.action"));
    }

    [Fact]
    public void GetUserCustomizations_ReturnsOnlyCustomized()
    {
        var registry = CreateRegistry();
        var action1 = CreateAction("action1", InputScope.Application, InputBinding.Keyboard(Key.A));
        var action2 = CreateAction("action2", InputScope.Application, InputBinding.Keyboard(Key.B));

        registry.RegisterAction(action1);
        registry.RegisterAction(action2);
        registry.SetUserBindings("action1", new List<InputBinding> { InputBinding.Keyboard(Key.X) });

        var customizations = registry.GetUserCustomizations();

        Assert.Single(customizations);
        Assert.True(customizations.ContainsKey("action1"));
        Assert.False(customizations.ContainsKey("action2"));
    }

    [Fact]
    public void LoadUserCustomizations_AppliesStoredBindings()
    {
        var registry = CreateRegistry();
        var action = CreateAction("edit.copy", InputScope.Application, InputBinding.Keyboard(Key.C, ModifierKeys.Control));
        var customBinding = InputBinding.Keyboard(Key.Insert, ModifierKeys.Control);

        registry.RegisterAction(action);
        registry.LoadUserCustomizations(new Dictionary<string, List<InputBinding>>
        {
            ["edit.copy"] = new List<InputBinding> { customBinding }
        });

        var bindings = registry.GetBindings("edit.copy");
        Assert.Single(bindings);
        Assert.Contains(customBinding, bindings);
    }

    [Fact]
    public void LoadUserCustomizations_IgnoresUnknownActions()
    {
        var registry = CreateRegistry();
        var action = CreateAction("known.action", InputScope.Application, InputBinding.Keyboard(Key.A));

        registry.RegisterAction(action);
        registry.LoadUserCustomizations(new Dictionary<string, List<InputBinding>>
        {
            ["unknown.action"] = new List<InputBinding> { InputBinding.Keyboard(Key.X) }
        });

        // Should not throw and known action should be unchanged
        var bindings = registry.GetBindings("known.action");
        Assert.Single(bindings);
    }

    [Fact]
    public void DetectConflicts_FindsConflictsInSameScope()
    {
        var registry = CreateRegistry();
        var binding = InputBinding.Keyboard(Key.C, ModifierKeys.Control);
        var action1 = CreateAction("action1", InputScope.Application, binding);
        var action2 = CreateAction("action2", InputScope.Application, binding);

        registry.RegisterAction(action1);
        registry.RegisterAction(action2);

        var conflicts = registry.DetectConflicts();

        Assert.Single(conflicts);
        Assert.Equal(binding, conflicts[0].Binding);
        Assert.Equal(InputScope.Application, conflicts[0].Scope);
    }

    [Fact]
    public void DetectConflicts_NoConflictAcrossScopes()
    {
        var registry = CreateRegistry();
        var binding = InputBinding.Keyboard(Key.C, ModifierKeys.Control);
        var appAction = CreateAction("app.action", InputScope.Application, binding);
        var editorAction = CreateAction("editor.action", InputScope.Editor, binding);

        registry.RegisterAction(appAction);
        registry.RegisterAction(editorAction);

        var conflicts = registry.DetectConflicts();

        Assert.Empty(conflicts);
    }

    [Fact]
    public void DetectConflicts_NoConflictWhenDifferentBindings()
    {
        var registry = CreateRegistry();
        var action1 = CreateAction("action1", InputScope.Application, InputBinding.Keyboard(Key.A));
        var action2 = CreateAction("action2", InputScope.Application, InputBinding.Keyboard(Key.B));

        registry.RegisterAction(action1);
        registry.RegisterAction(action2);

        var conflicts = registry.DetectConflicts();

        Assert.Empty(conflicts);
    }

    [Fact]
    public void GetActionsByCategory_GroupsCorrectly()
    {
        var registry = CreateRegistry();
        var action1 = new InputAction { Id = "file.open", Name = "Open", Category = "File", Scope = InputScope.Application };
        var action2 = new InputAction { Id = "file.save", Name = "Save", Category = "File", Scope = InputScope.Application };
        var action3 = new InputAction { Id = "edit.copy", Name = "Copy", Category = "Edit", Scope = InputScope.Application };

        registry.RegisterAction(action1);
        registry.RegisterAction(action2);
        registry.RegisterAction(action3);

        var byCategory = registry.GetActionsByCategory();

        Assert.Equal(2, byCategory.Count);
        Assert.Equal(2, byCategory["File"].Count);
        Assert.Single(byCategory["Edit"]);
    }

    [Fact]
    public void MultipleBindingsPerAction_AllResolve()
    {
        var registry = CreateRegistry();
        var binding1 = InputBinding.Keyboard(Key.C, ModifierKeys.Control);
        var binding2 = InputBinding.Keyboard(Key.Insert, ModifierKeys.Control);
        var action = CreateAction("edit.copy", InputScope.Application, binding1, binding2);

        registry.RegisterAction(action);

        var actions1 = registry.ResolveInput(binding1, InputScope.Application);
        var actions2 = registry.ResolveInput(binding2, InputScope.Application);

        Assert.Single(actions1);
        Assert.Single(actions2);
        Assert.Contains("edit.copy", actions1);
        Assert.Contains("edit.copy", actions2);
    }

    [Fact]
    public void MouseBindings_ResolveCorrectly()
    {
        var registry = CreateRegistry();
        var binding = InputBinding.Mouse(TEditMouseButton.Left, ModifierKeys.Control);
        var action = CreateAction("editor.draw.constrain", InputScope.Editor, binding);

        registry.RegisterAction(action);

        var actions = registry.ResolveInput(binding, InputScope.Editor);

        Assert.Single(actions);
        Assert.Contains("editor.draw.constrain", actions);
    }

    [Fact]
    public void WheelBindings_ResolveCorrectly()
    {
        var registry = CreateRegistry();
        var binding = InputBinding.Wheel(MouseWheelDirection.Up);
        var action = CreateAction("view.zoom.in", InputScope.Editor, binding);

        registry.RegisterAction(action);

        var actions = registry.ResolveInput(binding, InputScope.Editor);

        Assert.Single(actions);
        Assert.Contains("view.zoom.in", actions);
    }
}
