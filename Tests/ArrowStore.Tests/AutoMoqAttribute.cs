using AutoFixture.AutoMoq;
using AutoFixture;
using AutoFixture.Xunit2;

namespace ArrowStore.Tests
{
    public class AutoMoqAttribute : InlineAutoDataAttribute
    {
        public AutoMoqAttribute() : this(Array.Empty<object>())
        {
        }

        public AutoMoqAttribute(params object[] args) : base(new InnerAttribute(), args)
        {
        }

        private static IFixture Configure()
        {
            var fixture = new Fixture();
            return fixture
                .Customize(new AutoMoqCustomization { ConfigureMembers = true })
                .Customize(new SupportMutableValueTypesCustomization());
        }

        private class InnerAttribute : AutoDataAttribute
        {
            public InnerAttribute() : base(Configure)
            {
            }
        }
    }
}
