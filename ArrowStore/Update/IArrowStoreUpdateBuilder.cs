using ArrowStore.Put;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArrowStore.Update
{
    public interface IArrowStoreUpdateBuilder<TRecord>
    {
        IArrowStoreUpdateBuilder<TRecord> Set<TMember>(Expression<Func<TRecord, TMember>> memberAccessor, TMember value);

        IArrowStoreUpdateBuilder<TRecord> SetWhenNotExists<TMember>(Expression<Func<TRecord, TMember>> memberAccessor, TMember value);

        IArrowStoreUpdateBuilder<TRecord> Increase(Expression<Func<TRecord, long>> memberAccessor, long value);

        IArrowStoreUpdateBuilder<TRecord> Increase(Expression<Func<TRecord, short>> memberAccessor, short value);

        IArrowStoreUpdateBuilder<TRecord> Increase(Expression<Func<TRecord, int>> memberAccessor, int value);

        IArrowStoreUpdateBuilder<TRecord> Increase(Expression<Func<TRecord, decimal>> memberAccessor, decimal value);

        IArrowStoreUpdateBuilder<TRecord> AppendToList<TMember>(Expression<Func<TRecord, ICollection<TMember>>> memberAccessor, ICollection<TMember> value);

        IArrowStoreUpdateBuilder<TRecord> PrependToList<TMember>(Expression<Func<TRecord, ICollection<TMember>>> memberAccessor, ICollection<TMember> value);

        IArrowStoreUpdateBuilder<TRecord> DeleteFromList<TMember>(Expression<Func<TRecord, ICollection<TMember>>> memberAccessor, ICollection<TMember> value);

        IArrowStoreUpdateBuilder<TRecord> Remove<TMember>(Expression<Func<TRecord, TMember>> memberAccessor);

        IArrowStoreUpdateBuilder<TRecord> When(Expression<Func<TRecord, IArrowStorePredicateExtensions<TRecord>, bool>> conditionPredicate);

        Task<ArrowStoreUpdateResult> ExecuteAsync();
    }
}