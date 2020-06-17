using System;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	[ContentProperty("PopupContent")]
	public class PopupButton : PopupButtonBase
	{
		#region Dependency properties

		/// <summary>
		/// Popup content data template
		/// </summary>
		public static readonly BindableProperty PopupContentTemplateProperty =
			BindableProperty.Create("PopupContentTemplate", typeof(DataTemplate), typeof(PopupButton), null);

		public DataTemplate PopupContentTemplate
		{
			get { return (DataTemplate)GetValue(PopupContentTemplateProperty); }
			set { SetValue(PopupContentTemplateProperty, value); }
		}

		/// <summary>
		/// Actual popup content (could be created from popup content data template)
		/// </summary>
		public static readonly BindableProperty PopupContentProperty =
			BindableProperty.Create("PopupContent", typeof(View), typeof(PopupButton), null);

		public View PopupContent
		{
			get { return (View)GetValue(PopupContentProperty); }
			set { SetValue(PopupContentProperty, value); }
		}

        #endregion

        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == PopupContentProperty.PropertyName || propertyName == PopupContentTemplateProperty.PropertyName)
            {
                if (m_popup != null && m_popup.Content != null)
                {
                    View newContent = CreatePopupContent();

                    if (newContent != m_popup.Content)
                    {
                        m_popup.Content = newContent;
                    }
                }
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Create popup content from 'PopupContent' or 'PopupContentTemplate'
        /// </summary>
        protected override View CreatePopupContent()
        {
            if (PopupContent != null)
            {
                return PopupContent;
            }
            else if (PopupContentTemplate != null)
            {
                PopupContent = PopupContentTemplate.CreateContent() as View;
            }

            return PopupContent;
        }

        /// <summary>
        /// Called when 'IsToggled' changed
        /// </summary>
        protected override void OnIsToggledChanged(bool toggled)
		{
			base.OnIsToggledChanged(toggled);
			IsOpen = toggled;
		}

		/// <summary>
		/// Called when 'PopupContent' changed
		/// </summary>
		protected virtual void OnPopupContentChanged(View oldContent, View newContent)
		{
			return;
		}
	}
}

