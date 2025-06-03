using System;
using System.Globalization;
using System.Xml;

namespace DotTiled.Serialization.Tmx
{
  public static class TsxTilesetWriter
  {
    public static bool WriteTilesetFile(in XmlWriter writer, in Tileset tileset)
    {
      writer.WriteStartDocument();
      WriteTileset(writer, tileset);
      writer.WriteEndDocument();
      return true;
    }

    public static bool WriteTileset(in XmlWriter writer, in Tileset tileset)
    {
      writer.WriteStartElement("tileset");
      writer.WriteAttributeString("version", tileset.Version.ToString());
      writer.WriteAttributeString("tiledversion", tileset.TiledVersion.ToString());
      writer.WriteAttributeString("name", tileset.Name);
      writer.WriteAttributeString("tilewidth", tileset.TileWidth.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("tileheight", tileset.TileHeight.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("tilecount", tileset.TileCount.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("columns", tileset.Columns.ToString(CultureInfo.InvariantCulture));
      writer.WriteStartElement("grid");
      writer.WriteAttributeString("orientation", tileset.Grid.Value.Orientation.ToString());
      writer.WriteAttributeString("width", tileset.Grid.Value.Width.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("height", tileset.Grid.Value.Height.ToString(CultureInfo.InvariantCulture));
      writer.WriteEndElement(); // grid

      foreach (var tile in tileset.Tiles)
      {
        writer.WriteStartElement("tile");
        writer.WriteAttributeString("id", tile.ID.ToString());
        if (tile.Image != null)
        {
          writer.WriteStartElement("image");
          writer.WriteAttributeString("source", tile.Image.Value.Source.Value);
          writer.WriteAttributeString("width", tile.Image.Value.Width.ToString());
          writer.WriteAttributeString("height", tile.Image.Value.Height.ToString());
          writer.WriteEndElement(); // image
        }
        if (tile.Properties.Count > 0)
        {
          foreach (var property in tile.Properties)
          {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", property.Name);
            writer.WriteAttributeString("type", property.Type.ToString());

            _ = property switch
            {
              BoolProperty boolProp => boolProp.Value.ToString(),
              IntProperty intProp => intProp.Value.ToString(CultureInfo.InvariantCulture),
              FloatProperty floatProp => floatProp.Value.ToString(CultureInfo.InvariantCulture),
              StringProperty stringProp => stringProp.Value,
              ColorProperty colorProp => colorProp.Value.HasValue ? colorProp.Value.Value.ToString() : string.Empty,
              FileProperty fileProp => fileProp.Value,
              ObjectProperty objectProp => objectProp.Value.ToString(CultureInfo.InvariantCulture),
              _ => throw new NotImplementedException($"Writer not implemented for property type: {property.GetType().Name}")
            };

            writer.WriteEndElement(); // property
          }
        }
        writer.WriteEndElement(); // tile
      }

      writer.WriteEndElement(); // tileset
      return true;
    }
  }
}
