namespace ArrowStore.Expressions.Nodes
{
    public class ConstantNode : IExpressionNode
    {
        public ConstantNode(object? value)
        {
            Value = value;
        }

        public object? Value { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.Constant;
    }
}
