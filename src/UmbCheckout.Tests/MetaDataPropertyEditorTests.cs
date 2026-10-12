using System.Text.Json;
using UmbCheckout.Backoffice.ValueConverters;
using Moq;
using Xunit;
using UmbCheckout.Backoffice.Models;

namespace UmbCheckout.Tests
{
    public class MetaDataPropertyEditorTests
    {
        [Fact]
        public void JsonValueConverterTests()
        {
            //Arrange
            var metaData = new Dictionary<string, string>
            {
                { "Dictionary Key 1", "Dictionary Value 1" },
                { "Dictionary Key 2", "Dictionary Value 2" }
            };

            var jsonString = @"[
  {
    ""value"": ""Dictionary Value 1"",
    ""hasFocus"": true,
    ""name"": ""Dictionary Key 1""
  },
  {
    ""value"": ""Dictionary Value 2"",
    ""hasFocus"": true,
    ""name"": ""Dictionary Key 2""
  }
]";

            //Act
            var converter = new MetaDataValueConverter();
            var result = converter.ConvertSourceToIntermediate(null, null, jsonString, false);

            // Assert
            Assert.Equal(metaData, result);
        }

        [Fact]
        public void TypeValueConverterTests()
        {
            //Arrange
            var metaData = new List<UmbCheckoutMetaData>
            {
                new()
                {
                    Name = "Dictionary Key 1",
                    Value = "Dictionary Value 1"
                },
                new()
                {
                    Name = "Dictionary Key 2",
                    Value = "Dictionary Value 2"
                }
            };

            var metaDataDictionary = new Dictionary<string, string>
            {
                { "Dictionary Key 1", "Dictionary Value 1" },
                { "Dictionary Key 2", "Dictionary Value 2" }
            };

            var jsonString = JsonSerializer.Serialize(metaData);

            //Act
            var converter = new MetaDataValueConverter();
            var result = converter.ConvertSourceToIntermediate(null, null, jsonString, false);

            // Assert
            Assert.Equal(metaDataDictionary, result);
        }

        [Fact]
        public void ConvertsJsonNodeSource()
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse("[{\"name\":\"k\",\"value\":\"v\"}]");
            var result = new MetaDataValueConverter().ConvertSourceToIntermediate(null, null, node, false);
            Assert.Equal(new Dictionary<string, string> { ["k"] = "v" }, result);
        }

        [Fact]
        public void DuplicateNames_LastWins()
        {
            var result = new MetaDataValueConverter().ConvertSourceToIntermediate(null, null, "[{\"name\":\"k\",\"value\":\"1\"},{\"name\":\"k\",\"value\":\"2\"}]", false);
            Assert.Equal(new Dictionary<string, string> { ["k"] = "2" }, result);
        }

        [Fact]
        public void DeclaresDictionaryType()
            => Assert.Equal(typeof(Dictionary<string, string>), new MetaDataValueConverter().GetPropertyValueType(Mock.Of<Umbraco.Cms.Core.Models.PublishedContent.IPublishedPropertyType>()));
    }

}
