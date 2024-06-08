using System.Text.Json;

namespace Beseler.Shared.Extensions;
public static class JsonSerializerOptionsExt
{
    public static JsonSerializerOptions Web { get; } = new JsonSerializerOptions(JsonSerializerDefaults.Web);
}
