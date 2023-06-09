using System;
using Amazon.DynamoDBv2.Model;

namespace ArrowStore.Mapper
{
    public interface  IFromAttributeConverterBuilder<in TMember>
    {
        /// <summary>
        /// Provides the attribute path within the AttributeValue map
        /// For example, the attribute path for this AttributeValue-structure will be "record_data.user_details.user_name":
        /// {
        ///     "record_data": {
        ///         "M": {
        ///             "user_details": {
        ///                 "M": {
        ///                     "user_name": {
        ///                         "S": "user007"
        ///                     }
        ///                 }
        ///             }
        ///         }
        ///     }
        /// }
        /// </summary>
        /// <param name="attributePath">The target attribute name within the AttributeValue</param>
        void MapFrom(string attributePath);

        /// <summary>
        /// Provides the attribute path within the AttributeValue map
        /// For example, the attribute path for this AttributeValue-structure will be "record_data.user_details.user_name":
        /// {
        ///     "record_data": {
        ///         "M": {
        ///             "user_details": {
        ///                 "M": {
        ///                     "user_name": {
        ///                         "S": "user007"
        ///                     }
        ///                 }
        ///             }
        ///         }
        ///     }
        /// }
        /// </summary>
        /// <param name="attributePath">The target attribute name within the AttributeValue</param>
        /// <param name="convert">A custom converter of the AttributeValue the to member type</param>
        void MapFrom(string attributePath, Func<AttributeValue, TMember> convert);
    }
}