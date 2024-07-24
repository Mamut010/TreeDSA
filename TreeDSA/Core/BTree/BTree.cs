using System.Collections;
using TreeDSA.Extensions;

namespace TreeDSA.Core.BTree
{
    public class BTree<T> : ITree<T>
        where T : IComparable<T>
    {
        public const int LOWEST_MINIMUM_DEGREE = 2;
        public const int DEFAULT_MINIMUM_DEGREE = 42; // The answer to everything

        public int MinimumDegree { get; }
        public int Count { get; private set; } = 0;
        private BTreeNode<T>? Root { get; set; } = null;

        public BTree(int minimumDegree)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(minimumDegree, LOWEST_MINIMUM_DEGREE);

            MinimumDegree = minimumDegree;
        }

        public BTree() : this(DEFAULT_MINIMUM_DEGREE) { }

        public BTree(int minimumDegree, IEnumerable<T> items) : this(minimumDegree)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public BTree(IEnumerable<T> items) : this(DEFAULT_MINIMUM_DEGREE, items) { }

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

            var isAdded = Root.IsFull()
                ? InsertWhenRootIsFull(item)
                : InsertIntoNonFullNode(Root, item);

            if (isAdded)
            {
                Count++;
            }

            return isAdded;
        }

        public bool Contains(T item)
        {
            return Root is not null && FindItemInTree(Root, item) is not null;
        }

        public bool Remove(T item)
        {
            if (Root is null)
            {
                return false;
            }

            var isRemoved = RemoveInternal(Root, item);
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

        private void InitializeRoot(params T[] items)
        {
            Root = NewNode(isLeaf: true, items);
            Count = items.Length;
        }

        private BTreeNode<T> NewNode(bool isLeaf = false, params T[] items)
        {
            var newNode = new BTreeNode<T>(MinimumDegree, isLeaf);
            foreach (var item in items)
            {
                if (!newNode.InsertKey(item))
                {
                    break;
                }
            }
            return newNode;
        }

        private bool InsertWhenRootIsFull(T item)
        {
            if (Root is null)
            {
                throw new InvalidOperationException("Root must not be null");
            }

            var newRoot = NewNode(isLeaf: false);

            newRoot.SetChildAtIndex(Root, 0);
            newRoot.SplitChild(0);

            var childIndex = FindFirstIndexWhereItemIsLessOrEqual(newRoot, item);
            
            if (!InsertIntoNonFullNode(newRoot.ChildAt(childIndex), item))
            {
                return false;
            }
            else
            {
                Root = newRoot;
                return true;
            }
        }

        private void AssignNewRootWhenNoKey()
        {
            Root = Root is not null && !Root.IsLeaf()
                ? Root.FirstChild()
                : null;
        }

        private static bool InsertIntoNonFullNode(BTreeNode<T> node, T item)
        {
            return node.IsLeaf()
                ? InsertIntoNonFullLeafNode(node, item)
                : InsertIntoNonFullNonLeafNode(node, item);
        }

        private static bool InsertIntoNonFullLeafNode(BTreeNode<T> node, T item)
        {
            var i = node.LastKeyIndex();
            while (i >= 0 && item.CompareTo(node.KeyAt(i)).IsLess())
            {
                node.SetKeyAtIndex(node.KeyAt(i), i + 1);
                i--;
            }
            
            return node.SetKeyAtIndex(item, i + 1, isNewKey: true);
        }
        
        private static bool InsertIntoNonFullNonLeafNode(BTreeNode<T> node, T item)
        {
            var i = FindFirstIndexWhereItemIsLessOrEqual(node, item);
            var nextChild = node.ChildAt(i);

            if (nextChild.IsFull())
            {
                node.SplitChild(i);
                if (item.CompareTo(node.KeyAt(i)).IsGreater())
                {
                    nextChild = node.ChildAt(i + 1);
                }
            }

            return InsertIntoNonFullNode(nextChild, item);
        }

        private static BTreeNode<T>? FindItemInTree(BTreeNode<T> root, T item)
        {
            var keyIndex = FindFirstIndexWhereItemIsLessOrEqual(root, item);

            if (IsItemEqualToKeyValue(root, keyIndex, item))
            {
                return root;
            }
            else if (root.IsLeaf())
            {
                return null;
            }
            else
            {
                return FindItemInTree(root.ChildAt(keyIndex), item);
            }
        }

        private static int FindFirstIndexWhereItemIsLessOrEqual(BTreeNode<T> currentNode, T item)
        {
            var i = 0;
            while (i < currentNode.KeyCount && item.CompareTo(currentNode.KeyAt(i)).IsGreater())
            {
                i++;
            }
            return i;
        }

        private static bool IsItemEqualToKeyValue(BTreeNode<T> currentNode, int keyIndex, T item)
        {
            return keyIndex < currentNode.KeyCount && item.CompareTo(currentNode.KeyAt(keyIndex)).IsEqual();
        }

        private static bool RemoveInternal(BTreeNode<T> currentNode, T item)
        {
            var firstIndexWhereItemLessThanKey = FindFirstIndexWhereItemIsLessOrEqual(currentNode, item);
                
            if (IsItemEqualToKeyValue(currentNode, firstIndexWhereItemLessThanKey, item))
            {
                RemoveWhenKeyPresent(currentNode, firstIndexWhereItemLessThanKey);
                return true;
            }
            else
            {
                return RemoveWhenKeyNotPresent(currentNode, item, firstIndexWhereItemLessThanKey);
            }
        }

        private static void RemoveWhenKeyPresent(BTreeNode<T> currentNode, int keyIndex)
        {
            if (currentNode.IsLeaf())
            {
                RemoveKeyFromLeafNode(currentNode, keyIndex);
            }
            else
            {
                RemoveKeyFromNonLeafNode(currentNode, keyIndex);
            }
        }

        private static void RemoveKeyFromLeafNode(BTreeNode<T> currentNode, int keyIndex)
        {
            currentNode.RemoveKeyAtIndex(keyIndex);
        }

        private static void RemoveKeyFromNonLeafNode(BTreeNode<T> currentNode, int keyIndex)
        {
            var minKeyCount = currentNode.MinKeyCount;
            if (currentNode.ChildAt(keyIndex).KeyCount > minKeyCount)
            {
                var predecessor = ReplaceKeyWithPredecessor(currentNode, keyIndex);
                RemoveInternal(currentNode.ChildAt(keyIndex), predecessor);
            }
            else if (currentNode.ChildAt(keyIndex + 1).KeyCount > minKeyCount)
            {
                var successor = ReplaceKeyWithSuccessor(currentNode, keyIndex);
                RemoveInternal(currentNode.ChildAt(keyIndex + 1), successor);
            }
            else
            {
                var item = currentNode.KeyAt(keyIndex);

                currentNode.MergeWithChild(keyIndex);
                RemoveInternal(currentNode.ChildAt(keyIndex), item);
            }
        }

        private static bool RemoveWhenKeyNotPresent(BTreeNode<T> currentNode, T item, int firstIndexWhereItemLessThanKey)
        {
            if (currentNode.IsLeaf())
            {
                return false;
            }

            var childIndex = firstIndexWhereItemLessThanKey;
            bool isKeyPossiblyInLastChild = firstIndexWhereItemLessThanKey == currentNode.KeyCount;

            if (ShouldFillChild(currentNode, childIndex))
            {
                currentNode.FillChild(childIndex);
            }

            bool merged = HasLastChildBeenMerged(currentNode, isKeyPossiblyInLastChild, childIndex);
            childIndex = GetIndexOfChildToApplyRemove(childIndex, merged);

            return RemoveInternal(currentNode.ChildAt(childIndex), item);
        }

        private static T ReplaceKeyWithPredecessor(BTreeNode<T> currentNode, int keyIndex)
        {
            var predecessor = GetPredecessorOfKeyAtIndex(currentNode, keyIndex);
            currentNode.SetKeyAtIndex(predecessor, keyIndex);
            return predecessor;
        }

        private static T ReplaceKeyWithSuccessor(BTreeNode<T> currentNode, int keyIndex)
        {
            var successor = GetSuccessorOfKeyAtIndex(currentNode, keyIndex);
            currentNode.SetKeyAtIndex(successor, keyIndex);
            return successor;
        }

        private static T GetPredecessorOfKeyAtIndex(BTreeNode<T> node, int keyIndex)
        {
            var currentChild = node.ChildAt(keyIndex);
            while (!currentChild.IsLeaf())
            {
                currentChild = currentChild.LastChild();
            }
            return currentChild.LastKey();
        }

        private static T GetSuccessorOfKeyAtIndex(BTreeNode<T> node, int keyIndex)
        {
            var currentChild = node.ChildAt(keyIndex + 1);
            while (!currentChild.IsLeaf())
            {
                currentChild = currentChild.FirstChild();
            }
            return currentChild.FirstKey();
        }

        private static bool ShouldFillChild(BTreeNode<T> node, int childIndex)
        {
            return node.ChildAt(childIndex).KeyCount == node.MinKeyCount;
        }

        private static bool HasLastChildBeenMerged(BTreeNode<T> currentNode, bool isKeyPossiblyInLastChild, int firstIndexWhereItemLessThanKey)
        {
            return isKeyPossiblyInLastChild && firstIndexWhereItemLessThanKey > currentNode.KeyCount;
        }

        private static int GetIndexOfChildToApplyRemove(int supposedIndex, bool hasLastChildBeenMerged)
        {
            return hasLastChildBeenMerged ? supposedIndex - 1 : supposedIndex;
        }

        private static IEnumerator<T> GetEnumeratorInternal(BTreeNode<T> currentNode)
        {
            var i = 0;
            IEnumerator<T> childEnumerator;

            while (i < currentNode.KeyCount)
            {
                childEnumerator = GetChildEnumerator(currentNode, i);
                while (childEnumerator.MoveNext())
                {
                    yield return childEnumerator.Current;
                }

                yield return currentNode.KeyAt(i);
                i++;
            }

            childEnumerator = GetChildEnumerator(currentNode, i);
            while (childEnumerator.MoveNext())
            {
                yield return childEnumerator.Current;
            }
        }

        private static IEnumerator<T> GetChildEnumerator(BTreeNode<T> node, int childIndex)
        {
            if (!node.IsLeaf())
            {
                var childEnumerator = GetEnumeratorInternal(node.ChildAt(childIndex));
                while (childEnumerator.MoveNext())
                {
                    yield return childEnumerator.Current;
                }
            }
        }

        private static void DisplayInternal(BTreeNode<T> currentNode, int indent)
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
                DisplayKey(currentNode, i, indent);
                i--;
            }

            if (!currentNode.IsLeaf())
            {
                DisplayInternal(currentNode.ChildAt(i + 1), indent + indentDiff);
            }

            Console.WriteLine($"{indentString}\\/");
        }

        private static void DisplayKey(BTreeNode<T> currentNode, int index, int indent)
        {
            var indentString = " ".Repeat(indent);
            var nodeString = $"{currentNode.KeyAt(index)}";
            var prompt = $"{indentString}({nodeString})";

            Console.WriteLine(prompt);
        }
    }
}
