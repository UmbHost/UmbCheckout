using UmbCheckout.Backoffice.ValueConverters;
using Xunit;
using UmbCheckout.Backoffice.Models;
using Umbraco.Cms.Core.Serialization;

namespace UmbCheckout.Tests
{
    public class MetaDataPropertyEditorTests
    {
        private readonly IJsonSerializer _jsonSerializer;

        public MetaDataPropertyEditorTests(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

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
            var converter = new MetaDataValueConverter(_jsonSerializer);
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

            var jsonString = _jsonSerializer.Serialize(metaData);

            //Act
            var converter = new MetaDataValueConverter(_jsonSerializer);
            var result = converter.ConvertSourceToIntermediate(null, null, jsonString, false);

            // Assert
            Assert.Equal(metaDataDictionary, result);
        }
    }

}