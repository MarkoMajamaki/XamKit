using System;
using System.Collections.Generic;
using System.Reflection;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamKit
{
	[ContentProperty("Source")]
	public class ImageResourceExtension : IMarkupExtension
	{
		/// <summary>
		/// Assembly where images resources is located
		/// </summary>
		public static Assembly ImageSourceAssembly { get; set; }
		
		public string Source { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (Source == null)
			{
				return null;
			}

			// Do your translation lookup here, using whatever method you require
			ImageSource image = ImageSource.FromResource(Source, ImageSourceAssembly);

			return image;
  		}
	}
}
