using ArrowStore.Query;
using ArrowStore.Records;

namespace ArrowStore.Tests.Models
{
    public class UsersQueryIndex : ArrowStoreIndexBase<UserRecord>
    {
        private readonly string _tenantId;
        public UsersQueryIndex(string tenantId)
        {
            _tenantId = tenantId;
        }

        public override IReadOnlyCollection<IPartitionKey> GetPartitionKeys()
        {
            if (string.IsNullOrEmpty(_tenantId))
            {
                throw new ArgumentNullException(nameof(_tenantId));
            }

            return new IPartitionKey[]
            {
                new DynamoDBPartitionKey("RecordTypeId", DynamoDBPartitionKey.Composite("UserRecord", _tenantId))
            };
        }
    }
}
