using System;
using System.Collections.Generic;
using System.Linq;

namespace TEdit.Input;

/// <summary>
/// Central registry for all input actions and their bindings.
/// Manages default bindings, user customizations, and conflict detection.
/// </summary>
public class InputBindingRegistry
{
    private readonly Dictionary<string, InputAction> _actions = new();
    private readonly Dictionary<string, List<InputBinding>> _activeBindings = new();
    private readonly Dictionary<string, List<InputBinding>> _userCustomizations = new();

    // Reverse lookup: binding -> list of action IDs
    private readonly Dictionary<InputBinding, List<string>> _bindingToActions = new();

    /// <summary>All registered actions.</summary>
    public IReadOnlyDictionary<string, InputAction> Actions => _actions;

    /// <summary>Registers an action with its default bindings.</summary>
    public void RegisterAction(InputAction action)
    {
        _actions[action.Id] = action;
        _activeBindings[action.Id] = new List<InputBinding>(action.DefaultBindings);

        // Build reverse lookup
        foreach (var binding in action.DefaultBindings)
        {
            AddToReverseLookup(binding, action.Id);
        }
    }

    /// <summary>Gets the current bindings for an action (user customizations take precedence).</summary>
    public IReadOnlyList<InputBinding> GetBindings(string actionId)
    {
        if (_userCustomizations.TryGetValue(actionId, out var userBindings))
            return userBindings;

        if (_activeBindings.TryGetValue(actionId, out var bindings))
            return bindings;

        return Array.Empty<InputBinding>();
    }

    /// <summary>Gets the action by ID.</summary>
    public InputAction? GetAction(string actionId)
    {
        return _actions.TryGetValue(actionId, out var action) ? action : null;
    }

    /// <summary>Resolves an input binding to matching action IDs, filtered by scope.</summary>
    public List<string> ResolveInput(InputBinding binding, InputScope scope)
    {
        if (!_bindingToActions.TryGetValue(binding, out var actionIds))
            return new List<string>();

        return actionIds
            .Where(id => _actions.TryGetValue(id, out var action) && action.Scope == scope)
            .ToList();
    }

    /// <summary>Resolves an input binding to all matching action IDs (any scope).</summary>
    public List<string> ResolveInput(InputBinding binding)
    {
        if (!_bindingToActions.TryGetValue(binding, out var actionIds))
            return new List<string>();

        return new List<string>(actionIds);
    }

    /// <summary>Sets user customization for an action's bindings.</summary>
    public void SetUserBindings(string actionId, List<InputBinding> bindings)
    {
        // Remove old bindings from reverse lookup
        if (_userCustomizations.TryGetValue(actionId, out var oldBindings))
        {
            foreach (var binding in oldBindings)
                RemoveFromReverseLookup(binding, actionId);
        }
        else if (_activeBindings.TryGetValue(actionId, out var defaultBindings))
        {
            foreach (var binding in defaultBindings)
                RemoveFromReverseLookup(binding, actionId);
        }

        // Set new user bindings
        _userCustomizations[actionId] = new List<InputBinding>(bindings);

        // Add new bindings to reverse lookup
        foreach (var binding in bindings)
        {
            AddToReverseLookup(binding, actionId);
        }
    }

    /// <summary>Clears user customization for an action, restoring defaults.</summary>
    public void ResetToDefaults(string actionId)
    {
        if (!_userCustomizations.TryGetValue(actionId, out var userBindings))
            return;

        // Remove user bindings from reverse lookup
        foreach (var binding in userBindings)
            RemoveFromReverseLookup(binding, actionId);

        _userCustomizations.Remove(actionId);

        // Restore default bindings to reverse lookup
        if (_actions.TryGetValue(actionId, out var action))
        {
            foreach (var binding in action.DefaultBindings)
                AddToReverseLookup(binding, actionId);
        }
    }

    /// <summary>Resets all actions to their default bindings.</summary>
    public void ResetAllToDefaults()
    {
        foreach (var actionId in _userCustomizations.Keys.ToList())
        {
            ResetToDefaults(actionId);
        }
    }

    /// <summary>Gets all user customizations for persistence.</summary>
    public Dictionary<string, List<InputBinding>> GetUserCustomizations()
    {
        return new Dictionary<string, List<InputBinding>>(_userCustomizations);
    }

    /// <summary>Loads user customizations (typically on startup).</summary>
    public void LoadUserCustomizations(Dictionary<string, List<InputBinding>> customizations)
    {
        foreach (var (actionId, bindings) in customizations)
        {
            if (_actions.ContainsKey(actionId))
            {
                SetUserBindings(actionId, bindings);
            }
        }
    }

    /// <summary>Detects binding conflicts within the same scope.</summary>
    public List<BindingConflict> DetectConflicts()
    {
        var conflicts = new List<BindingConflict>();

        foreach (var (binding, actionIds) in _bindingToActions)
        {
            if (actionIds.Count <= 1) continue;

            // Group by scope and find conflicts within same scope
            var byScope = actionIds
                .Where(id => _actions.ContainsKey(id))
                .GroupBy(id => _actions[id].Scope);

            foreach (var group in byScope)
            {
                var ids = group.ToList();
                if (ids.Count <= 1) continue;

                for (int i = 0; i < ids.Count; i++)
                {
                    for (int j = i + 1; j < ids.Count; j++)
                    {
                        conflicts.Add(new BindingConflict
                        {
                            Binding = binding,
                            ActionId1 = ids[i],
                            ActionId2 = ids[j],
                            Scope = group.Key
                        });
                    }
                }
            }
        }

        return conflicts;
    }

    /// <summary>Checks if an action has been customized by the user.</summary>
    public bool IsCustomized(string actionId)
    {
        return _userCustomizations.ContainsKey(actionId);
    }

    /// <summary>Gets all actions grouped by category.</summary>
    public Dictionary<string, List<InputAction>> GetActionsByCategory()
    {
        return _actions.Values
            .GroupBy(a => a.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private void AddToReverseLookup(InputBinding binding, string actionId)
    {
        if (!binding.IsValid) return;

        if (!_bindingToActions.TryGetValue(binding, out var list))
        {
            list = new List<string>();
            _bindingToActions[binding] = list;
        }

        if (!list.Contains(actionId))
            list.Add(actionId);
    }

    private void RemoveFromReverseLookup(InputBinding binding, string actionId)
    {
        if (!binding.IsValid) return;

        if (_bindingToActions.TryGetValue(binding, out var list))
        {
            list.Remove(actionId);
            if (list.Count == 0)
                _bindingToActions.Remove(binding);
        }
    }
}

/// <summary>
/// Represents a binding conflict between two actions in the same scope.
/// </summary>
public class BindingConflict
{
    public InputBinding Binding { get; set; }
    public required string ActionId1 { get; set; }
    public required string ActionId2 { get; set; }
    public InputScope Scope { get; set; }

    public override string ToString() =>
        $"{Binding} is bound to both '{ActionId1}' and '{ActionId2}' in {Scope} scope";
}
