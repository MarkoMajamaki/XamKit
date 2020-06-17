using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

// Xamarin
using Xamarin.Forms;

// SkiaSparh
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Runtime.CompilerServices;

namespace XamKit
{
    public class ActivityIndicator : SKCanvasView
    {
        // 0 -> 1
        private double m_animationProcess = 0;

        private const string c_indicatorAnimationName = "indicatorAnimationName";

        private float m_deviceScale = 1;

        #region Dependency properties

        public static readonly BindableProperty IndicatorSizeProperty =
            BindableProperty.Create("IndicatorSize", typeof(Size), typeof(ActivityIndicator), new Size(50, 50));

        public Size IndicatorSize
        {
            get { return (Size)GetValue(IndicatorSizeProperty); }
            set { SetValue(IndicatorSizeProperty, value); }
        }

        public static readonly BindableProperty IndicatorThicknessProperty =
            BindableProperty.Create("IndicatorThickness", typeof(double), typeof(ActivityIndicator), 5.0);

        public double IndicatorThickness
        {
            get { return (double)GetValue(IndicatorThicknessProperty); }
            set { SetValue(IndicatorThicknessProperty, value); }
        }

        public static readonly BindableProperty IndicatorForegroundProperty =
            BindableProperty.Create("IndicatorForeground", typeof(Color), typeof(ActivityIndicator), Color.Black);

        public Color IndicatorForeground
        {
            get { return (Color)GetValue(IndicatorForegroundProperty); }
            set { SetValue(IndicatorForegroundProperty, value); }
        }

        public static readonly BindableProperty BackgroundProperty =
            BindableProperty.Create("Background", typeof(Color), typeof(ActivityIndicator), Color.Transparent);

        public Color Background
        {
            get { return (Color)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly BindableProperty IsRunningProperty =
            BindableProperty.Create("IsRunning", typeof(bool), typeof(ActivityIndicator), false, propertyChanged: OnIsRunningChanged);

        private static void OnIsRunningChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as ActivityIndicator).OnIsRunningChanged((bool)newValue);
        }

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }

        #endregion

