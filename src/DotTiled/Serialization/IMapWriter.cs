using System;
using System.IO;

namespace DotTiled.Serialization;

/// <summary>
/// Interface for writing a map to a Stream.
/// </summary>
public interface IMapWriter : IDisposable
{
  /// <summary>
  /// Writes a map to a <see cref="Stream"/>Stream</see>.
  /// </summary>
  /// <returns>The stream containing the serialized tileset.</returns>
  Stream WriteMap(Map map);
}
