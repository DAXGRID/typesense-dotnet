using System.Text.Json.Serialization;

namespace Typesense
{
    public class SearchParameters
    {
        [JsonPropertyName("q")]
        public string Text { get; set; }
        [JsonPropertyName("query_by")]
        public string QueryBy { get; set; }
        [JsonPropertyName("filter_by")]
        public string FilterBy { get; set; }
        [JsonPropertyName("sort_by")]
        public string SortBy { get; set; }
        [JsonPropertyName("group_by")]
        public string GroupBy { get; set; }
        [JsonPropertyName("group_limit")]
        public string GroupLimit { get; set; }
    }
}
