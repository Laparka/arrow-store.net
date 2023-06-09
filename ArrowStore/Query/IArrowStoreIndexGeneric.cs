using ArrowStore.Records;

namespace ArrowStore.Query
{
    public interface IArrowStoreIndex<TRecord> : IArrowStoreIndex where TRecord : IArrowStoreRecord
    {
    }
}
