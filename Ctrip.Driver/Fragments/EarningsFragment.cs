using Android.OS;
using Android.Views;

namespace Ctrip.Driver.Fragments
{
	public class EarningsFragment : Android.Support.V4.App.Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.earnings, container, false);

			return view;
		}
	}
}