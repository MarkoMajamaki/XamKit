using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Interface for any element which has Content property
    /// </summary>
    public interface IContent
    {
        object Content { get; set; }

        DataTemplate ContentTemplate { get; set; }
    }
}
