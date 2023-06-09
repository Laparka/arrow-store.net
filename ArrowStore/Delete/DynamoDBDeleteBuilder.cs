using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using ArrowStore.Config;
using ArrowStore.Expressions;
using ArrowStore.Expressions.Nodes;
using ArrowStore.Put;
using ArrowStore.Query;
using ArrowStore.Records;
using ArrowStore.Mapper;

namespace ArrowStore.Delete
{
    internal class DynamoDBDeleteBuilder<TRecord> : IArrowStoreDeleteBuilder<TRecord> where TRecord : IArrowStoreRecord
    {
        private readonly IArrowStoreIndex _recordId;
        private readonly IArrowStoreConfiguration _configuration;
        private readonly IExpressionTranspiler _transpiler;
        private readonly IArrowStoreMapper _mapper;

        private readonly List<IExpressionNode> _conditions;

        public DynamoDBDeleteBuilder(IArrowStoreIndex recordId,
            IArrowStoreConfiguration configuration,
            IExpressionTranspiler transpiler,
            IArrowStoreMapper mapper)
        {
            _recordId = recordId;
            _configuration = configuration;
            _transpiler = transpiler;
            _mapper = mapper;
            _conditions = new List<IExpressionNode>();
        }

        public IArrowStoreDeleteBuilder<TRecord> When(Expression<Func<TRecord, IArrowStorePredicateExtensions<TRecord>, bool>> conditionPredicate)
        {
            _conditions.Add(WhereExpressionParser.Instance.Tokenize(conditionPredicate));
            return this;
        }

        public async Task<ArrowStoreWriteResult> ExecuteAsync()
        {
            var client = await _configuration.ResolveClientAsync();
            var deleteReq = new DeleteItemRequest
            {
                TableName = await _configuration.GetRecordTableNameAsync<TRecord>(),
                ReturnValues = ReturnValue.NONE
            };

            var queryVars = new QueryVariables();
            if (_conditions.Count != 0)
            {
                var readProjection = _mapper.GetReadProjection(typeof(TRecord));
                var conditionExpr = new string[_conditions.Count];
                for (var i = 0; i < _conditions.Count; i++)
                {
                    if (_conditions[i].NodeType != ExpressionNodeType.InBraces && _conditions.Count > 1)
                    {
                        _conditions[i] = new InBracesNode(_conditions[i]);
                    }

                    conditionExpr[i] = _transpiler.ToExpression(_conditions[i], queryVars, readProjection);
                }

                deleteReq.ConditionExpression = string.Join(" and ", conditionExpr);
            }

            deleteReq.Key = _mapper.MapToKeyAttributes(_recordId.GetPartitionKeys(), queryVars);
            if (queryVars.AttributeNames.Count != 0)
            {
                deleteReq.ExpressionAttributeNames = new Dictionary<string, string>(queryVars.AttributeNames);
            }

            if (queryVars.HasAttributeValues)
            {
                deleteReq.ExpressionAttributeValues = queryVars.GetAttributeValues(_mapper);
            }

            var deleteResp = await client.DeleteItemAsync(deleteReq, CancellationToken.None);
            return new ArrowStoreWriteResult();
        }
    }
}
