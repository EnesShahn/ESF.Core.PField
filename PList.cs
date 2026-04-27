using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace ESF.Core.PField
{
    [Serializable]
    public class PList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
    {
        [SerializeReference] private List<T> _items = new List<T>();

        #region Props
        public int Capacity
        {
            get { return _items.Capacity; }
            set { _items.Capacity = value; }
        }
        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public bool IsSynchronized => ((ICollection)_items).IsSynchronized;

        public object SyncRoot => ((ICollection)_items).SyncRoot;

        public bool IsFixedSize => ((IList)_items).IsFixedSize;

        object IList.this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = (T)value; }
        }

        public T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }
        #endregion

        public void Add(T item) => _items.Add(item);
        public int Add(object value) => ((IList)_items).Add(value);
        public void Insert(int index, T item) => _items.Insert(index, item);
        public void Insert(int index, object value) => ((IList)_items).Insert(index, value);
        public bool Remove(T item) => _items.Remove(item);
        public void Remove(object value) => ((IList)_items).Remove(value);
        public void RemoveAt(int index) => _items.RemoveAt(index);
        public bool Contains(T item) => _items.Contains(item);
        public bool Contains(object value) => ((IList)_items).Contains(value);
        public int IndexOf(T item) => _items.IndexOf(item);
        public int IndexOf(object value) => ((IList)_items).IndexOf(value);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
        public void Clear() => _items.Clear();
    }
}