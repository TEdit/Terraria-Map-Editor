// Source: http://www.codeproject.com/KB/WPF/EnumItemList.aspx
// License: The Code Project Open License (CPOL) 1.02
// License URL: http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Resources;
using System.Diagnostics;
using System.Drawing;

namespace TEdit.UI.Xaml.Enum
{
    
    /// <summary>
    /// Enumerates the properties for EnumItem.
    /// </summary>
    public enum EnumItemsOrder
    {
        Custom,
        Value,
        DisplayValue,
        Text
    }

    /// <summary>
    /// Represents a list of enumeration values and corresponding values to be shown or used in the user interface.
    /// </summary>
    [Serializable()]
    [Localizability(LocalizationCategory.None,Modifiability=Modifiability.Unmodifiable)]
    public class EnumItemList : ObservableCollection<EnumItem>, IValueConverter
    {

    #region Constructors
        
        /// <summary>
        /// Creates a new empty EnumItemList.
        /// </summary>
        public EnumItemList()
        {
            SortBy = EnumItemsOrder.Text;
        }

        /// <summary>
        /// Creates a new EnumItemList for the given enumeration type.
        /// </summary>
        /// <param name="enumType">Type of enumeration values</param>
        public EnumItemList(Type enumType)
            : this()
        {
            EnumType = enumType;
        }
        
        /// <summary>
        /// Creates a new EnumItemList. 
        /// </summary>
        /// <param name="enumType">Type of enumeration values</param>
        /// <param name="items">Collection to copy enumeration items from.</param>
        public EnumItemList(Type enumType, IEnumerable<EnumItem> items)
            : base(items)
        {
            SortBy = EnumItemsOrder.Text;
            EnumType = enumType;
        }
		 
	#endregion

    #region Private members


        private Type m_enumType;

        private TypeConverter m_converter;

        private Type m_resourceType;


    #endregion       
        
    #region Public methods

        /// <summary>
        /// Converts the Value of an <see cref="EnumItem"/> item to the enumeration type of this EnumItemList.
        /// </summary>
        /// <param name="item"></param>
        public void ConvertValueToEnumType(EnumItem item)
        {
            if (m_enumType != null && item.Value != null && (m_enumType.IsAssignableFrom(item.Value.GetType()) == false))
            {
                item.Value = m_converter.ConvertFrom(null, CultureInfo.InvariantCulture, item.Value);
            }
        }

        /// <summary>
        /// Finds the EnumItem corresponding to the given enumeration value
        /// </summary>
        /// <param name="enumValue">Enumeration value to search for</param>
        /// <returns>The corresponding EnumItem or <c>null</c> if no match was found.</returns>
        public EnumItem FindItemByValue(object enumValue)
        {
            int index = IndexOfValue(enumValue);
            return index >= 0 ? this[index] : null;
        }

        /// <summary>
        /// Finds the index of the given enumeration value
        /// </summary>
        /// <param name="enumValue">The enumeration value to search for.</param>
        /// <returns>The zero-based index of the value or -1 if not found.</returns>
        public int IndexOfValue(object enumValue)
        {
            for (int index = 0; index < Count; index++)
            {
                if (Equals(this[index].Value, enumValue)) return index;
            }
            return -1;
        }

        /// <summary>
        /// Finds an EnumItem with the given DisplayValue.
        /// </summary>
        /// <param name="displayValue">DisplayValue to search for</param>
        /// <returns>The EnumItem with the given DisplayValue or <c>null</c> if value not found
        /// </returns>
        public EnumItem FindItemByDisplayValue(object displayValue)
        {
            for (int index = 0; index < Count; index++)
            {
                EnumItem item = this[index];
                if (Equals(item.Value, displayValue)) return item;
            }
            return null;
        }
        
     #endregion

    #region Public properties

        /// <summary>
        /// Gets or sets the type of enumeration that is handled by this EnumItemList.
        /// </summary>
        /// <remarks>
        /// When setting this property all existing values will be converted to the specified type. If the type
        /// is an <c>enum</c> the list will be populated with all available enum values.
        /// This property can only be set once.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">If value is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If the property has already been set to another value.</exception>
        public Type EnumType
        {
            get { return m_enumType; }
            set
            {
                if (value == null) throw new ArgumentNullException();

                if (m_enumType == value) return;

                if (m_enumType != null) throw new InvalidOperationException("The EnumType value cannot be changed once set.");

                m_enumType = value;

                // Cache TypeConverter to increase performance.
                m_converter = TypeDescriptor.GetConverter(m_enumType);

                // Convert any existing item values to type.
                foreach (EnumItem item in this)
                {
                    ConvertValueToEnumType(item);
                }

                if (DefaultItem != null)
                {
                    ConvertValueToEnumType(DefaultItem);
                }

                //For enum types, fill list with values and their names 
                if (m_enumType.IsEnum)
                {
                    // For all public static fields, i.e. all enum values
                    foreach (FieldInfo field in m_enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        object fieldValue = field.GetValue(null);

                        // Look for DescriptionAttributes.
                        object[] descriptions = field.GetCustomAttributes(typeof(DescriptionAttribute), true);

                        string displayName;
                        if (descriptions.Length > 0)
                        {
                            displayName = ((DescriptionAttribute)descriptions[0]).Description;
                        }
                        else
                        {
                            try
                            {
                                // Use type converter to support enums with custom TypeConverters.
                                displayName = m_converter.ConvertToString(fieldValue);
                            }
                            catch (Exception)
                            {
                                displayName = field.Name;
                            }
                        }
                        EnumItem item = new EnumItem() { Value = fieldValue, DisplayValue = displayName };
                        
                        if (IndexOfValue(item.Value) < 0)   // Do not add item if it already exists.
                        {
                            Add(item);
                        }
                    }
                }
                ReadValuesFromResources();
                OnPropertyChanged(new PropertyChangedEventArgs("EnumType"));
            }
        }

