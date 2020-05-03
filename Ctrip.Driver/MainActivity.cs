using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Android.Support.V4.View;
using Com.Ittianyu.Bottomnavigationviewex;
using System;
using Ctrip.Driver.Adapter;
using Ctrip.Driver.Fragments;
using Android.Graphics;
using Android;
using Ctrip.Driver.EventListeners;
using Android.Gms.Maps.Model;
using Android.Support.V4.Content;
using Ctrip.Driver.DataModels;
using Android.Media;
using Ctrip.Driver.Helpers;
using Android.Content;

namespace Ctrip.Driver
{
    [Activity(Label = "@string/app_name", Theme = "@style/UberTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
	    Button _goOnlineButton;
        ViewPager _viewpager;
        BottomNavigationViewEx _bnve;

        readonly HomeFragment _homeFragment = new HomeFragment();
        readonly RatingsFragment _ratingsFragment = new RatingsFragment();
        readonly EarningsFragment _earningsFragment = new EarningsFragment();
        readonly AccountFragment _accountFragment = new AccountFragment();
        NewRequestFragment _requestFoundDialogue;

        const int RequestId = 0;
        readonly string[] _permissionsGroup =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
        };

        readonly ProfileEventListener profileEventListener = new ProfileEventListener();
        AvailablityListener _availablityListener;
        RideDetailsListener _rideDetailsListener;
        NewTripEventListener _newTripEventListener;

        Android.Locations.Location _mLastLocation;
        LatLng _mLastLatLng;

        bool _availablityStatus;
        bool _isBackground;
        bool _newRideAssigned;
        string _status = "NORMAL"; //REQUESTFOUND, ACCEPTED, ONTRIP

        RideDetails _newRideDetails;

        MediaPlayer _player;

        MapFunctionHelper _mapHelper;

        Android.Support.V7.App.AlertDialog.Builder _alert;
        Android.Support.V7.App.AlertDialog _alertDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            ConnectViews();
            CheckSpecialPermission();
            profileEventListener.Create();
        }

        void ShowProgressDialogue()
        {
            _alert = new Android.Support.V7.App.AlertDialog.Builder(this);
            _alert.SetView(Resource.Layout.progress);
            _alert.SetCancelable(false);
            _alertDialog = _alert.Show();
        }

        private void CloseProgressDialogue()
        {
            if (_alert != null)
            {
                _alertDialog.Dismiss();
                _alertDialog = null;
                _alert = null;
            }
        }

        private void ConnectViews()
        {
            _goOnlineButton = FindViewById<Button>(Resource.Id.goOnlineButton);
            _bnve = FindViewById<BottomNavigationViewEx>(Resource.Id.bnve);
            _bnve.EnableItemShiftingMode(false);
            _bnve.EnableShiftingMode(false);

            _goOnlineButton.Click += GoOnlineButton_Click;
            _bnve.NavigationItemSelected += Bnve_NavigationItemSelected;


            var img0 = _bnve.GetIconAt(0);
            var txt0 = _bnve.GetLargeLabelAt(0);
            img0.SetColorFilter(Color.Rgb(24, 191, 242));
            txt0.SetTextColor(Color.Rgb(24, 191, 242));

            _viewpager = (ViewPager)FindViewById(Resource.Id.viewpager);
            _viewpager.OffscreenPageLimit = 3;
            _viewpager.BeginFakeDrag();

            SetupViewPager();

            _homeFragment.CurrentLocation += HomeFragment_CurrentLocation;
            _homeFragment.TripActionArrived += HomeFragment_TripActionArrived;
            _homeFragment.CallRider += HomeFragment_CallRider;
            _homeFragment.Navigate += HomeFragment_Navigate;
            _homeFragment.TripActionStartTrip += HomeFragment_TripActionStartTrip;
            _homeFragment.TripActionEndTrip += HomeFragment_TripActionEndTrip;
        }