        public ActivityIndicator()
        {
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == IsVisibleProperty.PropertyName)
            {
                // Continue animation if IsVisible changes false to true and IsBusy is true
                if (IsVisible == true && IsRunning)
                {
                    OnIsRunningChanged(true);
                }
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size size = new Size();

            if (double.IsInfinity(widthConstraint) == false && double.IsNaN(widthConstraint) == false && HorizontalOptions.Alignment == LayoutAlignment.Fill)
            {
                size.Width = widthConstraint;
            }
            else
            {
                size.Width = IndicatorSize.Width;
            }

            if (double.IsInfinity(heightConstraint) == false && double.IsNaN(heightConstraint) == false && VerticalOptions.Alignment == LayoutAlignment.Fill)
            {
                size.Height = heightConstraint;
            }
            else
            {
                size.Height = IndicatorSize.Height;
            }

            return new SizeRequest(size, size);
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear();

            m_deviceScale = (float)(e.Info.Width / Width);

            SKPaint paint = new SKPaint()
            {
                Color = IndicatorForeground.ToSKColor(),
                Style = SKPaintStyle.Fill,
            };

            Easing ease = Easing.SinOut;

            using (SKPath path = new SKPath())
            {
                double startAngle = 0;
                double endAngle = 0;

                if (m_animationProcess >= 0 && m_animationProcess <= 0.5)
                {
                    endAngle = ease.Ease(m_animationProcess * 2) * 270;
                    startAngle = -10;
                }
                if (m_animationProcess > 0.5 && m_animationProcess <= 1)
                {
                    endAngle = ease.Ease(1) * 270;
                    startAngle = ease.Ease((m_animationProcess - 0.5) * 2) * 250;
                }
                else if (m_animationProcess > 1 && m_animationProcess <= 1.5)
                {
                    endAngle = 270;
                    startAngle = 250;
                }
                else if (m_animationProcess > 1.5 && m_animationProcess <= 2)
                {
                    endAngle = 270 + (ease.Ease((m_animationProcess - 1.5) * 2) * 270);
                    startAngle = 250;
                }
                else if (m_animationProcess > 2 && m_animationProcess <= 2.5)
                {
                    endAngle = 270 + 270;
                    startAngle = 250 + (ease.Ease((m_animationProcess - 2) * 2) * 270);
                }
                else if (m_animationProcess > 2.5 && m_animationProcess <= 3)
                {
                    endAngle = 270 + 270;
                    startAngle = 250 + 270;
                }
                else if (m_animationProcess > 3 && m_animationProcess <= 3.5)
                {
                    endAngle = 270 + 270 + (ease.Ease((m_animationProcess - 3) * 2) * 270);
                    startAngle = 250 + 270;
                }
                else if (m_animationProcess > 3.5 && m_animationProcess <= 4)
                {
                    endAngle = 270 + 270 + 270;
                    startAngle = 250 + 270 + (ease.Ease((m_animationProcess - 3.5) * 2) * 270);
                }
                else if (m_animationProcess > 4 && m_animationProcess <= 4.5)
                {
                    endAngle = 270 + 270 + 270;
                    startAngle = 250 + 270 + 270;
                }
                else if (m_animationProcess > 4.5 && m_animationProcess <= 5)
                {
                    endAngle = 270 + 270 + 270 + (ease.Ease((m_animationProcess - 4.5) * 2) * 270);
                    startAngle = 250 + 270 + 270;
                }
                else if (m_animationProcess > 5 && m_animationProcess <= 5.5)
                {
                    endAngle = 270 + 270 + 270 + 270;
                    startAngle = 250 + 270 + 270 + (ease.Ease((m_animationProcess - 5) * 2) * 270);
                }
                else if (m_animationProcess > 5.5 && m_animationProcess <= 6)
                {
                    endAngle = 270 + 270 + 270 + 270;
                    startAngle = 250 + 270 + 270 + 270;
                }

                startAngle += (m_animationProcess / 2) * 360;
                endAngle += (m_animationProcess / 2) * 360;

                float yOffset = (float)((Width - IndicatorSize.Width) / 2) * m_deviceScale;
                float xOffset = (float)((Height - IndicatorSize.Height) / 2) * m_deviceScale;

                float skIndicatorThickness = (float)IndicatorThickness * m_deviceScale;
                SKSize skIndicatorSize = new SKSize((float)IndicatorSize.Width * m_deviceScale, (float)IndicatorSize.Height * m_deviceScale);

                SKRect indicatorOuterBounds = new SKRect();
                indicatorOuterBounds.Location = new SKPoint(xOffset, yOffset);
                indicatorOuterBounds.Size = new SKSize(skIndicatorSize.Width, skIndicatorSize.Height);

                SKRect indicatorInternalBounds = new SKRect();
                indicatorInternalBounds.Location = new SKPoint(xOffset + skIndicatorThickness, yOffset + skIndicatorThickness);
                indicatorInternalBounds.Size = new SKSize((float)Math.Max(1, skIndicatorSize.Width - (skIndicatorThickness * 2)), 
                                                          (float)Math.Max(1, skIndicatorSize.Height - (skIndicatorThickness * 2)));

                path.ArcTo(indicatorOuterBounds, (float)startAngle, (float)(endAngle - startAngle), false);
                path.ArcTo(indicatorInternalBounds, (float)endAngle, (float)-(endAngle - startAngle), false);

                e.Surface.Canvas.DrawPath(path, paint);
            }
        }

        /// <summary>
        /// Stop / run indicator animation
        /// </summary>
        private void OnIsRunningChanged(bool isRunning)
        {
            if (isRunning)
            {
                new Animation(d =>
                {
                    m_animationProcess = d;
                    InvalidateSurface();
                }, 0, 6).Commit(this, c_indicatorAnimationName, 64, 5500, null, null, () =>
                {
                    return IsVisible && IsRunning && IsVisible;
                });
            }
            else
            {
                this.AbortAnimation(c_indicatorAnimationName);
                InvalidateSurface();
            }
        }
    }
}