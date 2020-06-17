using System.Collections;
using Xamarin.Forms;

namespace XamKit
{
    public interface IGeneratorHost
    {
        /// <summary>
        /// Generator host items
        /// </summary>
        IList ItemsSource { get; }

        /// <summary>
        /// Return true if the item is (or should be) its own item container
        /// </summary>
        bool IsItemItsOwnContainer(object item);

        /// <summary>
        /// Return UI element inside item container from ItemTemplate or ItemTemplateSelector
        /// </summary>
        /// <param name="itemContainer">Item container</param>
        /// <param name="model">Item model</param>
        /// <returns>Item container content UI element</returns>
        DataTemplate CreateItemTemplate(object model);

        /// <summary>
        /// Prepare the element for the corresponding item model.
        /// </summary>
        void PrepareItemView(View itemView, object model);
    }
}
