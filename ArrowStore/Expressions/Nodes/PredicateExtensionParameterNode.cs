namespace ArrowStore.Expressions.Nodes
{
    public class PredicateExtensionParameterNode : IExpressionNode
    {
        public PredicateExtensionParameterNode(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.PredicateExtension;
    }
}
