using System.ComponentModel;
using System.Linq;

// Android
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using XamKit;

[assembly: ExportRenderer(typeof(Border), typeof(XamKit.Droid.BorderRenderer))]

// https://github.com/mrxten/XamEffects/blob/master/src/XamEffects.Droid/Renderers/BorderViewRenderer.cs better solution?

namespace XamKit.Droid
{
    public class BorderRenderer : VisualElementRenderer<Border>
    {
        public BorderRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Border> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement == null)
            {
                return;
            }

            UpdateBackground();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Border.BorderColorProperty.PropertyName ||
                e.PropertyName == Border.BorderThicknessProperty.PropertyName ||
                e.PropertyName == Border.CornerRadiusProperty.PropertyName ||
                e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
            {
                UpdateBackground();
            }
        }

        protected override void DispatchDraw(Canvas canvas)
        {
            canvas.Save(SaveFlags.Clip);
            SetClipPath(canvas);
            base.DispatchDraw(canvas);
            canvas.Restore();
        }

        private void UpdateBackground()
        {
            var borderWidth = Element.BorderThickness;
            var context = Context;

            GradientDrawable strokeDrawable = null;

            if (borderWidth > 0)
            {
                strokeDrawable = new GradientDrawable();
                strokeDrawable.SetColor(Element.BackgroundColor.ToAndroid());

                strokeDrawable.SetStroke((int)context.ToPixels(borderWidth), Element.BorderColor.ToAndroid());
                strokeDrawable.SetCornerRadius(context.ToPixels(Element.CornerRadius));
            }

            var backgroundDrawable = new GradientDrawable();
            backgroundDrawable.SetColor(Element.BackgroundColor.ToAndroid());
            backgroundDrawable.SetCornerRadius(context.ToPixels(Element.CornerRadius));

            if (strokeDrawable != null)
            {
                var ld = new LayerDrawable(new Drawable[] { strokeDrawable, backgroundDrawable });
                ld.SetLayerInset(1, (int)context.ToPixels(borderWidth), (int)context.ToPixels(borderWidth), (int)context.ToPixels(borderWidth), (int)context.ToPixels(borderWidth));
                SetBackgroundDrawable(ld);
            }
            else
            {
                SetBackgroundDrawable(backgroundDrawable);
            }

            SetPadding(
                (int)context.ToPixels(borderWidth + Element.Padding.Left),
                (int)context.ToPixels(borderWidth + Element.Padding.Top),
                (int)context.ToPixels(borderWidth + Element.Padding.Right),
                (int)context.ToPixels(borderWidth + Element.Padding.Bottom));
        }

        private double ThickestSide(Thickness t)
        {
            return new double[] {
                t.Left,
                t.Top,
                t.Right,
                t.Bottom
            }.Max();
        }

        private void SetClipPath(Canvas canvas)
        {
            var clipPath = new Path();
            var radius = Context.ToPixels(Element.CornerRadius) - Context.ToPixels(ThickestSide(Element.Padding));

            var w = Width;
            var h = Height;

            clipPath.AddRoundRect(new RectF(
                ViewGroup.PaddingLeft,
                ViewGroup.PaddingTop,
                w - ViewGroup.PaddingRight,
                h - ViewGroup.PaddingBottom),
                radius,
                radius,
                Path.Direction.Cw);

            canvas.ClipPath(clipPath);
        }
    }
}