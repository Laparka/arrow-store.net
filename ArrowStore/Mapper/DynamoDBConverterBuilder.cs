using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AutoMapper;

namespace ArrowStore.Mapper
{
    public class DynamoDBConverterBuilder : IConverterBuilder
    {
        private readonly MapperConfigurationExpression _mapperExpression;
        private readonly IDictionary<Type, AttributesProjection> _readProjections;
        private readonly IDictionary<Type, AttributesProjection> _writeProjections;

        public DynamoDBConverterBuilder()
        {
            _mapperExpression = new MapperConfigurationExpression();
            _readProjections = new Dictionary<Type, AttributesProjection>();
            _writeProjections = new Dictionary<Type, AttributesProjection>();
        }

        public ICustomTypeMappingExpression<TSource, TDestination> CreateCustomMap<TSource, TDestination>()
        {
            return new AutoMapperMappingExpressionProxy<TSource, TDestination>(_mapperExpression.CreateMap<TSource, TDestination>());
        }

        public IFromAttributesMapBuilder<TRecord> ReadFromAttributes<TRecord>()
        {
            if (!_readProjections.TryGetValue(typeof(TRecord), out var attributesProjection))
            {
                attributesProjection = _readProjections[typeof(TRecord)] = new AttributesProjection();
            }

            var mappingExp = _mapperExpression.CreateMap<Dictionary<string, AttributeValue>, TRecord>();
            return new FromAttributesMapBuilder<TRecord>(mappingExp, attributesProjection, _readProjections);
        }

        public IToAttributesMapBuilder<TRecord> WriteToAttributes<TRecord>()
        {
            _mapperExpression
                .CreateMap<TRecord, AttributeValue>()
                .ConvertUsing((record, _, ctx) => WriteToNestedAttribute(record, ctx));
            if (!_writeProjections.TryGetValue(typeof(TRecord), out var projection))
            {
                projection = _writeProjections[typeof(TRecord)] = new AttributesProjection();
            }

            return new ToAttributesMapBuilder<TRecord>(_mapperExpression.CreateMap<TRecord, IDictionary<string, AttributeValue>>(), projection, _writeProjections);
        }

        public void AddProfile(IMappingProfile profile)
        {
            profile.Configure(this);
        }

        public IArrowStoreMapper Build()
        {
            var config = new MapperConfiguration(_mapperExpression);
            return new DynamoDBMapper(config.CreateMapper(), _readProjections, _writeProjections);
        }

        private AttributeValue WriteToNestedAttribute<TRecord>(TRecord record, ResolutionContext context)
        {
            if (record == null)
            {
                return new AttributeValue { NULL = true };
            }

            return new AttributeValue
            {
                M = context.Mapper.Map<Dictionary<string, AttributeValue>>(record)
            };
        }
    }

    public class AutoMapperMappingExpressionProxy<TSource, TDestination> : ICustomTypeMappingExpression<TSource, TDestination>
    {
        private readonly IMappingExpression<TSource, TDestination> _createMap;

        public AutoMapperMappingExpressionProxy(IMappingExpression<TSource, TDestination> createMap)
        {
            _createMap = createMap;
        }
        public void ConvertUsing(Func<TSource, TDestination> mappingExpression)
        {
            _createMap.ConvertUsing((source, destination, ctx) => mappingExpression(source));
        }
    }
}
