using System;
using System.Reflection;

// Xamarin
using Xamarin.Forms;

// Skia
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Diagnostics;

namespace XamKit
{
    /// <summary>
    /// How image behaves when image edge is reached
    /// </summary>
    public enum ParallaxImageEdgeBehavior { Mirror, Default }

    /// <summary>
    /// Parallax background
    /// </summary>
    public class ParallaxImage : Layout<View>
    {
		// Parallax source element
		private IParallaxSource m_parallaxSourceView = null;

        private Rectangle m_parallaxViewportLocation = new Rectangle();

        private Image m_image1 = null;
        private Image m_image2 = null;

        private Size m_imageSize = Size.Zero;
        private bool m_isImageSizeValid = true;
        private Size m_availableSize = Size.Zero;

        private Assembly m_assembly = null;
        private SKImage m_skImage = null;

        #region Bindable properties

        /// <summary>
        /// Source vertical offset
        /// </summary>
        public static readonly BindableProperty SourceProperty =
			BindableProperty.Create("Source", typeof(View), typeof(ParallaxImage), propertyChanged: OnSourceChanged);

		private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
            (bindable as ParallaxImage).OnSourceChanged(oldValue as View, newValue as View);
		}

        public View Source
		{
			get { return (View)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		/// <summary>
		/// How image behaves when image edge is reached
		/// </summary>
		public static readonly BindableProperty ImageEdgeBehaviorProperty =
            BindableProperty.Create("ImageEdgeBehavior", typeof(ParallaxImageEdgeBehavior), typeof(ParallaxImage), ParallaxImageEdgeBehavior.Default);
        
        public ParallaxImageEdgeBehavior ImageEdgeBehavior
		{
			get { return (ParallaxImageEdgeBehavior)GetValue(ImageEdgeBehaviorProperty); }
			set { SetValue(ImageEdgeBehaviorProperty, value); }
		}

		/// <summary>
		/// A value of the enumeration that determines how the horizontal source offset and image are interpreted.
		/// </summary>
		public static readonly BindableProperty HorizontalShiftRatioProperty =
			BindableProperty.Create("HorizontalShiftRatio", typeof(double), typeof(ParallaxImage), 0.25);

		public double HorizontalShiftRatio
		{
			get { return (double)GetValue(HorizontalShiftRatioProperty); }
			set { SetValue(HorizontalShiftRatioProperty, value); }
		}

		/// <summary>
		/// A value of the enumeration that determines how the vertical source offset and image are interpreted.
		/// </summary>
		public static readonly BindableProperty VerticalShiftRatioProperty =
			BindableProperty.Create("VerticalShiftRatio", typeof(double), typeof(ParallaxImage), 0.25);

		public double VerticalShiftRatio
		{
			get { return (double)GetValue(VerticalShiftRatioProperty); }
			set { SetValue(VerticalShiftRatioProperty, value); }
		}

        /// <summary>
        /// Image source resource key
        /// </summary>
        public static readonly BindableProperty ResourceKeyProperty =
            BindableProperty.Create("ResourceKey", typeof(string), typeof(ParallaxImage), default(string));

        public string ResourceKey
        {
            get { return (string)GetValue(ResourceKeyProperty); }
            set { SetValue(ResourceKeyProperty, value); }
        }

        /// <summary>
        /// Assembly name. If null then use ImageResourceUtils.DefaultAssemblyName.
        /// </summary>
        public static readonly BindableProperty AssemblyNameProperty =
            BindableProperty.Create("AssemblyName", typeof(string), typeof(ParallaxImage));

        public string AssemblyName
        {
            get { return (string)GetValue(AssemblyNameProperty); }
            set { SetValue(AssemblyNameProperty, value); }
        }

        /// <summary>
        /// Parallax orientation
        /// </summary>
        /*public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create("Orientation", typeof(ScrollOrientation), typeof(ParallaxImage), ScrollOrientation.Horizontal);

        public ScrollOrientation Orientation
        {
            get { return (ScrollOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }*/

		#endregion
            
        public ParallaxImage()
        {
            m_parallaxViewportLocation = new Rectangle();

            m_image1 = new Image();
            m_image2 = new Image();

            if (string.IsNullOrEmpty(ResourceKey) == false && string.IsNullOrEmpty(AssemblyName) == false)
            {
                m_image1.AssemblyName = AssemblyName;
                m_image1.ResourceKey = ResourceKey;
                m_image2.AssemblyName = AssemblyName;
                m_image2.ResourceKey = ResourceKey;
            }

            Children.Add(m_image1);
            Children.Add(m_image2);
        }

        /// <summary>
        /// Take all available space
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size newAvailableSize = new Size(widthConstraint, heightConstraint);
            if (m_availableSize != newAvailableSize)
            {
                m_isImageSizeValid = false;
                m_availableSize = newAvailableSize;
            }

            if (double.IsInfinity(widthConstraint))
            {
                m_availableSize.Width = 0;
            }
            if (double.IsInfinity(heightConstraint))
            {
                m_availableSize.Height = 0;
            }

            if (m_isImageSizeValid == false)
            {
                m_imageSize = MeasureImageSize(widthConstraint, heightConstraint);
                m_isImageSizeValid = true;
            }

            return new SizeRequest(m_availableSize, m_availableSize);
        }

        /// <summary>
        /// Layout content
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            m_imageSize = MeasureImageSize(width, height);

            foreach (View child in Children)
            {
                if (child == m_image1 || child == m_image2)
                {
                    LayoutChildIntoBoundingRegion(child, new Rectangle(0, 0, m_imageSize.Width, m_imageSize.Height));
                }
                else
                {
                    Rectangle location = new Rectangle(0, 0, width, height);
                    if (child.Bounds != location)
                    {
                        LayoutChildIntoBoundingRegion(child, location);
                    }
                }
            }

            if (m_parallaxSourceView != null)
            {
                OnSourceOffsetChanged(m_parallaxSourceView.HorizontalOffset, m_parallaxSourceView.VerticalOffset);
            }
        }

