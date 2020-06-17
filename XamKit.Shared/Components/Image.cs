using System;
using System.Reflection;

// Xamarin
using Xamarin.Forms;

// Skia
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace XamKit
{
    public enum Aspect { AspectFill, AspectFit, None }

    public class Image : SKCanvasView
    {
        private SKImage _image = null;

        #region BindingProperties

        /// <summary>
        /// Image color
        /// </summary>
        public static readonly BindableProperty ColorProperty = 
			BindableProperty.Create(nameof(Color), typeof(Color), typeof(Image), Color.Black);

		public Color Color
        {
            get { return (Color) GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Image source resource key
        /// </summary>
        public static readonly BindableProperty ResourceKeyProperty =
            BindableProperty.Create("ResourceKey", typeof(string), typeof(Image));

        public string ResourceKey
        {
            get { return (string)GetValue(ResourceKeyProperty); }
            set { SetValue(ResourceKeyProperty, value); }
        }

        /// <summary>
        /// Assembly name. If null then use ImageResourceUtils.DefaultAssemblyName.
        /// </summary>
        public static readonly BindableProperty AssemblyNameProperty =
            BindableProperty.Create("AssemblyName", typeof(string), typeof(Image));

        public string AssemblyName
        {
            get { return (string)GetValue(AssemblyNameProperty); }
            set { SetValue(AssemblyNameProperty, value); }
        }

        /// <summary>
        /// Image blur scale
        /// </summary>
        public static readonly BindableProperty BlurProperty =
            BindableProperty.Create("Blur", typeof(double), typeof(Image));

        public double Blur
        {
            get { return (double)GetValue(BlurProperty); }
            set { SetValue(BlurProperty, value); }
        }

        /// <summary>
        /// Image aspect
        /// </summary>
        public static readonly BindableProperty AspectProperty =
            BindableProperty.Create("Aspect", typeof(Aspect), typeof(Image), Aspect.AspectFill);

        public Aspect Aspect
        {
            get { return (Aspect)GetValue(AspectProperty); }
            set { SetValue(AspectProperty, value); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Actual image size in Xamarin coordinates
        /// </summary>
        public Size ActualImageSize { get; protected set; }

        /// <summary>
        /// Actual assembly name. If null, then use default assembly
        /// </summary>
        protected string ActualAssemblyName
        {
            get
            {
                if (string.IsNullOrEmpty(AssemblyName))
                {
                    return ImageResourceUtils.DefaultAssemblyName;
                }
                else
                {
                    return AssemblyName;
                }
            }
        }

        #endregion

        /// <summary>
        /// Update when properties changes
        /// </summary>
        /// <param name="propertyName">Changed property name</param>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ResourceKeyProperty.PropertyName || propertyName == AssemblyNameProperty.PropertyName)
            {
                _image = null;

                if (string.IsNullOrEmpty(ActualAssemblyName) == false && string.IsNullOrEmpty(ResourceKey) == false)
                {
                    InvalidateSurface();
                }
            }
        }

        /// <summary>
        /// Paint svg image
        /// </summary>
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            if (string.IsNullOrEmpty(ResourceKey) || string.IsNullOrEmpty(AssemblyName))
            {
                return;
            }
             
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (_image == null)
            {
                Assembly assembly = Assembly.Load(new AssemblyName(ActualAssemblyName));

                using (var stream = assembly.GetManifestResourceStream(ActualAssemblyName + "." + ResourceKey))
                {
                    SKBitmap bitmap = SKBitmap.Decode(stream);
                    _image = SKImage.FromBitmap(bitmap);
                }
            }

            float deviceScale = (float)(info.Width / Width);
            float width = (float)(Width * deviceScale);
            float height = (float)(Height * deviceScale);

            double scale = 1;
            
            if (Aspect == Aspect.AspectFit)
            {
                // Scale based on min dimension and keep aspect ratio
                scale = Math.Min(width, height) / Math.Max(_image.Height, _image.Width);
            }
            else if (Aspect == Aspect.AspectFill)
            {
                // Scale based on max dimension and keep aspect ratio
                scale = Math.Max(width, height) / Math.Min(_image.Height, _image.Width);

            }
            else
            {
                // no scale
            }

            ActualImageSize = new Size(Math.Round((_image.Width * scale) / deviceScale, MidpointRounding.ToEven), Math.Round((_image.Height * scale) / deviceScale, MidpointRounding.ToEven));

            var paint = new SKPaint();

            if (Blur > 0)
            {
                paint.ImageFilter = SKImageFilter.CreateBlur((float)Blur, (float)Blur, null, null);
            }

            if (ActualImageSize.Height > Height)
            {
                canvas.Translate(0, -(float)((ActualImageSize.Height - Height) / 2) * deviceScale);
            }

            canvas.DrawImage(_image, SKRect.Create(new SKPoint(0, 0), new SKSize((float)ActualImageSize.Width * deviceScale, (float)ActualImageSize.Height * deviceScale)), paint);
        }
	}
}