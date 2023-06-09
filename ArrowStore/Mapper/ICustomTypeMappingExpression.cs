using System;

namespace ArrowStore.Mapper
{
    public interface ICustomTypeMappingExpression<out TSource, in TDestination>
    {
        void ConvertUsing(Func<TSource, TDestination> mappingExpression);
    }
}
