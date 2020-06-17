using System;
using System.Windows.Input;

namespace XamKit
{
	public interface ITappable
	{
		ICommand Command { get; set; }

		event EventHandler Tapped;
	}
}