        /// <summary>
        /// Get or sets the type for the typed resource file to load localized enum value representations from.
        /// </summary>
        public Type ResourceType
        {
            get { return m_resourceType; }
            set 
            {
                if( value == m_resourceType ) return;
                m_resourceType = value;
                ReadValuesFromResources();
                OnPropertyChanged(new PropertyChangedEventArgs("ResourceType"));
            }
        }

        private void ReadValuesFromResources()
        {
            if (m_resourceType == null || m_enumType == null) return;
            ResourceManager resourceManager = new ResourceManager(m_resourceType);

            foreach( EnumItem item in this )
            {
                if( item.Value != null )
                {
                    String resourceName = EnumType.Name+"_"+item.Value.ToString();
                    Object itemResource = resourceManager.GetObject(resourceName);
                    if( itemResource != null )
                    {
                        if (itemResource is Bitmap)
                        {
                            item.DisplayValue = ImageSourceHelpers.CreateFromBitmap((Bitmap)itemResource);
                        }
                        else if (itemResource is Icon)
                        {
                            item.DisplayValue = ImageSourceHelpers.CreateFromIcon((Icon)itemResource);
                        }
                        else
                        {
                            item.DisplayValue = itemResource;
                        }
                    }
                    else
                    {
                        Debug.Print("{0}: Failed to find a matching resource '{1}' for enum item '{2}' in resource file '{3}'.", 
                                    GetType().Name, resourceName, item.Value, resourceManager.BaseName);
                    }
                }
            }
            if (m_comparer != null) Sort(m_comparer);
        }

        /// <summary>
        /// Sorts the <see cref="EnumItemList"/> according to a custom comparer.
        /// </summary>
        /// <param name="comparer">Comparer to use</param>
        public void Sort(IComparer<EnumItem> comparer)
        {
            if (comparer == null) throw new ArgumentNullException();
            int newIndex = 0;
            foreach (EnumItem item in Items.OrderBy(k => k, comparer))
            {
                int oldIndex = Items.IndexOf(item);
                if (oldIndex != newIndex)
                {
                    Move(oldIndex, newIndex);
                }
                newIndex++;
            }
        }

        private class EnumItemComparer : IComparer<EnumItem>
        {
            private EnumItemsOrder m_sortBy;

            public EnumItemComparer(EnumItemsOrder sortBy)
            {
                m_sortBy = sortBy;
            }

            #region IComparer<EnumItem> Members

            public int Compare(EnumItem x, EnumItem y)
            {
                switch (m_sortBy)
                {
                    case EnumItemsOrder.Value:
                        return Comparer<Object>.Default.Compare(x.Value, y.Value);
                        
                    case EnumItemsOrder.DisplayValue:
                        return Comparer<Object>.Default.Compare(x.Value, y.Value);
                        
                    case EnumItemsOrder.Text:
                        return StringComparer.CurrentCultureIgnoreCase.Compare(x.Text, y.Text);
                    default:
                        return 0;
                }
            }

            #endregion
        }

        private IComparer<EnumItem> m_comparer;

        /// <summary>
        /// Gets or sets the comparer to use to sort the enum items.
        /// </summary>
        public IComparer<EnumItem> Comparer
        {
            get { return m_comparer; }
            set {
                m_comparer = value; OnPropertyChanged(new PropertyChangedEventArgs("Comparer"));
                if (m_comparer != null)
                {
                    Sort(m_comparer);
                }
            }
        }
        

        private EnumItemsOrder m_sortBy;

        /// <summary>
        /// Gets or sets the sort order to use.
        /// </summary>
        [Localizability(LocalizationCategory.None,Modifiability=Modifiability.Modifiable,Readability=Readability.Readable)]
        public EnumItemsOrder SortBy
        {
            get { return m_sortBy; }
            set {
                if (value == m_sortBy) return;
                Comparer = value == EnumItemsOrder.Custom ? null : new EnumItemComparer(value);
                m_sortBy = value; 
                OnPropertyChanged(new PropertyChangedEventArgs("SortBy"));
            }
        }
                 
