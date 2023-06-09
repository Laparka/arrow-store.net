using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Config;
using ArrowStore.Expressions;
using ArrowStore.Expressions.Nodes;
using ArrowStore.Mapper;
using ArrowStore.Query;
using ArrowStore.Records;

namespace ArrowStore.Put
{
    public class DynamoDBPutBuilder<TRecord> : IArrowStorePutBuilder<TRecord> where TRecord : IArrowStoreRecord
    {
        private readonly TRecord _record;
        private readonly IArrowStoreConfiguration _configuration;
        private readonly IArrowStoreMapper _mapper;
        private readonly IExpressionTranspiler _transpiler;

        private readonly List<IExpressionNode> _conditions;

        public DynamoDBPutBuilder(TRecord record, IArrowStoreConfiguration configuration, IArrowStoreMapper mapper)
        {
            _record = record;
            _configuration = configuration;
            _mapper = mapper;
            _transpiler = new DynamoDBExpressionTranspiler();

            _conditions = new List<IExpressionNode>();
        }

        public IArrowStorePutBuilder<TRecord> When(Expression<Func<TRecord, IArrowStorePredicateExtensions<TRecord>, bool>> conditionPredicate)
        {
            if (conditionPredicate == null)
            {
                throw new ArgumentNullException(nameof(conditionPredicate));
            }

            _conditions.Add(WhereExpressionParser.Instance.Tokenize(conditionPredicate));
            return this;
        }

        public async Task<ArrowStoreWriteResult> ExecuteAsync()
        {
            var client = await _configuration.ResolveClientAsync();
            var putReq = new PutItemRequest
            {
                TableName = await _configuration.GetRecordTableNameAsync<TRecord>(),
                ReturnValues = ReturnValue.NONE
            };

            var queryVars = new QueryVariables();
            if (_conditions.Count != 0)
            {
                var writeProjection = _mapper.GetWriteProjection(typeof(TRecord));
                var conditionExpr = new string[_conditions.Count];
                for (var i = 0; i < _conditions.Count; i++)
                {
                    if (_conditions[i].NodeType != ExpressionNodeType.InBraces && _conditions.Count > 1)
                    {
                        _conditions[i] = new InBracesNode(_conditions[i]);
                    }

                    conditionExpr[i] = _transpiler.ToExpression(_conditions[i], queryVars, writeProjection);
                }

                putReq.ConditionExpression = string.Join(" and ", conditionExpr);
            }

            putReq.Item = _mapper.MapToAttributes(_record, _record.GetRecordId(), queryVars);
            if (queryVars.AttributeNames.Count != 0)
            {
                putReq.ExpressionAttributeNames = new Dictionary<string, string>(queryVars.AttributeNames);
            }

            if (queryVars.HasAttributeValues)
            {
                putReq.ExpressionAttributeValues = queryVars.GetAttributeValues(_mapper);
            }

            var putResp = await client.PutItemAsync(putReq, CancellationToken.None);
            return new ArrowStoreWriteResult();
        }
    }
}
