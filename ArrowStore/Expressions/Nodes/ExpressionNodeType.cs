namespace ArrowStore.Expressions.Nodes
{
    public enum ExpressionNodeType
    {
        Constant,
        PredicateExtension,
        Record,
        RecordMemberAccessor,
        Compare,
        BinaryExpression,
        MethodCall,
        InBraces,
        MemberExistsCondition,
        Inverse
    }
}
