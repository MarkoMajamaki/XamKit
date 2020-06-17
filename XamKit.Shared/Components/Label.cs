using System;
using Xamarin.Forms;

namespace XamKit
{
	public class Label : Xamarin.Forms.Label
	{
        /*
		/// <summary>
		/// True if all characters are uppercase, false if all is lowercase. Null if ignored.
		/// </summary>
		public static readonly BindableProperty IsUpperProperty =
			BindableProperty.Create("IsUpper", typeof(bool?), typeof(XamKit.Label), null, propertyChanged: OnIsUpperChanged);

		static void OnIsUpperChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Label)bindable).OnIsUpperChanged();
		}

		public bool? IsUpper
		{
			get { return (bool?)GetValue(IsUpperProperty); }
			set { SetValue(IsUpperProperty, value); }
		}


		public static new readonly BindableProperty TextProperty =
			BindableProperty.Create("Text", typeof(string), typeof(XamKit.Label), null, propertyChanged: OnTextChanged);

		private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((XamKit.Label)bindable).OnTextChanged(oldValue as string, newValue as string);
		}

		public new string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public Label()
		{
		}

		/// <summary>
		/// Called when custom 'Text' changes
		/// </summary>
		private void OnTextChanged(string oldText, string newText)
		{
			base.Text = FormText(Text);
		}

		/// <summary>
		/// Called when 'IsUpper' changes
		/// </summary>
		public void OnIsUpperChanged()
		{
			OnTextChanged(null, this.Text);
		}

		/// <summary>
		/// Format text based on 'IsUpper'
		/// </summary>
		private string FormText(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (IsUpper != null)
				{
					if (IsUpper == true)
					{
						text = text.ToUpper();
					}
					else
					{
						text = text.ToLower();
					}
				}
			}
			return text;	
		}
        */
	}
}

