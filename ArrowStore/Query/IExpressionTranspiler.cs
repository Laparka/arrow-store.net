using ArrowStore.Expressions.Nodes;
using ArrowStore.Mapper;
using ArrowStore.Records;

namespace ArrowStore.Query
{
    public interface IExpressionTranspiler
    {
        string ToExpression(IExpressionNode expressionNode, QueryVariables vars, AttributesProjection? projection);

        string? GetProjectionExpression<TRecord>(QueryVariables queryVars, AttributesProjection? projection);
        
        string GetKeyConditionExpression<TRecord>(IArrowStoreIndex<TRecord> queryIndex, QueryVariables queryVars) where TRecord : IArrowStoreRecord;
    }
}