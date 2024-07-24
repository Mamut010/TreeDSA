using System.Collections;
using System.Xml.Linq;
using TreeDSA.Extensions;

namespace TreeDSA.Core.BPlusTree
{
    public class BPlusTree<T> : ITree<T>
        where T : IComparable<T>
    {
        public const int LOWEST_ORDER = 3;
        public const int DEFAULT_ORDER = 42; // The answer to everything

        public int Order { get; }
        public int Count { get; private set; } = 0;
        private BPlusTreeNode<T, T>? Root { get; set; } = null;

        public BPlusTree(int order)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(order, LOWEST_ORDER);

            Order = order;
        }

        public BPlusTree() : this(DEFAULT_ORDER) { }

        public BPlusTree(int order, IEnumerable<T> items) : this(order)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public BPlusTree(IEnumerable<T> items) : this(DEFAULT_ORDER, items) { }

        public bool IsEmpty()
        {
            return Root is null;
        }

        public void Clear()
        {
            Root = null;
            Count = 0;
        }

        public bool Add(T item)
        {
            if (Root is null)
            {
                InitializeRoot(item);
                return true;
            }

            var isAdded = AddInternal(Root, MakeKeyValue(item));
            if (isAdded)
            {
                Count++;
            }
            return isAdded;
        }

        public bool Contains(T item)
        {
            return Root is not null && IsItemInTree(Root, item);
        }

        public bool Remove(T item)
        {
            if (Root is null)
            {
                return false;
            }

            var isRemoved = RemoveInternal(Root, MakeKeyValue(item));
            if (Root.KeyCount == 0)
            {
                AssignNewRootWhenNoKey();
            }

            if (isRemoved)
            {
                Count--;
            }
            return isRemoved;
        }

        public T[] ToArray()
        {
            var values = new T[Count];
            int i = 0;
            foreach (var value in this)
            {
                values[i++] = value;
            }
            return values;
        }

