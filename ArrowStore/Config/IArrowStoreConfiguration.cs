using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;

namespace ArrowStore.Config
{
    public interface IArrowStoreConfiguration
    {
        /// <summary>
        /// Gets the table name for the given record type (TRecord)
        /// </summary>
        /// <typeparam name="TRecord">The object representation type of the item in DynamoDB</typeparam>
        /// <returns>The table name</returns>
        ValueTask<string> GetRecordTableNameAsync<TRecord>();

        /// <summary>
        /// Gets the table name for the given record type
        /// </summary>
        /// <param name="recordType">The object representation type of the item in DynamoDB</param>
        /// <returns></returns>
        ValueTask<string> GetRecordTableNameAsync(Type recordType);

        /// <summary>
        /// Returns the configured AmazonDynamoDBClient instance initialized with the AWS credentials
        /// </summary>
        ValueTask<IAmazonDynamoDB> ResolveClientAsync();

        /// <summary>
        /// The AES Encryption key in Base64 format to encrypt the LastEvaluatedKey when working with DynamoDB Query requests
        /// </summary>
        string EncryptionKey { get; }

        /// <summary>
        /// The AES Decryption key in Base64 format to decrypt the LastEvaluatedKey to Dictionary<string, AttributeValue> when working with DynamoDB Query requests
        /// </summary>
        string DecryptionKey { get; }
    }
}
