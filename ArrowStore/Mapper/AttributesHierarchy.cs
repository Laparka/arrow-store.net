using System.Collections.Generic;

namespace ArrowStore.Mapper
{
    public class AttributesHierarchy
    {
        public AttributesHierarchy()
        {
            Nested = new Dictionary<string, AttributesHierarchy>();
        }

        public IDictionary<string, AttributesHierarchy> Nested { get; private set; }

        public void ReplaceNested(IDictionary<string, AttributesHierarchy> nested)
        {
            Nested = nested;
        }
    }
}
