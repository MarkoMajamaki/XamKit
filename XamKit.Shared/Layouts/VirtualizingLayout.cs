using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamKit
{
    public abstract class VirtualizingLayout : Layout<View>, IVirtualizingLayout
    {
        private double _verticalOffset = 0;
        private double _horizontalOffset = 0;
        private Size _viewportSize = new Size();

        /// <summary>
        /// All items
        /// </summary>
        public ItemsGenerator ItemsGenerator { get; set; }

        #region IScrollInfo

        /// <summary>
        /// Host scrollviewer vertical scroll offset
        /// </summary>
        public double VerticalOffset
        {
            get
            {
                return _verticalOffset;
            }
            set
            {
                double oldVerticalOffset = _verticalOffset;
                _verticalOffset = value;
                OnVerticalScrollChanged(oldVerticalOffset, value);
            }
        }

        /// <summary>
        /// Host scrollviewer horizontal scroll offset
        /// </summary>
        public double HorizontalOffset
        {
            get
            {
                return _horizontalOffset;
            }
            set
            {
                double oldHorizontalOffset = _horizontalOffset;
                _horizontalOffset = value;
                OnHorizontalScrollChanged(oldHorizontalOffset, value);
            }
        }

        /// <summary>
        /// Host scrollviewer horizontal scroll offset
        /// </summary>
        public Size ViewportSize
        {
            get
            {
                return _viewportSize;
            }
            set
            {
                Size oldValue = _viewportSize;
                _viewportSize = value;

                OnViewportSizeChanged(oldValue, value);
            }
        }

        #endregion

        public VirtualizingLayout()
        {
        }

        /// <summary>
        /// Initialize layout for first measure
        /// </summary>
        public abstract void Initialize();        

        /// <summary>
        /// Called when host horizontal scroll offset changes. Used for items virtualization.
        /// </summary>
        protected virtual void OnHorizontalScrollChanged(double oldOffset, double newOffset)
        {
            return;
        }

        /// <summary>
        /// Called when host vertical scroll offset changes. Used for items virtualization.
        /// </summary>
        protected virtual void OnVerticalScrollChanged(double oldOffset, double newOffset)
        {
            return;
        }

        /// <summary>
        /// Called when host viewport size changed.
        /// </summary>
        protected virtual void OnViewportSizeChanged(Size oldSize, Size newSize)
        {
            return;
        }
    }
}
