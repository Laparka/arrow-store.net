using System;

namespace ArrowStore.Expressions.Nodes
{
    public class RecordMemberAccessorNode : IExpressionNode
    {
        public RecordMemberAccessorNode(IExpressionNode instance, string memberName, Type memberType)
        {
            Instance = instance;
            MemberName = memberName;
            MemberType = memberType;
        }


        public IExpressionNode Instance { get; }

        public string MemberName { get; }

        public Type MemberType { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.RecordMemberAccessor;
    }
}
