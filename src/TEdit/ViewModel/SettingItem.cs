using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TEdit.ViewModel;

public enum SettingEditorType
{
    CheckBox,
    Slider,
    ComboBox,
    Path
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

    public event PropertyChangedEventHandler PropertyChanged;

    public void RaiseValueChanged() =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
}

public class SettingEditorTemplateSelector : DataTemplateSelector
{
    public DataTemplate CheckBoxTemplate { get; set; }
    public DataTemplate SliderTemplate { get; set; }
    public DataTemplate ComboBoxTemplate { get; set; }
    public DataTemplate PathTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not SettingItem setting) return base.SelectTemplate(item, container);

        return setting.EditorType switch
        {
            SettingEditorType.CheckBox => CheckBoxTemplate,
            SettingEditorType.Slider => SliderTemplate,
            SettingEditorType.ComboBox => ComboBoxTemplate,
            SettingEditorType.Path => PathTemplate,
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
