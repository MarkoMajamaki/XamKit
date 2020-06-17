using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace XamKit
{
	public class Separator : Layout<View>
	{        
        private BoxView m_line = null;
        private Label m_text = null;
        
		#region Binding properties

		public static readonly BindableProperty TextProperty =
            BindableProperty.Create("Text", typeof(string), typeof(Separator), null);
        
        public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create("FontSize", typeof(double), typeof(Separator), 15.0);
        
        public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public static readonly BindableProperty TextMarginProperty =
            BindableProperty.Create("TextMargin", typeof(Thickness), typeof(Separator), new Thickness(0));
        
		public Thickness TextMargin
		{
			get { return (Thickness)GetValue(TextMarginProperty); }
			set { SetValue(TextMarginProperty, value); }
		}

		public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create("TextColor", typeof(Color), typeof(Separator), Color.Black);
        
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		public static readonly BindableProperty TextOpacityProperty =
            BindableProperty.Create("TextOpacity", typeof(double), typeof(Separator), 1.0);
        
		public double TextOpacity
		{
			get { return (double)GetValue(TextOpacityProperty); }
			set { SetValue(TextOpacityProperty, value); }
		}
        
        public static readonly BindableProperty FontAttributesProperty =
            BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(Separator), null);

        public FontAttributes FontAttributes
        {
            get { return (FontAttributes)GetValue(FontAttributesProperty); }
            set { SetValue(FontAttributesProperty, value); }
        }

        public static readonly BindableProperty IsUpperProperty =
            BindableProperty.Create("IsUpper", typeof(bool?), typeof(Separator), null);

        public bool? IsUpper
        {
            get { return (bool?)GetValue(IsUpperProperty); }
            set { SetValue(IsUpperProperty, value); }
        }

        #endregion

        #region Line

        public static readonly BindableProperty LineHeightRequestProperty =
            BindableProperty.Create("LineHeightRequest", typeof(double), typeof(Separator), 1.0);
        
        public double LineHeightRequest
        {
            get { return (double)GetValue(LineHeightRequestProperty); }
            set { SetValue(LineHeightRequestProperty, value); }
        }

        public static readonly BindableProperty LineColorProperty =
            BindableProperty.Create("LineColor", typeof(Color), typeof(Separator), Color.Black);
        
        public Color LineColor
        {
            get { return (Color)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        public static readonly BindableProperty LineMarginProperty =
            BindableProperty.Create("LineMargin", typeof(Thickness), typeof(Separator), new Thickness(0));

        public Thickness LineMargin
        {
            get { return (Thickness)GetValue(LineMarginProperty); }
            set { SetValue(LineMarginProperty, value); }
        }

        #endregion

        public Separator()
		{
            m_line = new BoxView();
            m_line.HeightRequest = LineHeightRequest;
            m_line.BackgroundColor = LineColor;
            m_line.Margin = LineMargin;

            Children.Add(m_line);

            m_text = new Label();
            m_text.Text = IsUpper.HasValue ? (IsUpper.Value ? Text.ToUpper() : Text.ToLower()) : Text;
            m_text.FontSize = FontSize;
            m_text.Margin = TextMargin;
            m_text.TextColor = TextColor;
            m_text.Opacity = TextOpacity;
            m_text.Margin = 0;
            m_text.Padding = 0;

            m_text.LineBreakMode = LineBreakMode.NoWrap;

            Children.Add(m_text);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (m_line != null)
            {
                if (propertyName == LineHeightRequestProperty.PropertyName)
                {
                    m_line.HeightRequest = LineHeightRequest;
                }
                else if (propertyName == LineColorProperty.PropertyName)
                {
                    m_line.BackgroundColor = LineColor;
                }
                else if (propertyName == LineMarginProperty.PropertyName)
                {
                    InvalidateMeasure();
                    InvalidateLayout();
                }
                else if (propertyName == TextProperty.PropertyName || propertyName == IsUpperProperty.PropertyName)
                {
                    m_text.IsVisible = string.IsNullOrEmpty(Text) == false;
                    m_text.Text = IsUpper.HasValue ? (IsUpper.Value ? Text.ToUpper() : Text.ToLower()) : Text;
                }
                else if (propertyName == FontSizeProperty.PropertyName)
                {
                    m_text.FontSize = FontSize;
                }
                else if (propertyName == TextColorProperty.PropertyName)
                {
                    m_text.TextColor = TextColor;
                }
                else if (propertyName == TextOpacityProperty.PropertyName)
                {
                    m_text.Opacity = TextOpacity;
                }
                else if (propertyName == FontAttributesProperty.PropertyName)
                {
                    m_text.FontAttributes = FontAttributes;
                }
            }

            base.OnPropertyChanged(propertyName);
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            SizeRequest textSize = new SizeRequest();
            double actualTextHeight = 0;

            if (m_text.IsVisible && string.IsNullOrEmpty(m_text.Text) == false)
            {
                textSize = m_text.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
                actualTextHeight = textSize.Request.Height + TextMargin.VerticalThickness;
            }

            double height = actualTextHeight + LineMargin.VerticalThickness + LineHeightRequest;
            Size totalSizeRequest = new Size(TextMargin.HorizontalThickness + textSize.Request.Width, height);
            Size totalSizeMinimum = new Size(TextMargin.HorizontalThickness + textSize.Minimum.Width, height);

            return new SizeRequest(totalSizeRequest, totalSizeMinimum);
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            Size m_textSize = new Size();
            if (m_text.IsVisible && string.IsNullOrEmpty(m_text.Text) == false)
            {
                m_textSize = m_text.Measure(width, height, MeasureFlags.IncludeMargins).Request;
            }

            Rectangle lineLocation = new Rectangle(LineMargin.Left, LineMargin.Top, width - LineMargin.HorizontalThickness, LineHeightRequest);
            if (m_line.Bounds != lineLocation)
            {
                LayoutChildIntoBoundingRegion(m_line, lineLocation);
            }

            Rectangle textLocation = new Rectangle(TextMargin.Left, lineLocation.Bottom + LineMargin.Bottom + TextMargin.Top, width, m_textSize.Height);
            if (m_text.IsVisible && m_text.Bounds != textLocation)
            {
                LayoutChildIntoBoundingRegion(m_text, textLocation);
            }
        }
    }
}
