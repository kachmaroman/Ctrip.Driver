using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Firebase.Auth;
using Firebase.Database;
using Java.Util;
using Ctrip.Driver.EventListeners;
using Ctrip.Driver.Helpers;

namespace Ctrip.Driver.Activities
{
    [Activity(Label = "RegistrationActivity", MainLauncher = false, Theme = "@style/UberTheme" )]
    public class RegistrationActivity : AppCompatActivity
    {
        TextInputLayout _fullNameText;
        TextInputLayout _phoneText;
        TextInputLayout _emailText;
        TextInputLayout _passwordText;
        Button _registerButton;
        CoordinatorLayout _rootView;

        FirebaseDatabase _database;
        FirebaseAuth _mAuth;

        readonly TaskCompletionListeners _taskCompletionListener = new TaskCompletionListeners();

        Android.Support.V7.App.AlertDialog.Builder _alert;
        Android.Support.V7.App.AlertDialog _alertDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);
            ConnectViews();
            SetupFireBase();
        }

        private void ShowProgressDialogue()
        {
            _alert = new Android.Support.V7.App.AlertDialog.Builder(this);
            _alert.SetView(Resource.Layout.progress);
            _alert.SetCancelable(false);
            _alertDialog = _alert.Show();
        }

        private void CloseProgressDialogue()
        {
	        if (_alert == null)
	        {
		        return;
            }

	        _alertDialog.Dismiss();
            _alertDialog = null;
            _alert = null;
        }

        private void SetupFireBase()
        {
            _database = AppDataHelper.GetDatabase();
            _mAuth = AppDataHelper.GetFirebaseAuth();
            AppDataHelper.GetCurrentUser();
        }

        private void ConnectViews()
        {
            _fullNameText = FindViewById<TextInputLayout>(Resource.Id.fullNameText);
            _phoneText = FindViewById<TextInputLayout>(Resource.Id.phoneText);
            _emailText = FindViewById<TextInputLayout>(Resource.Id.emailText);
            _passwordText = FindViewById<TextInputLayout>(Resource.Id.passwordText);
            _rootView = FindViewById<CoordinatorLayout>(Resource.Id.rootView);
            _registerButton = FindViewById<Button>(Resource.Id.registerButton);

            _registerButton.Click += RegisterButton_Click;
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
	        var fullname = _fullNameText.EditText.Text;
            var phone = _phoneText.EditText.Text;
            var email = _emailText.EditText.Text;
            var password = _passwordText.EditText.Text;

            if (fullname.Length < 3)
            {
                Snackbar.Make(_rootView, "Please Enter a Valid Name", Snackbar.LengthShort).Show();
                return;
            }

            if(phone.Length < 9)
            {
	            Snackbar.Make(_rootView, "Please Enter a Phone Number", Snackbar.LengthShort).Show();
	            return;
            }

            if (!email.Contains("@"))
            {
	            Snackbar.Make(_rootView, "Please Enter a Valid Email Address", Snackbar.LengthShort).Show();
	            return;
            }

            if (password.Length <8)
            {
	            Snackbar.Make(_rootView, "Please Enter a Valid Password", Snackbar.LengthShort).Show();
	            return;
            }

            ShowProgressDialogue();

            _mAuth.CreateUserWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, _taskCompletionListener)
                .AddOnFailureListener(this, _taskCompletionListener);

            _taskCompletionListener.Successful += (o, g) =>
            {
                CloseProgressDialogue();
                DatabaseReference newDriverRef = _database.GetReference("drivers/" + _mAuth.CurrentUser.Uid);
                HashMap map = new HashMap();

                map.Put("fullname", fullname);
                map.Put("phone", phone);
                map.Put("email", email);
                map.Put("created_at", DateTime.Now.ToString());

                newDriverRef.SetValue(map);
                Snackbar.Make(_rootView, "Driver was registered successfully", Snackbar.LengthShort).Show();
                StartActivity(typeof(MainActivity));
            };

            _taskCompletionListener.Failure += (w, r) =>
            {
                CloseProgressDialogue();
                Snackbar.Make(_rootView, "Driver could not be registered", Snackbar.LengthShort).Show();
            };
        }
    }
}