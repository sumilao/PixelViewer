﻿using System;

namespace Carina.PixelViewer.Media.ImageRenderers
{
    /// <summary>
    /// <see cref="IImageRenderer"/> which supports rendering image with 16-bit YUV444p based format.
    /// </summary>
    class P416ImageRenderer : BaseYuv444p16ImageRenderer
    {
        public P416ImageRenderer() : base(new ImageFormat(ImageFormatCategory.YUV, "P416", true, new ImagePlaneDescriptor[] {
            new ImagePlaneDescriptor(2),
            new ImagePlaneDescriptor(2),
            new ImagePlaneDescriptor(2),
        }), 16)
        { }


        // Select UV component.
        protected override void SelectUV(ushort uv1, ushort uv2, out ushort u, out ushort v)
        {
            u = uv1;
            v = uv2;
        }
    }
}
