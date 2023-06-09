using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArrowStore.Records;

namespace ArrowStore.Put
{
    public interface IArrowStorePutBuilder<TRecord> where TRecord : IArrowStoreRecord
    {
        IArrowStorePutBuilder<TRecord> When(Expression<Func<TRecord, IArrowStorePredicateExtensions<TRecord>, bool>> conditionPredicate);

        Task<ArrowStoreWriteResult> ExecuteAsync();
    }
}
