namespace ArrowStore.Expressions.Nodes
{
    public class InverseNode : IExpressionNode
    {
        public InverseNode(IExpressionNode body)
        {
            Body = body;
        }

        public IExpressionNode Body { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.Inverse;
    }
}
