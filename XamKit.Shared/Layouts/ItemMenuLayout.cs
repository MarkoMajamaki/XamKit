using System;
using System.Collections.Generic;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{    
    public class ItemMenuLayout : Layout<View>, IItemPanMenuLayout
    {
        private class ChildInfo
        {
            public View Child { get; private set; }
            public SizeRequest SizeRequest { get; private set; }

            public ChildInfo(View child, SizeRequest sizeRequest)
            {
                Child = child;
                SizeRequest = sizeRequest;
            }
		}
        
        private List<ChildInfo> m_childrenCache = null;

        public ItemMenuLayout()
        {
            m_childrenCache = new List<ChildInfo>();
        }

		/// <summary>
		/// Update children position based on pan
		/// </summary>
		public void PanUpdated(double horizontalPan)
        {
            double x = Math.Abs(horizontalPan / Width);            
			double xOffset = 0;

			foreach (ChildInfo childInfo in m_childrenCache)
			{
                if (horizontalPan < 0)
                {
                    LayoutChildIntoBoundingRegion(childInfo.Child, new Rectangle(xOffset * x + (Width * (1 - x)), 0, childInfo.SizeRequest.Request.Width * x, Height));
				}
                else
                {
					LayoutChildIntoBoundingRegion(childInfo.Child, new Rectangle(xOffset * x, 0, childInfo.SizeRequest.Request.Width * x, Height));
				}

                xOffset += childInfo.SizeRequest.Request.Width;
			}
        }

        /// <summary>
        /// Measure layout total size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return MeasureChildren(widthConstraint, heightConstraint);
        }

		/// <summary>
		/// Arrange children into layout
		/// </summary>
		protected override void LayoutChildren(double x, double y, double width, double height)
        {
            MeasureChildren(width, height);

            double xOffset = 0;
            foreach (ChildInfo childInfo in m_childrenCache)
            {
                LayoutChildIntoBoundingRegion(childInfo.Child, new Rectangle(xOffset, 0, childInfo.SizeRequest.Request.Width, height));
                xOffset += childInfo.SizeRequest.Request.Width;
            }
        }

        /// <summary>
        /// Measure all children and update cache
        /// </summary>
        private SizeRequest MeasureChildren(double width, double height)
        {
			m_childrenCache.Clear();
			Size totalSize = new Size();

			foreach (View child in Children)
			{
				SizeRequest size = child.Measure(width, height, MeasureFlags.IncludeMargins);
				ChildInfo info = new ChildInfo(child, size);
				m_childrenCache.Add(info);

				totalSize.Width += size.Request.Width;
			}

			totalSize.Height = height;

			return new SizeRequest(totalSize, totalSize);
        }
    }
}
