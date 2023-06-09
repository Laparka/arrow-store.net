namespace ArrowStore.Expressions.Nodes
{
    public class CompareOperatorNode : IExpressionNode
    {
        public CompareOperatorNode(IExpressionNode left, CompareOperator @operator, IExpressionNode right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }

        public IExpressionNode Left { get; }

        public CompareOperator Operator { get; }

        public IExpressionNode Right { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.Compare;
    }
}
