using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArrowStore.Put;
using ArrowStore.Records;

namespace ArrowStore.Query
{
    public interface IArrowStoreQueryBuilder<TRecord> where TRecord : IArrowStoreRecord
    {
        /// <summary>
        /// Query filter
        /// </summary>
        IArrowStoreQueryBuilder<TRecord> Where(Expression<Func<TRecord, IArrowStorePredicateExtensions<TRecord>, bool>> predicate);

        /// <summary>
        /// Starts querying from the given partition
        /// </summary>
        /// <param name="exclusiveStartKey">Encrypted partition key-values</param>
        IArrowStoreQueryBuilder<TRecord> ExclusiveStartKey(string exclusiveStartKey);

        /// <summary>
        /// Limits the query operation's read capacity
        /// </summary>
        /// <param name="limit">The Items count to return in a query's read capacity</param>
        IArrowStoreQueryBuilder<TRecord> Limit(int limit);

        /// <summary>
        /// Once the items count reaches the take-limit, the querying stops
        /// </summary>
        /// <param name="take">The Items count to stop processing the given partition</param>
        IArrowStoreQueryBuilder<TRecord> Take(int take);

        /// <summary>
        /// Scan index forward flag
        /// </summary>
        IArrowStoreQueryBuilder<TRecord> ScanIndexForward(bool scanIndexForward);

        /// <summary>
        /// Retrieves items
        /// </summary>
        Task<ArrowStoreListResult<TRecord>> ListAsync();

        /// <summary>
        /// Retrieves items from the consistent/primary storage node
        /// </summary>
        Task<ArrowStoreListResult<TRecord>> ListConsistentAsync();
    }
}
