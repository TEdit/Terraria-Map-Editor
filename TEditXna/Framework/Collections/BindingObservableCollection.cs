/* 
Copyright (c) 2011 BinaryConstruct
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TEdit.Framework.Collections
{
    public class BindingObservableCollection<T> : ObservableCollection<T>, INotifyPropertyChanged where T : INotifyPropertyChanged
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            Unsubscribe(e.OldItems);
            Subscribe(e.NewItems);
            base.OnCollectionChanged(e);
        }

        protected override void ClearItems()
        {
            foreach (T element in this)
                ((INotifyPropertyChanged)element).PropertyChanged -= ContainedElementChanged;

            base.ClearItems();
        }

        private void Subscribe(IList iList)
        {
            if (iList != null)
            {
                foreach (T element in iList)
                    ((INotifyPropertyChanged)element).PropertyChanged += ContainedElementChanged;
            }
        }

        private void Unsubscribe(IList iList)
        {
            if (iList != null)
            {
                foreach (T element in iList)
                    ((INotifyPropertyChanged)element).PropertyChanged -= ContainedElementChanged;
            }
        }

        private void ContainedElementChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        } 
    }
}