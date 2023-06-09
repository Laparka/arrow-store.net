using ArrowStore.Put;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using ArrowStore.Expressions.Nodes;

namespace ArrowStore.Expressions
{
    internal class WhereExpressionParser
    {
        public static readonly WhereExpressionParser Instance = new WhereExpressionParser();
        private WhereExpressionParser()
        {
        }

        public RecordMemberAccessorNode EnsurePropertyName<TInstance, TMember>(Expression<Func<TInstance, TMember>> predicate)
        {
            var expression = Visit(predicate, null);
            if (expression.NodeType != ExpressionNodeType.RecordMemberAccessor)
            {
                throw new ArgumentException($"The property accessor was expected, but received {expression.NodeType:G}");
            }

            return (RecordMemberAccessorNode)expression;
        }

        public IExpressionNode Tokenize<T>(Expression<Func<T, IArrowStorePredicateExtensions<T>, bool>> predicate)
        {
            var expression = Visit(predicate, null);
            return expression;
        }

        public static object? EvaluateMemberValue(Type instanceType, object? currentInstance, string memberPath, bool expectStatic)
        {
            string[] memberParts = memberPath.Split('.');

            foreach (string memberPart in memberParts)
            {
                if (currentInstance == null)
                {
                    // If the current instance is null, it means we are evaluating a static member
                    if (expectStatic)
                    {
                        currentInstance = instanceType;
                    }
                    else
                    {
                        return null;
                    }
                }

                var members = instanceType.GetMember(memberPart, BindingFlags.Instance | BindingFlags.Public);
                if (members.Length != 1)
                {
                    throw new InvalidOperationException($"The member \"{memberPart}\" was not found");
                }

                var memberInfo = members[0];
                switch (memberInfo)
                {
                    case FieldInfo fieldInfo:
                    {
                        currentInstance = fieldInfo.GetValue(currentInstance);
                        break;
                    }

                    case PropertyInfo propertyInfo:
                    {
                        currentInstance = propertyInfo.GetValue(currentInstance);
                        break;
                    }

                    default:
                    {
                        return null;
                    }
                }
            }

            return currentInstance;
        }

        private static IExpressionNode Visit(Expression expression, ParserContext? context)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                {
                    return Lambda((LambdaExpression)expression);
                }

                case ExpressionType.AndAlso:
                {
                    return And((BinaryExpression)expression, context);
                }

                case ExpressionType.OrElse:
                {
                    return Or((BinaryExpression)expression, context);
                }

                case ExpressionType.Equal:
                {
                    return Compare((BinaryExpression)expression, CompareOperator.Equal, context);
                }

                case ExpressionType.NotEqual:
                {
                    return Compare((BinaryExpression)expression, CompareOperator.NotEqual, context);
                }

                case ExpressionType.LessThan:
                {
                    return Compare((BinaryExpression)expression, CompareOperator.LessThan, context);
                }

                case ExpressionType.LessThanOrEqual:
                {
                    return Compare((BinaryExpression)expression, CompareOperator.LessThanOrEqual, context);
                }

                case ExpressionType.GreaterThan:
                {
                    return Compare((BinaryExpression)expression, CompareOperator.GreaterThan, context);
                }

                case ExpressionType.GreaterThanOrEqual:
                {
                    return Compare((BinaryExpression)expression, CompareOperator.GreaterThanOrEqual, context);
                }

                case ExpressionType.Call:
                {
                    return Call((MethodCallExpression)expression, context);
                }

                case ExpressionType.MemberAccess:
                {
                    return MemberAccess((MemberExpression)expression, context);
                }

                case ExpressionType.Constant:
                {
                    return VisitConstant((ConstantExpression)expression);
                }

                case ExpressionType.Convert:
                {
                    return Convert((UnaryExpression)expression, context);
                }

                case ExpressionType.Parameter:
                {
                    return Parameter((ParameterExpression)expression, context);
                }

                case ExpressionType.ArrayLength:
                {
                    return ArrayLength((UnaryExpression)expression, context);
                }

                case ExpressionType.Quote:
                {
                    return InnerLambdaQuote((UnaryExpression)expression);
                }

