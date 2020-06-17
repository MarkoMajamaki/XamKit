using System;
using System.Collections.Generic;
using System.Reflection;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public class VisualTreeHelper
	{
		public static T GetTemplateChild<T>(View parent, string name) where T : View
		{
			if (parent == null)
			{
				return null;
			}

			T templateChild = parent.FindByName<T>(name);

			if (templateChild != null)
			{
				return templateChild;
			}

			foreach (var child in FindVisualChildren<View>(parent, false))
			{
				templateChild = GetTemplateChild<T>(child, name);
				if (templateChild != null)
				{
					return templateChild;
				}
			}

			return null;
		}

		public static IEnumerable<T> FindVisualChildren<T>(Element element, bool recursive = true) where T : View
		{
			if (element != null && element is Layout)
			{
				ILayoutController childrenProperty = element as ILayoutController;

				if (childrenProperty != null)
				{
					foreach (var child in childrenProperty.Children)
					{
						if (child != null && child is T)
						{
							yield return (T)child;
						}
						if (recursive)
						{
							foreach (T childOfChild in FindVisualChildren<T>(child))
							{
								yield return childOfChild;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Get parent view by type
		/// </summary>
		public static T GetParent<T>(View source, Type stopWhenType = null) where T : View
		{
			View p = source.Parent as View;

			while (p != null)
			{
				if (p is T p2)
				{
					return p2;
				}

				p = p.Parent as View;

				if (stopWhenType != null && p.GetType() == stopWhenType)
				{
					return null;
				}
			}

			return null;
		}
	}
}
