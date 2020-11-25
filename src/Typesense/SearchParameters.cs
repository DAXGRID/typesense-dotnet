namespace Typesense
{
    public class SearchParameters
    {
        public string Q { get; set; }
        public string QueryBy { get; set; }
        public string FilterBy { get; set; }
        public string SortBy { get; set; }
        public string GroupBy { get; set; }
        public string GroupLimit { get; set; }
    }
}
