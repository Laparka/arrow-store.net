using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using ArrowStore.Mapper;

namespace ArrowStore.Query
{
    public class QueryVariables
    {
        private const string _AttrNameFormat = "#attr_name_{0}";
        private const string _AttrValueFormat = ":attr_val_{0}";
        private const string _AttrNullValue = ":attr_val_null";

        private readonly Dictionary<string, string> _attributeNames;
        private readonly Dictionary<string, string> _attributeNameAliases;
        private readonly Dictionary<Type, IDictionary<IComparable, string>> _comparableValues;
        private readonly Dictionary<Type, IDictionary<object, string>> _rawValues;
        private readonly Dictionary<string, object> _allValues;
        private int _valuesCounter;

        public QueryVariables()
        {
            _attributeNames = new Dictionary<string, string>();
            _attributeNameAliases = new Dictionary<string, string>();
            _comparableValues = new Dictionary<Type, IDictionary<IComparable, string>>();
            _rawValues = new Dictionary<Type, IDictionary<object, string>>();
            _allValues = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the attribute names references,
        /// where the Key is the DynamoDB Attribute Name alias and the value is the object's property name
        /// </summary>
        public IReadOnlyDictionary<string, string> AttributeNames => _attributeNames;

        /// <summary>
        /// Gets the property name to attribute aliases references
        /// </summary>
        public IReadOnlyDictionary<string, string> AttributeNameAliases => _attributeNameAliases;

        public bool HasAttributeValues => _valuesCounter > 0;


        public string SetAttributeNameAlias(string attributeName)
        {
            if (!_attributeNameAliases.TryGetValue(attributeName, out var alias))
            {
                alias = string.Format(_AttrNameFormat, _attributeNames.Count);
                _attributeNameAliases[attributeName] = alias ;
                _attributeNames[alias] = attributeName;
            }

            return alias;
        }

        public string SetValueAlias(object? rawValue)
        {
            if (rawValue == null)
            {
                if (!_allValues.ContainsKey(_AttrNullValue))
                {
                    _allValues[_AttrNullValue] = new AttributeValue { NULL = true };
                }

                return _AttrNullValue;
            }

            var type = rawValue.GetType();
            return SetValueAlias(rawValue, type);
        }

        public string SetValueAlias<T>(T value) where T : IComparable
        {
            if (!_comparableValues.TryGetValue(typeof(T), out var typeValues))
            {
                _comparableValues[typeof(T)] = typeValues = new Dictionary<IComparable, string>();
            }

            if (!typeValues.TryGetValue(value, out var alias))
            {
                alias = typeValues[value] = string.Format(_AttrValueFormat, _valuesCounter++);

            }

            _allValues[alias] = value;
            return alias;
        }

        public string SetValueAlias(object? rawValue, Type valueType)
        {
            if (rawValue == null)
            {
                return _AttrNullValue;
            }

            string alias;
            if (rawValue is IComparable comparable)
            {
                if (!_comparableValues.TryGetValue(valueType, out var typeValues))
                {
                    _comparableValues[valueType] = typeValues = new Dictionary<IComparable, string>();
                }

                if (!typeValues.TryGetValue(comparable, out alias))
                {
                    alias = typeValues[comparable] = string.Format(_AttrValueFormat, _valuesCounter++);
                }
            }
            else
            {
                if (!_rawValues.TryGetValue(valueType, out var rawValues))
                {
                    _rawValues[valueType] = rawValues = new Dictionary<object, string>();
                }

                alias = rawValues[rawValue] = string.Format(_AttrValueFormat, _valuesCounter++);
            }

            _allValues[alias] = rawValue;
            return alias;
        }

        public Dictionary<string, AttributeValue> GetAttributeValues(IArrowStoreMapper mapper)
        {
            if (!HasAttributeValues)
            {
                return new Dictionary<string, AttributeValue>(0);
            }

            var result = new Dictionary<string, AttributeValue>();
            foreach (var (alias, value) in _allValues)
            {
                if (alias == _AttrNullValue)
                {
                    result[alias] = new AttributeValue { NULL = true };
                }
                else
                {
                    result[alias] = mapper.MapToAttributeValue(value);
                }
            }

            return result;
        }
    }
}
