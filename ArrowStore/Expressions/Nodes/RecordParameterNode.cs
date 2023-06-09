using System;

namespace ArrowStore.Expressions.Nodes
{
    public class RecordParameterNode : IExpressionNode
    {
        public RecordParameterNode(string name, Type recordType)
        {
            Name = name;
            RecordType = recordType;
        }

        public string Name { get; }

        public Type RecordType { get; }

        public ExpressionNodeType NodeType => ExpressionNodeType.Record;
    }
}
