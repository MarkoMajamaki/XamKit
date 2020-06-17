using Xamarin.Forms;

namespace XamKit
{
	public interface IAnimation
	{
		uint Duration { get; }
		
		Animation Create(View target);
	}
}