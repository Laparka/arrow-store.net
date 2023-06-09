using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Config;
using ArrowStore.Expressions;
using ArrowStore.Expressions.Nodes;
using ArrowStore.Mapper;
using ArrowStore.Put;
using ArrowStore.Records;

namespace ArrowStore.Query
{
    internal class DynamoDBQueryBuilder<TRecord> : IArrowStoreQueryBuilder<TRecord> where TRecord : IArrowStoreRecord
    {
        private readonly IArrowStoreIndex<TRecord> _queryIndex;
        private readonly IArrowStoreConfiguration _configuration;
        private readonly IArrowStoreMapper _mapper;
        private readonly IExpressionTranspiler _transpiler;

        private readonly List<IExpressionNode> _predicates;

        private string? _exclusiveStartKey;
        private int _limit;
        private int _take;
        private bool _scanIndexFwd;

        public DynamoDBQueryBuilder(IArrowStoreIndex<TRecord> queryIndex, IArrowStoreConfiguration configuration, IArrowStoreMapper mapper, IExpressionTranspiler transpiler)
        {
            _queryIndex = queryIndex;
            _configuration = configuration;
            _mapper = mapper;
            _transpiler = transpiler;
            _predicates = new List<IExpressionNode>();
        }

        public IArrowStoreQueryBuilder<TRecord> Where(Expression<Func<TRecord, IArrowStorePredicateExtensions<TRecord>, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            _predicates.Add(WhereExpressionParser.Instance.Tokenize(predicate));
            return this;
        }

        public IArrowStoreQueryBuilder<TRecord> ExclusiveStartKey(string exclusiveStartKey)
        {
            _exclusiveStartKey = exclusiveStartKey;
            return this;
        }

        public IArrowStoreQueryBuilder<TRecord> Limit(int limit)
        {
            _limit = limit;
            return this;
        }

        public IArrowStoreQueryBuilder<TRecord> Take(int take)
        {
            _take = take;
            return this;
        }

        public IArrowStoreQueryBuilder<TRecord> ScanIndexForward(bool scanIndexForward)
        {
            _scanIndexFwd = scanIndexForward;
            return this;
        }

        public Task<ArrowStoreListResult<TRecord>> ListAsync()
        {
            return DoListAsync(false);
        }

        public Task<ArrowStoreListResult<TRecord>> ListConsistentAsync()
        {
            return DoListAsync(true);
        }

        private async Task<ArrowStoreListResult<TRecord>> DoListAsync(bool consistent)
        {
            var queryVars = new QueryVariables();
            var tableName = await _configuration.GetRecordTableNameAsync<TRecord>();

            var queryReq = new QueryRequest
            {
                TableName = tableName,
                IndexName = _queryIndex.IndexName,
                ConsistentRead = consistent,
                ScanIndexForward = _scanIndexFwd,
                KeyConditionExpression = _transpiler.GetKeyConditionExpression(_queryIndex, queryVars),
                ExclusiveStartKey = string.IsNullOrEmpty(_exclusiveStartKey) ? null : _mapper.MapToEvaluatedKeyAttributes(_exclusiveStartKey, _configuration.DecryptionKey)
            };

            if (_limit > 0)
            {
                queryReq.Limit = _limit;
            }

            var typeProjection = _mapper.GetReadProjection(typeof(TRecord));
            var filterExpressions = new string[_predicates.Count];
            for (var i = 0; i < _predicates.Count; i++)
            {
                var predicate = _predicates[i];
                if (_predicates.Count > 1 && predicate.NodeType != ExpressionNodeType.InBraces)
                {
                    predicate = new InBracesNode(predicate);
                }

                filterExpressions[i] = _transpiler.ToExpression(predicate, queryVars, typeProjection);
            }

            if (filterExpressions.Length != 0)
            {
                queryReq.FilterExpression = string.Join(" and ", filterExpressions);
            }
            
            queryReq.ProjectionExpression = _transpiler.GetProjectionExpression<TRecord>(queryVars, typeProjection);
            if (queryVars.AttributeNames.Count != 0)
            {
                queryReq.ExpressionAttributeNames = new Dictionary<string, string>(queryVars.AttributeNames);
            }

            if (queryVars.HasAttributeValues)
            {
                queryReq.ExpressionAttributeValues = queryVars.GetAttributeValues(_mapper);
            }

            var client = await _configuration.ResolveClientAsync();
            var items = new List<TRecord>();
            QueryResponse queryResp;
            do
            {
                queryResp = await client.QueryAsync(queryReq, CancellationToken.None);
                queryReq.ExclusiveStartKey = queryResp.LastEvaluatedKey;
                if (queryResp.Items.Count != 0)
                {
                    foreach (var item in queryResp.Items)
                    {
                        items.Add(_mapper.MapFromAttributes<TRecord>(item));
                        if (_take > 0 && items.Count >= _take)
                        {
                            break;
                        }
                    }
                }

            } while (queryResp.LastEvaluatedKey != null && queryResp.LastEvaluatedKey.Count != 0 && (_take <= 0 || items.Count < _take));

            return new ArrowStoreListResult<TRecord>(items, _mapper.MapFromEvaluatedKey(queryResp.LastEvaluatedKey, _configuration.EncryptionKey));
        }
    }
}
