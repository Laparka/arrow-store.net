namespace ArrowStore.Expressions.Nodes
{
    public class InBracesNode : IExpressionNode
    {
        public InBracesNode(IExpressionNode body)
        {
            Body = body;
        }

        public IExpressionNode Body { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.InBraces;
    }
}
