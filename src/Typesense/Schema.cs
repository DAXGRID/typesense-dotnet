using System.Collections.Generic;

namespace Typesense
{
    public class Schema
    {
        public string Name { get; set; }
        public string NumberOfDocuments { get; set; }
        public List<Field> Fields { get; set; }
        public string DefaultSortingField { get; set; }
    }
}
