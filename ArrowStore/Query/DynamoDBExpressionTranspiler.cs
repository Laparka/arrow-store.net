using System;
using System.Collections.Generic;
using ArrowStore.Expressions.Nodes;
using ArrowStore.Mapper;
using ArrowStore.Records;

namespace ArrowStore.Query
{
    public class DynamoDBExpressionTranspiler : IExpressionTranspiler
    {
        public string ToExpression(IExpressionNode expressionNode, QueryVariables vars, AttributesProjection? projection)
        {
            if (expressionNode == null)
            {
                throw new ArgumentNullException(nameof(expressionNode));
            }

            var context = new TraverseContext(projection);
            var expression = Visit(expressionNode, vars, context);
            return expression;
        }

        public string? GetProjectionExpression<TRecord>(QueryVariables queryVars, AttributesProjection? projection)
        {
            if (projection == null)
            {
                return null;
            }

            var visited = new List<IDictionary<string, AttributesHierarchy>>();
            var attributes = GetProjectionExpression(projection.TypeAttributesHierarchy, queryVars, visited);
            if (attributes.Count == 0)
            {
                return null;
            }

            return string.Join(", ", attributes);
        }

        public string GetKeyConditionExpression<TRecord>(IArrowStoreIndex<TRecord> queryIndex, QueryVariables queryVars) where TRecord : IArrowStoreRecord
        {
            var partitionKeys = queryIndex.GetPartitionKeys();
            var conditionExpressions = new List<string>(partitionKeys.Count);
            foreach (var partitionKey in partitionKeys)
            {
                var attributeNameAlias = queryVars.SetAttributeNameAlias(partitionKey.AttributeName);
                var attributeValueAlias = queryVars.SetValueAlias(partitionKey.AttributeValue);
                switch (partitionKey.Operator)
                {
                    case ArrowStoreQueryOperator.BeginsWith:
                    {
                        conditionExpressions.Add($"begins_with({attributeNameAlias}, {attributeValueAlias})");
                        break;
                    }

                    case ArrowStoreQueryOperator.Equals:
                    {
                        conditionExpressions.Add($"{attributeNameAlias} = {attributeValueAlias}");
                        break;
                    }

                    case ArrowStoreQueryOperator.Less:
                    {
                        conditionExpressions.Add($"{attributeNameAlias} < {attributeValueAlias}");
                        break;
                    }

                    case ArrowStoreQueryOperator.LessOrEqual:
                    {
                        conditionExpressions.Add($"{attributeNameAlias} <= {attributeValueAlias}");
                        break;
                    }

                    case ArrowStoreQueryOperator.Greater:
                    {
                        conditionExpressions.Add($"{attributeNameAlias} > {attributeValueAlias}");
                        break;
                    }

                    case ArrowStoreQueryOperator.GreaterOrEqual:
                    {
                        conditionExpressions.Add($"{attributeNameAlias} >= {attributeValueAlias}");
                        break;
                    }

                    default:
                    {
                        throw new NotSupportedException($"The key condition operator {partitionKey.Operator:G} is not supported");
                    }
                }
            }

            return string.Join(" and ", conditionExpressions);
        }

        private string Visit(IExpressionNode node, QueryVariables queryVars, TraverseContext context)
        {
            switch (node.NodeType)
            {
                case ExpressionNodeType.BinaryExpression:
                {
                    return VisitBinary((BinaryNode)node, queryVars, context);
                }

                case ExpressionNodeType.Compare:
                {
                    return VisitCompare((CompareOperatorNode)node, queryVars, context);
                }

                case ExpressionNodeType.RecordMemberAccessor:
                {
                    return VisitMemberAccessor((RecordMemberAccessorNode)node, queryVars, context);
                }

                case ExpressionNodeType.MethodCall:
                {
                    return VisitMethod((MethodCallNode)node, queryVars, context);
                }

                case ExpressionNodeType.Constant:
                {
                    return VisitConst((ConstantNode)node, queryVars, context);
                }

                case ExpressionNodeType.InBraces:
                {
                    return InBraces((InBracesNode)node, queryVars, context);
                }

                case ExpressionNodeType.MemberExistsCondition:
                {
                    return MemberExists((MemberExistsNode)node, queryVars, context);
                }

                case ExpressionNodeType.Inverse:
                {
                    return Inverse((InverseNode)node, queryVars, context);
                }
            }

            throw new NotSupportedException($"The node type is not supported {node.NodeType:G}");
        }

        private string Inverse(InverseNode node, QueryVariables queryVars, TraverseContext context)
        {
            var content = Visit(node.Body, queryVars, context);
            const string attributeExistsString = "attribute_exists";
            const string attributeNotExistsString = "attribute_not_exists";
            if (node.Body.NodeType == ExpressionNodeType.MemberExistsCondition)
            {
                if (content.StartsWith(attributeExistsString))
                {
                    return string.Join(string.Empty, attributeNotExistsString, content[attributeExistsString.Length..]);
                }

                if (content.StartsWith(attributeNotExistsString))
                {
                    return string.Join(string.Empty, attributeExistsString, content[attributeNotExistsString.Length..]);
                }
            }

            return string.Join(' ', "not", content);
        }

        private string MemberExists(MemberExistsNode node, QueryVariables queryVars, TraverseContext context)
        {
            var memberAccessor = Visit(node.MemberAccessor, queryVars, context);
            return $"attribute_exists({memberAccessor})";
        }

        private string InBraces(InBracesNode node, QueryVariables queryVars, TraverseContext context)
        {
            var body = Visit(node.Body, queryVars, context);
            return $"({body})";
        }

        private string VisitConst(ConstantNode node, QueryVariables queryVars, TraverseContext context)
        {
            return queryVars.SetValueAlias(node.Value);
        }

