using System;
using SkiaSharp;

namespace DotTiled;

/// <summary>
/// The format of an image.
/// </summary>
public enum ImageFormat
{
  /// <summary>
  /// Portable Network Graphics.
  /// </summary>
  Png,

  /// <summary>
  /// Graphics Interchange Format.
  /// </summary>
  Gif,

  /// <summary>
  /// Joint Photographic Experts Group.
  /// </summary>
  Jpg,

  /// <summary>
  /// Windows Bitmap.
  /// </summary>
  Bmp
}

/// <summary>
/// Represents an image that is used by a tileset.
/// </summary>
public class Image
{
  /// <summary>
  /// The format of the image.
  /// </summary>
  public Optional<ImageFormat> Format { get; set; } = Optional.Empty;

  /// <summary>
  /// The reference to the image file.
  /// </summary>
  public Optional<string> Source { get; set; } = Optional.Empty;

  /// <summary>
  /// Defines a specific color that is treated as transparent.
  /// </summary>
  public Optional<TiledColor> TransparentColor { get; set; } = Optional.Empty;

  /// <summary>
  /// The image width in pixels, used for tile index correction when the image changes.
  /// </summary>
  public Optional<int> Width { get; set; } = Optional.Empty;

  /// <summary>
  /// The image height in pixels, used for tile index correction when the image changes.
  /// </summary>
  public Optional<int> Height { get; set; } = Optional.Empty;

  public Image() { }

  /// <summary>
  /// Loads an image from a specified file path.
  /// </summary>
  /// <param name="source">The specified file path.</param>
  /// <exception cref="ArgumentException">An invalid argument has been passed to the function.</exception>
  /// <exception cref="NotSupportedException">Tiled does not support the image file provided.</exception>
  public Image(string source) {
    if (string.IsNullOrEmpty(source)) {
      throw new ArgumentException("Source cannot be null or empty.", nameof(source));
    }
    if (!System.IO.File.Exists(source)) {
      throw new ArgumentException("Source file does not exist.", nameof(source));
    }

    Source = source;

    // Load the image using SkiaSharp.
    using var image = SKImage.FromEncodedData(source) ?? throw new ArgumentException("Could not load image from source. Valid images files are PNG, GIF, JPG, and BMP.", nameof(source));
    Width = image.Width;
    Height = image.Height;

    // Determine the image format using SKCodec.
    using var codec = SKCodec.Create(image.EncodedData);
    Format = codec.EncodedFormat switch {
      SKEncodedImageFormat.Png => ImageFormat.Png,
      SKEncodedImageFormat.Gif => ImageFormat.Gif,
      SKEncodedImageFormat.Jpeg => ImageFormat.Jpg,
      SKEncodedImageFormat.Bmp => ImageFormat.Bmp,
      _ => throw new NotSupportedException("Unsupported image format.")
    };
  }
}
