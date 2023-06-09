using System.Text;
using ArrowStore.Expressions;
using ArrowStore.Expressions.Nodes;
using ArrowStore.Mapper;
using ArrowStore.Query;
using ArrowStore.Tests.Models;

namespace ArrowStore.Tests.Expressions
{
    public class WhereExpressionParserTests
    {
        [Theory, AutoMoq]
        public void UsesBeginsWithExpression(string userId, UserRecord refRecord)
        {
            var builder = new DynamoDBConverterBuilder();
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var node = WhereExpressionParser.Instance.Tokenize<UserRecord>((user, _) =>
                (user.UserId.StartsWith(userId) || user.TenantId == refRecord.TenantId) && user.IdentityInfo.Type == IdDocumentType.DriverLicense);

            var inBraces1st = Assert.IsType<InBracesNode>(node);
            var and = Assert.IsType<BinaryNode>(inBraces1st.Body);
            Assert.Equal(BinaryExpressionType.And, and.BinaryExpressionType);
            var andLeft = Assert.IsType<InBracesNode>(and.Left);
            var or = Assert.IsType<BinaryNode>(andLeft.Body);
            Assert.IsType<MethodCallNode>(or.Left);
            Assert.IsType<CompareOperatorNode>(or.Right);
            Assert.IsType<CompareOperatorNode>(and.Right);

            var dynamoTranspiler = new DynamoDBExpressionTranspiler();
            var vars = new QueryVariables();
            var expression = dynamoTranspiler.ToExpression(node, vars, mapper.GetReadProjection(typeof(UserRecord)));
            Assert.NotNull(expression);

            var sb = new StringBuilder(expression);
            foreach (var (propertyName, attributeName) in vars.AttributeNameAliases)
            {
                sb = sb.Replace(attributeName, propertyName);
            }

            var formatted = sb.ToString();
            Assert.Equal("((begins_with(record_id, :attr_val_0) or record_type = :attr_val_1) and record_data.identity_info.document_type = :attr_val_2)", formatted);
        }

        [Theory, AutoMoq]
        public void UsesSizeForStringLength(string userId, UserRecord refRecord)
        {
            var builder = new DynamoDBConverterBuilder();
            builder.AddProfile(new UserMappingProfile());
            var mapper = builder.Build();
            var node = WhereExpressionParser.Instance.Tokenize<UserRecord>((user, _) => user.UserName.Length > 0 || user.LoginHistory.Length == 0);

            var inBraces1st = Assert.IsType<InBracesNode>(node);
            var or = Assert.IsType<BinaryNode>(inBraces1st.Body);
            var nameLengthCompare = Assert.IsType<CompareOperatorNode>(or.Left);
            Assert.IsType<MethodCallNode>(nameLengthCompare.Left);
            Assert.IsType<ConstantNode>(nameLengthCompare.Right);

            var loginHistoryCompare = Assert.IsType<CompareOperatorNode>(or.Right);
            Assert.IsType<MethodCallNode>(loginHistoryCompare.Left);
            Assert.IsType<ConstantNode>(loginHistoryCompare.Right);

            var dynamoTranspiler = new DynamoDBExpressionTranspiler();
            var vars = new QueryVariables();
            var expression = dynamoTranspiler.ToExpression(node, vars, mapper.GetReadProjection(typeof(UserRecord)));
            Assert.NotNull(expression);

            var sb = new StringBuilder(expression);
            foreach (var (propertyName, attributeName) in vars.AttributeNameAliases)
            {
                sb = sb.Replace(attributeName, propertyName);
            }

            var formatted = sb.ToString();
            Assert.Equal("(size(record_data.user.name) > :attr_val_0 or size(record_data.login_history) = :attr_val_0)", formatted);
        }
    }
}