        public List<T> ToList()
        {
            return new List<T>(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Root is not null
                ? GetEnumeratorInternal(Root)
                : Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Display()
        {
            if (Root is null)
            {
                return;
            }

            const int startingIndent = 0;
            DisplayInternal(Root, startingIndent);
        }

        private void InitializeRoot(T item)
        {
            Root = NewNode(isLeaf: true, item);
            Count = 1;
        }

        private BPlusTreeNode<T, T> NewNode(bool isLeaf = false, params T[] items)
        {
            var newNode = new BPlusTreeNode<T, T>(Order, isLeaf);
            foreach (var item in items)
            {
                if (!newNode.InsertKeyValue(MakeKeyValue(item)))
                {
                    break;
                }
            }
            return newNode;
        }

        private bool AddInternal(BPlusTreeNode<T, T> root, KeyValuePair<T, T> item)
        {
            var leafNode = TraverseDownTreeToFindSuitableNodeForInsertion(root, item.Key);
            if (!leafNode.IsLeaf() || IsKeyInNodeKeys(leafNode, item.Key))
            {
                return false;
            }

            if (!leafNode.IsFull())
            {
                InsertIntoNonFullLeafNode(leafNode, item);
            }
            else
            {
                InsertIntoFullLeafNode(leafNode, item);
            }
            return true;
        }

        private void InsertIntoFullLeafNode(BPlusTreeNode<T, T> leafNode, KeyValuePair<T, T> item)
        {
            var keyValuePairs = CreateSortedListFromNodeKeyValuePairsPlusAdditionalData(leafNode, item);
            var newLeafNode = NewNode(isLeaf: true);

            var medianKey = RearrangeLeafNodeWithNewNextLeafNode(leafNode, newLeafNode, keyValuePairs);
            if (leafNode == Root)
            {
                AssignNewRootAfterInsertion(leafNode, newLeafNode, medianKey);
            }
            else
            {
                var parent = leafNode.Parent!;
                ShiftLevel(parent, newLeafNode, medianKey);
            }
        }

        private T RearrangeLeafNodeWithNewNextLeafNode(BPlusTreeNode<T, T> leafNode, BPlusTreeNode<T, T> newLeafNode,
            IList<KeyValuePair<T, T>> keyValuePairs)
        {
            var firstNodeSize = SizeOfFirstNodeWhenOverflowInLeafNode();
            var secondNodeSize = keyValuePairs.Count - firstNodeSize;

            for (var i = 0; i < firstNodeSize; i++)
            {
                leafNode.SetKeyValueAtIndex(keyValuePairs[i], i);
            }
            for (var i = 0; i < secondNodeSize; i++)
            {
                newLeafNode.InsertKeyValue(keyValuePairs[i + firstNodeSize]);
            }

            leafNode.TruncateKeyValuesToSize(firstNodeSize);

            var previouslyNextLeftNode = leafNode.Next;
            leafNode.Next = newLeafNode;
            newLeafNode.Next = previouslyNextLeftNode;

            var medianKey = newLeafNode.FirstKey();
            return medianKey;
        }

        private void AssignNewRootAfterInsertion(BPlusTreeNode<T, T> child, BPlusTreeNode<T, T> nextChild, T rootKey)
        {
            Root = NewNode(isLeaf: false, rootKey);
            Root.SetChildAtIndex(child, 0);
            Root.SetChildAtIndex(nextChild, 1);
        }

        private void ShiftLevel(BPlusTreeNode<T, T> parent, BPlusTreeNode<T, T> current, T key)
        {
            if (!parent.IsFull())
            {
                InsertIntoNonFullInternalNode(parent, current, key);
            }
            else
            {
                ShiftUp(parent, current, key);
            }
        }

        private void ShiftUp(BPlusTreeNode<T, T> current, BPlusTreeNode<T, T> child, T key)
        {
            var (keys, children) = CreateSortedListsFromNodeKeysAndChildrenPlusAdditionalData(current, key, child);
            var newInternalNode = NewNode(isLeaf: false);

            var medianKey = RearrangeInternalNodeWithNewNextInternalNode(current, newInternalNode, keys, children);
            if (current == Root)
            {
                AssignNewRootAfterInsertion(current, newInternalNode, medianKey);
            }
            else
            {
                var parent = current.Parent!;
                ShiftLevel(parent, newInternalNode, medianKey);
            }
        }

        private T RearrangeInternalNodeWithNewNextInternalNode(BPlusTreeNode<T, T> node, 
            BPlusTreeNode<T, T> newInternalNode, IList<T> keys, IList<BPlusTreeNode<T, T>> children)
        {
            var firstNodeSize = SizeOfFirstNodeWhenOverflowInInternalNode();
            var secondNodeSize = node.KeyCount - firstNodeSize;

            var i = 0;
            var correspondingIndexInWholeList = firstNodeSize + 1;
            while (i < secondNodeSize)
            {
                newInternalNode.InsertKey(keys[correspondingIndexInWholeList]);
                newInternalNode.SetChildAtIndex(children[correspondingIndexInWholeList], i);
                i++;
                correspondingIndexInWholeList++;
            }
            newInternalNode.SetChildAtIndex(children[correspondingIndexInWholeList], i);

            var medianKey = keys[firstNodeSize];
            node.TruncateKeysToSize(firstNodeSize);
            return medianKey;
        }

        private void AssignNewRootWhenNoKey()
        {
            Root = Root is not null && !Root.IsLeaf()
                ? Root.FirstChild()
                : null;
        }

        private static BPlusTreeNode<T, T> TraverseDownTreeToFindSuitableNodeForInsertion(BPlusTreeNode<T, T> root, T key)
        {
            var current = root;

            while (!current.IsLeaf())
            {
                var childIndex = FindFirstIndexWhereKeyIsLessOrEqual(current, key);
                if (IsNodeKeyAtIndexEqualToKey(current, childIndex, key))
                {
                    break;
                }
                current = current.ChildAt(childIndex);
            }

            return current;
        }

        private static void InsertIntoNonFullLeafNode(BPlusTreeNode<T, T> node, KeyValuePair<T, T> item)
        {
            var i = node.LastKeyIndex();
            while (i >= 0 && item.Key.CompareTo(node.KeyAt(i)).IsLessOrEqual())
            {
                var preceedingKeyValuePair = MakeKeyValue(node.KeyAt(i), node.ValueAt(i));
                node.SetKeyValueAtIndex(preceedingKeyValuePair, i + 1);
                i--;
            }

            node.SetKeyValueAtIndex(item, i + 1);
        }

        private static void InsertIntoNonFullInternalNode(BPlusTreeNode<T, T> current, BPlusTreeNode<T, T> child, T key)
        {
            var i = current.LastKeyIndex();
            var childIndex = i + 1;
            while (i >= 0 && key.CompareTo(current.KeyAt(i)).IsLessOrEqual())
            {
                var preceedingKey = current.KeyAt(i);
                var preceedingChild = current.ChildAt(childIndex);

                current.SetKeyAtIndex(preceedingKey, i + 1);
                current.SetChildAtIndex(preceedingChild, childIndex + 1);

                i--;
                childIndex--;
            }

            current.SetKeyAtIndex(key, i + 1);
            current.SetChildAtIndex(child, childIndex + 1);
        }

        private static IList<KeyValuePair<T, T>> CreateSortedListFromNodeKeyValuePairsPlusAdditionalData
            (BPlusTreeNode<T, T> node, KeyValuePair<T, T> additonalPair)
        {
            var keyValuePairs = new List<KeyValuePair<T, T>>(node.KeyCount + 1);

            var isAdditionalDataAdded = false;
            for (var i = 0; i < node.KeyCount; i++)
            {
                if (!isAdditionalDataAdded && additonalPair.Key.CompareTo(node.KeyAt(i)).IsLessOrEqual())
                {
                    keyValuePairs.Add(additonalPair);
                    isAdditionalDataAdded = true;
                }

                keyValuePairs.Add(KeyValuePair.Create(node.KeyAt(i), node.ValueAt(i)));
            }

            if (!isAdditionalDataAdded)
            {
                keyValuePairs.Add(additonalPair);
            }

            return keyValuePairs;
        }

        private static (IList<T> Keys, IList<BPlusTreeNode<T, T>> Children)
            CreateSortedListsFromNodeKeysAndChildrenPlusAdditionalData
            (BPlusTreeNode<T, T> node, T additonalKey, BPlusTreeNode<T, T> additionalChild)
        {
            var keys = new List<T>(node.KeyCount + 1);
            var children = new List<BPlusTreeNode<T, T>>(node.ChildCount + 1);

            var isAdditionalDataAdded = false;
            for (var i = 0; i < node.KeyCount; i++)
            {
                if (!isAdditionalDataAdded && additonalKey.CompareTo(node.KeyAt(i)).IsLessOrEqual())
                {
                    keys.Add(additonalKey);
                    children.Add(additionalChild);
                    isAdditionalDataAdded = true;
                }

                keys.Add(node.KeyAt(i));
                children.Add(node.ChildAt(i));
            }
            children.Add(node.LastChild());

            if (!isAdditionalDataAdded)
            {
                keys.Add(additonalKey);
                children.Add(additionalChild);
            }

            return (keys, children);
        }

        private static bool IsItemInTree(BPlusTreeNode<T, T> root, T item)
        {
            var keyIndex = FindFirstIndexWhereKeyIsLessOrEqual(root, item);

            if (IsNodeKeyAtIndexEqualToKey(root, keyIndex, item))
            {
                return true;
            }
            else if (root.IsLeaf())
            {
                return false;
            }
            else
            {
                return IsItemInTree(root.ChildAt(keyIndex), item);
            }
        }

        private static int FindFirstIndexWhereKeyIsLessOrEqual(BPlusTreeNode<T, T> currentNode, T key)
        {
            var i = 0;
            while (i < currentNode.KeyCount && key.CompareTo(currentNode.KeyAt(i)).IsGreater())
            {
                i++;
            }
            return i;
        }

        private static bool IsNodeKeyAtIndexEqualToKey(BPlusTreeNode<T, T> currentNode, int keyIndex, T key)
        {
            return keyIndex < currentNode.KeyCount && key.CompareTo(currentNode.KeyAt(keyIndex)).IsEqual();
        }

        private static bool IsKeyInNodeKeys(BPlusTreeNode<T, T> node, T key)
        {
            var keyIndex = FindFirstIndexWhereKeyIsLessOrEqual(node, key);
            return IsNodeKeyAtIndexEqualToKey(node, keyIndex, key);
        }

        private static bool RemoveInternal(BPlusTreeNode<T, T> currentNode, KeyValuePair<T, T> item)
        {
            var keyIndex = FindFirstIndexWhereKeyIsLessOrEqual(currentNode, item.Key);

            if (IsNodeKeyAtIndexEqualToKey(currentNode, keyIndex, item.Key))
            {
                RemoveWhenKeyPresent(currentNode, keyIndex, item);
                return true;
            }
            else if (currentNode.IsLeaf())
            {
                return false;
            }
            else
            {
                return RemoveInternal(currentNode.ChildAt(keyIndex), item);
            }
        }

        private static void RemoveWhenKeyPresent(BPlusTreeNode<T, T> currentNode, int keyIndex, KeyValuePair<T, T> item)
        {
            if (currentNode.IsLeaf())
            {
                RemoveFromOnlyLeafNode(currentNode, keyIndex);
            }
            else
            {
                var leafNode = GetLeafNodeHavingTheSameKey(currentNode, keyIndex);
                RemoveFromLeafAndInternalNode(leafNode, currentNode, item);
            }
        }

        private static BPlusTreeNode<T, T> GetLeafNodeHavingTheSameKey(BPlusTreeNode<T, T> internalNode, int keyIndex)
        {
            var leafNode = internalNode.ChildAt(keyIndex + 1);
            while (!leafNode.IsLeaf())
            {
                leafNode = leafNode.FirstChild();
            }
            return leafNode;
        }

        private static void RemoveFromOnlyLeafNode(BPlusTreeNode<T, T> currentNode, int keyIndex)
        {
            if (currentNode.KeyCount > currentNode.MinKeyCount)
            {
                RemoveFromOnlyLeafNodeWhenMoreThanMinimumKeyCount(currentNode, keyIndex);
            }
            else
            {
                RemoveFromOnlyLeafNodeWhenExactMinimumKeyCount(currentNode, keyIndex);
            }
        }

        private static void RemoveFromOnlyLeafNodeWhenMoreThanMinimumKeyCount(BPlusTreeNode<T, T> currentNode, int keyIndex)
        {
            currentNode.RemoveKeyValueAtIndex(keyIndex);
        }

        private static void RemoveFromOnlyLeafNodeWhenExactMinimumKeyCount(BPlusTreeNode<T, T> currentNode, int keyIndex)
        {
            // TODO
            throw new NotImplementedException();
        }

        private static void RemoveFromLeafAndInternalNode(BPlusTreeNode<T, T> leafNode, BPlusTreeNode<T, T> internalNode, 
            KeyValuePair<T, T> item)
        {
            // TODO
            throw new NotImplementedException();
        }

        private static IEnumerator<T> GetEnumeratorInternal(BPlusTreeNode<T, T> root)
        {
            var current = GetLeftMostLeaf(root);

            while (current is not null)
            {
                var currentEnumerator = GetLeafNodeEnumerator(current);
                while (currentEnumerator.MoveNext())
                {
                    yield return currentEnumerator.Current;
                }

                current = current.Next;
            }
        }

        private static BPlusTreeNode<T, T> GetLeftMostLeaf(BPlusTreeNode<T, T> node)
        {
            while (!node.IsLeaf())
            {
                node = node.FirstChild();
            }
            return node;
        }

        private static IEnumerator<T> GetLeafNodeEnumerator(BPlusTreeNode<T, T> leafNode)
        {
            for (var i = 0; i < leafNode.ValueCount; i++)
            {
                yield return leafNode.ValueAt(i);
            }
        }

        private static void DisplayInternal(BPlusTreeNode<T, T> currentNode, int indent)
        {
            const int indentDiff = 10;

            var indentString = " ".Repeat(indent);
            Console.WriteLine($"{indentString}/\\");

            var i = currentNode.LastKeyIndex();
            while (i >= 0)
            {
                if (!currentNode.IsLeaf())
                {
                    DisplayInternal(currentNode.ChildAt(i + 1), indent + indentDiff);
                }
                DisplayContent(currentNode, i, indent);
                i--;
            }

            if (!currentNode.IsLeaf())
            {
                DisplayInternal(currentNode.ChildAt(i + 1), indent + indentDiff);
            }

            Console.WriteLine($"{indentString}\\/");
        }

        private static void DisplayContent(BPlusTreeNode<T, T> currentNode, int index, int indent)
        {
            var indentString = " ".Repeat(indent);
            var contentString = currentNode.IsLeaf()
                ? $"{currentNode.KeyAt(index)}:{currentNode.ValueAt(index)}"
                : $"{currentNode.KeyAt(index)}";
            var prompt = $"{indentString}({contentString})";

            Console.WriteLine(prompt);
        }

        private int SizeOfFirstNodeWhenOverflowInLeafNode()
        {
            return (int)Math.Ceiling((decimal)(Order - 1) / 2);
        }

        private int SizeOfFirstNodeWhenOverflowInInternalNode()
        {
            return (int)Math.Ceiling((decimal)Order / 2) - 1;
        }

        private static KeyValuePair<TKey, TKey> MakeKeyValue<TKey>(TKey item)
        {
            return KeyValuePair.Create(item, item);
        }

        private static KeyValuePair<TKey, TValue> MakeKeyValue<TKey, TValue>(TKey key, TValue value)
        {
            return KeyValuePair.Create(key, value);
        }
    }
}