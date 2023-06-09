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
using ArrowStore.Mapper;
using ArrowStore.Put;
using ArrowStore.Query;

namespace ArrowStore.Update
{
    internal class ArrowStoreUpdateBuilder<TRecord> : IArrowStoreUpdateBuilder<TRecord>
    {
        private readonly IArrowStoreIndex _recordId;
        private readonly IArrowStoreMapper _mapper;
        private readonly IExpressionTranspiler _transpiler;
        private readonly IArrowStoreConfiguration _configuration;

        private readonly QueryVariables _queryVars;
        private readonly IDictionary<string, List<string>> _expressions;
        private readonly List<IExpressionNode> _conditions;

        public ArrowStoreUpdateBuilder(IArrowStoreIndex recordId, IArrowStoreMapper mapper, IExpressionTranspiler transpiler, IArrowStoreConfiguration configuration)
        {
            _recordId = recordId;
            _mapper = mapper;
            _transpiler = transpiler;
            _configuration = configuration;

            _queryVars = new QueryVariables();
            _expressions = new Dictionary<string, List<string>>();
            _conditions = new List<IExpressionNode>();
        }

        public IArrowStoreUpdateBuilder<TRecord> Set<TMember>(Expression<Func<TRecord, TMember>> memberAccessor, TMember value)
        {
            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            if (!_expressions.TryGetValue("SET", out var sets))
            {
                sets = new List<string>();
                _expressions["SET"] = sets;
            }

            var attrValueAlias = _queryVars.SetValueAlias(value, typeof(TMember));
            var writeProjection = _mapper.GetWriteProjection(typeof(TRecord));
            var attrPath = _transpiler.ToExpression(propertyAccessor, _queryVars, writeProjection);
            sets.Add($"{attrPath} = :{attrValueAlias}");
            return this;
        }

        public IArrowStoreUpdateBuilder<TRecord> SetWhenNotExists<TMember>(Expression<Func<TRecord, TMember>> memberAccessor, TMember value)
        {
            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            if (!_expressions.TryGetValue("SET", out var sets))
            {
                sets = new List<string>();
                _expressions["SET"] = sets;
            }

            var attrValueAlias = _queryVars.SetValueAlias(value, typeof(TMember));
            var writeProjection = _mapper.GetWriteProjection(typeof(TRecord));
            var attrPath = _transpiler.ToExpression(propertyAccessor, _queryVars, writeProjection);
            sets.Add($"{attrPath} = if_not_exists({attrPath}, :{attrValueAlias})");
            return this;
        }

        public IArrowStoreUpdateBuilder<TRecord> Increase(Expression<Func<TRecord, long>> memberAccessor, long value)
        {
            if (value == 0)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            var attrValue = _queryVars.SetValueAlias(Math.Abs(value));
            return Increase(propertyAccessor, attrValue, value > 0);
        }

        public IArrowStoreUpdateBuilder<TRecord> Increase(Expression<Func<TRecord, short>> memberAccessor, short value)
        {
            if (value == 0)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            var attrValue = _queryVars.SetValueAlias(Math.Abs(value));
            return Increase(propertyAccessor, attrValue, value > 0);
        }

        public IArrowStoreUpdateBuilder<TRecord> Increase(Expression<Func<TRecord, int>> memberAccessor, int value)
        {
            if (value == 0)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            var attrValue = _queryVars.SetValueAlias(Math.Abs(value));
            return Increase(propertyAccessor, attrValue, value > 0);
        }

        public IArrowStoreUpdateBuilder<TRecord> Increase(Expression<Func<TRecord, decimal>> memberAccessor, decimal value)
        {
            if (value == 0)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            var attrValue = _queryVars.SetValueAlias(Math.Abs(value));
            return Increase(propertyAccessor, attrValue, value > 0);
        }

        public IArrowStoreUpdateBuilder<TRecord> AppendToList<TMember>(Expression<Func<TRecord, ICollection<TMember>>> memberAccessor, ICollection<TMember> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Count == 0)
            {
                return this;
            }

            if (!_expressions.TryGetValue("SET", out var sets))
            {
                sets = new List<string>();
                _expressions["SET"] = sets;
            }

            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            var attrValue = _queryVars.SetValueAlias(value);

            var writeProjection = _mapper.GetWriteProjection(typeof(TRecord));
            var attrPath = _transpiler.ToExpression(propertyAccessor, _queryVars, writeProjection);
            sets.Add($"{attrPath} = list_append({attrPath}, :{attrValue})");
            return this;
        }

        public IArrowStoreUpdateBuilder<TRecord> PrependToList<TMember>(Expression<Func<TRecord, ICollection<TMember>>> memberAccessor, ICollection<TMember> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Count == 0)
            {
                return this;
            }

            if (!_expressions.TryGetValue("SET", out var sets))
            {
                sets = new List<string>();
                _expressions["SET"] = sets;
            }

            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            var attrValue = _queryVars.SetValueAlias(value);

