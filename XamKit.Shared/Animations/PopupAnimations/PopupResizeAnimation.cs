using Xamarin.Forms;

namespace XamKit
{
    public class PopupResizeAnimation : IAnimation
    {
        public bool IsHorizontalResizeEnabled { get; set; } = true;

        public bool IsContentTranslationEnabled { get; set; } = true;

        public uint Duration { get; set; } = 400;

        public Animation Create(View target)
        {
            PopupRootLayout rootLayout = (PopupRootLayout)target;
            Popup popup = rootLayout.Popup;

            if (rootLayout.Popup.IsOpen)
            {
                rootLayout.Opacity = 0;
            }

            Animation anim = new Animation(d =>
            {
                if (rootLayout.Popup.IsOpen)
                {
                    rootLayout.Opacity = 1;
                }
                else
                {
                    d = 1 - d;
                }

                Rectangle location = new Rectangle(rootLayout.X, rootLayout.Y, rootLayout.Width, rootLayout.Height);

                Rectangle clipRect = new Rectangle(0, 0, location.Width, location.Height);
                double contentY = 0;
                double contentX = 0;

                if (popup.ActualPlacement == PopupPlacements.RightTop)
                {
                    double width = IsHorizontalResizeEnabled ? location.Width * d : location.Width;
                    clipRect = new Rectangle(0, 0, width, location.Height * d);
                    contentY = -location.Height * (1 - d);
                }
                else if (popup.ActualPlacement == PopupPlacements.RightCenter)
                {
                    clipRect = new Rectangle(0, 0, location.Width * d, location.Height);
                    contentX = -location.Width * (1 - d);
                }
                else if (popup.ActualPlacement == PopupPlacements.RightBottom)
                {
                    double width = IsHorizontalResizeEnabled ? location.Width * d : location.Width;
                    clipRect = new Rectangle(0, location.Height * (1 - d), width, location.Height * d);
                }
                else if (popup.ActualPlacement == PopupPlacements.BottomRight)
                {
                    double width = IsHorizontalResizeEnabled ? location.Width * d : location.Width;
                    double x = IsHorizontalResizeEnabled ? location.Width * (1 - d) : 0;
                    clipRect = new Rectangle(x, 0, width, location.Height * d);
                    contentY = -location.Height * (1 - d);
                }
                else if (popup.ActualPlacement == PopupPlacements.BottomCenter || popup.ActualPlacement == PopupPlacements.BottomStretch)
                {
                    clipRect = new Rectangle(0, 0, location.Width, location.Height * d);
                    contentY = -location.Height * (1 - d);
                }
                else if (popup.ActualPlacement == PopupPlacements.BottomLeft)
                {
                    double width = IsHorizontalResizeEnabled ? location.Width * d : location.Width;
                    clipRect = new Rectangle(0, 0, width, location.Height * d);
                    contentY = -location.Height * (1 - d);
                }
                else if (popup.ActualPlacement == PopupPlacements.LeftBottom)
                {
                    double width = IsHorizontalResizeEnabled ? location.Width * d : location.Width;
                    double x = IsHorizontalResizeEnabled ? location.Width * (1 - d) : 0;
                    clipRect = new Rectangle(x, location.Height * (1 - d), width, location.Height * d);
                }
                else if (popup.ActualPlacement == PopupPlacements.LeftCenter)
                {
                    clipRect = new Rectangle(location.Width * (1 - d), 0, location.Width * d, location.Height);
                }
                else if (popup.ActualPlacement == PopupPlacements.LeftTop)
                {
                    double width = IsHorizontalResizeEnabled ? location.Width * d : location.Width;
                    double x = IsHorizontalResizeEnabled ? location.Width * (1 - d) : 0;
                    clipRect = new Rectangle(x, 0, width, location.Height * d);
                    contentY = -location.Height * (1 - d);
                }
                else if (popup.ActualPlacement == PopupPlacements.TopLeft)
                {
                    double width = IsHorizontalResizeEnabled ? location.Width * d : location.Width;
                    clipRect = new Rectangle(0, location.Height * (1 - d), width, location.Height * d);
                }
                else if (popup.ActualPlacement == PopupPlacements.TopCenter || popup.ActualPlacement == PopupPlacements.TopStretch)
                {
                    clipRect = new Rectangle(0, location.Height * (1 - d), location.Width, location.Height * d);
                }
                else if (popup.ActualPlacement == PopupPlacements.TopRight)
                {
                    double width = IsHorizontalResizeEnabled ? location.Width * d : location.Width;
                    double x = IsHorizontalResizeEnabled ? location.Width * (1 - d) : 0;
                    clipRect = new Rectangle(x, location.Height * (1 - d), width, location.Height * d);
                }
                else if (popup.ActualPlacement == PopupPlacements.FullScreen)
                {
                    rootLayout.TranslationY = location.Height * (1 - d);
                }
                else
                {
                    // No animation
                }

                if (popup.ActualPlacement != PopupPlacements.FullScreen)
                {
                    if (IsContentTranslationEnabled)
                    {
                        rootLayout.Content.TranslationX = contentX;
                        rootLayout.Content.TranslationY = contentY;
                    }

                    rootLayout.Clip(clipRect);
                }
            }, 0, 1, Easing.CubicOut);

            return anim;
        }
    }
}
