using System;
using Ctrip.Driver.Helpers;
using Firebase.Database;
using Java.Util;

namespace Ctrip.Driver.EventListeners
{
    public class AvailablityListener : Java.Lang.Object, IValueEventListener
    {
        FirebaseDatabase _database;
        DatabaseReference _availablityRef;

        public class RideAssignedIdEventArgs : EventArgs
        {
            public string RideId { get; set; }
        }

        public event EventHandler<RideAssignedIdEventArgs> RideAssigned;
        public event EventHandler RideCancelled;
        public event EventHandler RideTimedOut;


        public void OnCancelled(DatabaseError error)
        {
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Value != null)
            {
                string rideId = snapshot.Child("ride_id").Value.ToString();

                if (rideId != "waiting" && rideId != "timeout" && rideId != "cancelled")
                {
	                RideAssigned?.Invoke(this, new RideAssignedIdEventArgs { RideId = rideId });
                }
                else if (rideId == "timeout")
                {
	                RideTimedOut?.Invoke(this, new EventArgs());
                }
                else if (rideId == "cancelled")
                {
	                RideCancelled?.Invoke(this, new EventArgs());
                }
            }
        }

        public void Create (Android.Locations.Location myLocation)
        {
            _database = AppDataHelper.GetDatabase();
            string driverId = AppDataHelper.GetCurrentUser().Uid;

            _availablityRef = _database.GetReference("driversAvailable/" + driverId);

            HashMap location = new HashMap();
            location.Put("latitude", myLocation.Latitude);
            location.Put("longitude", myLocation.Longitude);

            HashMap driverInfo = new HashMap();
            driverInfo.Put("location", location);
            driverInfo.Put("ride_id", "waiting");

            _availablityRef.AddValueEventListener(this);
            _availablityRef.SetValue(driverInfo);
        }

        public void RemoveListener()
        {
            _availablityRef.RemoveValue();
            _availablityRef.RemoveEventListener(this);
            _availablityRef = null;
        }

        public void UpdateLocation(Android.Locations.Location mylocation)
        {
            string driverId = AppDataHelper.GetCurrentUser().Uid;

            if (_availablityRef != null)
            {
                DatabaseReference locationref = _database.GetReference("driversAvailable/" + driverId + "/location");
                HashMap locationMap = new HashMap();
                locationMap.Put("latitude", mylocation.Latitude);
                locationMap.Put("longitude", mylocation.Longitude);
                locationref.SetValue(locationMap);
            }
        }

        public void ReActivate()
        {
            _availablityRef.Child("ride_id").SetValue("waiting");
        }
    }
}
