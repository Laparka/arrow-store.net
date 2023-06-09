using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Config;
using ArrowStore.Mapper;
using ArrowStore.Query;
using ArrowStore.Tests.Models;
using AutoFixture.Xunit2;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace ArrowStore.Tests
{
    public class ArrowStoreDynamoDBServiceTests
    {
        [Theory, AutoMoq]
        public async Task BuildsQueryRequest(string tenantId,
            string password,
            string salt,
            [Frozen] Mock<IArrowStoreConfiguration> config,
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000);
            var aesKey = Convert.ToBase64String(deriveBytes.GetBytes(32));
            config
                .SetupGet(x => x.EncryptionKey)
                .Returns(aesKey);
            config
                .SetupGet(x => x.DecryptionKey)
                .Returns(aesKey);
            client
                .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new QueryResponse
                {
                    Items = new List<Dictionary<string, AttributeValue>>
                    {
                        new()
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
                    },
                    LastEvaluatedKey = new Dictionary<string, AttributeValue>
                    {
                        ["record_type"] = new() { S = string.Join('#', "tenant-id", "UserRecord") },
                        ["record_id"] = new() { S = "user-id-2" }
                    }
                })
                .Verifiable();
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var service = new ArrowStoreDynamoDBService(config.Object, mapper, new DynamoDBExpressionTranspiler());
            var queryIndex = new UsersQueryIndex(tenantId);
            var result = await service
                .Query(queryIndex)
                .Where((user, _) => user.Status == UserStatus.Active)
                .Take(5)
                .Limit(100)
                .ScanIndexForward(true)
                .ListAsync();

            Assert.NotNull(result.LastEvaluatedKey);
            Assert.NotNull(result.Items);
            var lastEvaluatedKey = mapper.MapToEvaluatedKeyAttributes(result.LastEvaluatedKey, config.Object.DecryptionKey);
            Assert.NotNull(lastEvaluatedKey);
            Assert.Equal(2, lastEvaluatedKey.Count);
            Assert.Equal("record_type", lastEvaluatedKey.First().Key);
            Assert.Equal(string.Join('#', "tenant-id", "UserRecord"), lastEvaluatedKey.First().Value.S);
            Assert.Equal("record_id", lastEvaluatedKey.ElementAt(1).Key);
            Assert.Equal("user-id-2", lastEvaluatedKey.ElementAt(1).Value.S);
            client.Verify();

        }

        [Theory, AutoMoq]
        public async Task GetsSingleRecord(string tenantId, string userId, [Frozen] IArrowStoreConfiguration config,
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
            var service = new ArrowStoreDynamoDBService(config, mapper, new DynamoDBExpressionTranspiler());
            var record = await service.GetAsync(new UserId(tenantId, userId));

            Assert.NotNull(record);
            Assert.Equal(UserStatus.Active, record.Status);
            Assert.Equal("Lorem", record.UserName);
            Assert.Equal("user-id", record.UserId);
            Assert.Equal("tenant-id", record.TenantId);
            Assert.NotNull(record.IdentityInfo);
            Assert.Equal(IdDocumentType.DriverLicense, record.IdentityInfo.Type);
        }

        [Theory, AutoMoq]
        public async Task GetsSingleRecordConsistently(string tenantId, string userId, [Frozen] IArrowStoreConfiguration config,
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
                })
                .Verifiable();
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var service = new ArrowStoreDynamoDBService(config, mapper, new DynamoDBExpressionTranspiler());
            var record = await service.GetConsistentAsync(new UserId(tenantId, userId));

            Assert.NotNull(record);
            Assert.Equal(UserStatus.Active, record.Status);
            Assert.Equal("Lorem", record.UserName);
            Assert.Equal("user-id", record.UserId);
            Assert.Equal("tenant-id", record.TenantId);
            Assert.NotNull(record.IdentityInfo);
            Assert.Equal(IdDocumentType.DriverLicense, record.IdentityInfo.Type);
            client.Verify();
        }

        [Theory, AutoMoq]
        public async Task ReplacesItem(UserRecord record, [Frozen] IArrowStoreConfiguration config,
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            client
                .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new PutItemResponse())
                .Verifiable();
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var service = new ArrowStoreDynamoDBService(config, mapper, new DynamoDBExpressionTranspiler());
            var executionResult = await service
                .Put(record)
                .When((userRecord, recordCondition) => recordCondition.MemberExists(r => r.IdentityInfo.Type))
                .ExecuteAsync();

            client.Verify();
        }

        [Theory, AutoMoq]
        public async Task ReplacesItemWhenUserBlocked(UserRecord record, [Frozen] IArrowStoreConfiguration config,
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            client
                .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new PutItemResponse());
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var service = new ArrowStoreDynamoDBService(config, mapper, new DynamoDBExpressionTranspiler());
            var executionResult = await service
                .Put(record)
                .When((userRecord, _) => userRecord.Status == UserStatus.Blocked)
                .ExecuteAsync();
        }

        [Theory, AutoMoq]
        public async Task AddsItem(UserRecord record, [Frozen] IArrowStoreConfiguration config,
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            client
                .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new PutItemResponse())
                .Verifiable();
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var service = new ArrowStoreDynamoDBService(config, mapper, new DynamoDBExpressionTranspiler());
            var executionResult = await service
                .Put(record)
                .When((userRecord, recordCondition) => !recordCondition.MemberExists(r => r.UserId))
                .ExecuteAsync();

            client.Verify();
        }

        [Theory, AutoMoq]
        public async Task DeletesItem(string tenantId, string userId, [Frozen] IArrowStoreConfiguration config,
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            client
                .Setup(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new DeleteItemResponse())
                .Verifiable();
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var service = new ArrowStoreDynamoDBService(config, mapper, new DynamoDBExpressionTranspiler());
            var deleteResult = await service
                .Delete(new UserId(tenantId, userId))
                .ExecuteAsync();

            client.Verify();
        }

        [Theory, AutoMoq]
        public async Task DeletesItemWhenUserStatusBlocked(string tenantId, string userId, [Frozen] IArrowStoreConfiguration config,
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            client
                .Setup(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new DeleteItemResponse())
                .Verifiable();
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var service = new ArrowStoreDynamoDBService(config, mapper, new DynamoDBExpressionTranspiler());
            var deleteResult = await service
                .Delete(new UserId(tenantId, userId))
                .When((r, _) => r.Status == UserStatus.Blocked)
                .ExecuteAsync();

            client.Verify();
        }

        [Theory, AutoMoq]
        public async Task UpdatesExistingItem([Frozen] IArrowStoreConfiguration config,
            [Frozen] Mock<IAmazonDynamoDB> client,
            DynamoDBConverterBuilder builder)
        {
            client
                .Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new UpdateItemResponse());
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var service = new ArrowStoreDynamoDBService(config, mapper, new DynamoDBExpressionTranspiler());
            var updateResp = await service
                .Update(new UserId("tenant-id", "user-id"))
                .Set(x => x.Status, UserStatus.Blocked)
                .SetWhenNotExists(x => x.IdentityInfo.Type, IdDocumentType.Passport)
                .Increase(x => x.FailedAuth, -1)
                .AppendToList(x => x.LoginHistory, new[] { DateTime.UtcNow })
                .Remove(x => x.UserName)
                .DeleteFromList(x => x.Permissions, new[] { "write" })
                .When((record, ext) => record.Status == UserStatus.Active || ext.MemberExists(r => r.UserName))
                .ExecuteAsync();
        }
    }
}