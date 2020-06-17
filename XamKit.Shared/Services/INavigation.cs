using System.Collections.Generic;
using System.Threading.Tasks;

namespace XamKit
{
    public interface INavigation
	{
		IReadOnlyList<ContentPage> NavigationStack { get; }
        Task PushAsync(ContentPage page, object parameter = null);
        Task PopAsync(object parameter = null);

        IReadOnlyList<ContentPage> ModalNavigationStack { get; }
        Task<object> PushModalAsync(ContentPage page, object parameter = null);
        Task PopModalAsync(object parameter = null, bool popAll = false);

        Task PushRootAsync(ContentPage page, object parameter = null, bool isAnimated = true);
        void Clear();
    }
}