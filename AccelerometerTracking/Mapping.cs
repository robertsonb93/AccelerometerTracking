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

using MathNet.Filtering.Kalman;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.LinearAlgebra; //For Matrix object


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
        private Matrix<double> state;
        private Matrix<double> stateCov;
        private InformationFilter KFilter;
        Matrix<double> ZMat = Matrix<double>.Build.Dense(1,3);
        Matrix<double> RMat = Matrix<double>.Build.DenseIdentity(3, 3);
        private float[] oldAccel;
        
        private int numRSamples = 0;

       public Mapping(Context cntxt)
        {
            context = cntxt;
            activity = (MainActivity)cntxt;
            velocity = 0;
            velocityXYZ = new float[]{ 0,0,0};
            positionXYZ = new float[] { 0, 0, 0 };
            oldAccel = new float[] { 0, 0, 0 };
            sw = new Stopwatch();

            state = getStateMat(velocityXYZ);
            stateCov = getStateCov();
            KFilter = new InformationFilter(state, stateCov);
        }

        public Matrix<double> getStateMat(float[] accelertation)
        {
            double A_x = accelertation[0];
            double A_y = accelertation[1];
            double A_z = accelertation[2];
            double[] temp = {  positionXYZ[0], positionXYZ[1], positionXYZ[2], velocityXYZ[0], velocityXYZ[1], velocityXYZ[2], A_x, A_y, A_z  };
            Vector<double> st = Vector<double>.Build.DenseOfArray(temp);

            return Matrix<double>.Build.DenseOfColumnVectors(st);
        }

        public Matrix<double> getStateCov()
        {
            Matrix<double> stCv = (Matrix<double>.Build.DenseIdentity(9,9));
            stCv *= 0.8;
            return stCv;
        }

        public Matrix<double> getStateUpdateMat(float time,float[] acceleration)
        {
            Matrix<double> stateUpdate = Matrix<double>.Build.DenseIdentity(9, 9);

            for(int i=0;i<3;i++)
            {
                stateUpdate[i, i + 3] = stateUpdate[i + 3, i + 6] = time;
                stateUpdate[i, i + 6] = 0.5 * time * time;
            }

            return stateUpdate;
        }
        public Matrix<double> getR(float[] accel)
        {
            if (numRSamples++ < 500 && numRSamples > 50 && false) //If we try and compute a covariance matrix, this will cause all the Kalman to filll with NaN.
            {
                for (int i = 0; i < 3; i++)
                {
                    double[] sampleX = { oldAccel[0], accel[i] };
                    double[] sampleY = { oldAccel[1], accel[i] };
                    double[] sampleZ = { oldAccel[2], accel[i] };
                    RMat[0,i] = Statistics.Variance(sampleX);
                    RMat[1, i] = Statistics.Variance(sampleY);
                    RMat[2, i] = Statistics.Variance(sampleZ);                 
                }
                for (int i = 0; i < 3; i++)
                    oldAccel[i] = accel[i];
            }

            return RMat*0.05;
        }

        public Matrix<double> getH(float time)
        {
            Matrix<double> H = Matrix<double>.Build.Dense(3, 9);
            for (int i = 0; i < 3; i++)
            {
                H[i, i + 6] = 1;
              //  H[i, i + 3] = time;
              //  H[i, i] = 0.5 * time * time;
            }
                return H;
        }

        public Matrix<double> getZ(float[] accel)
        {
            double[] tmp = { accel[0], accel[1], accel[2] };
            Vector<double> Zv = Vector<double>.Build.Dense(tmp);
            Matrix<double> Z = Matrix<double>.Build.DenseOfColumnVectors(Zv);
            return Z;
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
        void updateState()
        {
            for(int i=0;i<3;i++)
            {
                positionXYZ[i] = (float)state[i,0];
                velocityXYZ[i] = (float)state[i + 3,0];
            }
        }

        public void UpDateVelocity(float[] acceleration)
        {
            float interval = sw.ElapsedMilliseconds / 100.00f;

            KFilter.Update(getZ(acceleration), getH(interval), getR(acceleration));
            KFilter.Predict(getStateUpdateMat(interval, acceleration));

            state = KFilter.State;
            updateState();

            if(interval > 0)
            { 
                //Use the Pythagoras for the velocity. V = Root(A*A + B*B + C*C)
                velocity = (float)Math.Sqrt((velocityXYZ[0] * velocityXYZ[0] + velocityXYZ[1] * velocityXYZ[1] + velocityXYZ[2] * velocityXYZ[2]));

                string text = "State = ";
                for (int i = 0; i < 9; i++)
                    text += string.Format("{0:f}, ", state[i, 0]);
               VelText.Text = text;
                        // VelText.Text = string.Format("STATE = {0:f}m/s", velocity);
               xVel.Text = string.Format("x - Velocity = {0:f}m/s", velocityXYZ[0]);
               yVel.Text = string.Format("y - Velocity = {0:f}m/s", velocityXYZ[1]);
               zVel.Text = string.Format("z - Velocity = {0:f}m/s", velocityXYZ[2]);

                UpdatePosition();
                sw.Restart();
            }
        }

        private void UpdatePosition()//This has to be updated on each second
        {
            xDist.Text = string.Format("x - Offset = {0:f}m", positionXYZ[0]);
            yDist.Text = string.Format("y - Offset = {0:f}m", positionXYZ[1]);
            zDist.Text = string.Format("z - Offset = {0:f}m", positionXYZ[2]);
        }

        private void timeUpdate()
        {
                //K is the time step, or just refers to the current update.
            //x(k) = A*x(k-1) + w(k) 
            //z(k) = H*x(k) + v(k)
            //A is the change in state, I.e If x is just velocity, A is the acceleration. 
            //Do this for each individual sensor.


        }
    }



}