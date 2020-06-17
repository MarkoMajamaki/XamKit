using System.Diagnostics;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public class TestView : Layout<View>
	{
		public static readonly BindableProperty TagProperty =
			BindableProperty.Create("Tag", typeof(string), typeof(TestView), "test");

		public string Tag
		{
			get { return (string)GetValue(TagProperty); }
			set { SetValue(TagProperty, value); }
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			Debug.WriteLine("OnMeasure: " + Tag + " (" + widthConstraint + "," + heightConstraint + ")");

			return new SizeRequest(new Size(0, 0), new Size(0, 0));
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			Debug.WriteLine("LayoutChildren: " + Tag + " (" + x + "," + y + "," + width + "," + height + ")");
		}
	}
}
