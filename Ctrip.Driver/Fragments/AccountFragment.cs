using Android.OS;
using Android.Views;

namespace Ctrip.Driver.Fragments
{
	public class AccountFragment : Android.Support.V4.App.Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.account, container, false);

			return view;
		}
	}
}