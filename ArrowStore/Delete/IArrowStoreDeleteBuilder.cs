using ArrowStore.Put;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArrowStore.Delete
{
    public interface IArrowStoreDeleteBuilder<TRecord>
    {
        IArrowStoreDeleteBuilder<TRecord> When(Expression<Func<TRecord, IArrowStorePredicateExtensions<TRecord>, bool>> conditionPredicate);

        Task<ArrowStoreWriteResult> ExecuteAsync();
    }
}
