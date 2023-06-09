using System;
using ArrowStore.Records;
using System.Collections.Generic;

namespace ArrowStore.Query
{
    public interface IArrowStoreIndex
    {
        string? IndexName { get; }

        Type GetRecordType();

        IReadOnlyCollection<IPartitionKey> GetPartitionKeys();
    }
}
