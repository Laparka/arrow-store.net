using System;
using System.Linq.Expressions;

namespace ArrowStore.Mapper
{
    public interface IFromAttributesMapBuilder<TRecord>
    {
        /// <summary>
        /// Registers a required member mapping. If the provided attribute name does not exist, the InvalidCastException is thrown indicating whether the Attribute name within the AttributeValue map is missing
        /// </summary>
        /// <typeparam name="TMember">The member type</typeparam>
        /// <param name="targetMemberAccessor">The type's member accessor path Linq-expression</param>
        /// <param name="converterBuilder">The mapping builder</param>
        /// <returns></returns>
        IFromAttributesMapBuilder<TRecord> Required<TMember>(Expression<Func<TRecord, TMember>> targetMemberAccessor, Action<IFromAttributeConverterBuilder<TMember>> converterBuilder);

        /// <summary>
        /// Registers an optional member mapping. If the provided attribute name does not exist, the object's member will be filled with a default value
        /// </summary>
        /// <typeparam name="TMember">The member type</typeparam>
        /// <param name="targetMemberAccessor">The type's member accessor path Linq-expression</param>
        /// <param name="converterBuilder">The mapping builder</param>
        /// <returns></returns>
        IFromAttributesMapBuilder<TRecord> Optional<TMember>(Expression<Func<TRecord, TMember>> targetMemberAccessor, Action<IFromAttributeConverterBuilder<TMember>> converterBuilder);
    }
}