using UmbCheckout.Backoffice.ValueConverters;
using Xunit;
using Umbraco.Cms.Infrastructure.Serialization;
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
            var converter = new MetaDataValueConverter(new JsonNetSerializer());
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

            var jsonSerializer = new JsonNetSerializer();
            var jsonString = jsonSerializer.Serialize(metaData);

            //Act
            var converter = new MetaDataValueConverter(new JsonNetSerializer());
            var result = converter.ConvertSourceToIntermediate(null, null, jsonString, false);

            // Assert
            Assert.Equal(metaDataDictionary, result);
        }
    }

}