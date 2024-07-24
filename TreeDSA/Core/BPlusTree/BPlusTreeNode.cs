using System.Collections.Generic;
using System;
using TreeDSA.Extensions;

namespace TreeDSA.Core.BPlusTree
{
    internal sealed class BPlusTreeNode<TKey, TValue>(int order, bool isLeaf = false)
    {
        private readonly bool leaf = isLeaf;

        public int Order { get; } = order;
        public BPlusTreeNode<TKey, TValue>? Parent { get; private set; }
        public BPlusTreeNode<TKey, TValue>? Next { get; set; } = null;
        private List<TKey> Keys { get; } = [];
        private List<TValue> Values { get; } = [];
        private List<BPlusTreeNode<TKey, TValue>> Children { get; } = [];

        public int MinimumDegree { get => (int)Math.Ceiling((decimal)Order / 2); }
        public int MinKeyCount { get => MinimumDegree - 1; }
        public int MaxKeyCount { get => Order - 1; }
        public int KeyCount { get => Keys.Count; }
        public int ValueCount { get => !leaf ? 0 : KeyCount; }
        public int ChildCount { get => leaf ? 0 : KeyCount + 1; }

        public bool IsLeaf()
        {
            return leaf;
        }

        public bool IsFull()
        {
            return KeyCount == MaxKeyCount;
        }

        public int LastKeyIndex()
        {
            return KeyCount - 1;
        }

        public int LastValueIndex()
        {
            return LastKeyIndex();
        }

        public int LastChildIndex()
        {
            return KeyCount;
        }

        public TKey KeyAt(int index)
        {
            index = TranslateKeyIndex(index);
            return Keys[index];
        }

        public TKey FirstKey()
        {
            return KeyAt(0);
        }

        public TKey LastKey()
        {
            return KeyAt(-1);
        }

        public TValue ValueAt(int index)
        {
            index = TranslateValueIndex(index);
            return Values[index];
        }

        public TValue FirstValue()
        {
            return ValueAt(0);
        }

        public TValue LastValue()
        {
            return ValueAt(-1);
        }

        public BPlusTreeNode<TKey, TValue> ChildAt(int index)
        {
            index = TranslateChildIndex(index);
            return Children[index];
        }

        public BPlusTreeNode<TKey, TValue> FirstChild()
        {
            return ChildAt(0);
        }

        public BPlusTreeNode<TKey, TValue> LastChild()
        {
            return ChildAt(-1);
        }

        public List<KeyValuePair<TKey, TValue>> GetKeyValuePairsAsList()
        {
            var keyValuePairs = new List<KeyValuePair<TKey, TValue>>(KeyCount);
            for (int i = 0; i < KeyCount; i++)
            {
                keyValuePairs.Add(KeyValuePair.Create(Keys[i], Values[i]));
            }
            return keyValuePairs;
        }

        public List<BPlusTreeNode<TKey, TValue>> GetChildrenAsList()
        {
            return [.. Children];
        }

        public bool InsertKey(TKey key, int? index = null)
        {
            var insertedIndex = TranslateKeyIndex(index ?? KeyCount);
            if (!IsSettableIndex(insertedIndex))
            {
                return false;
            }

            Keys.Insert(insertedIndex, key);

            return true;
        }

        public bool SetKeyAtIndex(TKey key, int index)
        {
            index = TranslateKeyIndex(index);
            if (!IsSettableIndex(index))
            {
                return false;
            }

            Keys.AddOrSet(index, key);

            return true;
        }

        public bool RemoveKeyAtIndex(int index)
        {
            index = TranslateKeyIndex(index);
            if (index >= KeyCount)
            {
                return false;
            }

            Keys.RemoveAt(index);

            return true;
        }

        public bool InsertKeyValue(KeyValuePair<TKey, TValue> keyValuePair, int? index = null)
        {
            var insertedIndex = TranslateKeyIndex(index ?? KeyCount);

            var isKeyInserted = InsertKey(keyValuePair.Key, insertedIndex);
            if (isKeyInserted)
            {
                Values.Insert(insertedIndex, keyValuePair.Value);
            }

            return isKeyInserted;
        }

        public bool SetKeyValueAtIndex(KeyValuePair<TKey, TValue> keyValuePair, int index)
        {
            index = TranslateKeyIndex(index);

            var isKeySet = SetKeyAtIndex(keyValuePair.Key, index);
            if (isKeySet)
            {
                Values.AddOrSet(index, keyValuePair.Value);
            }

            return isKeySet;
        }

        public bool RemoveKeyValueAtIndex(int index)
        {
            index = TranslateKeyIndex(index);

            var isKeyRemoved = RemoveKeyAtIndex(index);
            if (isKeyRemoved)
            {
                Values.RemoveAt(index);
            }

            return isKeyRemoved;
        }

        public bool InsertChild(BPlusTreeNode<TKey, TValue> child, int? index = null)
        {
            var insertedIndex = TranslateKeyIndex(index ?? ChildCount);
            if (insertedIndex > ChildCount)
            {
                return false;
            }

            Children.Insert(insertedIndex, child);

            return true;
        }

        public bool SetChildAtIndex(BPlusTreeNode<TKey, TValue> child, int index)
        {
            index = TranslateChildIndex(index);
            if (index > ChildCount)
            {
                return false;
            }

            Children.AddOrSet(index, child);
            child.Parent = this;
            return true;
        }

        public bool RemoveChildAtIndex(int index)
        {
            index = TranslateChildIndex(index);
            if (index >= ChildCount)
            {
                return false;
            }

            Children.RemoveAt(index);

            return true;
        }

        public void TruncateKeysToSize(int newSize)
        {
            if (newSize < 0 && newSize >= KeyCount)
            {
                return;
            }

            Keys.RemoveRange(newSize, Keys.Count - newSize);
        }

        public void TruncateKeyValuesToSize(int newSize)
        {
            TruncateKeysToSize(newSize);

            if (newSize >= 0 && newSize < Values.Count)
            {
                Values.RemoveRange(newSize, Values.Count - newSize);
            }
        }

        private int TranslateKeyIndex(int index)
        {
            if (KeyCount == 0) return index;

            while (index < 0)
            {
                index += KeyCount;
            }
            return index;
        }

        private int TranslateValueIndex(int index)
        {
            return TranslateKeyIndex(index);
        }

        private int TranslateChildIndex(int index)
        {
            if (ChildCount == 0) return index;

            while (index < 0)
            {
                index += ChildCount;
            }
            return index;
        }

        private bool IsSettableIndex(int index)
        {
            return !IsFull() && index <= KeyCount;
        }
    }
}
