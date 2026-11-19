using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace TEdit.Scripting.Execution;

/// <summary>Tracks property-panel and nested entity edits between coordinated operations.</summary>
public sealed class WorldChangeTracker : IDisposable
{
    private readonly List<Action> _unsubscribe = new();
    private readonly HashSet<object> _seen = new(ReferenceEqualityComparer.Instance);
    private readonly Action _changed;
    private readonly Func<bool> _canChange;

    public WorldChangeTracker(object root, Action changed, Func<bool>? canChange = null)
    {
        _changed = changed;
        _canChange = canChange ?? (() => true);
        Visit(root);
    }

    private void Visit(object? value)
    {
        if (value == null || value is string || value.GetType().IsValueType || !_seen.Add(value)) return;
        if (value is INotifyPropertyChanging changing)
        {
            PropertyChangingEventHandler handler = (_, _) =>
            {
                if (!_canChange()) throw new InvalidOperationException(Properties.Language.scripting_busy);
            };
            changing.PropertyChanging += handler;
            _unsubscribe.Add(() => changing.PropertyChanging -= handler);
        }
        if (value is INotifyPropertyChanged properties)
        {
            PropertyChangedEventHandler handler = (_, args) =>
            {
                _changed();
                // A property panel can replace a nested object and then edit it
                // again before another coordinated operation rebuilds the tracker.
                if (!string.IsNullOrEmpty(args.PropertyName))
                {
                    var property = value.GetType().GetProperty(args.PropertyName);
                    if (property?.CanRead == true && property.GetIndexParameters().Length == 0 &&
                        !property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                        Visit(property.GetValue(value));
                }
            };
            properties.PropertyChanged += handler;
            _unsubscribe.Add(() => properties.PropertyChanged -= handler);
        }
        if (value is INotifyCollectionChanged collection)
        {
            NotifyCollectionChangedEventHandler handler = (_, args) =>
            {
                _changed();
                if (args.NewItems != null) foreach (var item in args.NewItems) Visit(item);
            };
            collection.CollectionChanged += handler;
            _unsubscribe.Add(() => collection.CollectionChanged -= handler);
        }
        if (value is IEnumerable items)
        {
            foreach (var item in items) Visit(item);
            return;
        }
        var ns = value.GetType().Namespace ?? "";
        if (!ns.StartsWith("TEdit.Terraria", StringComparison.Ordinal) && ns != "TEdit.Geometry") return;
        foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0 || property.PropertyType.IsValueType || property.PropertyType == typeof(string)) continue;
            Visit(property.GetValue(value));
        }
    }

    public void Dispose()
    {
        foreach (var unsubscribe in _unsubscribe) unsubscribe();
        _unsubscribe.Clear();
        _seen.Clear();
    }
}
