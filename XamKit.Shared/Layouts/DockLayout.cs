using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public enum Dock { Left, Top, Right, Bottom, Fill }

	public class DockLayout : Layout<View>
	{
		/// <summary>
		/// Child info
		/// </summary>
		protected class ChildInfo
		{
			public Size Size { get; set; }
			public View Child { get; set; }

			public ChildInfo(View child, Size size)
			{
				Size = size;
				Child = child;
			}
		}

		protected List<ChildInfo> m_childChache = null;

		private Size m_previousAvailableSize = new Size();
		private Size m_previousMeasuredSize = new Size();

		#region Attached properties

		/// <summary>
		/// Where child is layouted
		/// </summary>
		public static readonly BindableProperty DockProperty =
			BindableProperty.CreateAttached("Dock", typeof(Dock), typeof(DockLayout), Dock.Fill);

		public static Dock GetDock(BindableObject view)
		{
			return (Dock)view.GetValue(DockProperty);
		}

		public static void SetDock(BindableObject view, Dock value)
		{
			view.SetValue(DockProperty, value);
		}

		#endregion

		public bool IsDebugEnabled { get; set; } = false;

		public DockLayout()
		{
			m_childChache = new List<ChildInfo>();
		}

		/// <summary>
		/// Layout children to correct positions
		/// </summary>

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			if (IsDebugEnabled) { Debug.WriteLine("DockLayout.LayoutChildren"); }

			MeasureChildren(width, height);

			// Do measure for every child if not measured
			if (m_childChache.Count == 0)
			{
				foreach (View child in Children)
				{
					Size size = new Size(0, 0);
					if (child.IsVisible)
					{
                        size = child.Measure(width, height, MeasureFlags.IncludeMargins).Request;
					}

					m_childChache.Add(new ChildInfo(child, size));
				}
			}

			double accumulatedLeft = 0;
			double accumulatedTop = 0;
			double accumulatedRight = 0;
			double accumulatedBottom = 0;

			List<ChildInfo> filledChildren = new List<ChildInfo>();

			// Layout children to actual locations
			foreach (ChildInfo childInfo in m_childChache)
			{
				Rectangle childLocation = new Rectangle(
					accumulatedLeft,
					accumulatedTop,
					Math.Max(0.0, width - (accumulatedLeft + accumulatedRight)),
					Math.Max(0.0, height - (accumulatedTop + accumulatedBottom)));
				
				switch (GetDock(childInfo.Child))
				{
					case Dock.Left:
						accumulatedLeft += childInfo.Size.Width;
						childLocation.Width = childInfo.Size.Width;
						break;

					case Dock.Right:
						accumulatedRight += childInfo.Size.Width;
						childLocation.X = Math.Max(0.0, width - accumulatedRight);
						childLocation.Width = childInfo.Size.Width;
						break;

					case Dock.Top:
						accumulatedTop += childInfo.Size.Height;
						childLocation.Height = childInfo.Size.Height;
						break;

					case Dock.Bottom:
						accumulatedBottom += childInfo.Size.Height;
						childLocation.Y = Math.Max(0.0, height - accumulatedBottom);
						childLocation.Height = childInfo.Size.Height;
						break;
					case Dock.Fill:
						filledChildren.Add(childInfo);
						continue;
				}

				LayoutChildIntoBoundingRegion(childInfo.Child, childLocation);
			}

			foreach (ChildInfo childInfo in filledChildren)
			{
				Rectangle childLocation = new Rectangle(
					accumulatedLeft,
					accumulatedTop,
					Math.Max(0.0, width - (accumulatedLeft + accumulatedRight)),
					Math.Max(0.0, height - (accumulatedTop + accumulatedBottom)));
			
				LayoutChildIntoBoundingRegion(childInfo.Child, childLocation);
			}
		}

		/// <summary>
		/// Measure layout size
		/// </summary>
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (IsDebugEnabled) { Debug.WriteLine("DockLayout.OnMeasure"); }

			return MeasureChildren(widthConstraint, heightConstraint);
		}

		/// <summary>
		/// Do measure to all children
		/// </summary>
		private SizeRequest MeasureChildren(double width, double height)
		{
			if (m_previousAvailableSize.Width.Equals(width) && m_childChache.Count > 0)
			{
				return new SizeRequest(m_previousMeasuredSize, m_previousMeasuredSize);
			}

			m_childChache.Clear();

			List<View> nonFillChildren = new List<View>();
			Size filledChildrenMaxSize = new Size();

			// Measure all children which are Dock.Fill
			foreach (View child in Children)
			{
				if (GetDock(child) == Dock.Fill)
				{
					Size size = new Size(0, 0);
					if (child.IsVisible)
					{
                        size = child.Measure(width, height, MeasureFlags.IncludeMargins).Request;
					}

					m_childChache.Add(new ChildInfo(child, size));

					if (size.Width > filledChildrenMaxSize.Width)
					{
						filledChildrenMaxSize.Width = size.Width;
					}
					if (size.Height > filledChildrenMaxSize.Height)
					{
						filledChildrenMaxSize.Height = size.Height;
					}

                    if (child.HorizontalOptions.Alignment == LayoutAlignment.Fill && double.IsInfinity(width) == false && size.Width > 0)
					{
						filledChildrenMaxSize.Width = width;
					}
                    if (child.VerticalOptions.Alignment == LayoutAlignment.Fill && double.IsInfinity(height) == false && size.Height > 0)
					{
						filledChildrenMaxSize.Height = height;
					}
				}
				else
				{
					nonFillChildren.Add(child);
				}
			}

			double accumulatedWidth = 0;
			double accumulatedHeight = 0;

			// Measure other children around them
			foreach (View child in nonFillChildren)
			{
				Size size = new Size(0, 0);
				if (child.IsVisible)
				{
                    size = child.Measure(width, height, MeasureFlags.IncludeMargins).Request;
				}

				m_childChache.Add(new ChildInfo(child, size));

				switch (DockLayout.GetDock(child))
				{
					case Dock.Left:
					case Dock.Right:
						accumulatedWidth += size.Width;
                        accumulatedHeight = Math.Max(accumulatedHeight, size.Height);
						break;

					case Dock.Top:
					case Dock.Bottom:
						accumulatedHeight += size.Height;
                        accumulatedWidth = Math.Max(accumulatedWidth, size.Width);
						break;
				}
			}

			Size totalSize = new Size(Math.Max(0, filledChildrenMaxSize.Width + accumulatedWidth),
									  Math.Max(0, filledChildrenMaxSize.Height + accumulatedHeight));

			m_previousMeasuredSize = totalSize;

			return new SizeRequest(totalSize, totalSize);
		}
	}
}
