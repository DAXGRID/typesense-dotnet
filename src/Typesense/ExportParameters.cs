namespace Typesense;

public record ExportParameters
{
    public string? FilterBy { get; set; }
    public string? IncludeFields { get; set; }
    public string? ExcludeFields { get; set; }
}
