namespace ArrowStore.Mapper
{
    public interface IConverterBuilder
    {
        /// <summary>
        /// Registers a custom map from the source type to a destination type.
        /// It is useful when working with primitive type mappings, like string => AttributeValue { S: } or decimal => AttributeValue { N: }
        /// </summary>
        /// <typeparam name="TSource">The source type to convert from</typeparam>
        /// <typeparam name="TDestination">The destination type to convert to</typeparam>
        /// <returns></returns>
        ICustomTypeMappingExpression<TSource, TDestination> CreateCustomMap<TSource, TDestination>();

        /// <summary>
        /// Registers a mapper that maps the Dictionary<string, AttributeValue> to the given record type
        /// </summary>
        /// <typeparam name="TRecord">The object type to convert from DynamoDB AttributeValue map</typeparam>
        /// <returns>The mapping builder</returns>
        IFromAttributesMapBuilder<TRecord> ReadFromAttributes<TRecord>();

        /// <summary>
        /// Registers a mapper that maps the object representation to AttributeValue map for DynamoDB
        /// </summary>
        /// <typeparam name="TRecord">The record type</typeparam>
        /// <returns>The mapping builder</returns>
        IToAttributesMapBuilder<TRecord> WriteToAttributes<TRecord>();

        /// <summary>
        /// Adds a mapping profile/module
        /// </summary>
        /// <param name="profile">The mapping profile</param>
        void AddProfile(IMappingProfile profile);
    }
}
