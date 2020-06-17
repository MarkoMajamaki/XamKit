using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

// One
using XamKit;
using SkiaSharp;
using SkiaSharp.Views.Forms;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public class RadioButton : SelectionElementBase
    {
        /// <summary>
        /// All toggle groups
        /// </summary>
        private static readonly Dictionary<string, List<WeakReference>> m_currentlyRegisterdGroups = new Dictionary<string, List<WeakReference>>();
        
        private double _ellipseAnimationProcess = 0;

        private bool _ignoreIsToggledEventHandling = false;

        /// <summary>
        /// Event when any of this group radiobutton is toggled
        /// </summary>
        public event EventHandler GroupToggleChanged;

        #region Binding Properties - Group

        /// <summary>
        /// Toggle group name if only one item can be toggled
        /// </summary>
        public static readonly BindableProperty GroupNameProperty =
			BindableProperty.Create("GroupName", typeof(string), typeof(RadioButton), null, propertyChanged: OnGroupNameChanged);

		static void OnGroupNameChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((RadioButton)bindable).OnGroupNameChanged(oldValue as string, newValue as string);
		}

		public string GroupName
		{
			get { return (string)GetValue(GroupNameProperty); }
			set { SetValue(GroupNameProperty, value); }
		}

        /// <summary>
        /// Current toggled button of this radio button group. Null if none is toggled.
        /// </summary>
        public static readonly BindableProperty GroupToggledButtonProperty =
			BindableProperty.Create("GroupToggledButton", typeof(object), typeof(RadioButton), null);

        public object GroupToggledButton
		{
			get { return (object)GetValue(GroupToggledButtonProperty); }
			set { SetValue(GroupToggledButtonProperty, value); }
		}

        /// <summary>
        /// Identifier for this button if BindingContext is not used
        /// </summary>
        public static readonly BindableProperty TagProperty =
			BindableProperty.Create("Tag", typeof(object), typeof(RadioButton), null);
		
		public object Tag
		{
			get { return (object)GetValue(TagProperty); }
			set { SetValue(TagProperty, value); }
		}

        #endregion

        #region Binding properties - Indicator

        public static readonly BindableProperty IndicatorWidthRequestProperty =
            BindableProperty.Create("IndicatorWidthRequest", typeof(double), typeof(RadioButton), 20.0);

        public double IndicatorWidthRequest
        {
            get { return (double)GetValue(IndicatorWidthRequestProperty); }
            set { SetValue(IndicatorWidthRequestProperty, value); }
        }

        public static readonly BindableProperty IndicatorHeightRequestProperty =
            BindableProperty.Create("IndicatorHeightRequest", typeof(double), typeof(RadioButton), 20.0);

        public double IndicatorHeightRequest
        {
            get { return (double)GetValue(IndicatorHeightRequestProperty); }
            set { SetValue(IndicatorHeightRequestProperty, value); }
        }

        public static readonly BindableProperty InnerIndicatorSpacingProperty =
            BindableProperty.Create("InnerIndicatorSpacing", typeof(double), typeof(RadioButton), 2.0);

        public double InnerIndicatorSpacing
        {
            get { return (double)GetValue(InnerIndicatorSpacingProperty); }
            set { SetValue(InnerIndicatorSpacingProperty, value); }
        }

        public static readonly BindableProperty IndicatorMarginProperty =
            BindableProperty.Create("IndicatorMargin", typeof(Thickness), typeof(RadioButton), new Thickness(0));

        public Thickness IndicatorMargin
        {
            get { return (Thickness)GetValue(IndicatorMarginProperty); }
            set { SetValue(IndicatorMarginProperty, value); }
        }

        public static readonly BindableProperty IndicatorBorderThicknessProperty =
            BindableProperty.Create("IndicatorBorderThickness", typeof(double), typeof(RadioButton), 1.0);

        public double IndicatorBorderThickness
        {
            get { return (double)GetValue(IndicatorBorderThicknessProperty); }
            set { SetValue(IndicatorBorderThicknessProperty, value); }
        }

        public static readonly BindableProperty IndicatorVerticalOptionsProperty =
            BindableProperty.Create("IndicatorVerticalOptions", typeof(LayoutOptions), typeof(RadioButton), LayoutOptions.Center);

        public LayoutOptions IndicatorVerticalOptions
        {
            get { return (LayoutOptions)GetValue(IndicatorVerticalOptionsProperty); }
            set { SetValue(IndicatorVerticalOptionsProperty, value); }
        }

        public static readonly BindableProperty IndicatorLocationProperty =
            BindableProperty.Create("IndicatorLocation", typeof(HorizontalLocations), typeof(CheckBox), HorizontalLocations.Left);

        public HorizontalLocations IndicatorLocation
        {
            get { return (HorizontalLocations)GetValue(IndicatorLocationProperty); }
            set { SetValue(IndicatorLocationProperty, value); }
        }

        #endregion

        #region Binding properties - Colors

        // Inner indicator

        public static readonly BindableProperty IndicatorColorProperty =
            BindableProperty.Create("IndicatorColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorColor
        {
            get { return (Color)GetValue(IndicatorColorProperty); }
            set { SetValue(IndicatorColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorHoverColorProperty =
            BindableProperty.Create("IndicatorHoverColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorHoverColor
        {
            get { return (Color)GetValue(IndicatorHoverColorProperty); }
            set { SetValue(IndicatorHoverColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorPressedColorProperty =
            BindableProperty.Create("IndicatorPressedColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorPressedColor
        {
            get { return (Color)GetValue(IndicatorPressedColorProperty); }
            set { SetValue(IndicatorPressedColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorDisabledColorProperty =
            BindableProperty.Create("IndicatorDisabledColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorDisabledColor
        {
            get { return (Color)GetValue(IndicatorDisabledColorProperty); }
            set { SetValue(IndicatorDisabledColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorColorProperty =
            BindableProperty.Create("ToggledIndicatorColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorColor
        {
            get { return (Color)GetValue(ToggledIndicatorColorProperty); }
            set { SetValue(ToggledIndicatorColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorHoverColorProperty =
            BindableProperty.Create("ToggledIndicatorHoverColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorHoverColor
        {
            get { return (Color)GetValue(ToggledIndicatorHoverColorProperty); }
            set { SetValue(ToggledIndicatorHoverColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorPressedColorProperty =
            BindableProperty.Create("ToggledIndicatorPressedColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorPressedColor
        {
            get { return (Color)GetValue(ToggledIndicatorPressedColorProperty); }
            set { SetValue(ToggledIndicatorPressedColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorDisabledColorProperty =
            BindableProperty.Create("ToggledIndicatorDisabledColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorDisabledColor
        {
            get { return (Color)GetValue(ToggledIndicatorDisabledColorProperty); }
            set { SetValue(ToggledIndicatorDisabledColorProperty, value); }
        }

        // Indicator border

        public static readonly BindableProperty IndicatorBorderColorProperty =
            BindableProperty.Create("IndicatorBorderColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorBorderColor
        {
            get { return (Color)GetValue(IndicatorBorderColorProperty); }
            set { SetValue(IndicatorBorderColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorBorderHoverColorProperty =
            BindableProperty.Create("IndicatorBorderHoverColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorBorderHoverColor
        {
            get { return (Color)GetValue(IndicatorBorderHoverColorProperty); }
            set { SetValue(IndicatorBorderHoverColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorBorderPressedColorProperty =
            BindableProperty.Create("IndicatorBorderPressedColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorBorderPressedColor
        {
            get { return (Color)GetValue(IndicatorBorderPressedColorProperty); }
            set { SetValue(IndicatorBorderPressedColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorBorderDisabledColorProperty =
            BindableProperty.Create("IndicatorBorderDisabledColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorBorderDisabledColor
        {
            get { return (Color)GetValue(IndicatorBorderDisabledColorProperty); }
            set { SetValue(IndicatorBorderDisabledColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorBorderColorProperty =
            BindableProperty.Create("ToggledIndicatorBorderColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorBorderColor
        {
            get { return (Color)GetValue(ToggledIndicatorBorderColorProperty); }
            set { SetValue(ToggledIndicatorBorderColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorBorderHoverColorProperty =
            BindableProperty.Create("ToggledIndicatorBorderHoverColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorBorderHoverColor
        {
            get { return (Color)GetValue(ToggledIndicatorBorderHoverColorProperty); }
            set { SetValue(ToggledIndicatorBorderHoverColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorBorderPressedColorProperty =
            BindableProperty.Create("ToggledIndicatorBorderPressedColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorBorderPressedColor
        {
            get { return (Color)GetValue(ToggledIndicatorBorderPressedColorProperty); }
            set { SetValue(ToggledIndicatorBorderPressedColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorBorderDisabledColorProperty =
            BindableProperty.Create("ToggledIndicatorBorderDisabledColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorBorderDisabledColor
        {
            get { return (Color)GetValue(ToggledIndicatorBorderDisabledColorProperty); }
            set { SetValue(ToggledIndicatorBorderDisabledColorProperty, value); }
        }

        // Indicator background

        public static readonly BindableProperty IndicatorBackgroundColorProperty =
            BindableProperty.Create("IndicatorBackgroundColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorBackgroundColor
        {
            get { return (Color)GetValue(IndicatorBackgroundColorProperty); }
            set { SetValue(IndicatorBackgroundColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorBackgroundHoverColorProperty =
            BindableProperty.Create("IndicatorBackgroundHoverColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorBackgroundHoverColor
        {
            get { return (Color)GetValue(IndicatorBackgroundHoverColorProperty); }
            set { SetValue(IndicatorBackgroundHoverColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorBackgroundPressedColorProperty =
            BindableProperty.Create("IndicatorBackgroundPressedColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorBackgroundPressedColor
        {
            get { return (Color)GetValue(IndicatorBackgroundPressedColorProperty); }
            set { SetValue(IndicatorBackgroundPressedColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorBackgroundDisabledColorProperty =
            BindableProperty.Create("IndicatorBackgroundDisabledColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color IndicatorBackgroundDisabledColor
        {
            get { return (Color)GetValue(IndicatorBackgroundDisabledColorProperty); }
            set { SetValue(IndicatorBackgroundDisabledColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorBackgroundColorProperty =
            BindableProperty.Create("ToggledIndicatorBackgroundColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorBackgroundColor
        {
            get { return (Color)GetValue(ToggledIndicatorBackgroundColorProperty); }
            set { SetValue(ToggledIndicatorBackgroundColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorBackgroundHoverColorProperty =
            BindableProperty.Create("ToggledIndicatorBackgroundHoverColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorBackgroundHoverColor
        {
            get { return (Color)GetValue(ToggledIndicatorBackgroundHoverColorProperty); }
            set { SetValue(ToggledIndicatorBackgroundHoverColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorBackgroundPressedColorProperty =
            BindableProperty.Create("ToggledIndicatorBackgroundPressedColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorBackgroundPressedColor
        {
            get { return (Color)GetValue(ToggledIndicatorBackgroundPressedColorProperty); }
            set { SetValue(ToggledIndicatorBackgroundPressedColorProperty, value); }
        }

        public static readonly BindableProperty ToggledIndicatorBackgroundDisabledColorProperty =
            BindableProperty.Create("ToggledIndicatorBackgroundDisabledColor", typeof(Color), typeof(RadioButton), Color.Transparent);

        public Color ToggledIndicatorBackgroundDisabledColor
        {
            get { return (Color)GetValue(ToggledIndicatorBackgroundDisabledColorProperty); }
            set { SetValue(ToggledIndicatorBackgroundDisabledColorProperty, value); }
        }


        #endregion

        public RadioButton()
        {
            _selectionViewSize = new Size(IndicatorMargin.HorizontalThickness + IndicatorWidthRequest, IndicatorMargin.VerticalThickness + IndicatorHeightRequest);
            _selectionViewLocation = IndicatorLocation;
        }

        #region Paint

        /// <summary>
        /// Paint indicator
        /// </summary>
        protected override void OnPaintSelectionElement(SKPaintSurfaceEventArgs e)
        {
            Rectangle availableSpace = new Rectangle(EllipseDiameter, EllipseDiameter, Width, Height - Padding.VerticalThickness);

            Rectangle indicatorActualLocation = IndicatorActualLocation(availableSpace);
            float skIndicatorBorderThickness = (float)IndicatorBorderThickness * DeviceScale;
            float skIndicatorX = (float)indicatorActualLocation.X * DeviceScale;
            float skIndicatorY = (float)indicatorActualLocation.Y * DeviceScale;
            float skIndicatorWidth = (float)IndicatorWidthRequest * DeviceScale;
            float skIndicatorHeight = (float)IndicatorHeightRequest * DeviceScale;

            SKRect indicatorPaintRect = new SKRect(skIndicatorX + skIndicatorBorderThickness / 2,
                                                   skIndicatorY + skIndicatorBorderThickness / 2,
                                                   skIndicatorX + skIndicatorWidth - skIndicatorBorderThickness / 2,
                                                   skIndicatorY + skIndicatorHeight - skIndicatorBorderThickness / 2);

            float skCornerRadius = indicatorPaintRect.Width / 2;

            //
            // Paint ellipse animation
            //

            if (EllipseDiameter > 0 && _ellipseAnimationProcess > 0 && _ellipseAnimationProcess < 1 && IsEllipseAnimationEnabled)
            {
                SKPaint ellipsePaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                };

                if (_ellipseAnimationProcess <= 0.5)
                {
                    ellipsePaint.Color = EllipseColor.MultiplyAlpha(_ellipseAnimationProcess * 2).ToSKColor();
                }
                else
                {
                    ellipsePaint.Color = EllipseColor.MultiplyAlpha(1 - (_ellipseAnimationProcess - 0.5) * 2).ToSKColor();
                }

                e.Surface.Canvas.DrawCircle(new SKPoint(indicatorPaintRect.MidX, indicatorPaintRect.MidY), (float)(EllipseDiameter / 2) * DeviceScale, ellipsePaint);
            }

            //
            // Paint indicator  background
            //
            SKPaint indicatorBackgroundPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = AnimationUtils.ColorTransform(_toggledAnimationProcess, IndicatorBackgroundColor, ToggledIndicatorBackgroundColor).ToSKColor(),
            };

            e.Surface.Canvas.DrawRoundRect(indicatorPaintRect, skCornerRadius, skCornerRadius, indicatorBackgroundPaint);

            //
            // Paint border
            //
            SKPaint indicatorBorderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeWidth = skIndicatorBorderThickness,
                Color = AnimationUtils.ColorTransform(_toggledAnimationProcess, IndicatorBorderColor, ToggledIndicatorBorderColor).ToSKColor(),
            };

            e.Surface.Canvas.DrawRoundRect(indicatorPaintRect, skCornerRadius, skCornerRadius, indicatorBorderPaint);

            // 
            // Paint inner indicator
            //

            Color toggledColor = ToggledIndicatorColor;
            if (IndicatorColor != null && IndicatorColor != Color.Transparent && ToggledIndicatorColor != null && ToggledIndicatorColor != Color.Transparent)
            {
                toggledColor = AnimationUtils.ColorTransform(_toggledAnimationProcess, IndicatorColor, ToggledIndicatorColor);
            }

            SKPaint innerIndicatorPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = toggledColor.ToSKColor(),
            };

            double radius = ((IndicatorWidthRequest / 2) - InnerIndicatorSpacing - IndicatorBorderThickness) * DeviceScale * _toggledAnimationProcess;

            e.Surface.Canvas.DrawCircle(indicatorPaintRect.MidX, indicatorPaintRect.MidY, (float)radius, innerIndicatorPaint);
        }

        /// <summary>
        /// Calculate indicator actual location with alignment
        /// </summary>
        private Rectangle IndicatorActualLocation(Rectangle availableSpace)
        {
            double checkBoxX = availableSpace.X;
            double checkBoxY = availableSpace.Y;

            switch (IndicatorVerticalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                case LayoutAlignment.Start:
                    checkBoxX += IndicatorMargin.Left;
                    checkBoxY += IndicatorMargin.Top;
                    break;
                case LayoutAlignment.Center:
                    checkBoxX += IndicatorMargin.Left;
                    checkBoxY += (availableSpace.Height - IndicatorHeightRequest) / 2;
                    break;
                case LayoutAlignment.End:
                    checkBoxX += IndicatorMargin.Left;
                    checkBoxY += availableSpace.Height - IndicatorHeightRequest;
                    break;
            }

            return new Rectangle(checkBoxX, checkBoxY, IndicatorWidthRequest, IndicatorHeightRequest);
        }

        #endregion

        #region Group selection handling 

        /// <summary>
        /// Handle IsToggled changes
        /// </summary>
        private void HandleGroupIsToggledChanged(bool toggled)
		{
            if (toggled && string.IsNullOrEmpty(GroupName) == false)
            {
                if (m_currentlyRegisterdGroups.ContainsKey(GroupName))
                {
                    List<WeakReference> groupElements = m_currentlyRegisterdGroups[GroupName];

                    for (int i = 0; i < groupElements.Count; i++)
                    {
                        WeakReference reference = groupElements[i];
                        RadioButton rb = reference.Target as RadioButton;

                        if (rb == null)
                        {
                            groupElements.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            if (rb != this && rb.IsToggled == true)
                            {
                                rb.OnIsToggledChanged(false, false);
                            }

                            GroupToggledButton = this;

                            // Raise all GroupToggleChanged events which are listened
                            rb.GroupToggleChanged?.Invoke(this, new EventArgs());
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(GroupName) == false)
                {
                    throw new Exception("Radio button group not found!");
                }
            }
		}

		/// <summary>
		/// Register radio button group
		/// </summary>
		private void OnGroupNameChanged(string oldGroupName, string newGroupName)
		{
            if (string.IsNullOrWhiteSpace(oldGroupName) == false)
            {
                Unregister(oldGroupName, this);
            }

            if (string.IsNullOrWhiteSpace(newGroupName) == false)
            {
                Register(newGroupName, this);
            }
        }
        
        private void Register(string groupName, RadioButton radiobutton)
        {
            lock (m_currentlyRegisterdGroups)
            {
                if (m_currentlyRegisterdGroups.ContainsKey(groupName))
                {
                    List<WeakReference> groupElements = m_currentlyRegisterdGroups[groupName];

                    // Remove dead elements on this group
                    PurgeDead(groupElements, null);

                    groupElements.Add(new WeakReference(radiobutton));
                }
                else
                {
                    List<WeakReference> groupElements = new List<WeakReference>();
                    groupElements.Add(new WeakReference(radiobutton));

                    m_currentlyRegisterdGroups.Add(groupName, groupElements);
                }
            }
        }

        private void Unregister(string groupName, RadioButton radiobutton)
        {
            lock (m_currentlyRegisterdGroups)
            {
                if (m_currentlyRegisterdGroups.ContainsKey(groupName))
                {
                    List<WeakReference> groupElements = m_currentlyRegisterdGroups[groupName];

                    PurgeDead(groupElements, radiobutton);

                    if (groupElements.Count == 0)
                    {
                        m_currentlyRegisterdGroups.Remove(groupName);
                    }
                }
            }
        }

        private void PurgeDead(List<WeakReference> elements, RadioButton elementToRemove)
        {
            for (int i = 0; i < elements.Count;)
            {
                WeakReference weakReference = (WeakReference)elements[i];
                object element = weakReference.Target;
                if (element == null || element == elementToRemove)
                {
                    elements.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        #endregion

        #region Property changes

        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == IndicatorHeightRequestProperty.PropertyName ||
                propertyName == IndicatorWidthRequestProperty.PropertyName ||
                propertyName == InnerIndicatorSpacingProperty.PropertyName ||
                propertyName == IndicatorMarginProperty.PropertyName ||
                propertyName == IndicatorBorderThicknessProperty.PropertyName ||
                propertyName == IndicatorVerticalOptionsProperty.PropertyName ||
                propertyName == IndicatorBorderColorProperty.PropertyName)
            {
                _selectionViewSize = new Size(IndicatorMargin.HorizontalThickness + IndicatorWidthRequest, IndicatorMargin.VerticalThickness + IndicatorHeightRequest);
                _selectionViewLocation = IndicatorLocation;

                InvalidateMeasure();

                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName.Contains("Color"))
            {
                if (_skiaCanvas != null)
                {
                    _skiaCanvas.InvalidateSurface();
                }
            }
            else if (propertyName == IndicatorLocationProperty.PropertyName)
            {
                _selectionViewLocation = IndicatorLocation;
            }
            else if (propertyName == IsToggledProperty.PropertyName)
            {
                OnIsToggledChanged(IsToggled);
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Animate toggle
        /// </summary>
        private void OnIsToggledChanged(bool toggled, bool isEllipseAnimationEnabled = true)
        {
            if (_ignoreIsToggledEventHandling)
            {
                return;
            }

            _ignoreIsToggledEventHandling = true;
            IsToggled = toggled;
            _ignoreIsToggledEventHandling = false;

            AnimationExtensions.AbortAnimation(this, _animationName);

            if (IsToggled)
            {
                HandleGroupIsToggledChanged(IsToggled);
            }

            double start = _toggledAnimationProcessWithoutEasing;
            double end = 1;

            if (toggled == false)
            {
                start = _toggledAnimationProcessWithoutEasing;
                end = 0;
            }

            Animation anim = new Animation(d =>
            {
                _toggledAnimationProcess = AnimationEasing.Ease(d);
                _toggledAnimationProcessWithoutEasing = d;

                if (isEllipseAnimationEnabled)
                {
                    _ellipseAnimationProcess = d;
                }
                _skiaCanvas.InvalidateSurface();

            }, start, end);

            anim.Commit(this, _animationName, 64, (uint)AnimationDuration, Easing.Linear);
        }

        #endregion
    }
}