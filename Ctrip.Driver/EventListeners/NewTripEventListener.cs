using System;
using Ctrip.Driver.Helpers;
using Firebase.Database;

namespace Ctrip.Driver.EventListeners
{
    public class NewTripEventListener : Java.Lang.Object, IValueEventListener
    {
	    readonly string mRideID;
        Android.Locations.Location _mLastlocation;
        readonly FirebaseDatabase database;
        DatabaseReference _tripRef;

        bool _isAccepted;

        public NewTripEventListener(string rideId, Android.Locations.Location lastlocation)
        {
            mRideID = rideId;
            _mLastlocation = lastlocation;
            database = AppDataHelper.GetDatabase();
        }

        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Value != null)
            {
                if (!_isAccepted)
                {
                    _isAccepted = true;
                    Accept();
                }
            }
        }

        public void Create()
        {
	        _tripRef = database.GetReference("Ride_requests/" + mRideID);
            _tripRef.AddValueEventListener(this);
        }

        private void Accept()
        {
            _tripRef.Child("status").SetValue("accepted");
            _tripRef.Child("driver_name").SetValue(AppDataHelper.GetFullname());
            _tripRef.Child("driver_phone").SetValue(AppDataHelper.GetPhone());
            _tripRef.Child("driver_location").Child("latitude").SetValue(_mLastlocation.Latitude);
            _tripRef.Child("driver_location").Child("longitude").SetValue(_mLastlocation.Longitude);
            _tripRef.Child("driver_id").SetValue(AppDataHelper.GetCurrentUser().Uid);
        }

        public void UpdateLocation(Android.Locations.Location lastlocation)
        {
            _mLastlocation = lastlocation;
            _tripRef.Child("driver_location").Child("latitude").SetValue(_mLastlocation.Latitude);
            _tripRef.Child("driver_location").Child("longitude").SetValue(_mLastlocation.Longitude);
        }

        public void UpdateStatus(string status)
        {
            _tripRef.Child("status").SetValue(status);
        }

        public void EndTrip (double fares)
        {
            //TODO: Remove this shit somehow
	        //Update: Calls the garbage collector to release instances existing in memory. This handles error: Invalid Instance. 
            GC.Collect();

            if (_tripRef != null)
            { 
                _tripRef.Child("fares").SetValue(fares);
                _tripRef.Child("status").SetValue("ended");
                _tripRef.RemoveEventListener(this);
                _tripRef = null;
            }
        }
    }
}
