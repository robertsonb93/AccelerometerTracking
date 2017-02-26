using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;

namespace AccelerometerTracking
{
    class Accelerometer : Java.Lang.Object , ISensorEventListener
    {
        static readonly object _syncLock = new object();
        private SensorManager SM;
        private Sensor sensr;
        private TextView text_x, text_y, text_z;
        private float[] XYZ;
        private float[] gravity;
        private Context context;
        private Activity activity;

        public Accelerometer(Context cntext)
        {
            context = cntext;
            activity = (MainActivity)cntext;
            gravity = new float[3] { 0, 0, 0 };
            XYZ = new float[3] {0,0,0 };
        }


        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {

        }

        public void OnSensorChanged(SensorEvent e)
        {
            lock (_syncLock)
            {
                //Alpha = t / (t+dT
                //t = a time constant
                // the change in time between updates on events

                float alpha = 0.8f;
                for(int i =0;i<gravity.Length;i++)
                {
                    gravity[i] = alpha * gravity[i] + (1 - alpha) * e.Values[i];
                    XYZ[i] = ((e.Values[i] - gravity[i]) + XYZ[i] + XYZ[i]) / 3; //There is lots of noise, so this should smooth a bit
                }

                text_x.Text = string.Format("x = {0:f}", XYZ[0]);
                text_y.Text = string.Format("y = {0:f}", XYZ[1]);
                text_z.Text = string.Format("z = {0:f}", XYZ[2]);
            }
        }

        public void OnCreate()
        {
            SM = (SensorManager)context.GetSystemService(Context.SensorService);
            sensr = SM.GetDefaultSensor(SensorType.Accelerometer);

            text_x = activity.FindViewById<TextView>(Resource.Id.X_text);
            text_y = activity.FindViewById<TextView>(Resource.Id.Y_text);
            text_z = activity.FindViewById<TextView>(Resource.Id.Z_text);
           
        }

        public void OnResume()
        {
            SM.RegisterListener(this,SM.GetDefaultSensor(SensorType.Accelerometer),SensorDelay.Ui);
        }

        public void OnPause()
        {
            SM.UnregisterListener(this);
        }

    }
}
