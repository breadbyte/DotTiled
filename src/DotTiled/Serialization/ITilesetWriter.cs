using System;
using System.Xml;

namespace DotTiled.Serialization;

/// <summary>
/// Interface for writing a tileset to an XmlWriter.
/// </summary>
public interface ITilesetWriter : IDisposable
{
  /// <summary>
  /// Writes a tileset to an <see cref="XmlWriter">XmlWriter.</see>.
  /// </summary>
  /// <returns>true if the tileset has been written successfully.</returns>
  bool WriteTileset(in XmlWriter writer, in Tileset tileset);
}
