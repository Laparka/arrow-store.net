using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Query;
using ArrowStore.Records;

namespace ArrowStore.Mapper
{
    public interface IArrowStoreMapper
    {
        TRecord MapFromAttributes<TRecord>(Dictionary<string, AttributeValue> item);

        Dictionary<string, AttributeValue> MapToAttributes<TRecord>(TRecord record, IArrowStoreIndex recordId, QueryVariables queryVars);

        AttributeValue MapToAttributeValue(object value);

        AttributesProjection? GetReadProjection(Type type);

        AttributesProjection? GetWriteProjection(Type type);

        Dictionary<string, AttributeValue>? MapToEvaluatedKeyAttributes(string? evaluatedKey, string decryptionKey);

        string? MapFromEvaluatedKey(Dictionary<string, AttributeValue>? evaluatedKey, string encryptionKey);

        Dictionary<string, AttributeValue> MapToKeyAttributes(IReadOnlyCollection<IPartitionKey> partitionKeys, QueryVariables queryVars);
    }
}
