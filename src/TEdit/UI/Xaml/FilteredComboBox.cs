using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TEdit.UI.Xaml
{
    public class FilteredComboBox : ComboBox
    {
        public FilteredComboBox()
        {
            SetResourceReference(StyleProperty, typeof(ComboBox));
        }

        private string oldFilter = string.Empty;

        private string currentFilter = string.Empty;

        protected TextBox EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;


        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != null)
            {
                var view = CollectionViewSource.GetDefaultView(newValue);
                view.Filter += FilterItem;
            }

            if (oldValue != null)
            {
                var view = CollectionViewSource.GetDefaultView(oldValue);
                if (view != null) view.Filter -= FilterItem;
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                case Key.Enter:
                    IsDropDownOpen = false;
                    break;
                case Key.Escape:
                    IsDropDownOpen = false;
                    SelectedIndex = -1;
                    Text = currentFilter;
                    break;
                default:
                    if (e.Key == Key.Down) IsDropDownOpen = true;

                    base.OnPreviewKeyDown(e);
                    break;
            }

            // Cache text
            oldFilter = Text;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    break;
                case Key.Tab:
                case Key.Enter:

                    ClearFilter();
                    break;
                default:
                    if (Text != oldFilter)
                    {
                        var temp = Text;
                        RefreshFilter(); //RefreshFilter will change Text property
                        Text = temp;

                        if (SelectedIndex != -1 && Text != Items[SelectedIndex].ToString())
                        {
                            SelectedIndex = -1; //Clear selection. This line will also clear Text property
                            Text = temp;
                        }


                        IsDropDownOpen = true;

                        EditableTextBox.SelectionStart = int.MaxValue;
                    }

                    //automatically select the item when the input text matches it
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Text == Items[i].ToString())
                            SelectedIndex = i;
                    }

                    base.OnKeyUp(e);
                    currentFilter = Text;
                    break;
            }
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            ClearFilter();
            var temp = SelectedIndex;
            SelectedIndex = -1;
            Text = string.Empty;
            SelectedIndex = temp;
            base.OnPreviewLostKeyboardFocus(e);
        }

        private void RefreshFilter()
        {
            if (ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(ItemsSource);
            view.Refresh();
        }

        private void ClearFilter()
        {
            currentFilter = string.Empty;
            RefreshFilter();
        }

        private bool FilterItem(object value)
        {
            if (value == null) return false;
            if (Text.Length == 0) return true;

            return value.ToString().ToLower().Contains(Text.ToLower());
        }
    }
}
