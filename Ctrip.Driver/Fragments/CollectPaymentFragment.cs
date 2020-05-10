using System;
using System.Globalization;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Ctrip.Driver.Fragments
{
    public class CollectPaymentFragment : Android.Support.V4.App.DialogFragment
    {
	    readonly double mfares;

        TextView _totalfaresText;
        Button _collectPayButton;

        public event EventHandler PaymentCollected;

        public CollectPaymentFragment(double fares)
        {
            mfares = fares;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
	        View view = inflater.Inflate(Resource.Layout.collect_payment, container, false);

            _totalfaresText = view.FindViewById<TextView>(Resource.Id.totalfaresText);
            _collectPayButton = view.FindViewById<Button>(Resource.Id.collectPayButton);

            _totalfaresText.Text = mfares.ToString(CultureInfo.InvariantCulture) + " UAH";
            _collectPayButton.Click += CollectPayButton_Click;

            return view;
        }

        private void CollectPayButton_Click(object sender, EventArgs e)
        {
            PaymentCollected?.Invoke(this, new EventArgs());
        }
    }
}
