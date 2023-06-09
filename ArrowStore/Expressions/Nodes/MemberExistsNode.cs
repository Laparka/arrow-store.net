namespace ArrowStore.Expressions.Nodes
{
    public class MemberExistsNode : IExpressionNode
    {
        public MemberExistsNode(RecordMemberAccessorNode memberAccessor)
        {
            MemberAccessor = memberAccessor;
        }

        public RecordMemberAccessorNode MemberAccessor { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.MemberExistsCondition;
    }
}
