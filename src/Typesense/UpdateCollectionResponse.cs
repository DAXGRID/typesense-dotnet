using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Typesense;

public record UpdateCollectionResponse
{
    [JsonPropertyName("fields")]
    public IReadOnlyList<UpdateSchemaField> Fields { get; init; }

    [JsonConstructor]
    public UpdateCollectionResponse(IReadOnlyList<UpdateSchemaField> fields)
    {
        Fields = fields;
    }
}
