/* 
Copyright (c) Philippe Da Silva 2011
Source: http://igf.codeplex.com
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using TEdit.Framework.Threading;

namespace TEdit.Framework.Collections
{
    /// <summary>
    /// A thread safe hashtable.
    /// </summary>
    /// <typeparam name="TKey">The type of item to use as keys.</typeparam>
    /// <typeparam name="TData">The type of data stored.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class Hashtable<TKey, TData>
        : IEnumerable<KeyValuePair<TKey, TData>>
    {
        struct Node
        {
            public TKey Key;
            public TData Data;
            public Token Token;
        }

        enum Token
        {
            Empty,
            Used,
            Deleted
        }

        class Enumerator : IEnumerator<KeyValuePair<TKey, TData>>
        {
            int currentIndex = -1;
            Hashtable<TKey, TData> table;

            public Enumerator(Hashtable<TKey, TData> table)
            {
                this.table = table;
            }

            public KeyValuePair<TKey, TData> Current
            {
                get;
                private set;
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                Node node;
                do
                {
                    currentIndex++;
                    if (table.array.Length <= currentIndex)
                        return false;

                    node = table.array[currentIndex];
                } while (node.Token != Token.Used);

                Current = new KeyValuePair<TKey, TData>(node.Key, node.Data);
                return true;
            }

            public void Reset()
            {
                currentIndex = -1;
            }
        }

        volatile Node[] array;
        private OneManyLock writeLock;
        //SpinLock writeLock;

        static readonly Node DeletedNode = new Node() { Key = default(TKey), Data = default(TData), Token = Token.Deleted };

        /// <summary>
        /// Initializes a new instance of the <see cref="Hashtable&lt;Key, Data&gt;"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the table.</param>
        public Hashtable(int initialCapacity)
        {
            if (initialCapacity < 1)
                throw new ArgumentOutOfRangeException("initialCapacity", "cannot be < 1");
            array = new Node[initialCapacity];
            writeLock = new OneManyLock();
        }

        /// <summary>
        /// Adds an item to this hashtable.
        /// </summary>
        /// <param name="key">The key at which to add the item.</param>
        /// <param name="data">The data to add.</param>
        public void Add(TKey key, TData data)
        {
            bool inserted = false;

            try
            {
                writeLock.Enter(true);
                inserted = Insert(array, key, data);

                if (!inserted)
                {
                    Resize();
                    Insert(array, key, data);
                }
            }
            finally
            {
                writeLock.Leave(true);
            }
        }

        private void Resize()
        {
            var newArray = new Node[array.Length * 2];
            for (int i = 0; i < array.Length; i++)
            {
                var item = array[i];
                if (item.Token == Token.Used)
                    Insert(newArray, item.Key, item.Data);
            }

            array = newArray;
        }

        private bool Insert(Node[] table, TKey key, TData data)
        {
            var initialHash = Math.Abs(key.GetHashCode()) % table.Length;
            var hash = initialHash;
            bool inserted = false;
            do
            {
                var node = table[hash];
                // if node is empty, or marked with a tombstone
                if (node.Token == Token.Empty || node.Token == Token.Deleted || key.Equals(node.Key))
                {
                    table[hash] = new Node()
                    {
                        Key = key,
                        Data = data,
                        Token = Token.Used
                    };
                    inserted = true;
                    break;
                }
                else
                    hash = (hash + 1) % table.Length;
            } while (hash != initialHash);

            return inserted;
        }

        /// <summary>
        /// Sets the value of the item at the specified key location.
        /// This is only garanteed to work correctly if no other thread is modifying the same key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value.</param>
        public void UnsafeSet(TKey key, TData value)
        {
            Node[] table;
            do
            {
                table = array;
                var initialHash = Math.Abs(key.GetHashCode()) % table.Length;
                var hash = initialHash;
                bool inserted = false;

                do
                {
                    var node = table[hash];
                    if (key.Equals(node.Key))
                    {
                        table[hash] = new Node()
                        {
                            Key = key,
                            Data = value,
                            Token = Token.Used
                        };
                        inserted = true;
                        break;
                    }
                    else
                        hash = (hash + 1) % table.Length;
                } while (hash != initialHash);

                if (!inserted)
                    Add(key, value);
            } while (table != array);
        }

        private bool Find(TKey key, out Node node)
        {
            node = new Node();
            var table = array;
            var initialHash = Math.Abs(key.GetHashCode()) % table.Length;
            var hash = initialHash;

            do
            {
                Node n = table[hash];
                if (n.Token == Token.Empty)
                    return false;
                if (n.Token == Token.Deleted || !key.Equals(n.Key))
                    hash = (hash + 1) % table.Length;
                else
                {
                    node = n;
                    return true;
                }
            } while (hash != initialHash);

            return false;
        }

        /// <summary>
        /// Tries to get the data at the specified key location.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="data">The data at the key location.</param>
        /// <returns><c>true</c> if the data was found; else <c>false</c>.</returns>
        public bool TryGet(TKey key, out TData data)
        {
            Node n;
            if (Find(key, out n))
            {
                data = n.Data;
                return true;
            }
            else
            {
                data = default(TData);
                return false;
            }
        }

        /// <summary>
        /// Removes the data at the specified key location.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(TKey key)
        {
            Node[] table = array;
            var initialHash = Math.Abs(key.GetHashCode()) % table.Length;
            var hash = initialHash;

            try
            {
                writeLock.Enter(true);
                do
                {
                    Node n = table[hash];
                    if (n.Token == Token.Empty)
                        return;
                    if (n.Token == Token.Deleted || !key.Equals(n.Key))
                        hash = (hash + 1) % table.Length;
                    else
                        table[hash] = DeletedNode;
                } while (table != array);
            }
            finally
            {
                writeLock.Leave(true);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TData>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}