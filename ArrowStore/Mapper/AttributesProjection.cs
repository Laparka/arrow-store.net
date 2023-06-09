using System.Collections.Generic;

namespace ArrowStore.Mapper
{
    public class AttributesProjection
    {
        private AttributesProjection(int capacity)
        {
            TypeAttributeNameReferences = new Dictionary<string, AttributeNameReference>(capacity);
            TypeAttributesHierarchy = new Dictionary<string, AttributesHierarchy>(capacity);
        }

        public AttributesProjection()
        {
            TypeAttributeNameReferences = new Dictionary<string, AttributeNameReference>();
            TypeAttributesHierarchy = new Dictionary<string, AttributesHierarchy>();
        }

        public IDictionary<string, AttributeNameReference> TypeAttributeNameReferences { get; }

        public IDictionary<string, AttributesHierarchy> TypeAttributesHierarchy { get; }

        public static AttributesProjection Empty()
        {
            return new AttributesProjection(0);
        }
    }
}