        private EnumItem m_defaultItem;

        /// <summary>
        /// Gets or sets the default <see cref="EnumItem"/> that is returned when no other matching item is found
        /// during conversion.
        /// </summary>
        public EnumItem DefaultItem
        {
            get { return m_defaultItem; }
            set
            {
                if (value != null)
                {
                    ConvertValueToEnumType(value);
                }
                m_defaultItem = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DefaultItem")); 
            }
        }
        

     #endregion
        
    #region Protected members

        /// <summary>
        /// Called when a new item is added from XAML or code.
        /// </summary>
        /// <param name="index">Index of new item.</param>
        /// <param name="item">Item to insert</param>
        /// <remarks>
        /// This is overridden to 
        /// 1) Convert value to EnumType
        /// 2) Ensure that there is only one item for each unique value. 
        /// In case there is already an item with the same value that one will be removed. 
        /// 3) Insert the item in correct order when the list is sorted.
        /// </remarks>
        protected override void InsertItem(int index, EnumItem item)
        {
            if (item == null) throw new ArgumentNullException();

            ConvertValueToEnumType(item);

            // search for existing item with same value.
            int oldIndex = IndexOfValue(item.Value);
            // remove old item with same value.
            if (oldIndex >= 0)
            {
                
                RemoveAt(oldIndex);
                if (oldIndex < index)
                {
                    index--;
                }
            }

            if (m_comparer != null)
            {
                index = ((List<EnumItem>)Items).BinarySearch(item, m_comparer);
                if( index < 0 ) index = ~index;
            }
            
            base.InsertItem(index, item);

            
        }

        /// <summary>
        /// Called when an item is set through an indexer.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <remarks>
        /// Overriden to ensure that item is not null, value is convertible to item type and that the item
        /// is inserted in the correct order when the list is sorted.
        /// </remarks>
        protected override void SetItem(int index, EnumItem item)
        {
            RemoveAt(index);
            InsertItem(index, item);
        }

	#endregion
        
    #region IValueConverter Members

        /// <summary>
        /// Converts a enumeration value to the corresponding EnumItem found in this list
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != m_enumType && value != null && m_converter != null)
            {
                value = m_converter.ConvertFrom(null, culture, value);
            }
            EnumItem item = FindItemByValue(value);
            if (item == null)
            {
                if (DefaultItem != null)
                {
                    // Return a clone of the default item with the Value set to passed in value.
                    return new EnumItem() { Value = value, DisplayValue = DefaultItem.DisplayValue, Text = DefaultItem.Text };
                }
                else
                {
                    throw new ArgumentException($"'{value}' was not recognized as a valid enumeration value.");
                }
            }
            return item;
        }



        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumItem item = value as EnumItem;
            if (item != null && Contains(item)) return item.Value;
            item = FindItemByDisplayValue(value);
            if (item != null) return item.Value;
            throw new ArgumentException($"'{value}' was not recognized as a valid enumeration value.");
        }


        #endregion
    }

    /// <summary>
    /// A generic version of <see cref="EnumItemList"/> which provides type-safe methods to add items
    /// programmatically.
    /// </summary>
    /// <typeparam name="TEnum">Type of enumeration to be managed by this list.</typeparam>
    public class EnumItemList<TEnum> : EnumItemList
    {
        /// <summary>
        /// Creates a new typed EnumItemList.
        /// </summary>
        public EnumItemList()
            : base(typeof(TEnum))
        {
        }

        /// <summary>
        /// Creates a new typed EnumItemList with the specified sorting order.
        /// </summary>
        /// <param name="order">Sorting order.</param>
        public EnumItemList(EnumItemsOrder order)
            : base(typeof(TEnum))
        {
            SortBy = order;
        }

        /// <summary>
        /// Creates a new typed EnumItemList with the specified items.
        /// </summary>
        /// <param name="items">Items to use.</param>
        public EnumItemList(IEnumerable<EnumItem> items)
            : base(typeof(TEnum), items)
        {
        }

        /// <summary>
        /// Changes how an enumeration value is represented.
        /// </summary>
        /// <param name="value">Enumeration value to add or change</param>
        /// <param name="displayValue">Display value to be used.</param>
        /// <param name="itemText">Text to be shown.</param>
        public void SetItem(TEnum value, object displayValue, string itemText)
        {
            Add(new EnumItem() { Value = value, DisplayValue = displayValue, Text = itemText });
        }

        /// <summary>
        /// Changes how an enumeration value is represented.
        /// </summary>
        /// <param name="value">Enumeration value to add or change</param>
        /// <param name="displayValue">Display value to be used.</param>
        /// <param name="itemText">Text to be shown.</param>
        public void SetItem(TEnum value, object displayValue)
        {
            Add(new EnumItem() { Value = value, DisplayValue = displayValue });

        }
    }
}
