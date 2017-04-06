using System;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Hardware;

namespace AccelerometerTracking
{
    [Activity(Label = "AccelerometerTracking", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity , ISensorEventListener
    {
        // static readonly object _syncLock = new object();
        private SensorManager SM;
        private Sensor accelorSens;
        private bool settled = false;

        private Accelerometer accelor;
        private Mapping map;
        private TextView dbgTxt;
      

        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            // Set our view from the "main" layout resource

            SM = (SensorManager)GetSystemService(SensorService);
            accelorSens = SM.GetDefaultSensor(SensorType.Accelerometer);

            SetContentView(Resource.Layout.Main);

            accelor = new Accelerometer(this);
            map = new Mapping(this);
            accelor.OnCreate();
            map.onCreate();

            dbgTxt = FindViewById<TextView>(Resource.Id.dbg);
        }

        protected override void OnPause()
        {
            base.OnPause();
            SM.UnregisterListener(this);
        }
        protected override void OnResume()
        {
            base.OnResume();
            SM.RegisterListener(this, SM.GetDefaultSensor(SensorType.LinearAcceleration), SensorDelay.Ui);
        }

        public void OnAccuracyChanged(Sensor sens, SensorStatus st)
        { }


        public void OnSensorChanged(SensorEvent e)
        {
            
                
            accelor.OnSensorChanged(e);
           float[] XYZ = accelor.getAccelXYZ();
         
                dbgTxt.Text = string.Format("x = {0:f} y = {1:f} z = {2:f} ", XYZ[0], XYZ[1], XYZ[2]);
                for (int i=0;i<3;i++)
                     XYZ[i] = (float)Math.Round(XYZ[i], 2);
                map.UpDateVelocity(XYZ);

        }
    }
     
}

