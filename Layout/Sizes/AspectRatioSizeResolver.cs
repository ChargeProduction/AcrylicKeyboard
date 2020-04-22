using System;
using System.Windows;
using GlmSharp;

namespace AcrylicKeyboard.Layout.Sizes
{
    public class AspectRatioSizeResolver : IKeyboardSizeResolver
    {
        private float aspectRatio;
        private bool allowShrink;

        public AspectRatioSizeResolver(float aspectRatio, bool allowShrink = true)
        {
            this.aspectRatio = aspectRatio;
            this.allowShrink = allowShrink;
        }

        public (Rect, int) ResolveSize(Size sourceSize)
        {
            if (sourceSize.Width > 0 && sourceSize.Height > 0)
            {
                float sourceAspectRatio = (float) (sourceSize.Width / sourceSize.Height);
                float destinationHeight = 0;
                float destinationWidth = 0;
                if (sourceAspectRatio < aspectRatio)
                {
                    if (allowShrink)
                    {
                        destinationHeight = (float)sourceSize.Height;
                        destinationWidth = (float)sourceSize.Width;
                    }
                    else
                    {
                        var adjustedHeightFactor = sourceAspectRatio / aspectRatio;
                        destinationHeight = (float)sourceSize.Height * adjustedHeightFactor;
                        destinationWidth = destinationHeight * aspectRatio;
                    }
                }
                else
                {
                    destinationHeight = (float)sourceSize.Height;
                    destinationWidth = destinationHeight * aspectRatio;
                }

                var diameter = new vec2(destinationWidth, destinationHeight).Length;
                int keyGap = (int)Math.Max(Math.Min(diameter / 300, 5), 0) + 1;
                
                float offsetX = ((float)sourceSize.Width - destinationWidth) / 2f;
                float offsetY = ((float)sourceSize.Height - destinationHeight) / 2f;
                return (new Rect(offsetX, offsetY, destinationWidth, destinationHeight), keyGap);
            }
            return (Rect.Empty, 0);
        }

        public float AspectRatio
        {
            get => aspectRatio;
            set => aspectRatio = value;
        }
    }
}