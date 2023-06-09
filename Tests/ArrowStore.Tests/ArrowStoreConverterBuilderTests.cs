using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Config;
using ArrowStore.Mapper;
using ArrowStore.Query;
using ArrowStore.Tests.Models;
using AutoFixture.Xunit2;
using Moq;

namespace ArrowStore.Tests
{
    public class ArrowStoreConverterBuilderTests
    {
        [Theory, AutoMoq]
        public async Task GetsMappedItem([Frozen] IArrowStoreConfiguration configuration, 
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            client
                .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetItemResponse
                {
                    Item = new Dictionary<string, AttributeValue>
                    {
                        ["record_type"] = new() { S = string.Join('#', "tenant-id", "UserRecord") },
                        ["record_id"] = new() { S = "user-id" },
                        ["record_data"] = new()
                        {
                            M = new Dictionary<string, AttributeValue>
                            {
                                ["user"] = new()
                                {
                                    M = new Dictionary<string, AttributeValue>
                                    {
                                        ["name"] = new() { S = "Lorem" },
                                        ["status"] = new() { S = UserStatus.Active.ToString("G") }
                                    }
                                },
                                ["login_history"] = new()
                                {
                                    SS = new List<string>
                                    {
                                        DateTime.UtcNow.ToString("O")
                                    }
                                },
                                ["identity_info"] = new()
                                {
                                    M = new Dictionary<string, AttributeValue>
                                    {
                                        ["document_type"] = new(IdDocumentType.DriverLicense.ToString("G"))
                                    }
                                }
                            }
                        }
                    }
                });

            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var dynamoDBService = new ArrowStoreDynamoDBService(configuration, mapper, new DynamoDBExpressionTranspiler());
            var userRecord = await dynamoDBService.GetAsync(new UserId("tenant-id", "user-id"));
            Assert.NotNull(userRecord);
            Assert.Equal("user-id", userRecord.UserId);
            Assert.Equal("tenant-id", userRecord.TenantId);
            Assert.Equal("Lorem", userRecord.UserName);
            Assert.Equal(UserStatus.Active, userRecord.Status);
            Assert.NotNull(userRecord.LoginHistory);
            Assert.Equal(1, userRecord.LoginHistory.Length);
        }

        [Theory, AutoMoq]
        public async Task PutsMappedItem([Frozen] IArrowStoreConfiguration configuration,
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            client
                .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
                .Callback<PutItemRequest, CancellationToken>(((request, token) =>
                {
                    Assert.NotNull(request);
                }))
                .ReturnsAsync(() => new PutItemResponse())
                .Verifiable();

            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();

            var dynamoDBService = new ArrowStoreDynamoDBService(configuration, mapper, new DynamoDBExpressionTranspiler());
            var putResult = await dynamoDBService.Put(new UserRecord
            {
                UserId = "user-id",
                Status = UserStatus.Active,
                UserName = "user-name",
                LoginHistory = new[] { DateTime.UtcNow },
                IdentityInfo = new UserIdentityInfo { Type = IdDocumentType.Passport },
                TenantId = "tenant-id"
            }).ExecuteAsync();

            client.Verify();
        }
    }
}
