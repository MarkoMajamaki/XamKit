using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

// Xamarin
using Xamarin.Forms;

// Skiasharp
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace XamKit
{
    /// <summary>
    /// SVG image
    /// </summary>
    public class SvgImage : SKCanvasView
    {
        private SkiaSharp.Extended.Svg.SKSvg m_svg = null;

        /// <summary>
        /// Cache for SVG icons. Each SVG is kept in memory only once even if it's used multple places in application.
        /// </summary>
        private static readonly IDictionary<string, SkiaSharp.Extended.Svg.SKSvg> SvgCache = new Dictionary<string, SkiaSharp.Extended.Svg.SKSvg>();

        protected float DeviceScale { get; private set; } = 1;

        #region Properties

        /// <summary>
        /// File resource key (without assembly name)
        /// </summary>
        public static readonly BindableProperty ResourceKeyProperty =
            BindableProperty.Create(nameof(ResourceKey), typeof(string), typeof(SvgImage), default(string));
        
        public string ResourceKey
        {
            get { return (string)GetValue(ResourceKeyProperty); }
            set { SetValue(ResourceKeyProperty, value); }
        }

        /// <summary>
        /// The assembly name containing the svg file
        /// </summary>
        public static readonly BindableProperty AssemblyNameProperty =
            BindableProperty.Create(nameof(AssemblyName), typeof(string), typeof(SvgImage), default(string));
        
        public string AssemblyName
        {
            get { return (string)GetValue(AssemblyNameProperty); }
            set { SetValue(AssemblyNameProperty, value); }
        }

        /// <summary>
        /// Image override color
        /// </summary>
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create("Color", typeof(Color), typeof(SvgImage), Color.Black, propertyChanged: OnColorChanged);

        private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as SvgImage).InvalidateSurface();
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }


        /// <summary>
        /// Icon horizontal alignment
        /// </summary>
        public static readonly BindableProperty IconHorizontalOptionsProperty =
            BindableProperty.Create("IconHorizontalOptions", typeof(LayoutOptions), typeof(SvgImage), LayoutOptions.Center);
        
        public LayoutOptions IconHorizontalOptions
        {
            get { return (LayoutOptions)GetValue(IconHorizontalOptionsProperty); }
            set { SetValue(IconHorizontalOptionsProperty, value); }
        }

        /// <summary>
        /// Icon vertical alignment
        /// </summary>
        public static readonly BindableProperty IconVerticalOptionsProperty =
            BindableProperty.Create("IconVerticalOptions", typeof(LayoutOptions), typeof(SvgImage), LayoutOptions.Center);
        
        public LayoutOptions IconVerticalOptions
        {
            get { return (LayoutOptions)GetValue(IconVerticalOptionsProperty); }
            set { SetValue(IconVerticalOptionsProperty, value); }
        }

        #endregion

        public SvgImage()
        {            
        }

        /// <summary>
        /// Paint svg image
        /// </summary>
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            e.Surface.Canvas.Clear();

            if (string.IsNullOrEmpty(ResourceKey) || string.IsNullOrEmpty(AssemblyName))
            {
                return;
            }

            // Get device pixel intencity scale
            DeviceScale = (float)(e.Info.Width / Width);

            if (m_svg == null)
            {
                m_svg = GetSvgImage(AssemblyName, ResourceKey);
            }

            SKPoint position = CalculateTranslation(e.Info);
            float scale = CalculateScale(m_svg.Picture.CullRect.Size, WidthRequest, HeightRequest) * DeviceScale;

            SKMatrix matrix = new SKMatrix();
            matrix.SetScaleTranslate(scale, scale, position.X, position.Y);

            using (var paint = new SKPaint())
            {
                // paint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn);
                paint.Style = SKPaintStyle.Fill;
                paint.IsAntialias = true;
                paint.FilterQuality = SKFilterQuality.None;
                e.Surface.Canvas.DrawPicture(m_svg.Picture, ref matrix, paint);
            }
        }

        /// <summary>
        /// Load SVG image from resources
        /// </summary>
        public static SkiaSharp.Extended.Svg.SKSvg GetSvgImage(string assemblyName, string resourceKey)
        {
            SkiaSharp.Extended.Svg.SKSvg svg = null;

            string resourceId = assemblyName + "." + resourceKey;

            if (!SvgImage.SvgCache.TryGetValue(resourceId, out svg))
            {
                Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));

                using (Stream stream = assembly.GetManifestResourceStream(resourceId))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException($"SvgIcon : could not load SVG file {resourceId} in assembly {assembly}. Make sure the ID is correct, the file is there and it is set to Embedded Resource build action.");
                    }

                    svg = new SkiaSharp.Extended.Svg.SKSvg();
                    svg.Load(stream);
                    SvgImage.SvgCache.Add(resourceId, svg);
                }
            }

            return svg;
        }

        /// <summary>
        /// Get canvas scale to fill available space
        /// </summary>
        public static float CalculateScale(SKSize svgIconSize, double iconWidthRequest, double iconHeightRequest)
        {
            float svgMax = Math.Max(svgIconSize.Width, svgIconSize.Height);
            float iconMin = 0;

            if (iconWidthRequest >= 0 && iconHeightRequest >= 0)
            {
                iconMin = Math.Min((float)iconWidthRequest, (float)iconHeightRequest);
            }
            else if (iconWidthRequest >= 0)
            {
                iconMin = (float)iconWidthRequest;
            }
            else if (iconHeightRequest >= 0)
            {
                iconMin = (float)iconHeightRequest;
            }
            else
            {
                iconMin = svgMax;
            }

            float scale = iconMin / svgMax;

            return scale;
        }

        /// <summary>
        /// Calculate translation
        /// </summary>
        private SKPoint CalculateTranslation(SKImageInfo info)
        {
            float dx = 0;
            float dy = 0;

            float canvasMin = Math.Min(info.Width, info.Height);
            float svgMax = Math.Max(m_svg.Picture.CullRect.Width, m_svg.Picture.CullRect.Height);
            float scale = canvasMin / svgMax;

            switch (IconHorizontalOptions.Alignment)
            {
                case LayoutAlignment.Start:
                    dx = 0;
                    break;
                case LayoutAlignment.Center:
                    dx = (info.Width - (m_svg.Picture.CullRect.Width * scale)) / 2;
                    break;
                case LayoutAlignment.End:
                    dx = info.Width - (m_svg.Picture.CullRect.Width * scale);
                    break;
                case LayoutAlignment.Fill:
                    dx = 0;
                    break;
            }

            switch (IconVerticalOptions.Alignment)
            {
                case LayoutAlignment.Start:
                    dy = 0;
                    break;
                case LayoutAlignment.Center:
                    dy = (info.Height - (m_svg.Picture.CullRect.Height * scale)) / 2;
                    break;
                case LayoutAlignment.End:
                    dy = info.Height - (m_svg.Picture.CullRect.Height * scale);
                    break;
                case LayoutAlignment.Fill:
                    dy = 0;
                    break;
            }

            return new SKPoint(dx, dy);
        }

        /// <summary>
        /// Update when properties changes
        /// </summary>
        /// <param name="propertyName">Changed property name</param>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ResourceKeyProperty.PropertyName ||
                propertyName == AssemblyNameProperty.PropertyName ||
                propertyName == ColorProperty.PropertyName)
            {
                InvalidateSurface();
            }
        }
    }
}

