using CarinaStudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Carina.PixelViewer.Media.ImageRenderers
{
	/// <summary>
	/// Base implementation of <see cref="IImageRenderer"/> to rendering ARGB_4444 format image.
	/// </summary>
	class Argb4444ImageRenderer : SinglePlaneImageRenderer
	{
		/// <summary>
		/// Intiaize new <see cref="Argb4444ImageRenderer"/> instance.
		/// </summary>
		public Argb4444ImageRenderer() : base(new ImageFormat(ImageFormatCategory.ARGB, "ARGB_4444", true, new ImagePlaneDescriptor(2), new string[]{ "ARGB4444", "ARGB_4444"}))
		{ }


		// Render.
		protected override unsafe ImageRenderingResult OnRender(IImageDataSource source, Stream imageStream, IBitmapBuffer bitmapBuffer, ImageRenderingOptions renderingOptions, IList<ImagePlaneOptions> planeOptions, CancellationToken cancellationToken)
		{
			// get parameters
			var width = bitmapBuffer.Width;
			var height = bitmapBuffer.Height;
			var pixelStride = planeOptions[0].PixelStride;
			var rowStride = planeOptions[0].RowStride;
			if (width <= 0 || height <= 0 || pixelStride <= 0 || (pixelStride * width) > rowStride)
				throw new ArgumentException("Invalid dimensions, pixel-stride or row-stride.");

			// select byte ordering
			Func<byte, byte, int> pixelConversionFunc = renderingOptions.ByteOrdering switch
			{
				ByteOrdering.LittleEndian => (b1, b2) => (b2 << 8) | b1,
				_ => (b1, b2) => (b1 << 8) | b2,
			};

			// render
			bitmapBuffer.Memory.Pin((bitmapBaseAddress) =>
			{
				byte[] row = new byte[rowStride];
				fixed (byte* rowPtr = row)
				{
					var bitmapRowPtr = (byte*)bitmapBaseAddress;
					for (var y = height; y > 0; --y, bitmapRowPtr += bitmapBuffer.RowBytes)
					{
						var isLastRow = (imageStream.Read(row, 0, rowStride) < rowStride || y == 1);
						var pixelPtr = rowPtr;
						var bitmapPixelPtr = bitmapRowPtr;
						for (var x = width; x > 0; --x, pixelPtr += pixelStride, bitmapPixelPtr += 4)
						{
							var argb4444 = pixelConversionFunc(pixelPtr[0], pixelPtr[1]);
							var a = (argb4444 >> 4) & 0xf;
							var r = (argb4444 >> 4) & 0xf;
							var g = (argb4444 >> 4) & 0xf;
							var b = argb4444 & 0xf;
							bitmapPixelPtr[0] = (byte)(b * 17); // extend from 5 bits to 8 bits
							bitmapPixelPtr[1] = (byte)(g * 17); // extend from 6 bits to 8 bits
							bitmapPixelPtr[2] = (byte)(r * 17); // extend from 5 bits to 8 bits
							bitmapPixelPtr[3] = (byte)(a * 17);
						}
						if (isLastRow || cancellationToken.IsCancellationRequested)
							break;
						if (!isLastRow)
							Array.Clear(row, 0, rowStride);
					}
				}
			});

			// complete
			return new ImageRenderingResult();
		}
	}
}