                case ExpressionType.Not:
                {
                    return Not((UnaryExpression)expression, context);
                }
            }

            throw new InvalidOperationException($"Not supported expression type '{expression.NodeType:G}'");
        }

        private static IExpressionNode Not(UnaryExpression expression, ParserContext? context)
        {
            return new InverseNode(Visit(expression.Operand, context));
        }

        private static IExpressionNode InnerLambdaQuote(UnaryExpression expression)
        {
            return Visit(expression.Operand, null);
        }

        private static IExpressionNode ArrayLength(UnaryExpression expression, ParserContext? context)
        {
            return new MethodCallNode(Visit(expression.Operand, context), MethodCallType.Size, Array.Empty<IExpressionNode>());
        }

        private static ConstantNode VisitConstant(ConstantExpression expression)
        {
            return new ConstantNode(expression.Value);
        }

        private static IExpressionNode Lambda(LambdaExpression expression)
        {
            string? accessorName = null;
            string? extensionName = null;
            for(var i = 0; i < expression.Parameters.Count; i++)
            {
                var parameter = expression.Parameters[i];
                var node = Visit(parameter, null);
                switch (node.NodeType)
                {
                    case ExpressionNodeType.Record:
                    {
                        accessorName = ((RecordParameterNode)node).Name;
                        break;
                    }

                    case ExpressionNodeType.PredicateExtension:
                    {
                        extensionName = ((PredicateExtensionParameterNode)node).Name;
                        break;
                    }

                    default:
                    {
                        throw new NotSupportedException($"Invalid expression {expression.Parameters[i].Name}");
                    }
                }
            }

            if (string.IsNullOrEmpty(accessorName))
            {
                throw new ArgumentNullException(nameof(extensionName), "The record type argument is missing");
            }

            return Visit(expression.Body, new ParserContext(accessorName, extensionName));
        }

        private static IExpressionNode Parameter(ParameterExpression expression, ParserContext? context)
        {
            IExpressionNode? node = null;
            if (expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(IArrowStorePredicateExtensions<>))
            {
                if (context == null || !string.IsNullOrEmpty(context.ExtensionParameter) && context.ExtensionParameter == expression.Name)
                {
                    node = new PredicateExtensionParameterNode(expression.Name);
                }
            }
            else if (context == null || context.RecordParameter == expression.Name)
            {
                node = new RecordParameterNode(expression.Name, expression.Type);
            }

            if (node == null)
            {
                throw new InvalidOperationException("The Lambda-parameter is unknown or not supported");
            }

            return node;
        }

        private static IExpressionNode Call(MethodCallExpression expression, ParserContext? context)
        {
            IExpressionNode? instance = null;
            if (expression.Object != null)
            {
                instance = Visit(expression.Object, context);
            }

            var args = new IExpressionNode[instance == null ? expression.Arguments.Count - 1 : expression.Arguments.Count];
            for (var i = 0; i < expression.Arguments.Count; i++)
            {
                var arg = Visit(expression.Arguments[i], context);
                if (expression.Object == null)
                {
                    if (i == 0)
                    {
                        instance = arg;
                    }
                    else
                    {
                        args[i - 1] = arg;
                    }
                }
                else
                {
                    args[i] = arg;
                }
            }

            MethodCallType method;
            switch (expression.Method.Name)
            {
                case nameof(string.StartsWith):
                {
                    method = MethodCallType.BeginsWith;
                    break;
                }

                case nameof(string.Contains):
                {
                    method = MethodCallType.Contains;
                    break;
                }

                case nameof(Array.Length):
                case nameof(ICollection.Count):
                {
                    method = MethodCallType.Size;
                    break;
                }

                case "MemberExists":
                {
                    if (instance?.NodeType != ExpressionNodeType.PredicateExtension)
                    {
                        throw new InvalidOperationException($"Not supported method: {expression.Method.Name}");
                    }

                    if (args.Length != 1 && args[0].NodeType != ExpressionNodeType.RecordMemberAccessor)
                    {
                        throw new InvalidOperationException("The MemberExists argument must be a member accessor");
                    }

                    return new MemberExistsNode((RecordMemberAccessorNode)args[0]);
                }

                default:
                {
                    throw new InvalidOperationException($"Not supported method: {expression.Method.Name}");
                }
            }

            return new MethodCallNode(instance, method, args);
        }

        private static IExpressionNode MemberAccess(MemberExpression expression, ParserContext? context)
        {
            IExpressionNode? instance = null;
            if (expression.Expression != null)
            {
                instance = Visit(expression.Expression, context);
                if (instance.NodeType == ExpressionNodeType.Constant)
                {
                    var constant = (ConstantNode)instance;
                    if (constant.Value == null)
                    {
                        return constant;
                    }

                    return new ConstantNode(EvaluateMemberValue(constant.Value.GetType(), constant.Value, expression.Member.Name, false));
                }
            }

            if (expression.Member.MemberType != MemberTypes.Property)
            {
                throw new InvalidOperationException($"The member accessor must be a property: {expression.Member.Name}");
            }

            var propertyInfo = (PropertyInfo)expression.Member;
            if (instance == null)
            {
                // Static member accessor
                return new ConstantNode(EvaluateMemberValue(propertyInfo.PropertyType, null, expression.Member.Name, true));
            }

            if (expression.Member.Name == nameof(string.Length) && propertyInfo.PropertyType == typeof(int) && instance.NodeType == ExpressionNodeType.RecordMemberAccessor)
            {
                var memberAccessor = (RecordMemberAccessorNode)instance;
                if (memberAccessor.MemberType == typeof(string))
                {
                    return new MethodCallNode(instance, MethodCallType.Size, Array.Empty<IExpressionNode>());
                }
            }

            return new RecordMemberAccessorNode(instance, expression.Member.Name, propertyInfo.PropertyType);
        }

        private static IExpressionNode Convert(UnaryExpression expression, ParserContext? context)
        {
            return Visit(expression.Operand, context);
        }

        private static IExpressionNode Compare(BinaryExpression expression, CompareOperator compareOperator, ParserContext? context)
        {
            var left = Visit(expression.Left, context);
            var right = Visit(expression.Right, context);
            return new CompareOperatorNode(Cast(left, right), compareOperator, Cast(right, left));
        }

        private static IExpressionNode Or(BinaryExpression expression, ParserContext? context)
        {
            var left = Visit(expression.Left, context);
            var right = Visit(expression.Right, context);
            return new InBracesNode(new BinaryNode(left, BinaryExpressionType.Or, right));
        }

        private static IExpressionNode And(BinaryExpression expression, ParserContext? context)
        {
            var left = Visit(expression.Left, context);
            var right = Visit(expression.Right, context);
            return new InBracesNode(new BinaryNode(left, BinaryExpressionType.And, right));
        }

        private static IExpressionNode Cast(IExpressionNode toEval, IExpressionNode reference)
        {
            if (toEval.NodeType == ExpressionNodeType.Constant)
            {
                Type? type = null;
                if (reference.NodeType == ExpressionNodeType.MethodCall)
                {
                    type = ((MethodCallNode)reference).ReturnType;
                }
                else if (reference.NodeType == ExpressionNodeType.RecordMemberAccessor)
                {
                    var memberAccessor = (RecordMemberAccessorNode)reference;
                    type = memberAccessor.MemberType;
                }

                if (type != null)
                {
                    var convert = (IForceConverter)Activator.CreateInstance(typeof(ForceConverter<>).MakeGenericType(type));
                    var constant = (ConstantNode)toEval;
                    return new ConstantNode(convert.Convert(constant.Value));
                }
            }

            return toEval;
        }

        private class ParserContext
        {
            public ParserContext(string recordParameter, string? extensionParameter)
            {
                RecordParameter = recordParameter;
                ExtensionParameter = extensionParameter;
            }

            public string RecordParameter { get; }

            public string? ExtensionParameter { get; }
        }

        private interface IForceConverter
        {
            object? Convert(object? value);
        }

        private class ForceConverter<T> : IForceConverter
        {
            public object? Convert(object? value)
            {
                return DoConvert(value);
            }

            private T DoConvert(object? value)
            {
                if (value == null)
                {
                    return default;
                }

                return (T)value;
            }
        }
    }
}
