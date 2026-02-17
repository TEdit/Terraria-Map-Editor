using System.Collections.Generic;

namespace TEdit.Input;

/// <summary>
/// Represents a logical action that can be triggered by one or more input bindings.
/// Actions are categorized and scoped for conflict detection and organization.
/// </summary>
public class InputAction
{
    /// <summary>Unique identifier for the action (e.g., "edit.copy", "tool.draw.freehand").</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable display name.</summary>
    public required string Name { get; init; }

    /// <summary>Category for grouping in settings UI (e.g., "Editing", "Navigation", "Tools").</summary>
    public required string Category { get; init; }

    /// <summary>Where this action is valid - Application or Editor level.</summary>
    public InputScope Scope { get; init; } = InputScope.Application;

    /// <summary>Default bindings shipped with TEdit.</summary>
    public List<InputBinding> DefaultBindings { get; init; } = new();

    /// <summary>Whether this action can have multiple bindings.</summary>
    public bool AllowMultipleBindings { get; init; } = true;

    /// <summary>Optional description shown in settings UI.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// Categories for organizing actions in the settings UI.
/// </summary>
public static class InputCategory
{
    public const string File = "File";
    public const string Editing = "Editing";
    public const string Selection = "Selection";
    public const string Navigation = "Navigation";
    public const string Tools = "Tools";
    public const string ToolDrawing = "Tool Drawing";
    public const string Toggles = "Toggles";
    public const string View = "View";
    public const string Help = "Help";
}
