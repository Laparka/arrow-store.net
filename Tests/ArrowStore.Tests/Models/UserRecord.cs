using ArrowStore.Query;
using ArrowStore.Records;

namespace ArrowStore.Tests.Models
{
    public class UserRecord : IArrowStoreRecord
    {
        public string TenantId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public UserStatus Status { get; set; }

        public DateTime[] LoginHistory { get; set; }

        public string[] Permissions { get; set; }

        public UserIdentityInfo IdentityInfo { get; set; }

        public int FailedAuth { get; set; }

        IArrowStoreIndex IArrowStoreRecord.GetRecordId()
        {
            return new UserId(TenantId, UserId);
        }
    }
}
