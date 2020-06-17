using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using UIKit;
using Foundation;

namespace XamKit.iOS
{
    public class UIPressedGestureRecognizer : UIGestureRecognizer
    {
        public delegate void PressedEvent();

        public event PressedEvent Pressed;
        public event PressedEvent Released;
        public event PressedEvent Canceled;
        public event PressedEvent Moved;

        public UIPressedGestureRecognizer()
        {
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            if (Moved != null)
            {
                Moved();
            }
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
        
            if (Pressed != null)
            {
                Pressed();
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            if (Released != null)
            {
                Released();
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            if (Canceled != null)
            {
                Canceled();
            }
        }

        public override void IgnoreTouch(UITouch touch, UIEvent forEvent)
        {
            base.IgnoreTouch(touch, forEvent);

            if (Canceled != null)
            {
                Canceled();
            }
        }        
    }
}
