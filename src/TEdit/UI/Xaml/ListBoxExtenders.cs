// http://michlg.wordpress.com/2010/01/16/listbox-automatically-scroll-currentitem-into-view/

using System;
using System.Windows;
using System.Windows.Controls;

namespace TEdit.UI.Xaml;

/// <summary>
/// This class contains a few useful extenders for the ListBox
/// </summary>
public class ListBoxExtenders : DependencyObject
{
    #region Properties

    public static readonly DependencyProperty AutoScrollToCurrentItemProperty = DependencyProperty.RegisterAttached("AutoScrollToCurrentItem", typeof(bool), typeof(ListBoxExtenders), new UIPropertyMetadata(default(bool), OnAutoScrollToCurrentItemChanged));

    /// <summary>
    /// Returns the value of the AutoScrollToCurrentItemProperty
    /// </summary>
    /// <param name="obj">The dependency-object whichs value should be returned</param>
    /// <returns>The value of the given property</returns>
    public static bool GetAutoScrollToCurrentItem(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoScrollToCurrentItemProperty);
    }

    /// <summary>
    /// Sets the value of the AutoScrollToCurrentItemProperty
    /// </summary>
    /// <param name="obj">The dependency-object whichs value should be set</param>
    /// <param name="value">The value which should be assigned to the AutoScrollToCurrentItemProperty</param>
    public static void SetAutoScrollToCurrentItem(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoScrollToCurrentItemProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// This method will be called when the AutoScrollToCurrentItem
    /// property was changed
    /// </summary>
    /// <param name="s">The sender (the ListBox)</param>
    /// <param name="e">Some additional information</param>
    public static void OnAutoScrollToCurrentItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
    {
        var listBox = s as ListBox;
        if (listBox != null)
        {
            var listBoxItems = listBox.Items;
            if (listBoxItems != null)
            {
                var newValue = (bool)e.NewValue;

                var autoScrollToCurrentItemWorker = new EventHandler((s1, e2) => OnAutoScrollToCurrentItem(listBox, listBox.Items.CurrentPosition));

                if (newValue)
                    listBoxItems.CurrentChanged += autoScrollToCurrentItemWorker;
                else
                    listBoxItems.CurrentChanged -= autoScrollToCurrentItemWorker;
            }
        }
    }

    /// <summary>
    /// This method will be called when the ListBox should
    /// be scrolled to the given index
    /// </summary>
    /// <param name="listBox">The ListBox which should be scrolled</param>
    /// <param name="index">The index of the item to which it should be scrolled</param>
    public static void OnAutoScrollToCurrentItem(ListBox listBox, int index)
    {
        if (listBox != null && listBox.Items != null && listBox.Items.Count > index && index >= 0)
            listBox.ScrollIntoView(listBox.Items[index]);
    }

    #endregion
}