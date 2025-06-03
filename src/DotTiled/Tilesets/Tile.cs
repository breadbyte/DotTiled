using System;
using System.Collections.Generic;

namespace DotTiled;

/// <summary>
/// Represents a single tile in a tileset, when using a collection of images to represent the tileset.
/// <see href="https://doc.mapeditor.org/en/stable/reference/tmx-map-format/#tile">Tiled documentation for Tileset tiles</see>
/// </summary>
public class Tile : HasPropertiesBase
{
  /// <summary>
  /// The local tile ID within its tileset.
  /// </summary>
  public Optional<uint> ID { get; set; } = new();

  /// <summary>
  /// The class of the tile. Is inherited by tile objects
  /// </summary>
  public string Type { get; set; } = "";

  /// <summary>
  /// A percentage indicating the probability that this tile is chosen when it competes with others while editing with the terrain tool.
  /// </summary>
  public float Probability { get; set; } = 0f;

  /// <summary>
  /// The X position of the sub-rectangle representing this tile within the tileset image.
  /// </summary>
  public int X { get; set; } = 0;

  /// <summary>
  /// The Y position of the sub-rectangle representing this tile within the tileset image.
  /// </summary>
  public int Y { get; set; } = 0;

  /// <summary>
  /// The width of the sub-rectangle representing this tile within the tileset image.
  /// </summary>
  public required int Width { get; set; }

  /// <summary>
  /// The height of the sub-rectangle representing this tile within the tileset image.
  /// </summary>
  public required int Height { get; set; }

  /// <summary>
  /// Tile properties.
  /// </summary>
  public List<IProperty> Properties { get; set; } = [];

  /// <inheritdoc/>
  public override IList<IProperty> GetProperties() => Properties;

  /// <summary>
  /// The image representing this tile. Only used for tilesets that composed of a collection of images.
  /// </summary>
  public Optional<Image> Image { get; set; } = Optional.Empty;

  /// <summary>
  /// Used when the tile contains e.g. collision information.
  /// </summary>
  public Optional<ObjectLayer> ObjectLayer { get; set; } = Optional.Empty;

  /// <summary>
  /// The animation frames for this tile.
  /// </summary>
  public List<Frame> Animation { get; set; } = [];

  /// <summary>
  /// Default constructor.
  /// </summary>
  public Tile() { }

  /// <summary>
  /// Creates a new Tile from an image.
  /// </summary>
  /// <param name="image">The image representing the tile.</param>
  public Tile(Image image)
  {
    if (!image.Width.HasValue || !image.Height.HasValue)
      throw new ArgumentException("Image must have a defined width and height.", nameof(image));

    Image = image;
    Width = image.Width.Value;
    Height = image.Height.Value;
  }

  public void SetAnimation(Tile[] tiles, int frameDuration = 100)
  {
    if (tiles == null || tiles.Length == 0)
      throw new ArgumentException("Tiles array cannot be null or empty.", nameof(tiles));

    Animation.Clear();
    foreach (var tile in tiles)
    {
      if (tile.ID.HasValue)
      {
        Animation.Add(new Frame
        {
          TileID = tile.ID.Value,
          Duration = frameDuration
        });
      }
      else
      {
        throw new ArgumentException("Tile ID must be defined for animation frames.", nameof(tiles));
      }
    }
  }

  public void SetAnimation((Tile tile, int duration)[] tilesWithDurations)
  {
    Animation.Clear();
    foreach (var (tile, duration) in tilesWithDurations)
    {
      if (tile.ID.HasValue)
      {
        Animation.Add(new Frame
        {
          TileID = tile.ID.Value,
          Duration = duration
        });
      }
      else
      {
        throw new ArgumentException("Tile ID must be defined for animation frames.", nameof(tilesWithDurations));
      }
    }
  }
}
