using System;
using System.Collections.Generic;
using ArrowStore.Records;

namespace ArrowStore.Query
{
    public class ArrowStoreListResult<TRecord> where TRecord : IArrowStoreRecord
    {
        public ArrowStoreListResult(IReadOnlyCollection<TRecord> items, string? lastEvaluatedKey)
        {
            Items = items ?? Array.Empty<TRecord>();
            LastEvaluatedKey = lastEvaluatedKey;
        }

        public IReadOnlyCollection<TRecord> Items { get; }

        public string? LastEvaluatedKey { get; }
    }
}
