using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TEdit.View.Sidebar.Controls;

public enum ItemsSourceType
{
    ItemPropertyList,
    DictionaryKvp
}

public partial class ItemEditorControl : UserControl
{
    public ItemEditorControl()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty ItemIdProperty =
        DependencyProperty.Register(nameof(ItemId), typeof(int), typeof(ItemEditorControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public int ItemId
    {
        get => (int)GetValue(ItemIdProperty);
        set => SetValue(ItemIdProperty, value);
    }

    public static readonly DependencyProperty StackSizeProperty =
        DependencyProperty.Register(nameof(StackSize), typeof(int), typeof(ItemEditorControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public int StackSize
    {
        get => (int)GetValue(StackSizeProperty);
        set => SetValue(StackSizeProperty, value);
    }

    public static readonly DependencyProperty PrefixProperty =
        DependencyProperty.Register(nameof(Prefix), typeof(byte), typeof(ItemEditorControl),
            new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public byte Prefix
    {
        get => (byte)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public static readonly DependencyProperty ShowStackSizeProperty =
        DependencyProperty.Register(nameof(ShowStackSize), typeof(bool), typeof(ItemEditorControl),
            new PropertyMetadata(true));

    public bool ShowStackSize
    {
        get => (bool)GetValue(ShowStackSizeProperty);
        set => SetValue(ShowStackSizeProperty, value);
    }

    public static readonly DependencyProperty ShowPrefixProperty =
        DependencyProperty.Register(nameof(ShowPrefix), typeof(bool), typeof(ItemEditorControl),
            new PropertyMetadata(true));

    public bool ShowPrefix
    {
        get => (bool)GetValue(ShowPrefixProperty);
        set => SetValue(ShowPrefixProperty, value);
    }

    public static readonly DependencyProperty ShowActionButtonsProperty =
        DependencyProperty.Register(nameof(ShowActionButtons), typeof(bool), typeof(ItemEditorControl),
            new PropertyMetadata(true));

    public bool ShowActionButtons
    {
        get => (bool)GetValue(ShowActionButtonsProperty);
        set => SetValue(ShowActionButtonsProperty, value);
    }

    public static readonly DependencyProperty ShowPreviewImageProperty =
        DependencyProperty.Register(nameof(ShowPreviewImage), typeof(bool), typeof(ItemEditorControl),
            new PropertyMetadata(true));

    public bool ShowPreviewImage
    {
        get => (bool)GetValue(ShowPreviewImageProperty);
        set => SetValue(ShowPreviewImageProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemEditorControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceTypeProperty =
        DependencyProperty.Register(nameof(ItemsSourceType), typeof(ItemsSourceType), typeof(ItemEditorControl),
            new PropertyMetadata(ItemsSourceType.ItemPropertyList, OnItemsSourceTypeChanged));

    public ItemsSourceType ItemsSourceType
    {
        get => (ItemsSourceType)GetValue(ItemsSourceTypeProperty);
        set => SetValue(ItemsSourceTypeProperty, value);
    }

    public static readonly DependencyProperty PrefixSourceProperty =
        DependencyProperty.Register(nameof(PrefixSource), typeof(IEnumerable), typeof(ItemEditorControl),
            new PropertyMetadata(null, OnPrefixSourceChanged));

    public IEnumerable PrefixSource
    {
        get => (IEnumerable)GetValue(PrefixSourceProperty);
        set => SetValue(PrefixSourceProperty, value);
    }

    public static readonly DependencyProperty CopyCommandProperty =
        DependencyProperty.Register(nameof(CopyCommand), typeof(ICommand), typeof(ItemEditorControl));

    public ICommand CopyCommand
    {
        get => (ICommand)GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    public static readonly DependencyProperty PasteCommandProperty =
        DependencyProperty.Register(nameof(PasteCommand), typeof(ICommand), typeof(ItemEditorControl));

    public ICommand PasteCommand
    {
        get => (ICommand)GetValue(PasteCommandProperty);
        set => SetValue(PasteCommandProperty, value);
    }

    public static readonly DependencyProperty MaxStackCommandProperty =
        DependencyProperty.Register(nameof(MaxStackCommand), typeof(ICommand), typeof(ItemEditorControl));

    public ICommand MaxStackCommand
    {
        get => (ICommand)GetValue(MaxStackCommandProperty);
        set => SetValue(MaxStackCommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ItemEditorControl));

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty PasteCommandParameterProperty =
        DependencyProperty.Register(nameof(PasteCommandParameter), typeof(object), typeof(ItemEditorControl));

    public object PasteCommandParameter
    {
        get => GetValue(PasteCommandParameterProperty);
        set => SetValue(PasteCommandParameterProperty, value);
    }

    public static readonly DependencyProperty MaxStackCommandParameterProperty =
        DependencyProperty.Register(nameof(MaxStackCommandParameter), typeof(object), typeof(ItemEditorControl));

    public object MaxStackCommandParameter
    {
        get => GetValue(MaxStackCommandParameterProperty);
        set => SetValue(MaxStackCommandParameterProperty, value);
    }

    #endregion

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemEditorControl control)
            control.ConfigureItemCombo();
    }

    private static void OnItemsSourceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemEditorControl control)
            control.ConfigureItemCombo();
    }

    private static void OnPrefixSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemEditorControl control)
            control.PrefixCombo.ItemsSource = e.NewValue as IEnumerable;
    }

    private void ConfigureItemCombo()
    {
        var combo = ItemCombo;
        if (combo == null) return;

        combo.ItemsSource = ItemsSource;

        // Clear existing binding first
        BindingOperations.ClearBinding(combo, ComboBox.SelectedValueProperty);

        if (ItemsSourceType == ItemsSourceType.DictionaryKvp)
        {
            combo.SelectedValuePath = "Key";
            combo.ItemTemplate = TryFindResource("RarityDictItemWithPreviewTemplate") as DataTemplate
                              ?? TryFindResource("RarityDictItemTemplate") as DataTemplate;
            combo.ItemContainerStyle = TryFindResource("RarityDictItemContainerStyle") as Style;
        }
        else
        {
            combo.SelectedValuePath = "Id";
            combo.ItemTemplate = TryFindResource("RarityItemWithPreviewTemplate") as DataTemplate
                              ?? TryFindResource("RarityItemTemplate") as DataTemplate;
            combo.ItemContainerStyle = TryFindResource("RarityItemContainerStyle") as Style;
        }

        // Bind SelectedValue to ItemId
        var binding = new Binding(nameof(ItemId))
        {
            Source = this,
            Mode = BindingMode.TwoWay
        };
        combo.SetBinding(ComboBox.SelectedValueProperty, binding);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        ConfigureItemCombo();
    }
}
