using ArrowStore.Query;
using ArrowStore.Records;

namespace ArrowStore.Tests.Models
{
    public class UserId : ArrowStoreIndexBase<UserRecord>
    {
        private readonly string _tenantId;
        private readonly string _userId;

        public UserId(string tenantId, string userId)
        {
            _tenantId = tenantId;
            _userId = userId;
        }

        public override IReadOnlyCollection<IPartitionKey> GetPartitionKeys()
        {
            if (string.IsNullOrEmpty(_tenantId))
            {
                throw new ArgumentNullException(nameof(_tenantId));
            }

            if (string.IsNullOrEmpty(_userId))
            {
                throw new ArgumentNullException(nameof(_userId));
            }

            return new IPartitionKey[]
            {
                new DynamoDBPartitionKey("record_type", string.Join('#', _tenantId, "UserRecord")),
                new DynamoDBPartitionKey("record_id", _userId)
            };
        }

        public static string? ParseUserId(string? attributeValue)
        {
            return attributeValue;
        }

        public static string ParseTenantId(string attributeValue)
        {
            if (!string.IsNullOrEmpty(attributeValue))
            {
                var segments = attributeValue.Split('#');
                if (segments.Length > 1)
                {
                    return segments[0];
                }
            }

            throw new InvalidCastException("The Tenant ID is not found within the attribute value");
        }
    }
}
