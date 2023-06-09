using Amazon.DynamoDBv2.Model;
using ArrowStore.Mapper;
using ArrowStore.Tests.Models;

namespace ArrowStore.Tests
{
    internal class UserMappingProfile : IMappingProfile
    {
        public void Configure(IConverterBuilder builder)
        {
            builder
                .CreateCustomMap<UserStatus, AttributeValue>()
                .ConvertUsing(status => new AttributeValue(status.ToString("G")));
            builder
                .CreateCustomMap<AttributeValue, UserStatus>()
                .ConvertUsing(value => Enum.Parse<UserStatus>(value.S));
            builder
                .CreateCustomMap<AttributeValue, IdDocumentType>()
                .ConvertUsing(value => Enum.Parse<IdDocumentType>(value.S));
            builder
                .CreateCustomMap<IdDocumentType, AttributeValue>()
                .ConvertUsing(value => new AttributeValue { S = value.ToString("G") });

            builder
                .ReadFromAttributes<UserIdentityInfo>()
                .Required(target => target.Type, r => r.MapFrom("document_type"));
            builder
                .WriteToAttributes<UserIdentityInfo>()
                .From(source => source.Type, "document_type");

            builder
                .ReadFromAttributes<UserRecord>()
                .Required(target => target.UserName, r => r.MapFrom("record_data.user.name"))
                .Optional(target => target.Status, r => r.MapFrom("record_data.user.status"))
                .Required(target => target.LoginHistory, r => r.MapFrom("record_data.login_history"))
                .Required(target => target.IdentityInfo, r => r.MapFrom("record_data.identity_info"))
                .Required(target => target.UserId, r => r.MapFrom("record_id", value => UserId.ParseUserId(value.S)))
                .Required(target => target.TenantId, r => r.MapFrom("record_type", value => UserId.ParseTenantId(value.S)));

            builder
                .WriteToAttributes<UserRecord>()
                .PartitionReserved(r => r.UserId, "record_id")
                .PartitionReserved(r => r.TenantId, "record_type_id")
                .From(r => r.UserName, "record_data.user.name")
                .From(r => r.Status, "record_data.user.status")
                .From(r => r.LoginHistory, "record_data.login_history")
                .From(r => r.IdentityInfo, "record_data.identity_info")
                .From(r => r.Permissions, "record_data.permissions")
                .From(r => r.FailedAuth, "record_data.failed_auth_attempts");


            builder.AddProfile(new PrimitiveMappingProfile());
        }
    }
}
