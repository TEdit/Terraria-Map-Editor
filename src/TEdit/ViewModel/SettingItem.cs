using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TEdit.Input;

namespace TEdit.ViewModel;

public enum SettingEditorType
{
    CheckBox,
    Slider,
    ComboBox,
    Path,
    Keybinding
}

public class SettingItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public SettingEditorType EditorType { get; set; }

    public Func<object> Getter { get; set; }
    public Action<object> Setter { get; set; }

    public object Value
    {
        get => Getter?.Invoke();
        set
        {
            Setter?.Invoke(value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    // Slider-specific
    public double SliderMin { get; set; }
    public double SliderMax { get; set; }
    public double SliderStep { get; set; } = 1;

    // ComboBox-specific
    public IEnumerable ComboBoxItems { get; set; }

    // Path-specific
    public string Placeholder { get; set; }

    // Keybinding-specific
    public string ActionId { get; set; }
    public ObservableCollection<InputBinding> Bindings { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public void RaiseValueChanged() =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
}

public class SettingEditorTemplateSelector : DataTemplateSelector
{
    public DataTemplate SettingCheckBoxTemplate { get; set; }
    public DataTemplate SettingSliderTemplate { get; set; }
    public DataTemplate SettingComboBoxTemplate { get; set; }
    public DataTemplate SettingPathTemplate { get; set; }
    public DataTemplate SettingKeybindingTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not SettingItem setting) return base.SelectTemplate(item, container);

        return setting.EditorType switch
        {
            SettingEditorType.CheckBox => SettingCheckBoxTemplate,
            SettingEditorType.Slider => SettingSliderTemplate,
            SettingEditorType.ComboBox => SettingComboBoxTemplate,
            SettingEditorType.Path => SettingPathTemplate,
            SettingEditorType.Keybinding => SettingKeybindingTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}

public class StringNotEmptyConverter : IValueConverter
{
    public static readonly StringNotEmptyConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class PinIconConverter : IValueConverter
{
    public static readonly PinIconConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? "\u2605" : "\u2606"; // ★ vs ☆

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