        /// <summary>
        /// Handle properties changes
        /// </summary>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == AssemblyNameProperty.PropertyName || propertyName == ResourceKeyProperty.PropertyName)
            {
                if (string.IsNullOrEmpty(ResourceKey) == false && string.IsNullOrEmpty(AssemblyName) == false)
                {
                    m_image1.AssemblyName = AssemblyName;
                    m_image1.ResourceKey = ResourceKey;
                    m_image2.AssemblyName = AssemblyName;
                    m_image2.ResourceKey = ResourceKey;
                }

                m_isImageSizeValid = false;
            }

            if (Width > -1 && Height > -1 && m_isImageSizeValid == false && string.IsNullOrEmpty(AssemblyName) == false && string.IsNullOrEmpty(ResourceKey) == false)
            {
                m_imageSize = MeasureImageSize(Width, Height);
                m_isImageSizeValid = true;

                if (m_parallaxSourceView != null)
                {
                    OnSourceOffsetChanged(m_parallaxSourceView.HorizontalOffset, m_parallaxSourceView.VerticalOffset);
                }
            }
        }

		/// <summary>
		/// Called when IParallaxSource changes
		/// </summary>
		private void OnSourceChanged(View oldSource, View newSource)
        {
			if (m_parallaxSourceView != null)
			{
				m_parallaxSourceView.OffsetChanged -= OnSourceOffsetChanged;
                m_parallaxSourceView = null;
            }

			if (newSource is IParallaxSource)
			{
				m_parallaxSourceView = newSource as IParallaxSource;
				m_parallaxSourceView.OffsetChanged += OnSourceOffsetChanged;
			}
			else
			{
                m_parallaxSourceView = ParallaxLayout.FindParallaxSourceRecursive(newSource);

				if (m_parallaxSourceView != null)
				{
					m_parallaxSourceView.OffsetChanged += OnSourceOffsetChanged;
				}
			}

            if (m_parallaxSourceView != null)
            {
                OnSourceOffsetChanged(m_parallaxSourceView.HorizontalOffset, m_parallaxSourceView.VerticalOffset);
            }
        }

        /// <summary>
        /// Event handler for Source offset changes
        /// </summary>
        private void OnSourceOffsetChanged(double xOffset, double yOffset)
        {
            // Update current parallax viewport location
            m_parallaxViewportLocation = new Rectangle(-xOffset, -yOffset, Width, Height);
        
            Point pointFromOrigo = new Point(-(float)m_parallaxViewportLocation.X, -(float)m_parallaxViewportLocation.Y);

            int m = (int)Math.Floor((m_parallaxViewportLocation.X * HorizontalShiftRatio + Width) / m_imageSize.Width);

            int m1 = 0;
            int m2 = 0;
            if (m_parallaxViewportLocation.X >= -1)
            {
                m1 = Math.Max(0, m - 1);
                m2 = Math.Max(1, m);
            }
            else
            {
                m1 = m;
                m2 = Math.Min(-1, m - 1);
            }

            Point p1 = new Point((-m_parallaxViewportLocation.X * HorizontalShiftRatio + m1 * m_imageSize.Width), -m_parallaxViewportLocation.Y);
            Point p2 = new Point((-m_parallaxViewportLocation.X * HorizontalShiftRatio + m2 * m_imageSize.Width), -m_parallaxViewportLocation.Y);

            Rectangle r1 = new Rectangle(p1, m_imageSize);
            Rectangle r2 = new Rectangle(p2, m_imageSize);

            // Debug.WriteLine("r1:" + r1 + " r2:" + r2 + " m:" + m + " m1:" + m1 + " m2:" + m2);

            m_image1.TranslationX = p1.X;
            m_image2.TranslationX = p2.X;
        }

        /// <summary>
        /// Measure image size
        /// </summary>
        private Size MeasureImageSize(double width, double height)
        {
            Size size = new Size();

            if (string.IsNullOrEmpty(AssemblyName) == false && string.IsNullOrEmpty(ResourceKey) == false)
            {
                if (m_skImage == null)
                {
                    if (m_assembly == null)
                    {
                        m_assembly = Assembly.Load(new AssemblyName(AssemblyName));
                    }

                    using (var stream = m_assembly.GetManifestResourceStream(AssemblyName + "." + ResourceKey))
                    {
                        SKBitmap bitmap = SKBitmap.Decode(stream);
                        m_skImage = SKImage.FromBitmap(bitmap);
                    }
                }

                size = new Size(m_skImage.Width, m_skImage.Height);
                double scale = Math.Max(width, height) / Math.Min(size.Height, size.Width);
                size = new Size(size.Width * scale, size.Height * scale);
            }

            return size;
        }        
	}
}