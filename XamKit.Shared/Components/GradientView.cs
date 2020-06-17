using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace XamKit
{
    public partial class GradientView : SKCanvasView
    {
        private Color _startColor = Color.Transparent;
        private Color _endColor = Color.Transparent;
        private bool _horizontal = false;

        public Color StartColor
        { 
            get
            {
                return _startColor;
            }
            set
            {
                _startColor = value;
                InvalidateSurface();
            }
        }

        public Color EndColor
        {
            get
            {
                return _endColor;
            }
            set
            {
                _endColor = value;
                InvalidateSurface();
            }
        }

        public bool Horizontal
        {
            get
            {
                return _horizontal;
            }
            set
            {
                _horizontal = value;
                InvalidateSurface();
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (StartColor != Color.Transparent || EndColor != Color.Transparent)
            {
                var colors = new SKColor[] { StartColor.ToSKColor(), EndColor.ToSKColor() };
                SKPoint startPoint = new SKPoint(0, 0);
                SKPoint endPoint = Horizontal ? new SKPoint(info.Width, 0) : new SKPoint(0, info.Height);

                var shader = SKShader.CreateLinearGradient(startPoint, endPoint, colors, null, SKShaderTileMode.Clamp);

                SKPaint paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Shader = shader
                };

                canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), paint);
            }
        }
    }
}