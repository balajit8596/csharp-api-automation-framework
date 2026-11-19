using NJsonSchema;

namespace Qa.ApiFramework.Validation;

/// <summary>Validates a JSON response body against a JSON schema.</summary>
public static class SchemaValidator
{
    /// <summary>Returns a list of schema violations. An empty list means the body is valid.</summary>
    public static async Task<IReadOnlyList<string>> ValidateAsync(string schemaJson, string json)
    {
        var schema = await JsonSchema.FromJsonAsync(schemaJson);
        return schema.Validate(json).Select(error => error.ToString()).ToList();
    }
}
