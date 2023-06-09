using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Config;
using ArrowStore.Delete;
using ArrowStore.Mapper;
using ArrowStore.Put;
using ArrowStore.Query;
using ArrowStore.Records;
using ArrowStore.Update;

[assembly:InternalsVisibleTo("ArrowStore.Tests")]

namespace ArrowStore
{
    public class ArrowStoreDynamoDBService : IArrowStoreDynamoDBService
    {
        private readonly IArrowStoreConfiguration _configuration;
        private readonly IArrowStoreMapper _mapper;
        private readonly IExpressionTranspiler _transpiler;

        public ArrowStoreDynamoDBService(IArrowStoreConfiguration config, IArrowStoreMapper mapper, IExpressionTranspiler transpiler)
        {
            _configuration = config;
            _mapper = mapper;
            _transpiler = transpiler;
        }

        public IArrowStoreQueryBuilder<TRecord> Query<TRecord>(IArrowStoreIndex<TRecord> queryIndex) where TRecord : IArrowStoreRecord
        {
            return new DynamoDBQueryBuilder<TRecord>(queryIndex, _configuration, _mapper, _transpiler);
        }

        public Task<TRecord> GetAsync<TRecord>(IArrowStoreIndex<TRecord> recordId) where TRecord : IArrowStoreRecord
        {
            return DoGetAsync(recordId, false);
        }

        public Task<TRecord> GetConsistentAsync<TRecord>(IArrowStoreIndex<TRecord> recordId) where TRecord : IArrowStoreRecord
        {
            return DoGetAsync(recordId, true);
        }

        public IArrowStorePutBuilder<TRecord> Put<TRecord>(TRecord record) where TRecord : IArrowStoreRecord
        {
            return new DynamoDBPutBuilder<TRecord>(record, _configuration, _mapper);
        }

        public IArrowStoreDeleteBuilder<TRecord> Delete<TRecord>(IArrowStoreIndex<TRecord> recordId) where TRecord : IArrowStoreRecord
        {
            return new DynamoDBDeleteBuilder<TRecord>(recordId, _configuration, _transpiler, _mapper);
        }

        public IArrowStoreUpdateBuilder<TRecord> Update<TRecord>(TRecord record) where TRecord : IArrowStoreRecord
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return new ArrowStoreUpdateBuilder<TRecord>(record.GetRecordId(), _mapper, _transpiler, _configuration);
        }

        public IArrowStoreUpdateBuilder<TRecord> Update<TRecord>(IArrowStoreIndex<TRecord> recordId) where TRecord : IArrowStoreRecord
        {
            if (recordId == null)
            {
                throw new ArgumentNullException(nameof(recordId));
            }

            return new ArrowStoreUpdateBuilder<TRecord>(recordId, _mapper, _transpiler, _configuration);
        }

        private async Task<TRecord> DoGetAsync<TRecord>(IArrowStoreIndex<TRecord> recordId, bool consistent) where TRecord : IArrowStoreRecord
        {
            if (recordId == null)
            {
                throw new ArgumentNullException(nameof(recordId));
            }

            var client = await _configuration.ResolveClientAsync();
            var queryVars = new QueryVariables();
            var readProjection = _mapper.GetReadProjection(typeof(TRecord));
            var getRequest = new GetItemRequest
            {
                TableName = await _configuration.GetRecordTableNameAsync<TRecord>(),
                ConsistentRead = consistent,
                ReturnConsumedCapacity = ReturnConsumedCapacity.NONE,
                Key = _mapper.MapToKeyAttributes(recordId.GetPartitionKeys(), queryVars),
                ProjectionExpression = _transpiler.GetProjectionExpression<TRecord>(queryVars, readProjection),
                ExpressionAttributeNames = new Dictionary<string, string>(queryVars.AttributeNames)
            };

            var getResponse = await client.GetItemAsync(getRequest, CancellationToken.None);
            if (!getResponse.IsItemSet)
            {
                return default!;
            }

            return _mapper.MapFromAttributes<TRecord>(getResponse.Item);
        }
    }
}
