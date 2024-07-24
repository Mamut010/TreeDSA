using System.Collections;
using TreeDSA.Extensions;

namespace TreeDSA.Core.SplayTree
{
    public class SplayTree<T> : ITree<T>
        where T : IComparable<T>
    {
        public int Count {  get; private set; } = 0;
        private SplayTreeNode<T>? Root { get; set; } = null;

        public SplayTree()
        {

        }

        public SplayTree(IEnumerable<T> items)
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
            Root = Splay(Root, item);
            if (Root is null)
            {
                InitializeRoot(item);
                return true;
            }

            var itemCompareToRootValueResult = item.CompareTo(Root.Value);
            if (itemCompareToRootValueResult.IsEqual())
            {
                return false;
            }

            var addedNode = NewNode(item);
            if (itemCompareToRootValueResult.IsLess())
            {
                AddAsNewRootAndMakeRootRightChild(addedNode);
            }
            else
            {
                AddAsNewRootAndMakeRootLeftChild(addedNode);
            }

            return true;
        }

        public bool Contains(T item)
        {
            Root = Splay(Root, item);
            return Root is not null && item.CompareTo(Root.Value).IsEqual();
        }

        // Top-down approach
        public bool Remove(T item)
        {
            Root = Splay(Root, item);
            if (Root is null || item.CompareTo(Root.Value).IsNotEqual())
            {
                return false;
            }
            else
            {
                DeleteRootAndJoinLeftRightSubtreeAsNewTree();
                return true;
            }
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

        private void AddAsNewRootAndMakeRootRightChild(SplayTreeNode<T> newRoot)
        {
            newRoot.Right = Root ?? throw new InvalidOperationException("Root must not be null");
            newRoot.Left = Root.Left;
            Root.Left = null;

            Root = newRoot;
            Count++;
        }

        private void AddAsNewRootAndMakeRootLeftChild(SplayTreeNode<T> newRoot)
        {
            newRoot.Left = Root ?? throw new InvalidOperationException("Root must not be null");
            newRoot.Right = Root.Right;
            Root.Right = null;

            Root = newRoot;
            Count++;
        }

        private void DeleteRootAndJoinLeftRightSubtreeAsNewTree()
        {
            var maxValueNodeOfLeftSubtree = SplayMaxValueNodeOfLeftSubtree(Root ?? throw new InvalidOperationException("Root must not be null"));
            var rightSubtreeRoot = Root.Right;

            if (maxValueNodeOfLeftSubtree is null)
            {
                Root = rightSubtreeRoot;
            }
            else
            {
                Root = maxValueNodeOfLeftSubtree;
                Root.Right = rightSubtreeRoot;
            }
            
            Count--;
        }

        private static SplayTreeNode<T>? SplayMaxValueNodeOfLeftSubtree(SplayTreeNode<T> root)
        {
            var leftSubtreeRoot = root.Left;
            return Splay(leftSubtreeRoot, root.Value);
        }

        private static SplayTreeNode<T> NewNode(T item)
        {
            return new SplayTreeNode<T>(item);
        }

        /// <summary>
        /// This function brings the key at root if key is present in tree. 
        /// If key is not present, then it brings the last accessed item at root.
        /// This function modifies the tree and returns the new root.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static SplayTreeNode<T>? Splay(SplayTreeNode<T>? root, T item)
        {
            if (root is null)
            {
                return null;
            }

            int itemCompareToRootValueResult = item.CompareTo(root.Value);

            if (itemCompareToRootValueResult.IsLess())
            {
                return SplayCaseItemInLeftSubtree(root, item);
            }
            else if (itemCompareToRootValueResult.IsGreater())
            {
                return SplayCaseItemInRightSubtree(root, item);
            }
            else
            {
                return root;
            }
        }

        private static SplayTreeNode<T> SplayCaseItemInLeftSubtree(SplayTreeNode<T> root, T item)
        {
            var leftChild = root.Left;
            if (leftChild is null)
            {
                return root;
            }

            var itemCompareToLeftChildValue = item.CompareTo(leftChild.Value);

            if (itemCompareToLeftChildValue.IsLess())
            {
                return DoZigZigRotation(root, item);
            }
            else if (itemCompareToLeftChildValue.IsGreater())
            {
                return DoZagZigRotation(root, item);
            }
            else
            {
                return DoZigRotation(root);
            }
        }

        private static SplayTreeNode<T> SplayCaseItemInRightSubtree(SplayTreeNode<T> root, T item)
        {
            var rightChild = root.Right;
            if (rightChild is null)
            {
                return root;
            }

            var itemCompareToRightChildValue = item.CompareTo(rightChild.Value);

            if (itemCompareToRightChildValue.IsLess())
            {
                return DoZigZagRotation(root, item);
            }
            else if (itemCompareToRightChildValue.IsGreater())
            {
                return DoZagZagRotation(root, item);
            }
            else
            {
                return DoZagRotation(root);
            }
        }

        /// <summary>
        /// Double Right rotation.
        /// </summary>
        /// <param name="grandparent"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static SplayTreeNode<T> DoZigZigRotation(SplayTreeNode<T> grandparent, T item)
        {
            var parent = grandparent.Left ?? throw new ArgumentOutOfRangeException(nameof(grandparent));
            var child = parent.Left;

            child = Splay(child, item);
            parent.Left = child;

            grandparent = DoZigRotation(grandparent);
            return DoZigRotation(grandparent);
        }

        /// <summary>
        /// Left-Right rotation.
        /// </summary>
        /// <param name="grandparent"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static SplayTreeNode<T> DoZagZigRotation(SplayTreeNode<T> grandparent, T item)
        {
            var parent = grandparent.Left ?? throw new ArgumentOutOfRangeException(nameof(grandparent));
            var child = parent.Right;

            child = Splay(child, item);
            parent.Right = child;

            if (child is not null)
            {
                child = DoZagRotation(parent);
                grandparent.Left = child;
            }
            return DoZigRotation(grandparent);
        }

        /// <summary>
        /// Right-Left rotation.
        /// </summary>
        /// <param name="grandparent"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static SplayTreeNode<T> DoZigZagRotation(SplayTreeNode<T> grandparent, T item)
        {
            var parent = grandparent.Right ?? throw new ArgumentOutOfRangeException(nameof(grandparent));
            var child = parent.Left;

            child = Splay(child, item);
            parent.Left = child;

            if (child is not null)
            {
                child = DoZigRotation(parent);
                grandparent.Right = child;
            }
            return DoZagRotation(grandparent);
        }

        /// <summary>
        /// Double Left rotation.
        /// </summary>
        /// <param name="grandparent"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static SplayTreeNode<T> DoZagZagRotation(SplayTreeNode<T> grandparent, T item)
        {
            var parent = grandparent.Right ?? throw new ArgumentOutOfRangeException(nameof(grandparent));
            var child = parent.Right;

            child = Splay(child, item);
            parent.Right = child;

            grandparent = DoZagRotation(grandparent);
            return DoZagRotation(grandparent);
        }

        /// <summary>
        /// Single Right rotation.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private static SplayTreeNode<T> DoZigRotation(SplayTreeNode<T> root)
        {
            return root.Left is not null 
                ? RotateRight(root) 
                : root;
        }

        /// <summary>
        /// Single Left rotation.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private static SplayTreeNode<T> DoZagRotation(SplayTreeNode<T> root)
        {
            return root.Right is not null 
                ? RotateLeft(root) 
                : root;
        }

        private static SplayTreeNode<T> RotateLeft(SplayTreeNode<T> pivotNode)
        {
            var rightChild = pivotNode.Right
                ?? throw new ArgumentException($"{nameof(SplayTree)}: Attempted to rotate left a node having no right child");

            pivotNode.Right = rightChild.Left;
            rightChild.Left = pivotNode;

            return rightChild;
        }

        private static SplayTreeNode<T> RotateRight(SplayTreeNode<T> pivotNode)
        {
            var leftChild = pivotNode.Left
                ?? throw new ArgumentException($"{nameof(SplayTree)}: Attempted to rotate right a node having no left child");

            pivotNode.Left = leftChild.Right;
            leftChild.Right = pivotNode;

            return leftChild;
        }

        private static IEnumerator<T> GetEnumeratorInternal(SplayTreeNode<T> currentNode)
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

        private static void DisplayInternal(SplayTreeNode<T>? currentNode, int indent)
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

        private static void DisplayCurrentNodeValue(SplayTreeNode<T> currentNode, int indent)
        {
            var indentString = " ".Repeat(indent);
            var nodeString = $"{currentNode.Value}";
            var prompt = $"{indentString}({nodeString})";

            Console.WriteLine(prompt);
        }
    }
}
