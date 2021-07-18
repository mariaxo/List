using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace ListImplementation
{
    public class List<T> : IList<T>, System.Collections.IList, IReadOnlyList<T>
    {
        private const int MaxArrayLength = 0X7FEFFFFF;
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
                //Contract.Ensures(Contract.Result<int>() >= 0);
                return _size;
            }
        }

        public int Capacity
        {
            get
            {
                //Contract.Ensures(Contract.Result<int>() >= 0);
                return _items.Length;
            }
            set
            {
                if (value < _size)
                {
                    throw new ArgumentOutOfRangeException("value has small capacity");
                    //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
                }
                //Contract.EndContractBlock();

                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (_size > 0) //meaning if it isnt an _emptyArray
                        {
                            Array.Copy(_items, 0, newItems, 0, _size);
                        }

                        _items = newItems;
                    }
                    else //when the capacity is being assigned to 0
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
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");
            //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            //Contract.EndContractBlock();
            if (capacity == 0) _items = _emptyArray;
            else _items = new T[capacity];
        }

        public List(IEnumerable<T> collection)
        {
            if (collection == null)
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
                throw new ArgumentNullException("collection");
            //Contract.EndContractBlock();

            ICollection<T> c = collection as ICollection<T>;
            //if it's implementing the ICollection interface...
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
            else //not a collection so we need to enumerate on it to collect its items
            {
                _size = 0;
                _items = _emptyArray;
                // This enumerable could be empty.  Let Add allocate a new array, if needed.
                // Note it will also go to _defaultCapacity first, not 1, then 2, etc. how ??

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

        //indexers
        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_size)
                {
                    //ThrowHelper.ThrowArgumentOutOfRangeException();
                    throw new ArgumentOutOfRangeException();
                }
                //Contract.EndContractBlock();
                return _items[index];
            }
            set
            {
                if ((uint)index >= (uint)_size)
                {
                    //ThrowHelper.ThrowArgumentOutOfRangeException();
                    throw new ArgumentOutOfRangeException();
                }
                //Contract.EndContractBlock();
                _items[index] = value;
                //as there is a modification
                _version++;
            }
        }

        Object System.Collections.IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                //ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException)
                {
                    //ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
                }
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return ((value is T) || (value == null && default(T) == null));
        }

        //adds at the end of the list
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
        int System.Collections.IList.Add(object item)
        {
            //checking if item is null??
            //ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
            try
            {
                Add((T)item);
            }
            catch (InvalidCastException)
            {
                //ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
            }
            //this method shall return the index of the newly added item
            return Count - 1;
        }

        //chaqnges the capacity to whatever minimum size is needed
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

        //adds the elements of a collection to the end of this list
        //the capacity is changed to whichever is larger: the new collection's size or this list's size*2
        public void AddRange(IEnumerable<T> collection)
        {
            Contract.Ensures(Count >= Contract.OldValue(Count));
            //insert from index _size (after this list) the new collection
            InsertRange(_size, collection);

        }

        //insert a collection from a given index
        //if you want to add the new collection after the intial list, set the index to the list's size
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            }

            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException("index is out of range");
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            }
            Contract.EndContractBlock();

            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                int count = c.Count;
                if (count > 0) //means if there are items to add to our list, we need to ensure that we have space for them
                {
                    EnsureCapacity(_size + count);
                    if (index < _size)
                    {
                        //moving items to make place for the new collection 
                        Array.Copy(_items, index, _items, index + count, _size - index);
                    }
                    //if index == _size, no need to move anything, will add at the end of the current list

                    //if we are inserting a List into itself
                    if (this == c)
                    {
                        Array.Copy(_items, 0, _items, index, index);
                        Array.Copy(_items, index + count, _items, index * 2, _size - index);
                    }
                    else
                    {
                        T[] itemsToInsert = new T[count];
                        c.CopyTo(itemsToInsert, 0);
                        itemsToInsert.CopyTo(_items, index);
                    }
                    _size += count;
                }
            }
            else
            {
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Insert(index++, en.Current);
                    }

                }
            }
            //inserted new items - > change
            _version++;
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException("index out of range");
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
            }
            Contract.EndContractBlock();
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            if (index < _size)
            {
                Array.Copy(_items, index, _items, index + 1, _size - index);
            }
            _items[index] = item;
            _size++;
            //change 
            _version++;
        }

        void System.Collections.IList.Insert(int index, Object item)
        {
            //ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);

            try
            {
                Insert(index, (T)item);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("exception caught");
                // ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<T>>() != null);
            return new ReadOnlyCollection<T>(this);
        }

        //search in list's this area: area is starting from the index , "count" number of elements
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException();
            // ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (count < 0)
                throw new ArgumentOutOfRangeException();
            //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (_size - index < count)
                throw new ArgumentException();
            // ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            Contract.Ensures(Contract.Result<int>() <= index + count);
            Contract.EndContractBlock();

            return Array.BinarySearch(_items, index, count, item, comparer);
        }

        public int BinarySearch(T item)
        {
            //a postcondition contract, so the return value shall be <=Count
            Contract.Ensures(Contract.Result<int>() <= Count);
            return BinarySearch(0, Count, item, null);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            Contract.Ensures(Contract.Result<int>() <= Count);
            return BinarySearch(0, Count, item, comparer);
        }

        public void Clear()
        {
            if (_size > 0)
            {
                //we clear the elements so that the gc can reclaim the references.
                Array.Clear(_items, 0, _size);
                _size = 0;
            }
            //as there is a modification, we need to detect it
            _version++;
        }

        // Contains returns true if the specified element is in the List.
        // It does a linear, O(n) search. 
        //Equality is determined by calling item.Equals().
        //
        public bool Contains(T item)
        {
            if ((Object)item == null)
            {
                for (int i = 0; i < _size; i++)
                    if ((Object)_items[i] == null)
                        return true;
                return false;
            }
            else
            {
                EqualityComparer<T> c = EqualityComparer<T>.Default;
                for (int i = 0; i < _size; i++)
                {
                    if (c.Equals(_items[i], item)) return true;
                }
                return false;
            }
        }

        bool System.Collections.IList.Contains(Object item)
        {
            if (IsCompatibleObject(item))
            {
                return Contains((T)item);
            }
            return false;
        }

        //this method gets a method as an argument (a delegate type) 
        // ConvertAll uses that method inside to convert the list items from T type to TOutput type
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter is null");
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
            }
            Contract.EndContractBlock();

            List<TOutput> list = new List<TOutput>(_size);
            for (int i = 0; i < _size; i++)
            {
                list._items[i] = converter(_items[i]);
            }
            list._size = _size;
            return list;

        }

        //copies the list inside a new []T array
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            //the rest of the error checking is done in Array.Copy
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }
        void System.Collections.ICollection.CopyTo(Array array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1))
            {
                throw new ArgumentException("multidimentinal arrays not supported and array is not null");
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            }
            Contract.EndContractBlock();

            try
            {
                // Array.Copy will check for NULL.
                Array.Copy(_items, 0, array, arrayIndex, _size);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException("invalid array type");
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
            }
        }

        //copies a section of this list : starting from index, "count" items
        //to a new given array at the specified index

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if (_size - index < count)
            {
                throw new ArgumentException();
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            Contract.EndContractBlock();
            Array.Copy(_items, index, array, arrayIndex, count);
        }

        //FINDING methods
        //finds the index of the searching item, which corresponds to some predicate
        
        public bool Exists(Predicate<T> match)
        {
            return FindIndex(match) != -1;
        }

        public int FindIndex(Predicate<T> match)
        {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            return FindIndex(0, _size, match);
        }

        //startIndex is checked too, to find
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < startIndex + Count);
            return FindIndex(startIndex, _size - startIndex, match);
        }

        //find index which matches the criteria defined in the Predicate method
        //in our list start searching from startIndex and check "count" items
        //whatever criteria we must match to ensure we found the desired item is declared in the match() method
        //if not found, returns -1 
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            //what if it's equal to size? still wrong but it said that this trick eliminates one check :))
            if ((uint)startIndex > (uint)_size)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            }

            if (count < 0 || startIndex > _size - count)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            }

            if (match == null)
            {
                throw new ArgumentNullException();
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < startIndex + count);
            Contract.EndContractBlock();

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(_items[i])) return i;
            }
            return -1;
        }


        //finds a specific item and returns it
        //if not found, returns the default of the type
        public T Find(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            for (int i = 0; i < _size; i++)
            {
                if (match(_items[i]))
                {
                    return _items[i];
                }
            }
            return default(T);
        }

        //find all the items matching the criteria of the predicate
        //and returns a list containing those items
        public List<T> FindAll(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            List<T> list = new List<T>();
            for (int i = 0; i < _size; i++)
            {
                if (match(_items[i]))
                {
                    list.Add(_items[i]);
                }
            }
            return list;
        }

        //if not found, returns the default of the type
        public T FindLast(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            for (int i = _size - 1; i >= 0; i--)
            {
                if (match(_items[i]))
                {
                    return _items[i];
                }
            }
            return default(T);
        }

        //start the search from the end, find the last accuring item who matches the predicate
        public int FindLastIndex(Predicate<T> match)
        {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            return FindLastIndex(_size - 1, _size, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() <= startIndex);
            return FindLastIndex(startIndex, startIndex + 1, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
                //  ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() <= startIndex);
            Contract.EndContractBlock();

            if (_size == 0)
            {
                // Special case for 0 length List
                if (startIndex != -1)
                {
                    throw new ArgumentOutOfRangeException();
                    //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
                }
            }
            else
            {
                // Make sure we're not out of range            
                if ((uint)startIndex >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException();
                    //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
                }
            }

            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if (count < 0 || startIndex - count + 1 < 0)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            }

            int endIndex = startIndex - count;
            for (int i = startIndex; i > endIndex; i--)
            {
                if (match(_items[i]))
                {
                    return i;
                }
            }
            return -1;

        }

        //public void ForEach(Action<T> action) 
        //this was using BinaryCompatibility so I was not sure if this was needed

        //making this class enumerable
        //when enumerated, we need to make sure that no modifications are being done
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        //

        [Serializable]
        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private List<T> list;
            private int index;
            private int version;
            private T current;

            internal Enumerator(List<T> list)
            {
                this.list = list;
                index = 0;
                version = list._version;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public T Current
            {
                get
                {
                    return current;
                }
            }

            Object System.Collections.IEnumerator.Current
            {
                get
                {
                    //when index is 0, means we never moved once, we need to use MoveNext to be able to get the current
                    if (index == 0 || index == list._size + 1)
                    {
                        throw new InvalidOperationException();
                        //  ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return Current;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (version != list._version)
                {
                    throw new InvalidOperationException();
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }

                index = 0;
                current = default(T);
            }

            //we take whatever is under the current index, but we move the index once
            public bool MoveNext()
            {
                List<T> localList = list;
                if (version == localList._version && ((uint)index < (uint)localList._size))
                {
                    current = localList._items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            //when there is no place to go next
            //does necerssary things before returning false 
            private bool MoveNextRare()
            {
                if (version != list._version)
                {
                    throw new InvalidOperationException();
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                index = list._size + 1;
                current = default(T);
                return false;
            }
        }


        //synchronized and thread-safe part
        internal static IList<T> Synchronized(List<T> list)
        {
            return new SynchronizedList(list);
        }

        [Serializable()]
        internal class SynchronizedList : IList<T>
        {
            private List<T> _list;
            private Object _root;

            internal SynchronizedList(List<T> list)
            {
                _list = list;
                _root = ((System.Collections.ICollection)list).SyncRoot;
            }

            public int Count
            {
                get
                {
                    lock (_root)
                    {
                        return _list.Count;
                    }
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return ((ICollection<T>)_list).IsReadOnly;
                }
            }

            public void Add(T item)
            {
                lock (_root)
                {
                    _list.Add(item);
                }
            }

            public void Clear()
            {
                lock (_root)
                {
                    _list.Clear();
                }
            }

            public bool Contains(T item)
            {
                lock (_root)
                {
                    return _list.Contains(item);
                }
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                lock (_root)
                {
                    _list.CopyTo(array, arrayIndex);
                }
            }

            public bool Remove(T item)
            {
                lock (_root)
                {
                    return _list.Remove(item);
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                lock (_root)
                {
                    return _list.GetEnumerator();
                }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                lock (_root)
                {
                    return ((IEnumerable<T>)_list).GetEnumerator();
                }
            }

            public T this[int index]
            {
                get
                {
                    lock (_root)
                    {
                        return _list[index];
                    }
                }
                set
                {
                    lock (_root)
                    {
                        _list[index] = value;
                    }
                }
            }

            public int IndexOf(T item)
            {
                lock (_root)
                {
                    return _list.IndexOf(item);
                }
            }

            public void Insert(int index, T item)
            {
                lock (_root)
                {
                    _list.Insert(index, item);
                }
            }

            public void RemoveAt(int index)
            {
                lock (_root)
                {
                    _list.RemoveAt(index);
                }
            }
        }
        //


        //
        //get a part of the list in a new list
        public List<T> GetRange(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (_size - index < count)
            {
                throw new ArgumentException();
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            Contract.Ensures(Contract.Result<List<T>>() != null);
            Contract.EndContractBlock();

            List<T> list = new List<T>(count);
            Array.Copy(_items, index, list._items, 0, count);
            list._size = count;
            return list;
        }
        
        
         //finds the index of the item, the firsr occurance
        //forward searches the Item in the list
        public int IndexOf(T item)
        {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            return Array.IndexOf(_items, item, 0, _size);

        }
        int System.Collections.IList.IndexOf(Object item)
        {
            if (IsCompatibleObject(item))
            {
                return IndexOf((T)item);
            }
            return -1;
        }

        public int IndexOf(T item, int index)
        {
            if (index > _size)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            }
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            Contract.EndContractBlock();

            return Array.IndexOf(_items, item, index, _size - index);
        }


        //Object.Equals is used 
        public int IndexOf(T item, int index, int count)
        {
            if( index > _size)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            }

            if (count < 0 || index > _size - count)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            }
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            Contract.EndContractBlock();

            return Array.IndexOf(_items, item, index, count);
        }

        //Object.Equals is used
        public int LastIndexOf(T item)
        {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < Count);
            if (_size == 0)
            {  // Special case for empty list
                return -1;
            }
            else
            {
                return LastIndexOf(item, _size - 1, _size);
            }
        }
        public int LastIndexOf(T item, int index)
        {
            if (index >= _size)
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
                throw new ArgumentOutOfRangeException();
            }
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(((Count == 0) && (Contract.Result<int>() == -1)) || ((Count > 0) && (Contract.Result<int>() <= index)));
            Contract.EndContractBlock();

            //count is all the items from the beginning to index, so (index+1) items
            return LastIndexOf(item, index, index + 1);

        }

        //return the index of the item in this list
        //start looking from index and counting "count" numer of items
        public int LastIndexOf(T item, int index, int count)
        {
            if((Count!=0) && (index < 0))
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if((Count!=0) && (count < 0))
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(((Count == 0) && (Contract.Result<int>() == -1)) || ((Count > 0) && (Contract.Result<int>() <= index)));
            Contract.EndContractBlock();

            if (_size == 0)
            {  // Special case for empty list
                return -1;
            }

            if (index >= _size)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
            }

            if (count > index + 1)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
            }

            return Array.LastIndexOf(_items, item, index, count);
        }



        //removing : removes the element at the given index 
        //size of the list is decreased by one
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
            {
                throw new ArgumentOutOfRangeException();
                //hrowHelper.ThrowArgumentOutOfRangeException();
            }
            Contract.EndContractBlock();

            //now _size is the last index of the list
            _size--;

            //if there are items until the end of the list starting from index, move them to the left
            if(index < _size)
            {
                //count = _size - index: because we need one less item 
                //before _size - index would count the initial index too, but now we do the items after index, so one less 
                Array.Copy(_items, index + 1, _items, index, _size - index);
            }
            //last item in the list shall be the default
            _items[_size] = default(T);

            //change so
            _version++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if(index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        void System.Collections.IList.Remove(Object item)
        {
            if (IsCompatibleObject(item))
            {
                Remove((T)item);
            }
        }

        //removes a range of elements, starting from index, count items
        public void RemoveRange(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (_size - index < count)
            {
                throw new ArgumentException();
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            Contract.EndContractBlock();

            if (count > 0)
            {
                //why assigning? 
                int i = _size;

                //now _size is the index of the last item , after the removed items
                _size -= count;
                if (index < _size)
                {
                    Array.Copy(_items, index + count, _items, index, _size - index);
                }
                Array.Clear(_items, _size, count);
                
                //change, so
                _version++;
            }
        }

        // This method removes all items which matches the predicate.
        //returns the number of elements removed
        public int RemoveAll(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.Ensures(Contract.Result<int>() >= 0);

            //the value of Count at the start of the method shall be >= than the result
            Contract.Ensures(Contract.Result<int>() <= Contract.OldValue(Count));
            Contract.EndContractBlock();

            int freeIndex = 0;   // the first free slot in items array
                                 //it checks to see if that freeIndex index item needs to be removed or no
            

            _version++;
            return 1;
        }

        //the item in the index place will now be at index+(index+count-i-1)
        public void Reverse(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException();

                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (_size - index < count)
            {
                throw new ArgumentException();
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            Contract.EndContractBlock();
            Array.Reverse(_items, index, count);

            //change
            _version++;
        }

        public void Reverse()
        {
            Reverse(0, Count);
        }



        //sorting
        //uses Array.Sort

        //sort the list starting from index counting "count" items
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException();

                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (_size - index < count)
            {
                throw new ArgumentException();
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            Contract.EndContractBlock();

            Array.Sort<T>(_items, index, count, comparer);
            _version++;
        }
        public void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        }

        //uses the default comparer and Array.Sort
        public void Sort()
        {
            Sort(0, Count, null);
        }

        //Comparison<T> is a delegate, represents the method that compars two objects of the same type
        //public delegate int Comparison<in T>(T x, T y);
        // comparison(a,b) compares a and b
        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null)
            {
                throw new ArgumentNullException();
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();

            if(_size > 0)
            {
                //IComparer<T> comparer = new Array.FunctorComparer<T>(comparison);
                //Array.Sort(_items, 0, _size, comparer);
            }
        }
   
        //O(n) operation that copies each item into the new array
        //uses Array.Copy
        public T[] ToArray()
        {
            Contract.Ensures(Contract.Result<T[]>() != null);
            Contract.Ensures(Contract.Result<T[]>().Length == Count);

            T[] array = new T[_size];
            Array.Copy(_items, 0, array, 0, _size);

            return array;
        }
        
        public void TrimExcess()
        {
            int threshold = (int)(((double)_items.Length) * 0.9);
            if (_size < threshold)
            {
                Capacity = _size;
            }
        }

        public bool TrueForAll(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            Contract.EndContractBlock();
            for (int i = 0; i < _size; i++)
            {
                if(!match(_items[i]))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