        private string VisitMethod(MethodCallNode node, QueryVariables queryVars, TraverseContext context)
        {
            var args = new string[node.Args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = Visit(node.Args[i], queryVars, context);
            }

            string? instance = null;
            if (node.Instance != null)
            {
                instance = Visit(node.Instance, queryVars, context);
            }

            string method;
            switch (node.Method)
            {
                case MethodCallType.Size:
                {
                    if (args.Length != 0)
                    {
                        throw new InvalidOperationException("No arguments allowed for the Size-function");
                    }

                    if (string.IsNullOrEmpty(instance))
                    {
                        throw new InvalidOperationException("No Size function attribute is provided");
                    }

                    method = $"size({instance})";
                    break;
                }

                case MethodCallType.BeginsWith:
                {
                    if (args.Length != 1)
                    {
                        throw new InvalidOperationException("The begins_with function argument is missing");
                    }

                    if (string.IsNullOrEmpty(instance))
                    {
                        throw new InvalidOperationException("No begins_with function attribute is provided");
                    }

                    method = $"begins_with({instance}, {args[0]})";
                    break;
                }

                case MethodCallType.Contains:
                {
                    if (args.Length != 1)
                    {
                        throw new InvalidOperationException("The contains function argument is missing");
                    }

                    if (string.IsNullOrEmpty(instance))
                    {
                        throw new InvalidOperationException("No contains function attribute is provided");
                    }

                    method = $"contains({instance}, {args[0]})";
                    break;
                }

                default:
                {
                    throw new NotSupportedException($"The method-type is not supported: {node.Method:G}");
                }
            }

            return method;
        }

        private string VisitMemberAccessor(RecordMemberAccessorNode node, QueryVariables queryVars, TraverseContext context)
        {
            var accessorPath = new Stack<string>();
            IExpressionNode enumerateNode = node;
            while (enumerateNode.NodeType == ExpressionNodeType.RecordMemberAccessor)
            {
                var theNode = (RecordMemberAccessorNode)enumerateNode;
                accessorPath.Push(theNode.MemberName);
                enumerateNode = theNode.Instance;
            }

            if (enumerateNode.NodeType != ExpressionNodeType.Record)
            {
                throw new InvalidOperationException("The member accessor is not supported. Only the record type accessors are allowed");
            }

            var recordTypeNode = (RecordParameterNode)enumerateNode;
            var projection = context.AttributesProjection;
            if (projection == null)
            {
                throw new InvalidOperationException($"The property attributes projection was not found for the type {recordTypeNode.RecordType}");
            }

            var propertyRefs = projection.TypeAttributeNameReferences;
            var attributePath = new List<string>();
            while (accessorPath.TryPop(out var propertyName))
            {
                if (!propertyRefs.TryGetValue(propertyName, out var nameRefs))
                {
                    throw new InvalidOperationException($"The property's attribute projection was not found: '{propertyName}'");
                }

                foreach (var attributeName in nameRefs.AttributePath.Split('.'))
                {
                    attributePath.Add(queryVars.SetAttributeNameAlias(attributeName));
                }

                if (nameRefs.MemberTypeProjection.Count != 0)
                {
                    propertyRefs = nameRefs.MemberTypeProjection;
                }
            }

            return string.Join('.', attributePath);
        }

        private string VisitCompare(CompareOperatorNode node, QueryVariables queryVars, TraverseContext context)
        {
            var left = Visit(node.Left, queryVars, context);
            var right = Visit(node.Right, queryVars, context);
            string compareOp;
            switch (node.Operator)
            {
                case CompareOperator.Equal:
                {
                    compareOp = " = ";
                    break;
                }

                case CompareOperator.NotEqual:
                {
                    compareOp = " <> ";
                    break;
                }

                case CompareOperator.GreaterThan:
                {
                    compareOp = " > ";
                    break;
                }

                case CompareOperator.GreaterThanOrEqual:
                {
                    compareOp = " >= ";
                    break;
                }

                case CompareOperator.LessThan:
                {
                    compareOp = " < ";
                    break;
                }

                case CompareOperator.LessThanOrEqual:
                {
                    compareOp = " <= ";
                    break;
                }

                default:
                {
                    throw new InvalidOperationException($"Unknown compare operator: {node.Operator:G}");
                }
            }

            return string.Join(compareOp, left, right);
        }

        private string VisitBinary(BinaryNode node, QueryVariables queryVars, TraverseContext context)
        {
            var left = Visit(node.Left, queryVars, context);
            var right = Visit(node.Right, queryVars, context);
            return string.Join(node.BinaryExpressionType == BinaryExpressionType.Or ? " or " : " and ", left, right);
        }

        private static List<string> GetProjectionExpression(IDictionary<string, AttributesHierarchy> hierarchy, QueryVariables queryVars, List<IDictionary<string, AttributesHierarchy>> visited)
        {
            if (visited.Contains(hierarchy))
            {
                return new List<string>(0);
            }

            visited.Add(hierarchy);
            var attributes = new List<string>();
            foreach (var (attributeName, child) in hierarchy)
            {
                var alias = queryVars.SetAttributeNameAlias(attributeName);
                if (child.Nested.Count != 0)
                {
                    var nested = GetProjectionExpression(child.Nested, queryVars, visited);
                    foreach (var childAttr in nested)
                    {
                        attributes.Add(string.Join('.', alias, childAttr));
                    }
                }
                else
                {
                    attributes.Add(alias);
                }
            }

            return attributes;
        }

        private class TraverseContext
        {
            public TraverseContext(AttributesProjection? attributesProjection)
            {
                AttributesProjection = attributesProjection;
            }

            public AttributesProjection? AttributesProjection { get; }
        }
    }
}
