using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Amazon.DynamoDBv2.Model;

namespace ArrowStore.Mapper
{
    public class PrimitiveMappingProfile : IMappingProfile
    {
        private static readonly CultureInfo _Culture = new CultureInfo("en-US");

        public void Configure(IConverterBuilder builder)
        {
            ConfigureStrings(builder);
            ConfigureInts(builder);
            ConfigureShorts(builder);
            ConfigureDecimals(builder);
            ConfigureLongs(builder);
            ConfigureDates(builder);
        }

        private void ConfigureDates(IConverterBuilder builder)
        {
            builder
                .CreateCustomMap<DateTime, AttributeValue>()
                .ConvertUsing(date => ToDateAttribute(date, true));
            builder
                .CreateCustomMap<DateTime?, AttributeValue>()
                .ConvertUsing(date => ToDateAttribute(date, false));
            builder
                .CreateCustomMap<IEnumerable<DateTime>, AttributeValue>()
                .ConvertUsing(ToDateListAttribute);
            builder
                .CreateCustomMap<AttributeValue, DateTime>()
                .ConvertUsing(attr => FromDateAttribute(attr, true)!.Value);
            builder
                .CreateCustomMap<AttributeValue, DateTime?>()
                .ConvertUsing(attr => FromDateAttribute(attr, false));
            builder
                .CreateCustomMap<AttributeValue, DateTime[]>()
                .ConvertUsing(attr => FromDateListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, List<DateTime>>()
                .ConvertUsing(attr => FromDateListAttribute(attr).ToList());
            builder
                .CreateCustomMap<AttributeValue, IReadOnlyCollection<DateTime>>()
                .ConvertUsing(attr => FromDateListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, IEnumerable<DateTime>>()
                .ConvertUsing(FromDateListAttribute);
        }

        private void ConfigureLongs(IConverterBuilder builder)
        {
            builder
                .CreateCustomMap<long, AttributeValue>()
                .ConvertUsing(value => ToLongAttribute(value, true));
            builder
                .CreateCustomMap<long?, AttributeValue>()
                .ConvertUsing(value => ToLongAttribute(value, false));
            builder
                .CreateCustomMap<IEnumerable<long>, AttributeValue>()
                .ConvertUsing(ToLongListAttribute);
            builder
                .CreateCustomMap<AttributeValue, long>()
                .ConvertUsing(attr => FromLongAttribute(attr, true)!.Value);
            builder
                .CreateCustomMap<AttributeValue, long?>()
                .ConvertUsing(attr => FromLongAttribute(attr, false));
            builder
                .CreateCustomMap<AttributeValue, long[]>()
                .ConvertUsing(attr => FromLongListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, List<long>>()
                .ConvertUsing(attr => FromLongListAttribute(attr).ToList());
            builder
                .CreateCustomMap<AttributeValue, IReadOnlyCollection<long>>()
                .ConvertUsing(attr => FromLongListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, IEnumerable<long>>()
                .ConvertUsing(FromLongListAttribute);
        }

        private void ConfigureDecimals(IConverterBuilder builder)
        {
            builder
                .CreateCustomMap<decimal, AttributeValue>()
                .ConvertUsing(value => ToDecimalAttribute(value, true));
            builder
                .CreateCustomMap<decimal?, AttributeValue>()
                .ConvertUsing(value => ToDecimalAttribute(value, false));
            builder
                .CreateCustomMap<IEnumerable<decimal>, AttributeValue>()
                .ConvertUsing(ToDecimalListAttribute);
            builder
                .CreateCustomMap<AttributeValue, decimal>()
                .ConvertUsing(attr => FromDecimalAttribute(attr, true)!.Value);
            builder
                .CreateCustomMap<AttributeValue, decimal?>()
                .ConvertUsing(attr => FromDecimalAttribute(attr, false));
            builder
                .CreateCustomMap<AttributeValue, decimal[]>()
                .ConvertUsing(attr => FromDecimalListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, List<decimal>>()
                .ConvertUsing(attr => FromDecimalListAttribute(attr).ToList());
            builder
                .CreateCustomMap<AttributeValue, IReadOnlyCollection<decimal>>()
                .ConvertUsing(attr => FromDecimalListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, IEnumerable<decimal>>()
                .ConvertUsing(FromDecimalListAttribute);
        }

        private void ConfigureShorts(IConverterBuilder builder)
        {
            builder
                .CreateCustomMap<short, AttributeValue>()
                .ConvertUsing(value => ToShortAttribute(value, true));
            builder
                .CreateCustomMap<short?, AttributeValue>()
                .ConvertUsing(value => ToShortAttribute(value, false));
            builder
                .CreateCustomMap<IEnumerable<short>, AttributeValue>()
                .ConvertUsing(ToShortListAttribute);
            builder
                .CreateCustomMap<AttributeValue, short>()
                .ConvertUsing(attr => FromShortAttribute(attr, true)!.Value);
            builder
                .CreateCustomMap<AttributeValue, short?>()
                .ConvertUsing(attr => FromShortAttribute(attr, false));
            builder
                .CreateCustomMap<AttributeValue, short[]>()
                .ConvertUsing(attr => FromShortListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, List<short>>()
                .ConvertUsing(attr => FromShortListAttribute(attr).ToList());
            builder
                .CreateCustomMap<AttributeValue, IReadOnlyCollection<short>>()
                .ConvertUsing(attr => FromShortListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, IEnumerable<short>>()
                .ConvertUsing(FromShortListAttribute);
        }

        private void ConfigureInts(IConverterBuilder builder)
        {
            builder
                .CreateCustomMap<int, AttributeValue>()
                .ConvertUsing(value => ToIntAttribute(value, true));
            builder
                .CreateCustomMap<int?, AttributeValue>()
                .ConvertUsing(value => ToIntAttribute(value, false));
            builder
                .CreateCustomMap<IEnumerable<int>, AttributeValue>()
                .ConvertUsing(ToIntListAttribute);
            builder
                .CreateCustomMap<AttributeValue, int>()
                .ConvertUsing(attr => FromIntAttribute(attr, true)!.Value);
            builder
                .CreateCustomMap<AttributeValue, int?>()
                .ConvertUsing(attr => FromIntAttribute(attr, false));
            builder
                .CreateCustomMap<AttributeValue, int[]>()
                .ConvertUsing(attr => FromIntListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, List<int>>()
                .ConvertUsing(attr => FromIntListAttribute(attr).ToList());
            builder
                .CreateCustomMap<AttributeValue, IReadOnlyCollection<int>>()
                .ConvertUsing(attr => FromIntListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, IEnumerable<int>>()
                .ConvertUsing(FromIntListAttribute);
        }

        private void ConfigureStrings(IConverterBuilder builder)
        {
            builder
                .CreateCustomMap<string, AttributeValue>()
                .ConvertUsing(ToStringAttribute);
            builder
                .CreateCustomMap<IEnumerable<string>, AttributeValue>()
                .ConvertUsing(ToStringListAttribute);
            builder
                .CreateCustomMap<AttributeValue, string>()
                .ConvertUsing(FromStringAttribute);
            builder
                .CreateCustomMap<AttributeValue, string[]>()
                .ConvertUsing(attr => FromStringListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, List<string>>()
                .ConvertUsing(attr => FromStringListAttribute(attr).ToList());
            builder
                .CreateCustomMap<AttributeValue, IReadOnlyCollection<string>>()
                .ConvertUsing(attr => FromStringListAttribute(attr).ToArray());
            builder
                .CreateCustomMap<AttributeValue, IEnumerable<string>>()
                .ConvertUsing(FromStringListAttribute);
        }

        #region  DateTime
        private IEnumerable<DateTime> FromDateListAttribute(AttributeValue? attr)
        {
            if (attr == null || attr.NULL || attr.SS == null)
            {
                return Array.Empty<DateTime>();
            }

            var dates = new List<DateTime>(attr.SS.Count);
            foreach (var s in attr.SS)
            {
                dates.Add(ParseDate(s, true)!.Value);
            }

            return dates;
        }

        private DateTime? FromDateAttribute(AttributeValue? attr, bool required)
        {
            return ParseDate(attr?.S, required);
        }

        private AttributeValue ToDateListAttribute(IEnumerable<DateTime>? dates)
        {
            var stringSet = new List<string>();
            if (dates != null)
            {
                foreach (var dateTime in dates)
                {
                    stringSet.Add(ToDateString(dateTime));
                }
            }

            if (stringSet.Count == 0)
            {
                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { SS = stringSet };
        }

        private AttributeValue ToDateAttribute(DateTime? date, bool required)
        {
            var dateString = ToDateString(date);
            if (string.IsNullOrEmpty(dateString))
            {
                if (required)
                {
                    throw new InvalidCastException("The DateTime value is required");
                }

                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { S = dateString };
        }

        private DateTime? ParseDate(string? dateString, bool required)
        {
            if (string.IsNullOrEmpty(dateString) || !DateTime.TryParse(dateString, out var dateTime))
            {
                if (required)
                {
                    throw new InvalidCastException($"Failed to convert the date string to a DateTime: '{dateString}'");
                }

                return null;
            }

            return dateTime;
        }

        private string ToDateString(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            return dateTime.Value.ToString("O");
        }
        #endregion

        #region Long
        private IEnumerable<long> FromLongListAttribute(AttributeValue? attr)
        {
            if (attr == null || attr.NULL || attr.NS == null || attr.NS.Count == 0)
            {
                return Array.Empty<long>();
            }

            var result = new List<long>(attr.NS.Count);
            foreach (var n in attr.NS)
            {
                result.Add(ParseLong(n, true)!.Value);
            }

            return result;
        }

        private long? FromLongAttribute(AttributeValue? attr, bool required)
        {
            return ParseLong(attr?.N, required);
        }

        private AttributeValue ToLongListAttribute(IEnumerable<long>? values)
        {
            var result = new List<string>();
            if (values != null)
            {
                foreach (var value in values)
                {
                    result.Add(value.ToString(_Culture));
                }
            }

            if (result.Count == 0)
            {
                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { NS = result };
        }

        private AttributeValue ToLongAttribute(long? value, bool required)
        {
            if (!value.HasValue)
            {
                if (required)
                {
                    throw new InvalidCastException("The Long-value is required");
                }

                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { N = value.Value.ToString(_Culture) };
        }

        private long? ParseLong(string? value, bool required)
        {
            if (string.IsNullOrEmpty(value) || !long.TryParse(value, out var longValue))
            {
                if (required)
                {
                    throw new InvalidCastException($"The Long-value is required: '{value}'");
                }

                return null;
            }

            return longValue;
        }
        #endregion

        #region Decimal
        private IEnumerable<decimal> FromDecimalListAttribute(AttributeValue? attr)
        {
            if (attr == null || attr.NULL || attr.NS == null || attr.NS.Count == 0)
            {
                return Array.Empty<decimal>();
            }

            var result = new List<decimal>(attr.NS.Count);
            foreach (var n in attr.NS)
            {
                result.Add(ParseDecimal(n, true)!.Value);
            }

            return result;
        }

        private decimal? FromDecimalAttribute(AttributeValue? attr, bool required)
        {
            return ParseDecimal(attr?.N, required);
        }

        private AttributeValue ToDecimalListAttribute(IEnumerable<decimal>? values)
        {
            var result = new List<string>();
            if (values != null)
            {
                foreach (var value in values)
                {
                    result.Add(value.ToString(_Culture));
                }
            }

            if (result.Count == 0)
            {
                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { NS = result };
        }

        private AttributeValue ToDecimalAttribute(decimal? value, bool required)
        {
            if (!value.HasValue)
            {
                if (required)
                {
                    throw new InvalidCastException("The Decimal-value is required");
                }

                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { N = value.Value.ToString(_Culture) };
        }

        private decimal? ParseDecimal(string? value, bool required)
        {
            if (string.IsNullOrEmpty(value) || !decimal.TryParse(value, out var decimalValue))
            {
                if (required)
                {
                    throw new InvalidCastException($"The Decimal-value is required: '{value}'");
                }

                return null;
            }

            return decimalValue;
        }
        #endregion

        #region Short
        private IEnumerable<short> FromShortListAttribute(AttributeValue? attr)
        {
            if (attr == null || attr.NULL || attr.NS == null || attr.NS.Count == 0)
            {
                return Array.Empty<short>();
            }

            var result = new List<short>(attr.NS.Count);
            foreach (var n in attr.NS)
            {
                result.Add(ParseShort(n, true)!.Value);
            }

            return result;
        }

        private short? FromShortAttribute(AttributeValue? attr, bool required)
        {
            return ParseShort(attr?.N, required);
        }

        private AttributeValue ToShortListAttribute(IEnumerable<short>? values)
        {
            var result = new List<string>();
            if (values != null)
            {
                foreach (var value in values)
                {
                    result.Add(value.ToString(_Culture));
                }
            }

            if (result.Count == 0)
            {
                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { NS = result };
        }

        private AttributeValue ToShortAttribute(short? value, bool required)
        {
            if (!value.HasValue)
            {
                if (required)
                {
                    throw new InvalidCastException("The Short-value is required");
                }

                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { N = value.Value.ToString(_Culture) };
        }

        private short? ParseShort(string? value, bool required)
        {
            if (string.IsNullOrEmpty(value) || !short.TryParse(value, out var shortValue))
            {
                if (required)
                {
                    throw new InvalidCastException($"The Short-value is required: '{value}'");
                }

                return null;
            }

            return shortValue;
        }
        #endregion

        #region Int
        private IEnumerable<int> FromIntListAttribute(AttributeValue? attr)
        {
            if (attr == null || attr.NULL || attr.NS == null || attr.NS.Count == 0)
            {
                return Array.Empty<int>();
            }

            var result = new List<int>(attr.NS.Count);
            foreach (var n in attr.NS)
            {
                result.Add(ParseInt(n, true)!.Value);
            }

            return result;
        }

        private int? FromIntAttribute(AttributeValue? attr, bool required)
        {
            return ParseInt(attr?.N, required);
        }

        private AttributeValue ToIntListAttribute(IEnumerable<int>? values)
        {
            var result = new List<string>();
            if (values != null)
            {
                foreach (var value in values)
                {
                    result.Add(value.ToString(_Culture));
                }
            }

            if (result.Count == 0)
            {
                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { NS = result };
        }

        private AttributeValue ToIntAttribute(int? value, bool required)
        {
            if (!value.HasValue)
            {
                if (required)
                {
                    throw new InvalidCastException("The Int-value is required");
                }

                return new AttributeValue { NULL = true };
            }

            return new AttributeValue { N = value.Value.ToString(_Culture) };
        }

        private int? ParseInt(string? value, bool required)
        {
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out var intValue))
            {
                if (required)
                {
                    throw new InvalidCastException($"The Int-value is required: '{value}'");
                }

                return null;
            }

            return intValue;
        }
        #endregion

        private IEnumerable<string> FromStringListAttribute(AttributeValue? attr)
        {
            if (attr?.SS != null)
            {
                return attr.SS;
            }

            return Array.Empty<string>();
        }

        private string? FromStringAttribute(AttributeValue? attr)
        {
            return attr?.S;
        }

        private AttributeValue ToStringListAttribute(IEnumerable<string>? values)
        {
            var result = new List<string>();
            if (values != null)
            {
                foreach (var value in values)
                {
                    result.Add(value);
                }
            }

            if (result.Count == 0)
            {
                return new AttributeValue { NULL = true };
            }

            return new AttributeValue(result);
        }

        private AttributeValue ToStringAttribute(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new AttributeValue { NULL = true };
            }

            return new AttributeValue(value);
        }
    }
}
