using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AccelerometerTracking
{
  

    class Mapping : Java.Lang.Object
    {
        Stopwatch sw;
        public float[] velocityXYZ;//The velocity based off the Phones
        private float velocity;
        public float[] positionXYZ;//This is offset from the origin(when the app is reset or reset
        private double[] GlobalXYZ;//In relative to the map we are creating.  
        private Context context;
        private Activity activity;
        private TextView VelText, xVel, yVel, zVel,xDist,yDist,zDist;

       public Mapping(Context cntxt)
        {
            context = cntxt;
            activity = (MainActivity)cntxt;
            velocity = 0;
            velocityXYZ = new float[]{ 0,0,0};
            positionXYZ = new float[] { 0, 0, 0 };
            sw = new Stopwatch();
        }
        public void onCreate()
        {
            VelText = activity.FindViewById<TextView>(Resource.Id.Vel_text);
            xVel = activity.FindViewById<TextView>(Resource.Id.xVel);
            yVel = activity.FindViewById<TextView>(Resource.Id.yVel);
            zVel = activity.FindViewById<TextView>(Resource.Id.zVel);

            xDist = activity.FindViewById<TextView>(Resource.Id.xDist);
            yDist = activity.FindViewById<TextView>(Resource.Id.yDist);
            zDist = activity.FindViewById<TextView>(Resource.Id.zDist);

            sw.Start();
        }


        public void UpDateVelocity(float[] acceleration)
        {
            float interval = sw.ElapsedMilliseconds / 100.00f;
            if(interval > 0)
            {
                for (int v = 0; v < 3; v++)
                {
                    velocityXYZ[v] += acceleration[v] / interval;//TODO: Look at possible smoothing if its needed, also evaluate these values and determine if accel smoothing is needed.. 
                }
                //Use the Pythagoras for the velocity. V = Root(A*A + B*B + C*C)
                velocity = (float)Math.Sqrt((velocityXYZ[0] * velocityXYZ[0] + velocityXYZ[1] * velocityXYZ[1] + velocityXYZ[2] * velocityXYZ[2]));

                VelText.Text = string.Format("Velocity = {0:f}m/s", velocity);
                xVel.Text = string.Format("x - Velocity = {0:f}m/s", velocityXYZ[0]);
                yVel.Text = string.Format("y - Velocity = {0:f}m/s", velocityXYZ[1]);
                zVel.Text = string.Format("z - Velocity = {0:f}m/s", velocityXYZ[2]);


                UpdatePosition(interval);
                sw.Restart();
            }
        }

        private void UpdatePosition(float deltaTime)//This has to be updated on each second
        {
            

            for(int i=0;i<3;i++)
            {
                positionXYZ[i] += (velocityXYZ[i]/deltaTime);
            }
            xDist.Text = string.Format("x - Offset = {0:f}m", positionXYZ[0]);
            yDist.Text = string.Format("y - Offset = {0:f}m", sw.ElapsedMilliseconds);
            zDist.Text = string.Format("z - Offset = {0:f}m", positionXYZ[2]);
        }
    }



}