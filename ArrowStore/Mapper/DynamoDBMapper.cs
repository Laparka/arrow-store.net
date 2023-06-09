using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Query;
using ArrowStore.Records;
using AutoMapper;
using ThirdParty.Json.LitJson;

namespace ArrowStore.Mapper
{
    internal class DynamoDBMapper : IArrowStoreMapper
    {
        private readonly IMapper _mapper;
        private readonly IDictionary<Type, AttributesProjection> _readProjections;
        private readonly IDictionary<Type, AttributesProjection> _writeProjections;

        public DynamoDBMapper(IMapper mapper, IDictionary<Type, AttributesProjection> readProjections, IDictionary<Type, AttributesProjection> writeProjections)
        {
            _mapper = mapper;
            _readProjections = readProjections;
            _writeProjections = writeProjections;
        }

        public TRecord MapFromAttributes<TRecord>(Dictionary<string, AttributeValue> item)
        {
            return _mapper.Map<TRecord>(item);
        }

        public Dictionary<string, AttributeValue> MapToAttributes<TRecord>(TRecord record, IArrowStoreIndex recordId, QueryVariables queryVars)
        {
            var attributeValues = _mapper.Map<Dictionary<string, AttributeValue>>(record);
            var partitionKeys = recordId.GetPartitionKeys();
            foreach (var partitionKey in partitionKeys)
            {
                attributeValues[partitionKey.AttributeName] = new AttributeValue(partitionKey.AttributeValue);
            }

            return attributeValues;
        }

        public AttributeValue MapToAttributeValue(object value)
        {
            return _mapper.Map<AttributeValue>(value);
        }

        public AttributesProjection? GetReadProjection(Type type)
        {
            if (_readProjections.TryGetValue(type, out var result))
            {
                return result;
            }

            return null;
        }

        public AttributesProjection? GetWriteProjection(Type type)
        {
            if (_writeProjections.TryGetValue(type, out var result))
            {
                return result;
            }

            return null;
        }

        public Dictionary<string, AttributeValue>? MapToEvaluatedKeyAttributes(string? evaluatedKey, string decryptionKey)
        {
            if (string.IsNullOrEmpty(evaluatedKey) || string.IsNullOrEmpty(decryptionKey))
            {
                return null;
            }

            byte[] combinedBytes = Convert.FromBase64String(evaluatedKey);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(decryptionKey);
                // Extract IV from combined bytes
                byte[] iv = new byte[aesAlg.BlockSize / 8];
                byte[] encryptedBytes = new byte[combinedBytes.Length - iv.Length];
                Array.Copy(combinedBytes, iv, iv.Length);
                Array.Copy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                aesAlg.IV = iv;

                using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    using (var memoryStream = new MemoryStream(encryptedBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cryptoStream))
                            {
                                var text = reader.ReadToEnd();
                                return JsonMapper.ToObject<Dictionary<string, AttributeValue>>(text);
                            }
                        }
                    }
                }
            }
        }

        public string? MapFromEvaluatedKey(Dictionary<string, AttributeValue>? evaluatedKey, string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey) || evaluatedKey == null || evaluatedKey.Count == 0)
            {
                return null;
            }

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(encryptionKey);
                aesAlg.GenerateIV();

                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    var jsonBytes = Encoding.UTF8.GetBytes(JsonMapper.ToJson(evaluatedKey));
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(jsonBytes, 0, jsonBytes.Length);
                            cryptoStream.FlushFinalBlock();
                        }

                        byte[] encryptedBytes = memoryStream.ToArray();

                        // Combine IV and ciphertext into a single string
                        byte[] combinedBytes = new byte[aesAlg.IV.Length + encryptedBytes.Length];
                        Array.Copy(aesAlg.IV, 0, combinedBytes, 0, aesAlg.IV.Length);
                        Array.Copy(encryptedBytes, 0, combinedBytes, aesAlg.IV.Length, encryptedBytes.Length);

                        return Convert.ToBase64String(combinedBytes);
                    }
                }
            }
        }

        public Dictionary<string, AttributeValue> MapToKeyAttributes(IReadOnlyCollection<IPartitionKey> partitionKeys, QueryVariables queryVars)
        {
            var attributeValues = new Dictionary<string, AttributeValue>(2);
            foreach (var partitionKey in partitionKeys)
            {
                var nameAlias = queryVars.SetAttributeNameAlias(partitionKey.AttributeName);
                attributeValues[nameAlias] = new AttributeValue(partitionKey.AttributeValue);
            }

            return attributeValues;
        }
    }
}
