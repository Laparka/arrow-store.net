<p align="center">
	<a href="https://www.nuget.org/packages/ArrowStore/"><img src="arrow-store.png" width="150" /></a>
</p>

<h1 align="center">ArrowStore</h1>

# Introduction
Welcome to the documentation for ArrowStore, an Object-Relational Mapping (ORM) library for DynamoDB in .NET Core. ArrowStore simplifies the process of accessing and manipulating data in DynamoDB by providing a convenient API and leveraging the power of Linq expressions.

## Purpose
The main goal of ArrowStore is to enable developers to seamlessly transition from traditional SQL databases and Entity Framework to DynamoDB. With ArrowStore, you can utilize familiar Linq expressions to query, put, update, and delete data in DynamoDB, eliminating the need to work directly with AttributeValue maps and low-level DynamoDB requests.

## Features
* Simplified API: ArrowStore provides a straightforward API that abstracts away the complexities of working with DynamoDB, allowing you to focus on your application logic.
* Linq Expression Support: Leverage the power of Linq expressions to build expressive and type-safe queries for DynamoDB.
* Automatic Mapping: ArrowStore automatically converts your POCO (Plain Old CLR Object) classes to and from the AttributeValue maps required by DynamoDB, saving you time and effort.
* Custom Mapping: Register custom mappings for primitive types and define mappings for complex POCO objects, ensuring seamless conversion between your domain model and DynamoDB.

## Getting Started
To start using ArrowStore in your .NET Core projects, you need to install the ArrowStore NuGet package. Follow the steps below to add the package to your project:

# Installation
To use ArrowStore in your .NET Core projects, you need to install the ArrowStore NuGet package. Follow the steps below to add the package to your project:

```
dotnet add package ArrowStore --version 1.0.0
```

Once the package is installed, you can start using ArrowStore in your code by importing the necessary namespaces and configuring the library, as explained in the next sections.

# Configuration
To configure mappings and implement the IArrowStoreConfiguration interface, follow the steps below:

## Define the Mappings:
* The library utilizes a mapping mechanism to convert between POCO objects and Dictionary<string, AttributeValue> used by DynamoDB. The mapping registration is done using the DynamoDBConverterBuilder class, which implements the IConverterBuilder interface.

### Registering Custom Maps for Primitive Types:
If you need to map primitive types (e.g., string, decimal) to and from AttributeValue, you can use the CreateCustomMap method of the DynamoDBConverterBuilder. This allows you to define custom conversions for specific types.

### Mapping POCO Objects:
To map your POCO objects, you can use the ReadFromAttributes and WriteToAttributes methods of the DynamoDBConverterBuilder. These methods allow you to specify mappings from Dictionary<string, AttributeValue> to your object type and vice versa.
Example:
```
class UserMappingProfile : IMappingProfile
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
```

### Implement the IArrowStoreConfiguration Interface:
To integrate the ArrowStore library into your project, you need to implement the IArrowStoreConfiguration interface. This interface provides methods to configure the ArrowStore library and resolve the initialized AmazonDynamoDBClient instance.

```
GetRecordTableNameAsync<TRecord>
```
Implement this method to provide the table name for the given record type TRecord. It allows the library to determine the appropriate table name for each record type.

```
GetRecordTableNameAsync(Type recordType):
```
Implement this method to provide the table name for a given recordType. It is an overload of the previous method that accepts a Type parameter.

```
ResolveClientAsync
```
Implement this method to return the configured AmazonDynamoDBClient instance initialized with the AWS credentials. You can use this method to create and return the appropriate AmazonDynamoDBClient instance.

```
EncryptionKey and DecryptionKey
```
These properties allow you to specify the AES encryption and decryption keys in Base64 format. They are used to encrypt and decrypt the LastEvaluatedKey when working with DynamoDB Query requests.

Example:
```
class MyConfig : IArrowStoreConfiguration
{
    private static string _EncryptionKey;
    public ValueTask<string> GetRecordTableNameAsync<TRecord>()
    {
        return GetRecordTableNameAsync(typeof(TRecord));
    }

    public ValueTask<string> GetRecordTableNameAsync(Type recordType)
    {
        var attributes = recordType.GetCustomAttributes(typeof(DynamoDBTableAttribute), false);
        if (attributes.Length == 0)
        {
            throw new InvalidOperationException("The DynamoDBTableAttribute was not found");
        }

        return new ValueTask<string>(((DynamoDBTableAttribute)attributes[0]).TableName);
    }

    public ValueTask<IAmazonDynamoDB> ResolveClientAsync()
    {
        return new ValueTask<IAmazonDynamoDB>(new AmazonDynamoDBClient(new EnvironmentVariablesAWSCredentials(), RegionEndpoint.USWest2));
    }

    public string EncryptionKey => GetEncryptionKey();
    public string DecryptionKey => GetEncryptionKey();

    private static string GetEncryptionKey()
    {
        if (!string.IsNullOrEmpty(_EncryptionKey))
        {
            return _EncryptionKey;
        }

        using var deriveBytes = new Rfc2898DeriveBytes("mujhe", Encoding.UTF8.GetBytes("passand-he"), 10000);
        return _EncryptionKey = Convert.ToBase64String(deriveBytes.GetBytes(32));
    }
}
```

## Integration with ArrowStore:
Once you have implemented the IArrowStoreConfiguration interface, you can use it to initialize the ArrowStore library. The ArrowStoreDynamoDBService class implements the IArrowStoreDynamoDBService interface and acts as the entry point for interacting with DynamoDB.

### Initialize the ArrowStoreDynamoDBService:
Create an instance of the ArrowStoreDynamoDBService by passing your implemented IArrowStoreConfiguration instance, IArrowStoreMapper, and IExpressionTranspiler to its constructor.

Example:
```
var mapBuilder = new DynamoDBConverterBuilder();
mapBuilder.AddProfile(new UserMappingProfile());
var mapper = mapBuilder.Build();
var config = new MyConfig();
var transpiler = new DynamoDBExpressionTranspiler();
var arrowStoreClient = new ArrowStoreDynamoDBService(config, mapper, transpiler); 
```
### Use the ArrowStoreDynamoDBService:
With the initialized ArrowStoreDynamoDBService, you can now perform DynamoDB operations such as Query, Put, Update, and Delete using the simplified API provided by the ArrowStore library.

By following these steps, you can configure the mappings for your POCO objects and implement the IArrowStoreConfiguration interface to integrate the ArrowStore library into your project.

# License

This project is licensed under the [MIT License](LICENSE).