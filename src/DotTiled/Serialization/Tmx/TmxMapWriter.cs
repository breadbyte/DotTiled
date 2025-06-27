using System;
using System.Globalization;
using System.IO;
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

      if (map.BackgroundColor.ToString() != "#00000000")
      {
        // Need to manually convert the color to hex format, because TiledColor returns ARGB, but backgroundcolor is RGB.
        var color = map.BackgroundColor;
        writer.WriteAttributeString("backgroundcolor", $"#{color.R:X2}{color.G:X2}{color.B:X2}");
      }

      writer.WriteAttributeString("nextLayerId", map.NextLayerID.ToString(CultureInfo.InvariantCulture));
      writer.WriteAttributeString("nextObjectId", map.NextObjectID.ToString(CultureInfo.InvariantCulture));

      if (map.Properties.Count != 0)
      {
        writer.WriteStartElement("properties");
        foreach (var property in map.Properties)
        {
          writer.WriteStartElement("property");
          writer.WriteAttributeString("name", property.Name);

          // string properties somehow don't have a type attribute, so we skip it
          if (property.Type != PropertyType.String)
            writer.WriteAttributeString("type", property.Type.ToString());

          switch (property.Type)
          {
            case PropertyType.String:
              // todo: WriteAttributeString is escaped. We don't want that
              writer.WriteAttributeString("value", (property as StringProperty).Value);
              break;
            case PropertyType.Int:
              writer.WriteAttributeString("value",
                (property as IntProperty).Value.ToString(CultureInfo.InvariantCulture));
              break;
            case PropertyType.Float:
              writer.WriteAttributeString("value",
                (property as FloatProperty).Value.ToString(CultureInfo.InvariantCulture));
              break;
            case PropertyType.Bool:
              writer.WriteAttributeString("value", (property as BoolProperty).Value.ToString());
              break;
            case PropertyType.Color:
              if (!(property as ColorProperty).Value.HasValue)
                writer.WriteAttributeString("value", string.Empty);
              else
                writer.WriteAttributeString("value", (property as ColorProperty).Value.Value.ToString());
              break;
            case PropertyType.File:
              writer.WriteAttributeString("value", (property as FileProperty).Value);
              break;
            case PropertyType.Object:
              writer.WriteAttributeString("value",
                (property as ObjectProperty).Value.ToString(CultureInfo.InvariantCulture));
              break;
            case PropertyType.Class:
            case PropertyType.Enum:
              throw new NotImplementedException();
            default:
              throw new NotSupportedException($"Property type {property.Type} is not supported.");
          }

          writer.WriteEndElement(); // property
        }

        writer.WriteEndElement(); // properties
      }

      foreach (var tileset in map.Tilesets)
      {
          TsxTilesetWriter.WriteTileset(writer, tileset);
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

            writer.WriteString("\n"); // Prettify
            switch (tileLayer.Data.Value.Encoding.Value)
            {
              case DataEncoding.Base64:
                MemoryStream compressionStream;
                if (!tileLayer.Data.Value.Compression.HasValue) {
                  using var bw = new BinaryWriter(compressionStream = new MemoryStream());
                  foreach (var gid in tileLayer.Data.Value.GlobalTileIDs.Value) {
                    bw.Write(gid);
                  }
                  bw.Flush();
                  compressionStream.Flush();
                  writer.WriteBase64(compressionStream.ToArray(), 0, (int)compressionStream.Length);
                  break;
                }

                // Todo: Decompression returns wrong data.
                switch (tileLayer.Data.Value.Compression.Value)
                {
                  case DataCompression.GZip:
                    {
                      using var rawStream = new MemoryStream();
                      using var bw = new BinaryWriter(rawStream);
                      foreach (var gid in tileLayer.Data.Value.GlobalTileIDs.Value)
                        bw.Write(gid);
                      bw.Flush();
                      compressionStream = new MemoryStream();
                      compressionStream.Flush();
                      using var gzipStream = new System.IO.Compression.GZipStream(compressionStream, System.IO.Compression.CompressionMode.Compress, false);
                      gzipStream.Write(rawStream.ToArray(), 0, (int)rawStream.Length);
                      gzipStream.Flush();
                      compressionStream.Flush();
                      writer.WriteString(Convert.ToBase64String(compressionStream.ToArray()));
                    }
                    break;
                  case DataCompression.ZLib:
                    {
                      using var rawStream = new MemoryStream();
                      using var bw = new BinaryWriter(rawStream);
                      foreach (var gid in tileLayer.Data.Value.GlobalTileIDs.Value)
                        bw.Write(gid);
                      bw.Flush();
                      compressionStream = new MemoryStream();
                      compressionStream.Flush();
                      using var gzipStream = new System.IO.Compression.ZLibStream(compressionStream, System.IO.Compression.CompressionMode.Compress, false);
                      gzipStream.Write(rawStream.ToArray(), 0, (int)rawStream.Length);
                      gzipStream.Flush();
                      compressionStream.Flush();
                      writer.WriteString(Convert.ToBase64String(compressionStream.ToArray()));
                    }
                    break;
                  case DataCompression.ZStd:
                    // Not implemented for the whole library yet
                    throw new NotImplementedException();
                    break;
                  default:
                    break;
                }
                break;
              case DataEncoding.Csv:
                // this, but add new line every width tiles
                // writer.WriteString(string.Join(",", tileLayer.Data.Value.GlobalTileIDs.Value.Select(gid => gid.ToString(CultureInfo.InvariantCulture))));
                for (int i = 0; i < tileLayer.Data.Value.GlobalTileIDs.Value.Length; i++)
                {
                  if (i > 0 && i % tileLayer.Width == 0)
                    writer.WriteString("\n");
                  writer.WriteString(tileLayer.Data.Value.GlobalTileIDs.Value[i].ToString(CultureInfo.InvariantCulture));
                  if (i < tileLayer.Data.Value.GlobalTileIDs.Value.Length - 1)
                    writer.WriteString(",");
                }
                break;
              default:
                throw new NotImplementedException($"Encoding {tileLayer.Data.Value.Encoding.Value} is not implemented.");
                break;
            }
            writer.WriteString("\n"); // Prettify
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

      writer.WriteEndElement(); // map
      return true;
    }
  }
}
