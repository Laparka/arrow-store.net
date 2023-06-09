using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Expressions;
using AutoMapper;

namespace ArrowStore.Mapper
{
    internal class FromAttributesMapBuilder<TRecord> : IFromAttributesMapBuilder<TRecord>
    {
        private readonly IMappingExpression<Dictionary<string, AttributeValue>, TRecord> _builder;
        private readonly AttributesProjection _attributesProjection;
        private readonly IDictionary<Type, AttributesProjection> _typesProjections;

        public FromAttributesMapBuilder(IMappingExpression<Dictionary<string, AttributeValue>, TRecord> builder,
            AttributesProjection attributesProjection,
            IDictionary<Type, AttributesProjection> typesProjections)
        {
            _builder = builder;
            _attributesProjection = attributesProjection;
            _typesProjections = typesProjections;
        }

        public IFromAttributesMapBuilder<TRecord> Required<TMember>(
            Expression<Func<TRecord, TMember>> targetMemberAccessor,
            Action<IFromAttributeConverterBuilder<TMember>> converterBuilder)
        {
            var converter = new DynamoDBFromAttributeConverterBuilder<TMember>(this, targetMemberAccessor, true);
            converterBuilder(converter);
            return this;
        }

        public IFromAttributesMapBuilder<TRecord> Optional<TMember>(
            Expression<Func<TRecord, TMember>> targetMemberAccessor,
            Action<IFromAttributeConverterBuilder<TMember>> converterBuilder)
        {
            var converter = new DynamoDBFromAttributeConverterBuilder<TMember>(this, targetMemberAccessor, false);
            converterBuilder(converter);
            return this;
        }

        private class DynamoDBFromAttributeConverterBuilder<TMember> : IFromAttributeConverterBuilder<TMember>
        {
            private readonly FromAttributesMapBuilder<TRecord> _recordBuilder;
            private readonly Expression<Func<TRecord, TMember>> _targetMemberAccessor;
            private readonly bool _mustExist;

            public DynamoDBFromAttributeConverterBuilder(
                FromAttributesMapBuilder<TRecord> recordBuilder,
                Expression<Func<TRecord, TMember>> targetMemberAccessor,
                bool mustExist)
            {
                _recordBuilder = recordBuilder;
                _targetMemberAccessor = targetMemberAccessor;
                _mustExist = mustExist;
            }

            public void MapFrom(string attributePath)
            {
                AddReferences(attributePath);
                _recordBuilder._builder.ForMember(_targetMemberAccessor,
                    r => r.MapFrom((source, record, member, context) => Map(source, member, context, attributePath)));
            }

            public void MapFrom(string attributePath, Func<AttributeValue, TMember> convert)
            {
                AddReferences(attributePath);
                _recordBuilder._builder.ForMember(_targetMemberAccessor,
                    r => r.MapFrom(source => DoConvert(convert, GetAttributeValue(source, attributePath))));
            }

            private void AddReferences(string attributePath)
            {
                if (!_recordBuilder._typesProjections.TryGetValue(typeof(TMember), out var memberProjection))
                {
                    memberProjection = _recordBuilder._typesProjections[typeof(TMember)] = AttributesProjection.Empty();
                }

                var property = WhereExpressionParser.Instance.EnsurePropertyName(_targetMemberAccessor);
                _recordBuilder._attributesProjection.TypeAttributeNameReferences[property.MemberName] = new AttributeNameReference(attributePath, typeof(TMember), memberProjection.TypeAttributeNameReferences);
                FillHierarchy(_recordBuilder._attributesProjection.TypeAttributesHierarchy, attributePath, memberProjection.TypeAttributesHierarchy);
            }

            private void FillHierarchy(IDictionary<string, AttributesHierarchy> hierarchy, string attributePath, IDictionary<string, AttributesHierarchy> nestedTypeHierarchy)
            {
                var segments = attributePath.Split('.');
                for (var i = 0; i < segments.Length; i++)
                {
                    var segment = segments[i];
                    if (!hierarchy.TryGetValue(segment, out var property))
                    {
                        hierarchy[segment] = property = new AttributesHierarchy();
                    }

                    if (i < segments.Length - 1)
                    {
                        hierarchy = property.Nested;
                    }
                    else
                    {
                        property.ReplaceNested(nestedTypeHierarchy);
                    }
                }
            }

            private TMember Map(Dictionary<string, AttributeValue> source, TMember member, ResolutionContext context, string attributePath)
            {
                var attributeValue = GetAttributeValue(source, attributePath);
                if (attributeValue != null)
                {
                    if (attributeValue.IsMSet)
                    {
                        return context.Mapper.Map<TMember>(attributeValue.M);
                    }

                    return context.Mapper.Map<TMember>(attributeValue);
                }

                return member;
            }

            private AttributeValue? GetAttributeValue(IDictionary<string, AttributeValue> source, string attributePath)
            {
                var segments = attributePath.Split('.');
                AttributeValue? attributeValue = null;
                foreach (var segment in segments)
                {
                    if (!source.TryGetValue(segment, out attributeValue))
                    {
                        break;
                    }

                    source = attributeValue.M;
                }

                if (attributeValue == null)
                {
                    if (_mustExist)
                    {
                        throw new InvalidCastException($"The attribute name was not found: {attributePath}");
                    }
                }

                return attributeValue;
            }

            private static TMember DoConvert(Func<AttributeValue, TMember> converter, AttributeValue? attributeValue)
            {
                if (attributeValue == null)
                {
                    return default;
                }

                return converter(attributeValue);
            }
        }
    }
}