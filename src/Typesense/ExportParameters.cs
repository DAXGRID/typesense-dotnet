using System;

namespace Typesense;

public record ExportParameters
{
    public string? FilterBy { get; set; }
    public string IncludeFields { get; set; }
    public string ExcludeFields { get; set; }

    [Obsolete("Use multi-arity constructor instead.")]
    public ExportParameters()
    {
        IncludeFields = string.Empty;
        ExcludeFields = string.Empty;
        FilterBy = null;
    }

    public ExportParameters(string includeFields, string excludeFields)
    {
        IncludeFields = includeFields;
        ExcludeFields = excludeFields;
    }

    public ExportParameters(string includeFields, string excludeFields, string filterBy)
    {
        IncludeFields = includeFields;
        ExcludeFields = excludeFields;
        FilterBy = filterBy;
    }
}
