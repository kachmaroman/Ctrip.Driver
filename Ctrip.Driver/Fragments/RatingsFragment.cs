using Android.OS;
using Android.Views;

namespace Ctrip.Driver.Fragments
{
	public class RatingsFragment : Android.Support.V4.App.Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.rating, container, false);

			return view;
		}
	}
}