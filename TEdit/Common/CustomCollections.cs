// -----------------------------------------------------------------------
// <copyright file="CustomCollections.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace TEdit.Common
{
    public static class ObservableCollectionExtensions
    {
        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public static void AddRange<T>(this ObservableCollection<T> current, IEnumerable<T> collection)
        {
            foreach (var i in collection) current.Add(i);
        }

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). 
        /// </summary> 
        public static void RemoveRange<T>(this ObservableCollection<T> current, IEnumerable<T> collection)
        {
            foreach (var i in collection) current.Remove(i);
            //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary> 
        /// Clears the current collection and replaces it with the specified item. 
        /// </summary> 
        public static void Replace<T>(this ObservableCollection<T> current, T item)
        {
            current.ReplaceRange(new T[] { item });
        }
        /// <summary> 
        /// Clears the current collection and replaces it with the specified collection. 
        /// </summary> 
        public static void ReplaceRange<T>(this ObservableCollection<T> current, IEnumerable<T> collection)
        {
            //List<T> old = new List<T>(current);
            current.Clear();
            foreach (var i in collection) current.Add(i);
            //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public class FilteringCollection<T, M> : AdaptingCollection<T, M>
    {
        public FilteringCollection(Func<Lazy<T, M>, bool> filter)
            : base(e => e.Where(filter))
        {
        }
    }

    public class OrderingCollection<T, M> : AdaptingCollection<T, M>
    {
        public OrderingCollection(Func<Lazy<T, M>, object> keySelector, bool descending = false)
            : base(e => descending ? e.OrderByDescending(keySelector) : e.OrderBy(keySelector))
        {
        }
    }

    public class AdaptingCollection<T> : AdaptingCollection<T, IDictionary<string, object>>
    {
        public AdaptingCollection(Func<IEnumerable<Lazy<T, IDictionary<string, object>>>,
                                      IEnumerable<Lazy<T, IDictionary<string, object>>>> adaptor)
            : base(adaptor)
        {
        }
    }

    public class AdaptingCollection<T, M> : ICollection<Lazy<T, M>>, INotifyCollectionChanged
    {
        private readonly Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>> _adaptor;
        private readonly List<Lazy<T, M>> _allItems = new List<Lazy<T, M>>();
        private List<Lazy<T, M>> _adaptedItems;

        public AdaptingCollection()
            : this(null)
        {
        }

        public AdaptingCollection(Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>> adaptor)
        {
            _adaptor = adaptor;
        }

        private List<Lazy<T, M>> AdaptedItems
        {
            get
            {
                if (_adaptedItems == null)
                {
                    _adaptedItems = Adapt(_allItems).ToList();
                }

                return _adaptedItems;
            }
        }

        #region ICollection Implementation

        // Accessors work directly against adapted collection
        public bool Contains(Lazy<T, M> item)
        {
            return AdaptedItems.Contains(item);
        }

        public void CopyTo(Lazy<T, M>[] array, int arrayIndex)
        {
            AdaptedItems.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return AdaptedItems.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<Lazy<T, M>> GetEnumerator()
        {
            return AdaptedItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Mutation methods work against complete collection
        // and then force a reset of the adapted collection
        public void Add(Lazy<T, M> item)
        {
            _allItems.Add(item);
            ReapplyAdaptor();
        }

        public void Clear()
        {
            _allItems.Clear();
            ReapplyAdaptor();
        }

        public bool Remove(Lazy<T, M> item)
        {
            bool removed = _allItems.Remove(item);
            ReapplyAdaptor();
            return removed;
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        public void ReapplyAdaptor()
        {
            if (_adaptedItems != null)
            {
                _adaptedItems = null;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected virtual IEnumerable<Lazy<T, M>> Adapt(IEnumerable<Lazy<T, M>> collection)
        {
            if (_adaptor != null)
            {
                return _adaptor.Invoke(collection);
            }

            return collection;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;

            if (collectionChanged != null)
            {
                collectionChanged.Invoke(this, e);
            }
        }
    }
}