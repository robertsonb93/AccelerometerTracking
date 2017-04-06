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
    class Accelerometer : Java.Lang.Object
    {

        private int history = 10;
        private TextView text_x, text_y, text_z;
        private CheckBox[] gravCheck;
        private float[] XYZ; //Measured ins M/s2
        private float[][] XYZhistory;

        private float[] gravity;
        private int gravAxis = 2;//This will defeault the axis to z, ie phone is on its back
        private Context context;
        private Activity activity;

        public Accelerometer(Context cntext)
        {
            context = cntext;
            activity = (MainActivity)cntext;
            gravity = new float[3] { 0, 0, 0 };
            XYZ = new float[3] {0,0,0 };
            XYZhistory = new float[3][];
            for (int i = 0; i < 3; i++)
            {
                XYZhistory[i] = new float[history];
                for (int s = 0; s < history; s++)
                {
                    XYZhistory[i][s] = 0;
                }
            }
        }

        public float[] getAccelXYZ()
        {
            return XYZ;
        }

        public void OnSensorChanged(SensorEvent e)
        {
          
                //Alpha = t / (t+dT
                //t = a time constant
                // the change in time between updates on events

                float alpha = 0.025f;
            for (int i = 0; i < gravity.Length; i++)
            {
                if (gravCheck[i].Checked == false)
                    gravity[i] = alpha * gravity[i] + (1 - alpha) * e.Values[i];
                else
                    gravity[i] = 0;

                XYZ[i] = ((e.Values[i]) - gravity[i]);
            }
                text_x.Text = string.Format("x = {0:f} m/s2", XYZ[0]);
                text_y.Text = string.Format("y = {0:f} m/s2", XYZ[1]);
                text_z.Text = string.Format("z = {0:f} m/s2", XYZ[2]);
        }

        public void OnCreate()
        {
            gravCheck = new CheckBox[3];
            gravCheck[0] = activity.FindViewById<CheckBox>(Resource.Id.ZeroXGravity);
            gravCheck[1] = activity.FindViewById<CheckBox>(Resource.Id.ZeroYGravity);
            gravCheck[2] = activity.FindViewById<CheckBox>(Resource.Id.ZeroZGravity);
            text_x = activity.FindViewById<TextView>(Resource.Id.X_text);
            text_y = activity.FindViewById<TextView>(Resource.Id.Y_text);
            text_z = activity.FindViewById<TextView>(Resource.Id.Z_text);         
        }

    }
}
