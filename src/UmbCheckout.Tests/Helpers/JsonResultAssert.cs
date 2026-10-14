using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>Reads a <see cref="JsonResult"/> as the System.Text.Json result executor writes it, and checks a response grew only additively.</summary>
    internal static class JsonResultAssert
    {
        /// <summary>The JSON the executor would write: the value's runtime type, with the result's own serializer options.</summary>
        public static JsonNode Body(IActionResult result)
        {
            var json = Assert.IsType<JsonResult>(result);
            var options = Assert.IsType<JsonSerializerOptions>(json.SerializerSettings);
            return JsonNode.Parse(JsonSerializer.Serialize(json.Value, json.Value?.GetType() ?? typeof(object), options)).Required();
        }

        /// <summary>
        /// Every property of <paramref name="before"/> is in <paramref name="after"/> with the same JSON, recursively; the only new
        /// properties are named in <paramref name="added"/>.
        /// </summary>
        public static void Additive(JsonNode? before, JsonNode? after, params string[] added)
        {
            switch (before)
            {
                case JsonObject b:
                    var a = Assert.IsType<JsonObject>(after);
                    foreach (var (key, value) in b)
                    {
                        Assert.True(a.ContainsKey(key), $"'{key}' is missing");
                        Additive(value, a[key], added);
                    }

                    foreach (var (key, _) in a)
                    {
                        Assert.True(b.ContainsKey(key) || added.Contains(key), $"'{key}' is new but not expected");
                    }

                    break;
                case JsonArray b:
                    var array = Assert.IsType<JsonArray>(after);
                    Assert.Equal(b.Count, array.Count);
                    for (var i = 0; i < b.Count; i++)
                    {
                        Additive(b[i], array[i], added);
                    }

                    break;
                default:
                    Assert.Equal(before?.ToJsonString(), after?.ToJsonString());
                    break;
            }
        }
    }
}
