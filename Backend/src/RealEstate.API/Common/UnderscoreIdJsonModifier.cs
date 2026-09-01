using System.Text.Json.Serialization.Metadata;

namespace RealEstate.WebApi.Common;

/// <summary>
/// The frontend was built for a Mongo-style API and reads every resource id as `_id`.
/// This modifier renames the serialized name of any property called `id` to `_id`
/// across all response DTOs (including nested ones).
/// </summary>
public static class UnderscoreIdJsonModifier
{
    public static void RenameIdToUnderscoreId(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (var property in typeInfo.Properties)
        {
            if (string.Equals(property.Name, "id", StringComparison.OrdinalIgnoreCase))
                property.Name = "_id";
        }
    }
}
