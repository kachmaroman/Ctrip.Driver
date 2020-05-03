using Android.App;
using Android.Content;
using Ctrip.Driver.Helpers;
using Firebase.Database;

namespace Ctrip.Driver.EventListeners
{
    public class ProfileEventListener : Java.Lang.Object, IValueEventListener
    {
	    readonly ISharedPreferences _preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor _editor;

        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if(snapshot.Value != null)
            {
	            var fullname = snapshot.Child("fullname") != null ? snapshot.Child("fullname").Value.ToString() : string.Empty;
                var email = snapshot.Child("email") != null ? snapshot.Child("email").Value.ToString() : string.Empty;
                var phone = snapshot.Child("phone") != null ? snapshot.Child("phone").Value.ToString() : string.Empty;

                _editor.PutString("fullname", fullname);
                _editor.PutString("phone", phone);
                _editor.PutString("email", email);
                _editor.Apply();
            }
        }

        public void Create()
        {
            _editor = _preferences.Edit();
            FirebaseDatabase database = AppDataHelper.GetDatabase();
            string driverId = AppDataHelper.GetCurrentUser().Uid;
            DatabaseReference driverRef = database.GetReference("drivers/" + driverId);
            driverRef.AddValueEventListener(this);
        }
    }
}
