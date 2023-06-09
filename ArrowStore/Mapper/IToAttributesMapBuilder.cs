using System;
using System.Linq.Expressions;

namespace ArrowStore.Mapper
{
    public interface IToAttributesMapBuilder<TRecord>
    {
        /// <summary>
        /// Describes a mapping of the record's member to DynamoDB AttributeValue with the given AttributeName-path
        /// </summary>
        /// <typeparam name="TMember">The record type</typeparam>
        /// <param name="getter">The property accessor</param>
        /// <param name="targetAttributePath">The target attribute path</param>
        IToAttributesMapBuilder<TRecord> From<TMember>(Expression<Func<TRecord, TMember>> getter, string targetAttributePath);

        /// <summary>
        /// Describes the property as a reserved for the partition key.
        /// The method does not use it in mapping, but marks it for condition expressions
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="partitionRelatedProperty">The property accessor</param>
        /// <param name="targetAttributePath">The target attribute path</param>
        /// <returns></returns>
        IToAttributesMapBuilder<TRecord> PartitionReserved<TMember>(Expression<Func<TRecord, TMember>> partitionRelatedProperty, string targetAttributePath);
    }
}