using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Expressions;
using ArrowStore.Expressions.Nodes;
using AutoMapper;

namespace ArrowStore.Mapper
{
    internal class ToAttributesMapBuilder<TRecord> : IToAttributesMapBuilder<TRecord>
    {
        private readonly AttributesProjection _projection;
        private readonly IDictionary<Type, AttributesProjection> _writeProjections;

        private readonly IDictionary<string, RecordMemberAccessorNode> _attributePropertyPath;

        public ToAttributesMapBuilder(IMappingExpression<TRecord, IDictionary<string, AttributeValue>> builder,
            AttributesProjection projection,
            IDictionary<Type, AttributesProjection> writeProjections)
        {
            _projection = projection;
            _writeProjections = writeProjections;

            _attributePropertyPath = new Dictionary<string, RecordMemberAccessorNode>();

            builder.ConvertUsing(Convert);
        }

        public IToAttributesMapBuilder<TRecord> From<TMember>(Expression<Func<TRecord, TMember>> getter, string targetAttributePath)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter));
            }

            if (string.IsNullOrEmpty(targetAttributePath))
            {
                throw new ArgumentNullException(nameof(targetAttributePath));
            }

            var propertyNode = WhereExpressionParser.Instance.EnsurePropertyName(getter);
            _attributePropertyPath[targetAttributePath] = propertyNode;
            AddReferences(targetAttributePath, propertyNode, typeof(TMember));
            return this;
        }

        public IToAttributesMapBuilder<TRecord> PartitionReserved<TMember>(Expression<Func<TRecord, TMember>> partitionRelatedProperty, string targetAttributePath)
        {
            if (partitionRelatedProperty == null)
            {
                throw new ArgumentNullException(nameof(partitionRelatedProperty));
            }

            if (string.IsNullOrEmpty(targetAttributePath))
            {
                throw new ArgumentNullException(nameof(targetAttributePath));
            }

            var propertyNode = WhereExpressionParser.Instance.EnsurePropertyName(partitionRelatedProperty);
            AddReferences(targetAttributePath, propertyNode, typeof(TMember));
            return this;
        }

        private IDictionary<string, AttributeValue> Convert(TRecord source, IDictionary<string, AttributeValue> target, ResolutionContext context)
        {
            var result = new Dictionary<string, AttributeValue>();
            foreach (var (attributePath, getter) in _attributePropertyPath)
            {
                var sourceValue = WhereExpressionParser.EvaluateMemberValue(typeof(TRecord), source, getter.MemberName, false);
                var attributeValue = sourceValue == null ? new AttributeValue { NULL = true } : context.Mapper.Map<AttributeValue>(sourceValue);

                SetAttributeValue(attributePath.Split('.'), result, attributeValue);
            }

            return result;
        }

        private void AddReferences(string attributePath, RecordMemberAccessorNode memberNode, Type memberType)
        {
            if (!_writeProjections.TryGetValue(memberType, out var memberProjection))
            {
                memberProjection = _writeProjections[memberType] = AttributesProjection.Empty();
            }

            _projection.TypeAttributeNameReferences[memberNode.MemberName] = new AttributeNameReference(attributePath, memberType, memberProjection.TypeAttributeNameReferences);
            FillHierarchy(_projection.TypeAttributesHierarchy, attributePath, memberProjection.TypeAttributesHierarchy);
        }

        private static void SetAttributeValue(string[] segments, IDictionary<string, AttributeValue> itemAttributes, AttributeValue value)
        {
            if (segments.Length == 1)
            {
                itemAttributes[segments[0]] = value;
            }
            else
            {
                for(var i = 0; i < segments.Length; i++)
                {
                    var name = segments[i];
                    if (i == segments.Length - 1)
                    {
                        itemAttributes[name] = value;
                    }
                    else
                    {
                        if (!itemAttributes.TryGetValue(name, out var attribute))
                        {
                            attribute = new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>()
                            };

                            itemAttributes[name] = attribute;
                        }

                        itemAttributes = attribute.M;
                    }
                }
            }
        }

        private static void FillHierarchy(IDictionary<string, AttributesHierarchy> hierarchy, string attributePath, IDictionary<string, AttributesHierarchy> nestedTypeHierarchy)
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
    }
}