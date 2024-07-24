namespace TreeDSA.Core.AvlTree
{
    internal sealed class AvlTreeNode<T>
    {
        public T Value { get; set; }
        public AvlTreeNode<T>? Left { get; set; } = null;
        public AvlTreeNode<T>? Right { get; set; } = null;
        public int Height { get; set; } = 1;

        public AvlTreeNode(T value)
        {
            Value = value;
        }
    }
}
