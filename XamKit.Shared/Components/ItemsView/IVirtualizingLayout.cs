using Xamarin.Forms;

namespace XamKit
{
    public interface IVirtualizingLayout
    {
        /// <summary>
        /// Vertical scroll offset
        /// </summary>
        double VerticalOffset { get; set; }

        /// <summary>
        /// Horizontal scroll offset
        /// </summary>
        double HorizontalOffset { get; set; }

        /// <summary>
        /// Scrollviewer viewport size. Used in virtualization.
        /// </summary>
        Size ViewportSize { get; set; }

        /// <summary>
        /// Items container generator
        /// </summary>
        ItemsGenerator ItemsGenerator { get; set; }

        /// <summary>
        /// Initialize panel for first measure and layout
        /// </summary>
        void Initialize();
    }

    public interface IItemsViewLayout
    {
        /// <summary>
        /// Add child without layout and measure invalidation
        /// </summary>
        void AddChild(View child);

        /// <summary>
        /// Remove child without layout and measure invalidation
        /// </summary>
        void RemoveChild(View child);

        /// <summary>
        /// Insert child without layout and measure invalidation
        /// </summary>
        void InsertChild(int index, View child);
    }
}
