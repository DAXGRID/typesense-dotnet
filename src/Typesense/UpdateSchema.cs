using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record UpdateSchemaField : Field
{
    public bool? Drop { get; init; }

    public UpdateSchemaField(string name, bool drop) : base(name)
    {
        Drop = drop;
    }

    public UpdateSchemaField(
        string name,
        FieldType type) : base(name, type) { }

    public UpdateSchemaField(
        string name,
        FieldType type,
        bool? facet = null) : base(name, type, facet) { }

    public UpdateSchemaField(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null) : base(name, type, facet, optional) { }

    public UpdateSchemaField(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null) : base(name, type, facet, optional, index) { }

    public UpdateSchemaField(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null,
        bool? sort = null) : base(name, type, facet, optional, index, sort) { }

    public UpdateSchemaField(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null,
        bool? sort = null,
        bool? infix = null) : base(name, type, facet, optional, index, sort, infix) { }

    public UpdateSchemaField(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null,
        bool? sort = null,
        bool? infix = null,
        string? locale = null) : base(name, type, facet, optional, index, sort, infix, locale) { }

    [JsonConstructor]
    public UpdateSchemaField(
        string name,
        FieldType type,
        bool? facet = null,
        bool? optional = null,
        bool? index = null,
        bool? sort = null,
        bool? infix = null,
        string? locale = null,
        bool? drop = null) : base(name, type, facet, optional, index, sort, infix, locale)
    {
        Drop = drop;
    }

    protected UpdateSchemaField(Field original) : base(original) { }
}

public record UpdateSchema
{
    [JsonPropertyName("fields")]
    public IReadOnlyCollection<UpdateSchemaField> Fields { get; init; }

    [JsonConstructor]
    public UpdateSchema(IReadOnlyCollection<UpdateSchemaField> fields)
    {
        Fields = fields;
    }
}
