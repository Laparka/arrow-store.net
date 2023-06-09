using ArrowStore.Query;

namespace ArrowStore.Records
{
    public interface IPartitionKey
    {
        string AttributeName { get; }

        string AttributeValue { get; }

        ArrowStoreQueryOperator Operator { get; }
    }
}
