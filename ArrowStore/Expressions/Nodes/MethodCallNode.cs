using System;

namespace ArrowStore.Expressions.Nodes
{
    public class MethodCallNode : IExpressionNode
    {
        public MethodCallNode(IExpressionNode? instance, MethodCallType methodType, IExpressionNode[] args)
        {
            Instance = instance;
            Method = methodType;
            Args = args;
        }

        public IExpressionNode? Instance { get; }

        public MethodCallType Method { get; }

        public IExpressionNode[] Args { get; }

        public Type ReturnType => Method == MethodCallType.Size ? typeof(int) : typeof(bool);

        public ExpressionNodeType NodeType => ExpressionNodeType.MethodCall;
    }
}
