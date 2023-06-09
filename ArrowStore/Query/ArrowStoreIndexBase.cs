using System;
using System.Collections.Generic;
using ArrowStore.Records;

namespace ArrowStore.Query
{
    public abstract class ArrowStoreIndexBase<TRecord> : IArrowStoreIndex<TRecord> where TRecord : IArrowStoreRecord
    {
        public virtual string? IndexName => null;

        public Type GetRecordType()
        {
            return typeof(TRecord);
        }

        public abstract IReadOnlyCollection<IPartitionKey> GetPartitionKeys();
    }
}
