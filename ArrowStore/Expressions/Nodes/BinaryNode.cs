namespace ArrowStore.Expressions.Nodes
{
    public class BinaryNode : IExpressionNode
    {
        public BinaryNode(IExpressionNode left, BinaryExpressionType binaryExpressionType, IExpressionNode right)
        {
            Left = left;
            BinaryExpressionType = binaryExpressionType;
            Right = right;
        }

        public IExpressionNode Left { get; }

        public BinaryExpressionType BinaryExpressionType { get; }

        public IExpressionNode Right { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.BinaryExpression;
    }
}
