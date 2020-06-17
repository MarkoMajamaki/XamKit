using System.Collections;
using System.Windows.Input;

namespace XamKit
{
	public interface IMenuItem
	{
		string Text { get; set; }
		ICommand Command { get; }
		IList ItemsSource { get; }
		bool IsToggable { get; }
		string IconResourceKey { get; set; }
        string IconAssemblyName { get; set; }
    }

    public interface IMenuItemSeparator
    {
        string Text { get; set; }
    }
}
