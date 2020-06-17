using Xamarin.Forms;

namespace XamKit
{
	public class NavigationAnimationGroup : BindableObject
	{		
		public static readonly BindableProperty InProperty =
			BindableProperty.Create("In", typeof(IAnimation), typeof(NavigationAnimationGroup), null);

		public IAnimation In
		{
			get { return (IAnimation)GetValue(InProperty); }
			set { SetValue(InProperty, value); }
		}

		public static readonly BindableProperty BackInProperty =
			BindableProperty.Create("BackIn", typeof(IAnimation), typeof(NavigationAnimationGroup), null);

		public IAnimation BackIn
		{
			get { return (IAnimation)GetValue(BackInProperty); }
			set { SetValue(BackInProperty, value); }
		}

		public static readonly BindableProperty OutProperty =
			BindableProperty.Create("Out", typeof(IAnimation), typeof(NavigationAnimationGroup), null);

		public IAnimation Out
		{
			get { return (IAnimation)GetValue(OutProperty); }
			set { SetValue(OutProperty, value); }
		}

		public static readonly BindableProperty BackOutProperty =
			BindableProperty.Create("BackOut", typeof(IAnimation), typeof(NavigationAnimationGroup), null);

		public IAnimation BackOut
		{
			get { return (IAnimation)GetValue(BackOutProperty); }
			set { SetValue(BackOutProperty, value); }
		}

		// Modal navigation animations

		public static readonly BindableProperty ModalInProperty =
			BindableProperty.Create("ModalIn", typeof(IAnimation), typeof(NavigationAnimationGroup), null);

		public IAnimation ModalIn
		{
			get { return (IAnimation)GetValue(ModalInProperty); }
			set { SetValue(ModalInProperty, value); }
		}

		public static readonly BindableProperty ModalOutProperty =
			BindableProperty.Create("ModalOut", typeof(IAnimation), typeof(NavigationAnimationGroup), null);

		public IAnimation ModalOut
		{
			get { return (IAnimation)GetValue(ModalOutProperty); }
			set { SetValue(ModalOutProperty, value); }
		}

		/// <summary>
		/// Is dark overlay between pages enabled when navigating
		/// </summary>
		public static readonly BindableProperty IsDarkOverlayEnabledProperty =
            BindableProperty.Create("IsDarkOverlayEnabled", typeof(bool), typeof(NavigationAnimationGroup), true);

        public bool IsDarkOverlayEnabled
        {
            get { return (bool)GetValue(IsDarkOverlayEnabledProperty); }
            set { SetValue(IsDarkOverlayEnabledProperty, value); }
        }

		/// <summary>
		/// Is new page shadow enabled when navigating
		/// </summary>
		public static readonly BindableProperty IsShadowEnabledProperty =
			BindableProperty.Create("IsShadowEnabled", typeof(bool), typeof(NavigationAnimationGroup), true);

		public bool IsShadowEnabled
		{
			get { return (bool)GetValue(IsShadowEnabledProperty); }
			set { SetValue(IsShadowEnabledProperty, value); }
		}
		/// <summary>
		/// Override previous page out navigation animation
		/// </summary>
		public static readonly BindableProperty PreviousPageOutOverrideProperty =
            BindableProperty.Create("PreviousPageOutOverride", typeof(IAnimation), typeof(NavigationAnimationGroup), null);

        public IAnimation PreviousPageOutOverride
        {
            get { return (IAnimation)GetValue(PreviousPageOutOverrideProperty); }
            set { SetValue(PreviousPageOutOverrideProperty, value); }
        }

        /// <summary>
        /// Override previous page back in navigation animation
        /// </summary>
        public static readonly BindableProperty PreviousPageBackInOverrideProperty =
            BindableProperty.Create("PreviousPageBackInOverride", typeof(IAnimation), typeof(NavigationAnimationGroup), null);

        public IAnimation PreviousPageBackInOverride
        {
            get { return (IAnimation)GetValue(PreviousPageBackInOverrideProperty); }
            set { SetValue(PreviousPageBackInOverrideProperty, value); }
        }
    }
}