        async void HomeFragment_TripActionEndTrip(object sender, EventArgs e)
        {
            //Reset app
            _status = "NORMAL";
            _homeFragment.ResetAfterTrip();

            ShowProgressDialogue();
            LatLng pickupLatLng = new LatLng(_newRideDetails.PickupLat, _newRideDetails.PickupLng);
            double fares = await _mapHelper.CalculateFares(pickupLatLng, _mLastLatLng);
            CloseProgressDialogue();

            _newTripEventListener.EndTrip(fares);
            _newTripEventListener = null;

            CollectPaymentFragment collectPaymentFragment = new CollectPaymentFragment(fares);
            collectPaymentFragment.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            collectPaymentFragment.Show(trans, "pay");
            collectPaymentFragment.PaymentCollected += (o, u) =>
            {
                collectPaymentFragment.Dismiss();
            };

            _availablityListener.ReActivate();

        }

        void HomeFragment_TripActionStartTrip(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder startTripAlert = new Android.Support.V7.App.AlertDialog.Builder(this);
            startTripAlert.SetTitle("START TRIP");
            startTripAlert.SetMessage("Are you sure");
            startTripAlert.SetPositiveButton("Continue", (senderAlert, args) =>
            {
                _status = "ONTRIP";
                _newTripEventListener.UpdateStatus("ontrip");
            });

            startTripAlert.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                startTripAlert.Dispose();
            });

            startTripAlert.Show();
        }

        void HomeFragment_Navigate(object sender, EventArgs e)
        {
            string uriString;

            if (_status == "ACCEPTED")
            {
                uriString = "google.navigation:q=" + _newRideDetails.PickupLat + "," + _newRideDetails.PickupLng;
            }
            else
            {
                uriString = "google.navigation:q=" + _newRideDetails.DestinationLat + "," + _newRideDetails.DestinationLng;
            }

            Android.Net.Uri googleMapIntentUri = Android.Net.Uri.Parse(uriString);
            Intent mapIntent = new Intent(Intent.ActionView, googleMapIntentUri);
            mapIntent.SetPackage("com.google.android.apps.maps");

            try
            {
                StartActivity(mapIntent);
            }
            catch
            {
                Toast.MakeText(this, "Google Map is not Installed on this device", ToastLength.Short).Show();
            }
        }


        void HomeFragment_CallRider(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("tel:" + _newRideDetails.RiderPhone);
            Intent intent = new Intent(Intent.ActionDial, uri);
            StartActivity(intent);
        }


        async void HomeFragment_TripActionArrived(object sender, EventArgs e)
        {
	        _newTripEventListener.UpdateStatus("arrived");
            _status = "ARRIVED";

            LatLng pickupLatLng = new LatLng(_newRideDetails.PickupLat, _newRideDetails.PickupLng);
            LatLng destinationLatLng = new LatLng(_newRideDetails.DestinationLat, _newRideDetails.DestinationLng);

            ShowProgressDialogue();
            string directionJson = await _mapHelper.GetDirectionJsonAsync(pickupLatLng, destinationLatLng);
            CloseProgressDialogue();

            _homeFragment.MainMap.Clear();
            _mapHelper.DrawTripToDestination(directionJson);
        }


        void HomeFragment_CurrentLocation(object sender, Helpers.LocationCallbackHelper.OnLocationCaptionEventArgs e)
        {
            _mLastLocation = e.Location;
            _mLastLatLng = new LatLng(_mLastLocation.Latitude, _mLastLocation.Longitude);

            _availablityListener?.UpdateLocation(_mLastLocation);

            if (_availablityStatus && _availablityListener == null)
            {
                TakeDriverOnline();
            }

            if (_status == "ACCEPTED")
            {
	            LatLng pickupLatLng = new LatLng(_newRideDetails.PickupLat, _newRideDetails.PickupLng);
                _mapHelper.UpdateMovement(_mLastLatLng, pickupLatLng, "Rider");
                _newTripEventListener.UpdateLocation(_mLastLocation);
            }
            else if (_status == "ARRIVED")
            {
                _newTripEventListener.UpdateLocation(_mLastLocation);
            }
            else if (_status == "ONTRIP")
            {
	            LatLng destinationLatLng = new LatLng(_newRideDetails.DestinationLat, _newRideDetails.DestinationLng);
                _mapHelper.UpdateMovement(_mLastLatLng, destinationLatLng, "Destination");
                _newTripEventListener.UpdateLocation(_mLastLocation);
            }
        }

        private void TakeDriverOnline()
        {
            _availablityListener = new AvailablityListener();
            _availablityListener.Create(_mLastLocation);
            _availablityListener.RideAssigned += AvailablityListener_RideAssigned;
            _availablityListener.RideTimedOut += AvailablityListener_RideTimedOut;
            _availablityListener.RideCancelled += AvailablityListener_RideCancelled;
        }

        private  void TakeDriverOffline()
        {
            _availablityListener.RemoveListener();
            _availablityListener = null;
        }

        private void AvailablityListener_RideAssigned(object sender, AvailablityListener.RideAssignedIDEventArgs e)
        {
	        _rideDetailsListener = new RideDetailsListener();
            _rideDetailsListener.Create(e.RideId);
            _rideDetailsListener.RideDetailsFound += RideDetailsListener_RideDetailsFound;
            _rideDetailsListener.RideDetailsNotFound += RideDetailsListener_RideDetailsNotFound;
        }

        private void RideDetailsListener_RideDetailsNotFound(object sender, EventArgs e)
        {

        }

        void CreateNewRequestDialogue()
        {
            _requestFoundDialogue = new NewRequestFragment(_newRideDetails.PickupAddress, _newRideDetails.DestinationAddress);
            _requestFoundDialogue.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            _requestFoundDialogue.Show(trans, "Request");

            //Play Alert
            _player = MediaPlayer.Create(this, Resource.Raw.alert);
            _player.Start();

            _requestFoundDialogue.RideRejected += RequestFoundDialogue_RideRejected;
            _requestFoundDialogue.RideAccepted += RequestFoundDialogue_RideAccepted;
        }

        async void RequestFoundDialogue_RideAccepted(object sender, EventArgs e)
        {
            _newTripEventListener = new NewTripEventListener(_newRideDetails.RideId, _mLastLocation);
            _newTripEventListener.Create();

            _status = "ACCEPTED";

            if (_player != null)
            {
                _player.Stop();
                _player = null;
            }

            if (_requestFoundDialogue != null)
            {
                _requestFoundDialogue.Dismiss();
                _requestFoundDialogue = null;
            }

            _homeFragment.CreateTrip(_newRideDetails.RiderName);
            _mapHelper = new MapFunctionHelper(Resources.GetString(Resource.String.mapKey), _homeFragment.MainMap);
            LatLng pickupLatLng = new LatLng(_newRideDetails.PickupLat, _newRideDetails.PickupLng);

            ShowProgressDialogue();
            string directionJson = await _mapHelper.GetDirectionJsonAsync(_mLastLatLng, pickupLatLng);
            CloseProgressDialogue();

            _mapHelper.DrawTripOnMap(directionJson);
        }


        void RequestFoundDialogue_RideRejected(object sender, EventArgs e)
        {
	        if (_player != null)
            {
                _player.Stop();
                _player = null;
            }

            if (_requestFoundDialogue != null)
            {
                _requestFoundDialogue.Dismiss();
                _requestFoundDialogue = null;
            }

            _availablityListener.ReActivate();
        }


        void RideDetailsListener_RideDetailsFound(object sender, RideDetailsListener.RideDetailsEventArgs e)
        {
            if (_status != "NORMAL")
            {
                return;
            }

            _newRideDetails = e.RideDetails;

            if (!_isBackground)
            {
                CreateNewRequestDialogue();
            }
            else
            {
                _newRideAssigned = true;

                NotificationHelper notificationHelper = new NotificationHelper();

                if ((int)Build.VERSION.SdkInt >= 26)
                {
                    notificationHelper.NotifyVersion26(this, Resources, (NotificationManager)GetSystemService(NotificationService));
                }
            }
        }

        void AvailablityListener_RideTimedOut(object sender, EventArgs e)
        {
            if (_requestFoundDialogue != null)
            {
                _requestFoundDialogue.Dismiss();
                _requestFoundDialogue = null;
                _player.Stop();
                _player = null;
            }

            Toast.MakeText(this, "New trip Timeout", ToastLength.Short).Show();
            _availablityListener.ReActivate();
        }

        private void AvailablityListener_RideCancelled(object sender, EventArgs e)
        {
            if (_requestFoundDialogue != null)
            {
                _requestFoundDialogue.Dismiss();
                _requestFoundDialogue = null;
                _player.Stop();
                _player = null;
            }

            Toast.MakeText(this, "New trip was cancelled", ToastLength.Short).Show();
            _availablityListener.ReActivate();
        }

        private void GoOnlineButton_Click(object sender, EventArgs e)
        {
            if (!CheckSpecialPermission())
            {
                return;
            }

            if (_availablityStatus)
            {
                Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(this);
                alert.SetTitle("GO OFFLINE");
                alert.SetMessage("You will not be able to receive Ride Request");
                alert.SetPositiveButton("Continue", (senderAlert, args) =>
                {
                    _homeFragment.GoOffline();
                    _goOnlineButton.Text = "Go Online";
                    _goOnlineButton.Background = ContextCompat.GetDrawable(this, Resource.Drawable.uberroundbutton);

                    _availablityStatus = false;
                    TakeDriverOffline();
                });

                alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    alert.Dispose();
                });

                alert.Show();
            }
            else
            {
                _availablityStatus = true;
                _homeFragment.GoOnline();
                _goOnlineButton.Text = "Go offline";
                _goOnlineButton.Background = ContextCompat.GetDrawable(this, Resource.Drawable.uberroundbutton_green);
            }
        }

        private void Bnve_NavigationItemSelected(object sender, Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            if (e.Item.ItemId == Resource.Id.action_earning)
            {
                _viewpager.SetCurrentItem(1, true);
                BnveToAccentColor(1);
            }
            else if (e.Item.ItemId == Resource.Id.action_home)
            {
                _viewpager.SetCurrentItem(0, true);
                BnveToAccentColor(0);
            }
            else if (e.Item.ItemId == Resource.Id.action_rating)
            {
                _viewpager.SetCurrentItem(2, true);
                BnveToAccentColor(2);

            }
            else if (e.Item.ItemId == Resource.Id.action_account)
            {
                _viewpager.SetCurrentItem(3, true);
                BnveToAccentColor(3);
            }
        }

        private void BnveToAccentColor(int index)
        {
            //Set all to white
            var img = _bnve.GetIconAt(1);
            var txt = _bnve.GetLargeLabelAt(1);
            img.SetColorFilter(Color.Rgb(255, 255, 255));
            txt.SetTextColor(Color.Rgb(255, 255, 255));

            var img0 = _bnve.GetIconAt(0);
            var txt0 = _bnve.GetLargeLabelAt(0);
            img0.SetColorFilter(Color.Rgb(255, 255, 255));
            txt0.SetTextColor(Color.Rgb(255, 255, 255));

            var img2 = _bnve.GetIconAt(2);
            var txt2 = _bnve.GetLargeLabelAt(2);
            img2.SetColorFilter(Color.Rgb(255, 255, 255));
            txt2.SetTextColor(Color.Rgb(255, 255, 255));

            var img3 = _bnve.GetIconAt(3);
            var txt3 = _bnve.GetLargeLabelAt(3);
            img2.SetColorFilter(Color.Rgb(255, 255, 255));
            txt2.SetTextColor(Color.Rgb(255, 255, 255));

            //Sets Accent Color
            var imgindex = _bnve.GetIconAt(index);
            var textindex = _bnve.GetLargeLabelAt(index);
            imgindex.SetColorFilter(Color.Rgb(24, 191, 242));
            textindex.SetTextColor(Color.Rgb(24, 191, 242));
        }

        private void SetupViewPager()
        {
            ViewPagerAdapter adapter = new ViewPagerAdapter(SupportFragmentManager);
            adapter.AddFragment(_homeFragment, "Home");
            adapter.AddFragment(_earningsFragment, "Earnings");
            adapter.AddFragment(_ratingsFragment, "Rating");
            adapter.AddFragment(_accountFragment, "Account");
            _viewpager.Adapter = adapter;
        }

        bool CheckSpecialPermission()
        {
            bool permissionGranted = false;
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted &&
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(_permissionsGroup, RequestId);
            }
            else
            {
                permissionGranted = true;
            }

            return permissionGranted;
        }

        protected override void OnPause()
        {
            _isBackground = true;
            base.OnPause();
        }

        protected override void OnResume()
        {
            _isBackground = false;
            if (_newRideAssigned)
            {
                CreateNewRequestDialogue();
                _newRideAssigned = false;
            }
            base.OnResume();
        }
    }
}