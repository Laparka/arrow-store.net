using ArrowStore.Query;

namespace ArrowStore.Records
{
    public interface IArrowStoreRecord
    {
        IArrowStoreIndex GetRecordId();
    }
}
