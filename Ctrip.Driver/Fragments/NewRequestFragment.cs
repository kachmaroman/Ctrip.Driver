using System;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Ctrip.Driver.Fragments
{
	public class NewRequestFragment : Android.Support.V4.App.DialogFragment
	{
		RelativeLayout _acceptRideButton;
		RelativeLayout _rejectRideButton;
		TextView _pickupAddressText;
		TextView _destinationAddressText;

		readonly string _mPickupAddress;
		readonly string _mDestinationAddress;

		public event EventHandler RideAccepted;
		public event EventHandler RideRejected;

		public NewRequestFragment(string pickupAddress, string destinationAddress)
		{
			_mPickupAddress = pickupAddress;
			_mDestinationAddress = destinationAddress;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.newrequest_dialogue, container, false);
			_pickupAddressText = view.FindViewById<TextView>(Resource.Id.newridePickupText);
			_destinationAddressText = view.FindViewById<TextView>(Resource.Id.newrideDestinationText);

			_pickupAddressText.Text = _mPickupAddress;
			_destinationAddressText.Text = _mDestinationAddress;

			_acceptRideButton = view.FindViewById<RelativeLayout>(Resource.Id.acceptRideButton);
			_rejectRideButton = view.FindViewById<RelativeLayout>(Resource.Id.rejectRideButton);

			_acceptRideButton.Click += AcceptRideButton_Click;
			_rejectRideButton.Click += RejectRideButton_Click;

			return view;
		}

		private void AcceptRideButton_Click(object sender, EventArgs e)
		{
			RideAccepted?.Invoke(this, new EventArgs());
		}

		private void RejectRideButton_Click(object sender, EventArgs e)
		{
			RideRejected?.Invoke(this, new EventArgs());
		}
	}
}
