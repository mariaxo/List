using System;
using System.Collections.Generic;

namespace ListImplementation
{
    public class List<T> : IList<T>, System.Collections.IList, IReadOnlyList<T>
    {
        const int MaxArrayLength = 0X7FEFFFFF;

        private const int _defaultCapacity = 4;
        private T[] _items;
        private static readonly T[] _emptyArray = new T[0];
        private int _size;
        private int _version;

        //question 1
        [NonSerialized]
        private Object _syncRoot;
        //
        public int Count
        {
            get
            {
                //Contract code
                return _size;
            }
        }

        public int Capacity
        {
            get
            {
                //Contract code
                return _items.Length;
            }
            set
            {
                if (value < _size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
                }
                //Contract code
                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (_size > 0)
                        {
                            Array.Copy(_items, 0, newItems, 0, _size);
                        }

                        _items = newItems;
                    }
                    else //when the capacity is < 0 or ==0
                    {
                        _items = _emptyArray;
                    }
                }
            }
        }

        //constructors
        public List()
        {
            _items = _emptyArray;
        }

        public List(int capacity)
        {
            if(capacity < 0 ) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            //Contract code
            if (capacity == 0) _items = _emptyArray;
            else _items = new T[capacity];
        }

        public List(IEnumerable<T> collection)
        {
            if (collection == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            //Contract.EndContractBlock();

            ICollection<T> c = collection as ICollection<T>;
            //if it's a collection, means <as> worked
            if (c != null)
            {
                int count = c.Count;
                if (count == 0)
                {
                    _items = _emptyArray;
                }
                else
                {
                    _items = new T[count];
                    c.CopyTo(_items, 0);
                    _size = count;
                }
            }
            else
            {
                _size = 0;
                _items = _emptyArray;
                // This enumerable could be empty.  Let Add allocate a new array, if needed.
                // Note it will also go to _defaultCapacity first, not 1, then 2, etc. ??

                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Add(en.Current);
                    }
                }
            }
        }

        //methods
        //pt1
        bool System.Collections.IList.IsFixedSize
        {
            get { return false; }
        }
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        bool System.Collections.IList.IsReadOnly
        {
            get { return false; }
        }

        //Question 1 (continueing)
        // Is this List synchronized (thread-safe)?
        bool System.Collections.ICollection.IsSynchronized
        {
            get { return false; }
        }

        //Synchronization root for this object.
        Object System.Collections.ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }
        //


        public void Add(T item)
        {
            if (_size == _items.Length)
            {
                EnsureCapacity(_size + 1);
            }
            _items[_size] = item;
            _size++;
            //modification of the list so detection needed
            _version++;
        }
        public int Add(object item)
        {
            //checking if item is null??
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
            try
            {
                Add((T)item);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
            }
            //this method shall return the index of the newly added item
            return Count - 1;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
                //the list might grow and pass the MaxArrayLength, so a check is needed
                if ((uint)newCapacity > MaxArrayLength) newCapacity = MaxArrayLength;
               
                //when doubling the capacity, still we can get less than the min, so we ensure it's at least the min
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }


        }

        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
                _size = 0;
            }
            //as there is a modification, we need to detect it
            _version++;
        }

    }
}
