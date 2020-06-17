using Xamarin.Forms;

namespace XamKit
{
	public interface ILayout
	{
		/// <summary>
		/// Bring child into view by child index
		/// </summary>
		void BringIntoViewport(int index, SnapPointsAlignments location = SnapPointsAlignments.Start, bool isAnimated = false);

		/// <summary>
		/// Bring child into view by child binding context
		/// </summary>
		void BringIntoViewport(View child, SnapPointsAlignments location = SnapPointsAlignments.Start, bool isAnimated = false);

		void AddChild(View child, bool ignoreInvalidation = false);
		void InsertChild(int index, View child, bool ignoreInvalidation = false);
		void RemoveChild(View child, bool ignoreInvalidation = false);
	}
}
