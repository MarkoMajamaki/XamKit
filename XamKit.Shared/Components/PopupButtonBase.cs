using System;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public abstract class PopupButtonBase : ToggleButton
    {
        protected Popup m_popup = null;

        /// <summary>
        /// Event to raise when 'IsOpen' changes
        /// </summary>
        public event EventHandler<bool> IsOpenChanged;

        #region Dependency properties

        /// <summary>
        /// Is popup open
        /// </summary>
        public static readonly BindableProperty IsOpenProperty =
            BindableProperty.Create("IsOpen", typeof(bool), typeof(PopupButtonBase), false, propertyChanged: OnIsOpenChanged);

        static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((PopupButtonBase)bindable).OnIsOpenChanged((bool)oldValue, (bool)newValue);
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Popup placement
        /// </summary>
        public static readonly BindableProperty PopupPlacementProperty =
            BindableProperty.Create("PopupPlacement", typeof(PopupPlacements), typeof(PopupButtonBase), PopupPlacements.TopLeft);

        public PopupPlacements PopupPlacement
        {
            get { return (PopupPlacements)GetValue(PopupPlacementProperty); }
            set { SetValue(PopupPlacementProperty, value); }
        }

        /// <summary>
        /// Actual popup placement which depends screen edges and popup size etc.
        /// </summary>
        public static readonly BindableProperty ActualPopupPlacementProperty =
            BindableProperty.Create("ActualPopupPlacement", typeof(PopupPlacements), typeof(PopupButtonBase), PopupPlacements.TopLeft, propertyChanged: OnActualPopupPlacementChanged);

        private static void OnActualPopupPlacementChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as PopupButtonBase).OnActualPopupPlacementChanged((PopupPlacements)newValue);
        }

        public PopupPlacements ActualPopupPlacement
        {
            get { return (PopupPlacements)GetValue(ActualPopupPlacementProperty); }
            protected set { SetValue(ActualPopupPlacementProperty, value); }
        }

        /// <summary>
        /// Spacing between popup and PlacementTarget
        /// </summary>
        public static readonly BindableProperty PopupSpacingProperty =
            BindableProperty.Create("PopupSpacing", typeof(double), typeof(PopupButtonBase), 0.0);

        public double PopupSpacing
        {
            get { return (double)GetValue(PopupSpacingProperty); }
            set { SetValue(PopupSpacingProperty, value); }
        }

        /// <summary>
        /// Popup style
        /// </summary>
        public static readonly BindableProperty PopupStyleProperty =
            BindableProperty.Create("PopupStyle", typeof(Style), typeof(PopupButtonBase), null);

        public Style PopupStyle
        {
            get { return (Style)GetValue(PopupStyleProperty); }
            set { SetValue(PopupStyleProperty, value); }
        }

        #endregion

        protected virtual void OnActualPopupPlacementChanged(PopupPlacements newPlacement)
        {
            return;
        }

        /// <summary>
        /// Create popup content
        /// </summary>
        protected virtual View CreatePopupContent()
        {
            return null;
        }

        /// <summary>
        /// Called when 'IsOpen' changed
        /// </summary>
        private void OnIsOpenChanged(bool oldValue, bool isOpen)
        {
            if (m_popup == null && isOpen)
            {
                m_popup = CreatePopup();
            }

            if (IsOpenChanged != null)
            {
                IsOpenChanged(this, isOpen);
            }

            m_popup.IsOpen = isOpen;

            if (isOpen == false)
            {
                _skiaCanvas.InvalidateSurface();
            }
        }

        /// <summary>
        /// Create popup and add PopupContent to it
        /// </summary>
        protected Popup CreatePopup()
        {
            Popup popup = new Popup();
            popup.Style = PopupStyle;
            popup.IsOpenChanged += OnIsPopupOpenChanged;
            popup.PlacementTarget = this;
            popup.Content = CreatePopupContent();

            Binding bind = new Binding("PopupPlacement");
            bind.Source = this;
            popup.SetBinding(Popup.PlacementProperty, bind);

            bind = new Binding("ActualPopupPlacement");
            bind.Source = this;
            bind.Mode = BindingMode.OneWay;
            popup.SetBinding(Popup.ActualPlacementProperty, bind);

            bind = new Binding("PopupSpacing");
            bind.Source = this;
            popup.SetBinding(Popup.SpacingProperty, bind);

            return popup;
        }

        /// <summary>
        /// Event handler for popup 'IsOpen' changes
        /// </summary>
        private void OnIsPopupOpenChanged(object sender, bool isOpen)
        {
            IsOpen = isOpen;

            if (IsOpen)
            {
                OnPopupOpened();
            }
            else
            {
                OnPopupClosed();
            }
        }

        protected virtual void OnPopupOpened()
        {
            return;
        }

        protected virtual void OnPopupClosed()
        {
            return;
        }
    }
}
