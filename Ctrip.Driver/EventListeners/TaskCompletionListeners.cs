using System;
using Android.Gms.Tasks;
namespace Ctrip.Driver.EventListeners
{
    public class TaskCompletionListeners : Java.Lang.Object, IOnSuccessListener, IOnFailureListener
    {
        public event EventHandler Successful;
        public event EventHandler Failure;

        public void OnFailure(Java.Lang.Exception e)
        {
            Failure?.Invoke(this, new EventArgs());
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            Successful?.Invoke(this, new EventArgs());
        }
    }
}