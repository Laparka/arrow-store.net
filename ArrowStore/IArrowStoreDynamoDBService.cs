using System.Threading.Tasks;
using ArrowStore.Delete;
using ArrowStore.Put;
using ArrowStore.Query;
using ArrowStore.Records;
using ArrowStore.Update;

namespace ArrowStore
{
    public interface IArrowStoreDynamoDBService
    {
        IArrowStoreQueryBuilder<TRecord> Query<TRecord>(IArrowStoreIndex<TRecord> queryIndex) where TRecord : IArrowStoreRecord;

        Task<TRecord> GetAsync<TRecord>(IArrowStoreIndex<TRecord> recordId) where TRecord : IArrowStoreRecord;

        Task<TRecord> GetConsistentAsync<TRecord>(IArrowStoreIndex<TRecord> recordId) where TRecord : IArrowStoreRecord;

        IArrowStorePutBuilder<TRecord> Put<TRecord>(TRecord record) where TRecord : IArrowStoreRecord;

        IArrowStoreDeleteBuilder<TRecord> Delete<TRecord>(IArrowStoreIndex<TRecord> recordId) where TRecord : IArrowStoreRecord;

        IArrowStoreUpdateBuilder<TRecord> Update<TRecord>(TRecord record) where TRecord : IArrowStoreRecord;

        IArrowStoreUpdateBuilder<TRecord> Update<TRecord>(IArrowStoreIndex<TRecord> recordId) where TRecord : IArrowStoreRecord;
    }
}