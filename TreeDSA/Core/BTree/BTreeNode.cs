namespace TreeDSA.Core.BTree
{
    internal sealed class BTreeNode<T>
    {
        private readonly bool leaf;

        public int MinimumDegree { get; }
        public int KeyCount { get; private set; }
        private T[] Keys { get; }
        private BTreeNode<T>[] Children { get; }

        public int MinKeyCount { get => MinimumDegree - 1; }
        public int MaxKeyCount { get => 2 * MinimumDegree - 1; }
        public int MaxChildCount { get => 2 * MinimumDegree; }
        public int ChildCount { get => leaf ? 0 : KeyCount + 1; }

        public BTreeNode(int minimumDegree, bool isLeaf = false)
        {
            leaf = isLeaf;
            MinimumDegree = minimumDegree;
            KeyCount = 0;
            Keys = new T[MaxKeyCount];
            Children = new BTreeNode<T>[MaxChildCount];
        }

        public bool IsLeaf()
        {
            return leaf;
        }

        public bool IsFull()
        {
            return KeyCount == MaxKeyCount;
        }

        public bool InsertKey(T key, int? index = null)
        {
            var insertedIndex = TranslateKeyIndex(index ?? KeyCount);
            if (!IsInsertableIndex(insertedIndex))
            {
                return false;
            }

            MoveKeysForwardStartingFrom(insertedIndex);

            Keys[insertedIndex] = key;
            KeyCount++;
            return true;
        }

        public bool SetKeyAtIndex(T key, int index, bool isNewKey = false)
        {
            index = TranslateKeyIndex(index);
            if (index > KeyCount)
            {
                return false;
            }

            Keys[index] = key;
            if (isNewKey)
            {
                KeyCount++;
            }
            return true;
        }

        public bool SetChildAtIndex(BTreeNode<T> child, int index)
        {
            index = TranslateChildIndex(index);
            if (index > ChildCount)
            {
                return false;
            }

            Children[index] = child;
            return true;
        }

        public bool RemoveKeyAtIndex(int index)
        {
            index = TranslateKeyIndex(index);
            if (index >= KeyCount)
            {
                return false;
            }

            MoveKeysBackwardStartingFrom(index + 1);

            KeyCount--;
            return true;
        }

        public int LastKeyIndex()
        {
            return KeyCount - 1;
        }

        public int LastChildIndex()
        {
            return KeyCount;
        }

        public T KeyAt(int index)
        {
            index = TranslateKeyIndex(index);
            return Keys[index];
        }

        public T FirstKey()
        {
            return Keys[0];
        }

        public T LastKey()
        {
            return Keys[LastKeyIndex()];
        }

        public BTreeNode<T> ChildAt(int index)
        {
            index = TranslateChildIndex(index);
            return Children[index];
        }

        public BTreeNode<T> FirstChild()
        {
            return Children[0];
        }

        public BTreeNode<T> LastChild()
        {
            return Children[LastChildIndex()];
        }

        public void SplitChild(int childIndex)
        {
            var child = Children[childIndex];
            var newNode = new BTreeNode<T>(child.MinimumDegree, child.IsLeaf());

            var lastHalfKeysCount = newNode.CopyLastHalfKeysOf(child);
            if (!child.IsLeaf())
            {
                newNode.CopyLastHalfChildrenOf(child);
            }

            child.TruncateNumberOfKeysTo(lastHalfKeysCount);

            CreateSpaceForLastNewChild(childIndex);
            SetAsChildAtIndex(newNode, childIndex + 1);
            MoveKeysForwardStartingFrom(childIndex);
            SetAnotherNodeMiddleKeyInThisAtIndex(child, childIndex);

            KeyCount++;
        }

        public void FillChild(int childIndex)
        {
            if (childIndex != 0 && Children[childIndex - 1].KeyCount > MinKeyCount)
            {
                BorrowFromPrevious(childIndex);
            }
            else if (!IsLastChildIndex(childIndex) && Children[childIndex + 1].KeyCount > MinKeyCount)
            {
                BorrowFromNext(childIndex);
            }
            else
            {
                MergeWithChild(childIndex);
            }
        }

        public void MergeWithChild(int childIndex)
        {
            if (!IsLastChildIndex(childIndex))
            {
                MergeWithChildInternal(childIndex);
            }
            else
            {
                MergeWithChildInternal(childIndex - 1);
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

        private int TranslateChildIndex(int index)
        {
            if (ChildCount == 0) return index;

            while (index < 0)
            {
                index += ChildCount;
            }
            return index;
        }

        private bool IsInsertableIndex(int index)
        {
            return !IsFull() && index <= KeyCount;
        }

        private int CopyLastHalfKeysOf(BTreeNode<T> source)
        {
            var lastHalfKeysCount = source.MinimumDegree - 1;
            for (int i = 0; i < lastHalfKeysCount; i++)
            {
                var sourceKey = source.Keys[i + MinimumDegree];
                InsertKey(sourceKey);
            }
            return lastHalfKeysCount;
        }

        private void CopyLastHalfChildrenOf(BTreeNode<T> source)
        {
            var lastHalfChildrenCount = MinimumDegree;
            for (int i = 0; i < lastHalfChildrenCount; i++)
            {
                var sourceChild = source.Children[i + MinimumDegree];
                Children[i] = sourceChild;
            }
        }

        private void TruncateNumberOfKeysTo(int newKeyCount)
        {
            if (newKeyCount >= KeyCount)
            {
                return;
            }
            KeyCount = newKeyCount;
        }

        private void CreateSpaceForLastNewChild(int currentKeyIndex)
        {
            MoveChildrenForwardStartingFrom(currentKeyIndex + 1);
        }

        private void SetAsChildAtIndex(BTreeNode<T> newChild, int index)
        {
            Children[index] = newChild;
        }

        private void MoveKeysForwardStartingFrom(int startIndex)
        {
            for (int i = LastKeyIndex(); i >= startIndex; i--)
            {
                Keys[i + 1] = Keys[i];
            }
        }

        private void MoveKeysBackwardStartingFrom(int startIndex)
        {
            for (int i = startIndex; i < KeyCount; i++)
            {
                Keys[i - 1] = Keys[i];
            }
        }

        private void MoveAllKeysBackward()
        {
            MoveKeysBackwardStartingFrom(1);
        }

        private void MoveChildrenForwardStartingFrom(int startIndex)
        {
            for (int i = LastChildIndex(); i >= startIndex; i--)
            {
                Children[i + 1] = Children[i];
            }
        }

        private void MoveChildrenBackwardStartingFrom(int startIndex)
        {
            for (int i = startIndex; i < ChildCount; i++)
            {
                Children[i - 1] = Children[i];
            }
        }

        private void MoveAllChildrenForward()
        {
            MoveChildrenForwardStartingFrom(0);
        }

        private void MoveAllChildrenBackward()
        {
            MoveChildrenBackwardStartingFrom(1);
        }

        private void SetAnotherNodeMiddleKeyInThisAtIndex(BTreeNode<T> otherNode, int index)
        {
            var middleKeyIndex = otherNode.MinimumDegree - 1;
            Keys[index] = otherNode.Keys[middleKeyIndex];
        }

        private bool IsLastChildIndex(int index)
        {
            return index == LastChildIndex();
        }

        private void BorrowFromPrevious(int childIndex)
        {
            var child = Children[childIndex];
            var previousSibling = Children[childIndex - 1];

            child.InsertKey(Keys[childIndex - 1], 0);
            Keys[childIndex - 1] = previousSibling.LastKey();

            if (!child.IsLeaf())
            {
                child.MoveAllChildrenForward();
                child.Children[0] = previousSibling.LastChild();
            }

            previousSibling.KeyCount--;
        }

        private void BorrowFromNext(int childIndex)
        {
            var child = Children[childIndex];
            var nextSibling = Children[childIndex + 1];

            child.InsertKey(Keys[childIndex]);
            Keys[childIndex] = nextSibling.FirstKey();

            nextSibling.MoveAllKeysBackward();

            if (!nextSibling.IsLeaf())
            {
                child.Children[child.LastChildIndex() + 1] = nextSibling.FirstChild();
                nextSibling.MoveAllChildrenBackward();
            }

            nextSibling.KeyCount--;
        }

        private void MergeWithChildInternal(int childIndex)
        {
            MergeCurrentWithChild(childIndex);
            MergeNextSiblingWithChild(childIndex);

            RemoveChildAtIndex(childIndex + 1);
            RemoveKeyAtIndex(childIndex);
        }

        private void MergeCurrentWithChild(int childIndex)
        {
            var child = Children[childIndex];

            child.InsertKey(Keys[childIndex]);
        }

        private void MergeNextSiblingWithChild(int childIndex)
        {
            var child = Children[childIndex];
            var nextSibling = Children[childIndex + 1];

            for (int i = 0; i < nextSibling.KeyCount; i++)
            {
                child.InsertKey(nextSibling.Keys[i]);
            }

            if (!nextSibling.IsLeaf())
            {
                for (int i = 0; i < nextSibling.ChildCount; i++)
                {
                    child.Children[i + child.MinimumDegree] = nextSibling.Children[i];
                }
            }
        }

        private void RemoveChildAtIndex(int index)
        {
            MoveChildrenBackwardStartingFrom(index + 1);
        }
    }
}
