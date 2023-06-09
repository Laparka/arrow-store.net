using System;
using System.Web;
using ArrowStore.Query;

namespace ArrowStore.Records
{
    public class DynamoDBPartitionKey : IPartitionKey
    {
        public DynamoDBPartitionKey(string name, string value): this(name, value, ArrowStoreQueryOperator.Equals)
        {
        }

        private DynamoDBPartitionKey(string name, string value, ArrowStoreQueryOperator queryOperator)
        {
            AttributeName = name;
            AttributeValue = value;
            Operator = queryOperator;
        }

        public string AttributeName { get; }

        public string AttributeValue { get; }

        public ArrowStoreQueryOperator Operator { get; }

        public static string Composite(params string[] segments)
        {
            if (segments == null || segments.Length == 0)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            var resultSegments = new string[segments.Length];
            for (var i = 0; i < segments.Length; i++)
            {
                resultSegments[i] = HttpUtility.UrlEncode(segments[i]);
            }

            return string.Join('#', resultSegments);
        }
    }
}
