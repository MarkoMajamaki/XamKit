using System;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	/// <summary>
	/// Event when index changed
	/// </summary>
	public delegate void IndexChangedEvent(object sender, int index);

	/// <summary>
	/// Event to raise in custom layouts when 'HasHiddenChindren' changes
	/// </summary>
	public delegate void HasHiddenChildrenDelegate(Layout sender, bool hasHiddenChildren);

	/// <summary>
	/// Event to raise in custom layouts when 'HiddenChildrenCount' changes
	/// </summary>
	public delegate void HiddenChildrenCountDelegate(Layout sender, int count);

	/// <summary>
	/// Event to raise when layout is ready
	/// </summary>
	public delegate void LayoutReady(Layout sender);
}
