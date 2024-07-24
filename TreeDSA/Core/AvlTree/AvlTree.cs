using System.Collections;
using TreeDSA.Extensions;

namespace TreeDSA.Core.AvlTree
{
    public class AvlTree<T> : ITree<T>
            where T : IComparable<T>
    {
        public int Count { get; private set; } = 0;
        private AvlTreeNode<T>? Root { get; set; } = null;

        public AvlTree()
        {

        }

        public AvlTree(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

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

            var newRoot = AddInternal(Root, item, out var addedNode);
            var isAdded = addedNode is not null;

            if (isAdded)
            {
                Root = newRoot;
                Count++;
            }

            return isAdded;
        }

        public bool Contains(T item)
        {
            return FindItemInTree(Root, item) is not null;
        }

        public bool Remove(T item)
        {
            var newRoot = RemoveInternal(Root, item, out var removedNode);
            var isRemoved = removedNode is not null;

            if (isRemoved)
            {
                Root = newRoot;
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
            const int startingIndent = 0;
            DisplayInternal(Root, startingIndent);
        }

        private void InitializeRoot(T item)
        {
            Root = NewNode(item);
            Count = 1;
        }

        private static AvlTreeNode<T> NewNode(T item)
        {
            return new AvlTreeNode<T>(item);
        }

        private static int Height(AvlTreeNode<T>? node)
        {
            return node is not null
                ? node.Height
                : 0;
        }

        private static void UpdateHeight(AvlTreeNode<T> node)
        {
            node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;
        }

        private static int BalanceFactor(AvlTreeNode<T>? node)
        {
            return node is not null
                ? Height(node.Left) - Height(node.Right)
                : 0;
        }

        private static AvlTreeNode<T> AddInternal(AvlTreeNode<T> currentNode, T item, out AvlTreeNode<T>? addedNode)
        {
            addedNode = DoNormalBstInsertion(currentNode, item);
            if (addedNode is null)
            {
                return currentNode;
            }

            UpdateHeight(currentNode);

            return CheckAfterInsertionAndBalanceTreeIfNeeded(currentNode, item);
        }

        private static AvlTreeNode<T>? DoNormalBstInsertion(AvlTreeNode<T> currentNode, T item)
        {
            AvlTreeNode<T>? addedNode = null;
            var compareToResult = item.CompareTo(currentNode.Value);

            if (compareToResult.IsLess())
            {
                if (currentNode.Left is not null)
                {
                    currentNode.Left = AddInternal(currentNode.Left, item, out addedNode);
                }
                else
                {
                    addedNode = NewNode(item);
                    currentNode.Left = addedNode;
                }
            }
            else if (compareToResult.IsGreater())
            {
                if (currentNode.Right is not null)
                {
                    currentNode.Right = AddInternal(currentNode.Right, item, out addedNode);
                }
                else
                {
                    addedNode = NewNode(item);
                    currentNode.Right = addedNode;
                }
            }

            return addedNode;
        }

        private static AvlTreeNode<T>? RemoveInternal(AvlTreeNode<T>? currentNode, T item, out AvlTreeNode<T>? removedNode)
        {
            removedNode = DoNormalBstDeletion(ref currentNode, item);
            if (removedNode is null || currentNode is null)
            {
                return currentNode;
            }

            UpdateHeight(currentNode);

            return CheckAfterDeletionAndBalanceTreeIfNeeded(currentNode);
        }

        private static AvlTreeNode<T>? DoNormalBstDeletion(ref AvlTreeNode<T>? currentNode, T item)
        {
            if (currentNode is null)
            {
                return null;
            }

            AvlTreeNode<T>? removedNode = null;
            var compareToResult = item.CompareTo(currentNode.Value);

            if (compareToResult.IsEqual())
            {
                removedNode = currentNode;
                DetachNodeFromTree(ref currentNode);
            }
            else if (compareToResult.IsLess() && currentNode.Left is not null)
            {
                currentNode.Left = RemoveInternal(currentNode.Left, item, out removedNode);
            }
            else if (compareToResult.IsGreater() && currentNode.Right is not null)
            {
                currentNode.Right = RemoveInternal(currentNode.Right, item, out removedNode);
            }

            return removedNode;
        }

        private static void DetachNodeFromTree(ref AvlTreeNode<T>? currentNode)
        {
            if (currentNode is null)
            {
                return;
            }

            if (currentNode.Left is null || currentNode.Right is null)
            {
                currentNode = currentNode.Left ?? currentNode.Right;
            }
            else
            {
                var smallestInorderSuccessor = GetMinValueNodeInTree(currentNode.Right);
                currentNode.Value = smallestInorderSuccessor.Value;
                currentNode.Right = RemoveInternal(currentNode.Right, smallestInorderSuccessor.Value, out var _);
            }
        }

        private static AvlTreeNode<T> CheckAfterInsertionAndBalanceTreeIfNeeded(AvlTreeNode<T> root, T item)
        {
            var rootBalanceFactor = BalanceFactor(root);
            var leftSubtreeBalanceFactorThreshold = 1;
            var rightSubtreeBalanceFactorThreshold = -leftSubtreeBalanceFactorThreshold;

            if (rootBalanceFactor > leftSubtreeBalanceFactorThreshold)
            {
                return HandleInsertionLeftUnbalancedTree(root, item);
            }
            else if (rootBalanceFactor < rightSubtreeBalanceFactorThreshold)
            {
                return HandleInsertionRightUnbalancedTree(root, item);
            }

            return root;
        }

        private static AvlTreeNode<T> CheckAfterDeletionAndBalanceTreeIfNeeded(AvlTreeNode<T> root)
        {
            var rootBalanceFactor = BalanceFactor(root);
            var leftSubtreeBalanceFactorThreshold = 1;
            var rightSubtreeBalanceFactorThreshold = -leftSubtreeBalanceFactorThreshold;

            if (rootBalanceFactor > leftSubtreeBalanceFactorThreshold)
            {
                return HandleDeletionLeftUnbalancedTree(root);
            }
            else if (rootBalanceFactor < rightSubtreeBalanceFactorThreshold)
            {
                return HandleDeletionRightUnbalancedTree(root);
            }

            return root;
        }

        private static AvlTreeNode<T> HandleInsertionLeftUnbalancedTree(AvlTreeNode<T> root, T item)
        {
            if (root.Left is null)
            {
                throw new ArgumentOutOfRangeException(nameof(root));
            }

            return item.CompareTo(root.Left.Value).IsLess()
                ? HandleLeftLeftCase(root)
                : HandleLeftRightCase(root);
        }

        private static AvlTreeNode<T> HandleInsertionRightUnbalancedTree(AvlTreeNode<T> root, T item)
        {
            if (root.Right is null)
            {
                throw new ArgumentOutOfRangeException(nameof(root));
            }

            return item.CompareTo(root.Right.Value).IsGreater()
                ? HandleRightRightCase(root)
                : HandleRightLeftCase(root);
        }

        private static AvlTreeNode<T> HandleDeletionLeftUnbalancedTree(AvlTreeNode<T> root)
        {
            var leftChildBalanceFactor = BalanceFactor(root.Left ?? throw new ArgumentOutOfRangeException(nameof(root)));
            return leftChildBalanceFactor >= 0
                ? HandleLeftLeftCase(root)
                : HandleLeftRightCase(root);
        }

        private static AvlTreeNode<T> HandleDeletionRightUnbalancedTree(AvlTreeNode<T> root)
        {
            var rightChildBalanceFactor = BalanceFactor(root.Right ?? throw new ArgumentOutOfRangeException(nameof(root)));
            return rightChildBalanceFactor <= 0
                ? HandleRightRightCase(root)
                : HandleRightLeftCase(root);
        }

        private static AvlTreeNode<T> HandleLeftLeftCase(AvlTreeNode<T> root)
        {
            return RotateRight(root);
        }

        private static AvlTreeNode<T> HandleLeftRightCase(AvlTreeNode<T> root)
        {
            root.Left = RotateLeft(root.Left ?? throw new ArgumentOutOfRangeException(nameof(root)));
            return RotateRight(root);
        }

        private static AvlTreeNode<T> HandleRightRightCase(AvlTreeNode<T> root)
        {
            return RotateLeft(root);
        }

        private static AvlTreeNode<T> HandleRightLeftCase(AvlTreeNode<T> root)
        {
            root.Right = RotateRight(root.Right ?? throw new ArgumentOutOfRangeException(nameof(root)));
            return RotateLeft(root);
        }

        private static AvlTreeNode<T> RotateLeft(AvlTreeNode<T> pivotNode)
        {
            var rightChild = pivotNode.Right
                ?? throw new ArgumentException($"{nameof(AvlTree)}: Attempted to rotate left a node having no right child");

            pivotNode.Right = rightChild.Left;
            rightChild.Left = pivotNode;

            UpdateHeight(pivotNode);
            UpdateHeight(rightChild);

            return rightChild;
        }

        private static AvlTreeNode<T> RotateRight(AvlTreeNode<T> pivotNode)
        {
            var leftChild = pivotNode.Left
                ?? throw new ArgumentException($"{nameof(AvlTree)}: Attempted to rotate right a node having no left child");

            pivotNode.Left = leftChild.Right;
            leftChild.Right = pivotNode;

            UpdateHeight(pivotNode);
            UpdateHeight(leftChild);

            return leftChild;
        }

        private static AvlTreeNode<T> GetMinValueNodeInTree(AvlTreeNode<T> root)
        {
            var current = root;
            while (current.Left is not null)
            {
                current = current.Left;
            }
            return current;
        }

        private static AvlTreeNode<T>? FindItemInTree(AvlTreeNode<T>? root, T item)
        {
            if (root is null)
            {
                return null;
            }

            var compareToResult = item.CompareTo(root.Value);
            if (compareToResult.IsEqual())
            {
                return root;
            }

            return compareToResult.IsLess()
                ? FindItemInTree(root.Left, item)
                : FindItemInTree(root.Right, item);
        }

        private static IEnumerator<T> GetEnumeratorInternal(AvlTreeNode<T> currentNode)
        {
            if (currentNode.Left is not null)
            {
                var leftSubtreeEnumerator = GetEnumeratorInternal(currentNode.Left);
                while (leftSubtreeEnumerator.MoveNext())
                {
                    yield return leftSubtreeEnumerator.Current;
                }
            }

            yield return currentNode.Value;

            if (currentNode.Right is not null)
            {
                var rightSubtreeEnumerator = GetEnumeratorInternal(currentNode.Right);
                while (rightSubtreeEnumerator.MoveNext())
                {
                    yield return rightSubtreeEnumerator.Current;
                }
            }
        }

        private static void DisplayInternal(AvlTreeNode<T>? currentNode, int indent)
        {
            if (currentNode is null)
            {
                return;
            }

            const int indentDiff = 10;

            DisplayInternal(currentNode.Right, indent + indentDiff);
            DisplayCurrentNodeValue(currentNode, indent);
            DisplayInternal(currentNode.Left, indent + indentDiff);
        }

        private static void DisplayCurrentNodeValue(AvlTreeNode<T> currentNode, int indent)
        {
            var indentString = " ".Repeat(indent);
            var heightString = $"<{currentNode.Height}> ";
            var nodeString = $"{currentNode.Value}";
            var prompt = $"{indentString}({heightString}{nodeString})";

            Console.WriteLine(prompt);
        }
    }
}
