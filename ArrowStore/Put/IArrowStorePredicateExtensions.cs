using System;
using System.Linq.Expressions;

namespace ArrowStore.Put
{
    public interface IArrowStorePredicateExtensions<T>
    {
        bool MemberExists(Expression<Func<T, object>> memberAccessor);
    }
}
