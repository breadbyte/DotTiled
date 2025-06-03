using System;
using System.IO;

namespace DotTiled.Serialization;

/// <summary>
/// Interface for writing a tileset to a stream.
/// </summary>
public interface ITilesetWriter : IDisposable
{
  /// <summary>
  /// Writes a tileset to a <see cref="Stream">Stream</see>.
  /// </summary>
  /// <returns>The stream containing the serialized tileset.</returns>
  Stream WriteTileset(Tileset tileset);
}
