using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Skia
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace XamKit
{
    public class SkiaUtils
    {
        /// <summary>
        /// Draw multiline text in Skia coordinates
        /// </summary>
        /// <param name="canvas">Canvas where text is drawn</param>
        /// <param name="paint">Paint for drawn</param>
        /// <param name="x">Start x coordinate</param>
        /// <param name="y">Start y coordinate</param>
        /// <param name="maxWidth">Max width</param>
        /// <param name="lineHeight">Pre measured line height</param>
        /// <param name="isMultiline">Is text wrapped to multiple lines</param>
        /// <param name="text">Actual text</param>
        public static void DrawTextArea(SKCanvas canvas, SKPaint paint, float x, float y, float maxWidth, float lineHeight, bool isMultiline, string text)
        {
            if (isMultiline)
            {
                var spaceWidth = paint.MeasureText(" ");
                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                lines = lines.SelectMany(l => SplitLine(paint, maxWidth, l, spaceWidth)).ToArray();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    canvas.DrawText(line, x, y, paint);
                    y += lineHeight;
                }
            }
            else
            {
                long maxCharacters = paint.BreakText(text, maxWidth); 

                string actualString = text;

                if (maxCharacters + 1 < text.Count()) // +1 quick and dirty for UWP
                {
                    float dotsWidth = paint.MeasureText("...");

                    maxCharacters = paint.BreakText(text, maxWidth - dotsWidth);

                    actualString = text.Substring(0, (int)maxCharacters);
                    actualString = actualString.Trim();
                    actualString += "...";
                }

                canvas.DrawText(actualString, x, y, paint);
            }
        }

        /// <summary>
        /// Measure multiline text size
        /// </summary>
        /// <returns>Text size on Xamarin pixels</returns>
        /// <param name="paint">Paint for draw</param>
        /// <param name="maxWidth">Text max width in Xamarin pixel units</param>
        /// <param name="lineHeight">Pre measured line height in Skia pixel units</param>
        /// <param name="isMultiline">Is text multiline</param>
        /// <param name="text">Actual text</param>
        public static Size MeasureText(SKPaint paint, float maxWidth, float lineHeight, bool isMultiline, string text)
        {
            float height = 0;

            var spaceWidth = paint.MeasureText(" ");
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            lines = lines.SelectMany(l => SplitLine(paint, maxWidth, l, spaceWidth)).ToArray();

            if (lines.Count() > 1 && isMultiline)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    height += lineHeight;
                }

                return new Size(maxWidth, height);
            }
            else
            {
                SKRect bounds = new SKRect();
                paint.MeasureText(text, ref bounds);

                return new Size(Math.Min(bounds.Width, maxWidth), Math.Max(bounds.Height, lineHeight));
            }
        }

        /// <summary>
        /// Split text for multiple lines
        /// </summary>
        /// <returns>Array of text lines</returns>
        /// <param name="paint">Paint for drawn</param>
        /// <param name="maxWidth">Max width</param>
        /// <param name="text">Actual text</param>
        /// <param name="spaceWidth">Space character width</param>
        private static string[] SplitLine(SKPaint paint, float maxWidth, string text, float spaceWidth)
        {
            var result = new List<string>();

            string[] words = text.TrimEnd().Split(new[] { " " }, StringSplitOptions.None);

            var line = new StringBuilder();
            float width = 0;
            foreach (var word in words)
            {
                var wordWidth = paint.MeasureText(word);
                var wordWithSpaceWidth = wordWidth + spaceWidth;
                var wordWithSpace = word + " ";

                if (width + wordWidth > maxWidth)
                {
                    result.Add(line.ToString());
                    line = new StringBuilder(wordWithSpace);
                    width = wordWithSpaceWidth;
                }
                else
                {
                    line.Append(wordWithSpace);
                    width += wordWithSpaceWidth;
                }
            }

            result.Add(line.ToString());

            return result.ToArray();
        }

        public static SKFontStyleWeight ConvertToSKFontWeight(FontWeights fontWeight)
        {
            switch (fontWeight)
            {
                case FontWeights.Normal:
                    return SKFontStyleWeight.Normal;
                case FontWeights.SemiBold:
                    return SKFontStyleWeight.SemiBold;
                case FontWeights.Bold:
                    return SKFontStyleWeight.Bold;
                case FontWeights.Thin:
                    return SKFontStyleWeight.Light;
                default:
                    return SKFontStyleWeight.Normal;
            }
        }

        public static SKFontStyleSlant ConvertToSKFontStyle(FontStyles fontStyle)
        {
            switch (fontStyle)
            {
                case FontStyles.Oblique:
                    return SKFontStyleSlant.Oblique;
                case FontStyles.Italic:
                    return SKFontStyleSlant.Italic;
                case FontStyles.Upright:
                    return SKFontStyleSlant.Upright;
                default:
                    return SKFontStyleSlant.Upright;
            }
        }

        /// <summary>
        /// Draw shadow using gradient
        /// </summary>
        public static void DrawShadow(SKPaintSurfaceEventArgs e, SKRect shadowLocation, float skCornerRadius, float skShadowLenght, Color color, double shadowOpacity, bool isFilled)
        {
            SKColor shadowColor = color.MultiplyAlpha(shadowOpacity).ToSKColor();
            SKColor toColor = Color.Transparent.ToSKColor();

            SKPaint shadowPaint = new SKPaint();
            shadowPaint.Style = SKPaintStyle.Fill;

            float skActualCornerRadius = Math.Min(skCornerRadius, Math.Min(shadowLocation.Width / 2, shadowLocation.Height / 2));

            // Create side gradients

            SKRect rect = new SKRect(shadowLocation.Left + skActualCornerRadius, 0, shadowLocation.Right - skActualCornerRadius, skShadowLenght);
            if (rect.Left < rect.Right)
            {
                shadowPaint.Shader = SKShader.CreateLinearGradient(new SKPoint(skActualCornerRadius, 0), new SKPoint(skActualCornerRadius, skShadowLenght), new SKColor[] { toColor, shadowColor }, null, SKShaderTileMode.Clamp);
                e.Surface.Canvas.DrawRect(rect, shadowPaint);
            }

            rect = new SKRect(shadowLocation.Right, shadowLocation.Top + skActualCornerRadius, shadowLocation.Right + skShadowLenght, shadowLocation.Bottom - skActualCornerRadius);
            if (rect.Top < rect.Bottom)
            {
                shadowPaint.Shader = SKShader.CreateLinearGradient(new SKPoint(shadowLocation.Right + skShadowLenght, skActualCornerRadius), new SKPoint(shadowLocation.Right, skActualCornerRadius), new SKColor[] { toColor, shadowColor }, null, SKShaderTileMode.Clamp);
                e.Surface.Canvas.DrawRect(rect, shadowPaint);
            }

            rect = new SKRect(shadowLocation.Left + skActualCornerRadius, shadowLocation.Bottom, shadowLocation.Right - skActualCornerRadius, shadowLocation.Bottom + skShadowLenght);
            if (rect.Left < rect.Right)
            {
                shadowPaint.Shader = SKShader.CreateLinearGradient(new SKPoint(skActualCornerRadius, shadowLocation.Bottom), new SKPoint(skActualCornerRadius, shadowLocation.Bottom + skShadowLenght), new SKColor[] { shadowColor, toColor }, null, SKShaderTileMode.Clamp);
                e.Surface.Canvas.DrawRect(rect, shadowPaint);
            }

            rect = new SKRect(shadowLocation.Left - skShadowLenght, shadowLocation.Top + skActualCornerRadius, shadowLocation.Left, shadowLocation.Bottom - skActualCornerRadius);
            if (rect.Top < rect.Bottom)
            {
                shadowPaint.Shader = SKShader.CreateLinearGradient(new SKPoint(shadowLocation.Left, skActualCornerRadius), new SKPoint(shadowLocation.Left - skShadowLenght, skActualCornerRadius), new SKColor[] { shadowColor, toColor }, null, SKShaderTileMode.Clamp);
                e.Surface.Canvas.DrawRect(rect, shadowPaint);
            }

            // Create corners

            SKSize skCircleSize = new SKSize((skActualCornerRadius + skShadowLenght) * 2, (skActualCornerRadius + skShadowLenght) * 2);

            float outerRadius = Math.Min(skCircleSize.Width, skCircleSize.Height) / 2;
            float innerRadius = outerRadius - skShadowLenght;
            float radius = outerRadius - (skShadowLenght / 2);

            shadowPaint = new SKPaint();
            shadowPaint.Style = SKPaintStyle.Stroke;
            shadowPaint.StrokeWidth = skShadowLenght;
            shadowPaint.IsAntialias = true;

            float start = (2 * innerRadius / outerRadius) / 2;
            float end = 1;

            // Top left
            
            SKRect location = new SKRect(
                shadowLocation.Left - skShadowLenght, 
                shadowLocation.Top - skShadowLenght,
                shadowLocation.Left + skShadowLenght + (skActualCornerRadius * 2), 
                shadowLocation.Top + skShadowLenght + (skActualCornerRadius * 2));
            
            shadowPaint.Shader = SKShader.CreateRadialGradient(
                                new SKPoint(location.MidX, location.MidY),
                                outerRadius,
                                new SKColor[] { shadowColor, toColor },
                                new float[] { start, end },
                                SKShaderTileMode.Clamp);

            e.Surface.Canvas.Save();
            rect = new SKRect(shadowLocation.Left - skShadowLenght, shadowLocation.Top - skShadowLenght, shadowLocation.Left + skActualCornerRadius, shadowLocation.Top  + skActualCornerRadius);
            e.Surface.Canvas.ClipRect(rect);
            e.Surface.Canvas.DrawCircle(new SKPoint(location.MidX, location.MidY), radius, shadowPaint);
            e.Surface.Canvas.Restore();
            
            // Top right
            
            location = new SKRect();
            location.Left = shadowLocation.Right - skShadowLenght - skActualCornerRadius;
            location.Top = shadowLocation.Top - skShadowLenght + skActualCornerRadius;
            location.Right = location.Left + (skShadowLenght * 2);
            location.Bottom = location.Top + (skShadowLenght * 2);

            shadowPaint.Shader = SKShader.CreateRadialGradient(
                                new SKPoint(location.MidX, location.MidY),
                                outerRadius,
                                new SKColor[] { shadowColor, toColor },
                                new float[] { start, end },
                                SKShaderTileMode.Clamp);

            e.Surface.Canvas.Save();
            rect = new SKRect(shadowLocation.Right - skActualCornerRadius, shadowLocation.Top - skShadowLenght, shadowLocation.Right + skShadowLenght, shadowLocation.Top + skActualCornerRadius);
            e.Surface.Canvas.ClipRect(rect);
            e.Surface.Canvas.DrawCircle(new SKPoint(location.MidX, location.MidY), radius, shadowPaint);
            e.Surface.Canvas.Restore();

            // Bottom right
            
            location = new SKRect();
            location.Left = shadowLocation.Right - skActualCornerRadius * 2;
            location.Top = shadowLocation.Bottom - skActualCornerRadius * 2;
            location.Right = location.Left + (skActualCornerRadius * 2);
            location.Bottom = location.Top + (skActualCornerRadius * 2);

            shadowPaint.Shader = SKShader.CreateRadialGradient(
                                new SKPoint(location.MidX, location.MidY),
                                outerRadius,
                                new SKColor[] { shadowColor, toColor },
                                new float[] { start, end },
                                SKShaderTileMode.Clamp);

            e.Surface.Canvas.Save();
            rect = new SKRect(shadowLocation.Right - skActualCornerRadius, shadowLocation.Bottom - skActualCornerRadius, shadowLocation.Right + skShadowLenght, shadowLocation.Bottom + skShadowLenght);
            e.Surface.Canvas.ClipRect(rect);
            e.Surface.Canvas.DrawCircle(new SKPoint(location.MidX, location.MidY), radius, shadowPaint);
            e.Surface.Canvas.Restore();

            // Bottom left

            location = new SKRect();
            location.Left = shadowLocation.Left - skShadowLenght;
            location.Top = shadowLocation.Bottom - skShadowLenght - (skActualCornerRadius * 2);
            location.Right = shadowLocation.Left + skShadowLenght + (skActualCornerRadius * 2);
            location.Bottom = shadowLocation.Bottom + skShadowLenght;

            shadowPaint.Shader = SKShader.CreateRadialGradient(
                                new SKPoint(location.MidX, location.MidY),
                                outerRadius,
                                new SKColor[] { shadowColor, toColor },
                                new float[] { start, end },
                                SKShaderTileMode.Clamp);

            e.Surface.Canvas.Save();
            rect = new SKRect(shadowLocation.Left - skShadowLenght, shadowLocation.Bottom - skActualCornerRadius, shadowLocation.Left + skActualCornerRadius, shadowLocation.Bottom + skShadowLenght);
            e.Surface.Canvas.ClipRect(rect);
            e.Surface.Canvas.DrawCircle(new SKPoint(location.MidX, location.MidY), radius, shadowPaint);
            e.Surface.Canvas.Restore();

            if (isFilled)
            {
                SKPaint backgroundPaint = new SKPaint();
                backgroundPaint.IsAntialias = true;
                backgroundPaint.Style = SKPaintStyle.Fill;
                backgroundPaint.Color = shadowColor;

                SKRoundRect backgroundRoundedRect = new SKRoundRect(shadowLocation, skCornerRadius, skCornerRadius);
                e.Surface.Canvas.DrawRoundRect(backgroundRoundedRect, backgroundPaint);
            }
        }
    }
}
