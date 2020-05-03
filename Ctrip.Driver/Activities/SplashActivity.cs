using Android.App;
using Android.Support.V7.App;
using Firebase.Auth;
using Ctrip.Driver.Helpers;

namespace Ctrip.Driver.Activities
{
    [Activity(Label = "SplashActivity", Theme ="@style/MyTheme.Splash", MainLauncher = true)]
    public class SplashActivity : AppCompatActivity
    {
	    protected override void OnResume()
        {
            base.OnResume();

            FirebaseUser currentUser = AppDataHelper.GetCurrentUser();

            StartActivity(currentUser == null ? typeof(LoginActivity) : typeof(MainActivity));
        }
    }
}