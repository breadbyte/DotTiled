using System.Globalization;
using System.Xml;
using DotTiled.Serialization.Tmx;

namespace DotTiled.Tests
{
  public class TmxMapWriterTests
  {
    public static IEnumerable<object[]> Maps => TestData.MapTests;
    [Theory]
    [MemberData(nameof(Maps))]
    public void WriterOutputsTheSameTmxFile(string testDataFile, Func<string, Map> expectedMap, IReadOnlyCollection<ICustomTypeDefinition> customTypeDefinitions)
    {
      // Code copied from TmxMapReaderTests.cs
      #region MapReaderSetup

      // Arrange
      testDataFile += ".tmx";
      var fileDir = Path.GetDirectoryName(testDataFile);
      using var reader = TestData.GetXmlReaderFor(testDataFile);
      Template ResolveTemplate(string source)
      {
        using var xmlTemplateReader = TestData.GetXmlReaderFor($"{fileDir}/{source}");
        using var templateReader = new TxTemplateReader(xmlTemplateReader, ResolveTileset, ResolveTemplate, ResolveCustomType);
        return templateReader.ReadTemplate();
      }
      Tileset ResolveTileset(string source)
      {
        using var xmlTilesetReader = TestData.GetXmlReaderFor($"{fileDir}/{source}");
        using var tilesetReader = new TsxTilesetReader(xmlTilesetReader, ResolveTileset, ResolveTemplate, ResolveCustomType);
        return tilesetReader.ReadTileset();
      }
      Optional<ICustomTypeDefinition> ResolveCustomType(string name)
      {
        if (customTypeDefinitions.FirstOrDefault(ctd => ctd.Name == name) is ICustomTypeDefinition ctd)
        {
          return new Optional<ICustomTypeDefinition>(ctd);
        }

        return Optional.Empty;
      }
      using var mapReader = new TmxMapReader(reader, ResolveTileset, ResolveTemplate, ResolveCustomType);

      // Act
      var map = mapReader.ReadMap();

      #endregion

      // Original source for the test data file
      var src = TestData.GetRawStringFor(testDataFile);

      // Write the map to a string using TmxMapWriter
      Stream internalStream = new MemoryStream();
      XmlWriter xmlWriter = XmlWriter.Create(internalStream, new XmlWriterSettings() {Indent = true});
      TmxMapWriter.WriteMap(xmlWriter, map);
      xmlWriter.Flush();

      // Reset the internal stream position and read the output
      internalStream.Position = 0;
      using var stringReader = new StreamReader(internalStream);
      var output = stringReader.ReadToEnd();

      // Assert
      Assert.NotNull(map);
      Assert.NotNull(output);
      Assert.NotEmpty(output);
      Assert.Equal<string>(src, output, StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols));
    }
  }
}
