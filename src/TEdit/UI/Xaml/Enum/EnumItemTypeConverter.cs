// Source: http://www.codeproject.com/KB/WPF/EnumItemList.aspx
// License: The Code Project Open License (CPOL) 1.02
// License URL: http://www.codeproject.com/info/cpol10.aspx

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using System.Drawing;

namespace TEdit.UI.Xaml.Enum
{
    internal class EnumItemTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            // Support conversion from any type for use in DefaultItem.
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // Support conversion from any type for use in DefaultItem.
            return value is EnumItem ? value : new EnumItem() { DisplayValue = value };
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(UIElement) || destinationType == typeof(String);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            EnumItem item = value as EnumItem;
            if (item != null)
            {
                if (destinationType == typeof(String))
                {
                    return item.ToString();
                }
                if (destinationType == typeof(UIElement))
                {
                    object displayValue = item.DisplayValue;
                    if (displayValue == null || displayValue is String)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = item.ToString();
                        return textBlock;
                    }
                    else if (displayValue is UIElement)
                    {
                        if (VisualTreeHelper.GetParent((UIElement)displayValue) != null)
                        {
                            // Clone UIElement to allow it to be used several times.
                            string str = XamlWriter.Save(displayValue);
                            StringReader sr = new StringReader(str);
                            XmlReader xr = XmlReader.Create(sr);
                            UIElement ret = (UIElement)XamlReader.Load(xr);
                            return ret;
                        }
                        else
                        {
                            return displayValue;
                        }
                    }
                    if (displayValue is DataTemplate)
                    {
                        ContentPresenter presenter = new ContentPresenter();
                        presenter.Content = item;
                        presenter.ContentTemplate = (DataTemplate)displayValue;
                        return presenter;
                    }
                    else if (displayValue is Style)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Style = (Style)displayValue;
                        textBlock.Text = item.ToString();
                        return textBlock;
                    }
                    else if (displayValue is ImageSource)
                    {
                        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                        image.Source = (ImageSource)displayValue;
                        return image;
                    }
                    else if (displayValue is Drawing)
                    {
                        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                        image.Source = new DrawingImage((Drawing)displayValue);
                        return image;
                    }
                    else if (displayValue is System.Windows.Media.Brush)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Background = (System.Windows.Media.Brush)displayValue;
                        textBlock.Text = item.ToString();
                        return textBlock;
                    }
                    else if (displayValue is Bitmap)
                    {
                        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                        image.Source = ImageSourceHelpers.CreateFromBitmap((Bitmap)displayValue);
                        return image;
                    }
                    else if (displayValue is Icon)
                    {
                        Icon icon = (Icon)displayValue;
                        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                        image.Source = ImageSourceHelpers.CreateFromIcon(icon);
                        return image;
                    }
                    else
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(displayValue);
                        if (converter.CanConvertTo(typeof(UIElement)))
                        {
                            UIElement element = converter.ConvertTo(displayValue, typeof(UIElement)) as UIElement;
                            if (element != null) return element;
                        }
                        String text;
                        if (converter.CanConvertTo(typeof(string)))
                        {
                            text = converter.ConvertToString(context, culture, displayValue);
                        }
                        else
                        {
                            text = displayValue.ToString();
                        }
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = text;
                        return textBlock;
                    }

                }
                throw new InvalidOperationException("Unable to convert item value to destination type.");
            }
            return null;
        }


    }
}
