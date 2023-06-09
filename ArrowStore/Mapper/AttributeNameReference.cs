using System;
using System.Collections.Generic;

namespace ArrowStore.Mapper
{
    public class AttributeNameReference
    {
        public AttributeNameReference(string attributePath, Type memberType, IDictionary<string, AttributeNameReference> memberTypeProjection)
        {
            AttributePath = attributePath;
            MemberType = memberType;
            MemberTypeProjection = memberTypeProjection;
        }


        public string AttributePath { get; }

        public Type MemberType { get; }

        public IDictionary<string, AttributeNameReference> MemberTypeProjection { get; }
    }
}
