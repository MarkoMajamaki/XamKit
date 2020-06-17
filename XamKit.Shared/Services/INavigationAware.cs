namespace XamKit
{
    public abstract class NavigationEventArgs
    {
        public NavigationDirection Direction { get; private set; }

        public NavigationEventArgs(NavigationDirection direction)
        {
            Direction = direction;
        }
    }

    public class AppearEventArgs : NavigationEventArgs
    {
        public object Parameter { get; private set; }

        public AppearEventArgs(NavigationDirection direction, object parameter) 
            : base(direction)
        {
            Parameter = parameter;
        }
    }

    public class DissapearEventArgs : NavigationEventArgs
    {
        public DissapearEventArgs(NavigationDirection direction) 
            : base(direction)
        {
        }
    }

    /// <summary>
    /// Interface for page or viewmodel which navigation methods are called during navigation
    /// </summary>
    public interface INavigationAware
	{        
		void OnAppearing(AppearEventArgs args);

		void OnAppeared(AppearEventArgs args);

		void OnDissapearing(DissapearEventArgs args);

        void OnDissapeared(DissapearEventArgs args);
	}
}