            var writeProjection = _mapper.GetWriteProjection(typeof(TRecord));
            var attrPath = _transpiler.ToExpression(propertyAccessor, _queryVars, writeProjection);
            sets.Add($"{attrPath} = list_append(:{attrValue}, {attrPath})");
            return this;
        }

        public IArrowStoreUpdateBuilder<TRecord> DeleteFromList<TMember>(Expression<Func<TRecord, ICollection<TMember>>> memberAccessor, ICollection<TMember> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Count == 0)
            {
                return this;
            }

            if (!_expressions.TryGetValue("DELETE", out var deletes))
            {
                deletes = new List<string>();
                _expressions["DELETE"] = deletes;
            }

            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            var attrValue = _queryVars.SetValueAlias(value);

            var writeProjection = _mapper.GetWriteProjection(typeof(TRecord));
            var attrPath = _transpiler.ToExpression(propertyAccessor, _queryVars, writeProjection);
            deletes.Add($"{attrPath} :{attrValue})");
            return this;
        }

        public IArrowStoreUpdateBuilder<TRecord> Remove<TMember>(Expression<Func<TRecord, TMember>> memberAccessor)
        {
            if (!_expressions.TryGetValue("REMOVE", out var deletes))
            {
                deletes = new List<string>();
                _expressions["REMOVE"] = deletes;
            }

            var propertyAccessor = WhereExpressionParser.Instance.EnsurePropertyName(memberAccessor);
            var writeProjection = _mapper.GetWriteProjection(typeof(TRecord));
            var attrPath = _transpiler.ToExpression(propertyAccessor, _queryVars, writeProjection);
            deletes.Add(attrPath);
            return this;
        }

        public IArrowStoreUpdateBuilder<TRecord> When(Expression<Func<TRecord, IArrowStorePredicateExtensions<TRecord>, bool>> conditionPredicate)
        {
            if (conditionPredicate == null)
            {
                throw new ArgumentNullException(nameof(conditionPredicate));
            }

            _conditions.Add(WhereExpressionParser.Instance.Tokenize(conditionPredicate));
            return this;
        }

        public async Task<ArrowStoreUpdateResult> ExecuteAsync()
        {
            if (_expressions.Count == 0)
            {
                throw new InvalidOperationException("Nothing to update");
            }

            var client = await _configuration.ResolveClientAsync();
            var updateReq = new UpdateItemRequest
            {
                TableName = await _configuration.GetRecordTableNameAsync<TRecord>(),
                ReturnValues = ReturnValue.NONE,
                Key = _mapper.MapToKeyAttributes(_recordId.GetPartitionKeys(), _queryVars)
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

                updateReq.ConditionExpression = string.Join(" and ", conditionExpr);
            }

            var updateExpr = new List<string>(_expressions.Count);
            if (_expressions.TryGetValue("SET", out var sets))
            {
                updateExpr.Add($"SET {string.Join(", ", sets)}");
            }

            if (_expressions.TryGetValue("REMOVE", out var removes))
            {
                updateExpr.Add($"REMOVE {string.Join(", ", removes)}");
            }

            if (_expressions.TryGetValue("ADD", out var adds))
            {
                updateExpr.Add($"ADD {string.Join(", ", adds)}");
            }

            if (_expressions.TryGetValue("DELETE", out var deletes))
            {
                updateExpr.Add($"DELETE {string.Join(", ", deletes)}");
            }

            updateReq.UpdateExpression = string.Join(' ', updateExpr);

            if (_conditions.Count != 0)
            {
                var projection = _mapper.GetWriteProjection(typeof(TRecord));
                var conditionExp = new string[_conditions.Count];
                for(var i = 0; i < _conditions.Count; i++)
                {
                    conditionExp[i] = _transpiler.ToExpression(_conditions.Count > 1 ? new InBracesNode(_conditions[i]) : _conditions[i], _queryVars, projection);
                }

                updateReq.ConditionExpression = string.Join(" and ", conditionExp);
            }

            if (queryVars.AttributeNames.Count != 0)
            {
                updateReq.ExpressionAttributeNames = new Dictionary<string, string>(queryVars.AttributeNames);
            }

            if (queryVars.HasAttributeValues)
            {
                updateReq.ExpressionAttributeValues = queryVars.GetAttributeValues(_mapper);
            }

            var updateResp = await client.UpdateItemAsync(updateReq, CancellationToken.None);
            return new ArrowStoreUpdateResult();
        }

        private IArrowStoreUpdateBuilder<TRecord> Increase(RecordMemberAccessorNode propertyAccessor, string valueAlias, bool doIncrease)
        {
            var writeProjection = _mapper.GetWriteProjection(typeof(TRecord));
            var writeExpr = _transpiler.ToExpression(propertyAccessor, _queryVars, writeProjection);
            var attrAccessor = _queryVars.SetAttributeNameAlias(writeExpr);
            if (!_expressions.TryGetValue("SET", out var sets))
            {
                sets = new List<string>();
                _expressions["SET"] = sets;
            }

            sets.Add(doIncrease
                ? $"{attrAccessor} = {attrAccessor} + :{valueAlias}"
                : $"{attrAccessor} = {attrAccessor} - :{valueAlias}");

            return this;
        }
    }
}
