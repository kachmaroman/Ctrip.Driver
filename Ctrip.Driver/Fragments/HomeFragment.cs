using System;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Views;
using Android.Widget;
using Ctrip.Driver.Helpers;
using static Ctrip.Driver.Helpers.LocationCallbackHelper;

namespace Ctrip.Driver.Fragments
{
	public class HomeFragment : Android.Support.V4.App.Fragment, IOnMapReadyCallback
	{
		public EventHandler<OnLocationCaptionEventArgs> CurrentLocation;
		public GoogleMap MainMap;

		ImageView _centerMarker;

		LocationRequest _mLocationRequest;
		FusedLocationProviderClient _locationProviderClient;
		Android.Locations.Location _mLastlocation;
		readonly LocationCallbackHelper mLocationCallback = new LocationCallbackHelper();

		static int UPDATE_INTERVAL = 5; //Seconds
		static int FASTEST_INTERVAL = 5; //Seconds
		static int DISPLACEMENT = 1; //METRES;

		LinearLayout _rideInfoLayout;
		TextView _riderNameText;
		ImageButton _cancelTripButton;
		ImageButton _callRiderButton;
		ImageButton _navigateButton;
		Button _tripButton;

		bool _tripCreated = false;
		bool _driverArrived = false;
		bool _tripStarted = false;

		public event EventHandler CallRider;
		public event EventHandler Navigate;
		public event EventHandler TripActionStartTrip;
		public event EventHandler TripActionArrived;
		public event EventHandler TripActionEndTrip;

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			CreateLocationRequest();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.home, container, false);
			SupportMapFragment mapFragment = (SupportMapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map);
			_centerMarker = view.FindViewById<ImageView>(Resource.Id.centerMarker);
			mapFragment.GetMapAsync(this);

			_cancelTripButton = view.FindViewById<ImageButton>(Resource.Id.cancelTripButton);
			_callRiderButton = view.FindViewById<ImageButton>(Resource.Id.callRiderButton);
			_navigateButton = view.FindViewById<ImageButton>(Resource.Id.navigateButton);
			_tripButton = view.FindViewById<Button>(Resource.Id.tripButton);
			_riderNameText = view.FindViewById<TextView>(Resource.Id.riderNameText);
			_rideInfoLayout = view.FindViewById<LinearLayout>(Resource.Id.rideInfoLayout);

			_tripButton.Click += TripButton_Click;
			_callRiderButton.Click += CallRiderButton_Click;
			_navigateButton.Click += NavigateButton_Click;

			return view;
		}

		private void NavigateButton_Click(object sender, EventArgs e)
		{
			Navigate?.Invoke(this, new EventArgs());
		}

		private void CallRiderButton_Click(object sender, EventArgs e)
		{
			CallRider?.Invoke(this, new EventArgs());
		}

		private void TripButton_Click(object sender, EventArgs e)
		{
			if (!_driverArrived && _tripCreated)
			{
				_driverArrived = true;
				TripActionArrived?.Invoke(this, new EventArgs());
				_tripButton.Text = "Start Trip";

				return;
			}

			if (!_tripStarted && _driverArrived)
			{
				_tripStarted = true;
				TripActionStartTrip?.Invoke(this, new EventArgs());
				_tripButton.Text = "End Trip";

				return;
			}

			if (_tripStarted)
			{
				TripActionEndTrip?.Invoke(this, new EventArgs());
			}
		}

		public void OnMapReady(GoogleMap googleMap)
		{
			MainMap = googleMap;
		}

		private void CreateLocationRequest()
		{
			_mLocationRequest = new LocationRequest();
			_mLocationRequest.SetInterval(UPDATE_INTERVAL);
			_mLocationRequest.SetFastestInterval(FASTEST_INTERVAL);
			_mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
			_mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
			mLocationCallback.MyLocation += MLocationCallback_MyLocation;
			_locationProviderClient = LocationServices.GetFusedLocationProviderClient(Activity);
		}

		private void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnLocationCaptionEventArgs e)
		{
			_mLastlocation = e.Location;

			LatLng myposition = new LatLng(_mLastlocation.Latitude, _mLastlocation.Longitude);
			MainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15));

			CurrentLocation?.Invoke(this, new OnLocationCaptionEventArgs { Location = e.Location });
		}

		private void StartLocationUpdates()
		{
			_locationProviderClient.RequestLocationUpdates(_mLocationRequest, mLocationCallback, null);
		}

		private void StopLocationUpdates()
		{
			_locationProviderClient.RemoveLocationUpdates(mLocationCallback);
		}

		public void GoOnline()
		{
			_centerMarker.Visibility = ViewStates.Visible;
			StartLocationUpdates();
		}

		public void GoOffline()
		{
			_centerMarker.Visibility = ViewStates.Invisible;
			StopLocationUpdates();
		}

		public void CreateTrip(string ridername)
		{
			_centerMarker.Visibility = ViewStates.Invisible;
			_riderNameText.Text = ridername;
			_rideInfoLayout.Visibility = ViewStates.Visible;
			_tripCreated = true;
		}

		public void ResetAfterTrip()
		{
			_tripButton.Text = "Arrived Pickup";
			_centerMarker.Visibility = ViewStates.Visible;
			_riderNameText.Text = "";
			_rideInfoLayout.Visibility = ViewStates.Invisible;
			_tripCreated = false;
			_driverArrived = false;
			_tripStarted = false;
			MainMap.Clear();
			MainMap.TrafficEnabled = false;
			MainMap.UiSettings.ZoomControlsEnabled = false;
		}
	}
}