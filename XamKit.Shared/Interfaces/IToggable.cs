using System;
using System.Windows.Input;

namespace XamKit
{
	public interface IToggable
	{
		event EventHandler<bool> IsToggledChanged;

		ICommand ToggledCommand { get; }

		bool IsToggled { get; set; }
	}
}
