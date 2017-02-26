using Android.App;
using Android.Widget;
using Android.OS;
using Android.Hardware;

namespace AccelerometerTracking
{
    [Activity(Label = "AccelerometerTracking", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        private Accelerometer accelor;
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            accelor = new Accelerometer(this);
            accelor.OnCreate();
          

        }

        protected override void OnPause()
        {
            base.OnPause();
            accelor.OnPause();

        }
        protected override void OnResume()
        {
            base.OnResume();
            accelor.OnResume();
        }
        public void OnSensorChanged(SensorEvent e)
        {
            accelor.OnSensorChanged(e);
        }

    }
     
}

