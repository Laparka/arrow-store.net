using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace ArrowStore.Mapper
{
    public interface ITypeConverter
    {
        bool CanRead(Type type);

        bool CanWrite(Type type);

        Type GetTargetType();

        ValueTask<object> ReadAsync(IDictionary<string, AttributeValue> itemAttributes);

        ValueTask<IDictionary<string, AttributeValue>> WriteAsync(object item);
    }

    public interface IArrowStoreTypeConverter<T> : ITypeConverter
    {
        new ValueTask<T> ReadAsync(IDictionary<string, AttributeValue> itemAttributes);

        ValueTask<IDictionary<string, AttributeValue>> WriteAsync(T item);
    }
}
