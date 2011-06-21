// -----------------------------------------------------------------------
// <copyright file="CustomCollections.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TEditWPF.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.Specialized;

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
        private readonly List<Lazy<T, M>> _allItems = new List<Lazy<T, M>>();
        private readonly Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>> _adaptor = null;
        private List<Lazy<T, M>> _adaptedItems = null;

        public AdaptingCollection()
            : this(null)
        {
        }

        public AdaptingCollection(Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>> adaptor)
        {
            this._adaptor = adaptor;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void ReapplyAdaptor()
        {
            if (this._adaptedItems != null)
            {
                this._adaptedItems = null;
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected virtual IEnumerable<Lazy<T, M>> Adapt(IEnumerable<Lazy<T, M>> collection)
        {
            if (this._adaptor != null)
            {
                return this._adaptor.Invoke(collection);
            }

            return collection;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;

            if (collectionChanged != null)
            {
                collectionChanged.Invoke(this, e);
            }
        }

        private List<Lazy<T, M>> AdaptedItems
        {
            get
            {
                if (this._adaptedItems == null)
                {
                    this._adaptedItems = Adapt(this._allItems).ToList();
                }

                return this._adaptedItems;
            }
        }

        #region ICollection Implementation
        // Accessors work directly against adapted collection
        public bool Contains(Lazy<T, M> item)
        {
            return this.AdaptedItems.Contains(item);
        }

        public void CopyTo(Lazy<T, M>[] array, int arrayIndex)
        {
            this.AdaptedItems.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.AdaptedItems.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<Lazy<T, M>> GetEnumerator()
        {
            return this.AdaptedItems.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        // Mutation methods work against complete collection
        // and then force a reset of the adapted collection
        public void Add(Lazy<T, M> item)
        {
            this._allItems.Add(item);
            ReapplyAdaptor();
        }

        public void Clear()
        {
            this._allItems.Clear();
            ReapplyAdaptor();
        }

        public bool Remove(Lazy<T, M> item)
        {
            bool removed = this._allItems.Remove(item);
            ReapplyAdaptor();
            return removed;
        }
        #endregion
    }
}
