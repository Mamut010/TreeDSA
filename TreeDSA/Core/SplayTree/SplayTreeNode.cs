namespace TreeDSA.Core.SplayTree
{
    internal sealed class SplayTreeNode<T>
    {
        public T Value { get; set; }
        public SplayTreeNode<T>? Left { get; set; } = null;
        public SplayTreeNode<T>? Right { get; set; } = null;

        public SplayTreeNode(T value)
        {
            Value = value;
        }
    }
}
