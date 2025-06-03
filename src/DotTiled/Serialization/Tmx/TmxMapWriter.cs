using System;
using System.Globalization;
using System.Xml;

namespace DotTiled.Serialization.Tmx
{
  public static class TmxMapWriter
  {
    public static bool WriteMap(in XmlWriter writer, in Map map)
    {
      writer.WriteStartDocument();
      writer.WriteStartElement("map");
      writer.WriteAttributeString("version", map.Version);
      writer.WriteAttributeString("tiledversion", map.TiledVersion.Value);
      writer.WriteAttributeString("orientation", map.Orientation.ToString().ToLowerInvariant());

      // The render order doesn't ToString cleanly, so we handle it manually.
      switch (map.RenderOrder) {
        case RenderOrder.RightDown:
          writer.WriteAttributeString("renderorder", "right-down");
          break;
        case RenderOrder.RightUp:
          writer.WriteAttributeString("renderorder", "right-up");
          break;
        case RenderOrder.LeftDown:
          writer.WriteAttributeString("renderorder", "left-down");
          break;
        case RenderOrder.LeftUp:
          writer.WriteAttributeString("renderorder", "left-up");
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(map), map.RenderOrder, "Unknown render order");
      }

      writer.WriteAttributeString("width", map.Width.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("height", map.Height.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("tilewidth", map.TileWidth.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("tileheight", map.TileHeight.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("infinite", map.Infinite ? "1" : "0");
      writer.WriteAttributeString("nextLayerId", map.NextLayerID.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("nextObjectId", map.NextObjectID.ToString(CultureInfo.InvariantCulture));

      foreach (var tileset in map.Tilesets)
      {
        writer.WriteStartElement("tileset");

        // External tileset, so it's straightforward to write.
        if (tileset.Source.HasValue)
        {
          writer.WriteAttributeString("firstgid", tileset.FirstGID.Value.ToString(CultureInfo.InvariantCulture));
          writer.WriteAttributeString("source", tileset.Source.Value);
        }
        else
        {
          TsxTilesetWriter.WriteTileset(writer, tileset);
        }
      }

      foreach (var layer in map.Layers)
      {
        // Get the internal type of the layer
        // Todo: other tile layer types
        if (layer is TileLayer tileLayer)
        {
          writer.WriteStartElement("layer");
          writer.WriteAttributeString("id", layer.ID.ToString(CultureInfo.InvariantCulture));
          writer.WriteAttributeString("name", layer.Name);
          writer.WriteAttributeString("width", tileLayer.Width.ToString(CultureInfo.InvariantCulture));
          writer.WriteAttributeString("height", tileLayer.Height.ToString(CultureInfo.InvariantCulture));

          if (tileLayer.Data.HasValue) {
            writer.WriteStartElement("data");
            if (tileLayer.Data.Value.Encoding.HasValue) {
              writer.WriteAttributeString("encoding", tileLayer.Data.Value.Encoding.Value.ToString().ToLowerInvariant());
            }
            if (tileLayer.Data.Value.Compression.HasValue) {
              writer.WriteAttributeString("compression", tileLayer.Data.Value.Compression.Value.ToString().ToLowerInvariant());
            }

            writer.WriteString(tileLayer.Data.Value.ToString());
            writer.WriteEndElement(); // data
          }

          writer.WriteEndElement(); // layer
        }
        else if (layer is ObjectLayer objectLayer)
        {
          writer.WriteStartElement("objectgroup");
          writer.WriteAttributeString("id", layer.ID.ToString(CultureInfo.InvariantCulture));
          writer.WriteAttributeString("name", layer.Name);

          // Write objects
          foreach (var obj in objectLayer.Objects)
          {
            writer.WriteStartElement("object");
            writer.WriteAttributeString("id", obj.ID.Value.ToString(CultureInfo.InvariantCulture));

            // Todo: other tile object types
            if (obj is TileObject tileObject) {
              writer.WriteAttributeString("gid", tileObject.GID.ToString(CultureInfo.InvariantCulture));
              writer.WriteAttributeString("x", tileObject.X.ToString(CultureInfo.InvariantCulture));
              writer.WriteAttributeString("y", tileObject.Y.ToString(CultureInfo.InvariantCulture));
              writer.WriteAttributeString("width", tileObject.Width.ToString(CultureInfo.InvariantCulture));
              writer.WriteAttributeString("height", tileObject.Height.ToString(CultureInfo.InvariantCulture));
            } else if (obj is PointObject pointObject) {
              writer.WriteAttributeString("x", pointObject.X.ToString(CultureInfo.InvariantCulture));
              writer.WriteAttributeString("y", pointObject.Y.ToString(CultureInfo.InvariantCulture));
              writer.WriteStartElement("point");
              writer.WriteEndElement(); // point
            }
            writer.WriteEndElement(); // object
          }
          writer.WriteEndElement(); // objectgroup
        }
      }

      writer.WriteEndElement();
      return true;
    }
  }
}
