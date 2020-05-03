using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Firebase.Auth;
using Ctrip.Driver.EventListeners;
using Ctrip.Driver.Helpers;

namespace Ctrip.Driver.Activities
{
	[Activity(Label = "LoginActivity", Theme = "@style/UberTheme", MainLauncher = false)]
	public class LoginActivity : AppCompatActivity
	{
		Button _loginButton;
		TextInputLayout _textInputEmail;
		TextInputLayout _textInputPassword;
		CoordinatorLayout _rootView;
		TextView _clickToSignUp;

		FirebaseAuth _mAuth;

		Android.Support.V7.App.AlertDialog.Builder _alert;
		Android.Support.V7.App.AlertDialog _alertDialog;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.login);
			ConnectViews();
			InitializeFirebase();
		}

		private void InitializeFirebase()
		{
			_mAuth = AppDataHelper.GetFirebaseAuth();
			AppDataHelper.GetCurrentUser();
			AppDataHelper.GetDatabase();
		}

		private void ConnectViews()
		{
			_loginButton = (Button)FindViewById(Resource.Id.loginButton);
			_textInputEmail = (TextInputLayout)FindViewById(Resource.Id.emailText);
			_textInputPassword = (TextInputLayout)FindViewById(Resource.Id.passwordText);
			_rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
			_clickToSignUp = (TextView)FindViewById(Resource.Id.clickToSignUpText);

			_loginButton.Click += LoginButton_Click;
			_clickToSignUp.Click += ClickToSignUp_Click;
		}

		private void ClickToSignUp_Click(object sender, EventArgs e)
		{
			StartActivity(typeof(RegistrationActivity));
		}

		private void LoginButton_Click(object sender, EventArgs e)
		{
			string email = _textInputEmail.EditText.Text;
			string password = _textInputPassword.EditText.Text;

			ShowProgressDialogue();

			TaskCompletionListeners taskCompletionListener = new TaskCompletionListeners();
			taskCompletionListener.Successful += TaskCompletionListener_Successful;
			taskCompletionListener.Failure += TaskCompletionListener_Failure;

			_mAuth.SignInWithEmailAndPassword(email, password)
				.AddOnSuccessListener(this, taskCompletionListener)
				.AddOnFailureListener(this, taskCompletionListener);
		}

		private void TaskCompletionListener_Failure(object sender, EventArgs e)
		{
			CloseProgressDialogue();
			Snackbar.Make(_rootView, "Login Failed", Snackbar.LengthShort).Show();
		}

		private void TaskCompletionListener_Successful(object sender, EventArgs e)
		{
			CloseProgressDialogue();
			StartActivity(typeof(MainActivity));
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
			if (_alert != null)
			{
				_alertDialog.Dismiss();
				_alertDialog = null;
				_alert = null;
			}
		}

	}
}