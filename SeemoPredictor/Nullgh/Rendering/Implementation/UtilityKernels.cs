using ILGPU;
using ILGPU.Runtime;
using NullEngine.Rendering.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace NullEngine.Rendering.Implementation
{
    public static class UtilityKernels
    {
        
        public static Vec3 readFrameBuffer(ArrayView1D<float, Stride1D.Dense> frame, int width, int x, int y)
        {
            int newIndex = ((y * width) + x) * 3;
            return readFrameBuffer(frame, newIndex);
        }

        public static Vec3 readFrameBuffer(ArrayView1D<float, Stride1D.Dense> frame, int index) //distance also can use this..
        {
            return new Vec3(frame[index], frame[index + 1], frame[index + 2]);
        }

        //Material ID 2
        public static Vec3 readFrameMaterialID2Buffer(ArrayView1D<int, Stride1D.Dense> frame, int width, int x, int y)
        {
            int newIndex = ((y * width) + x) * 3;
            return readFrameMaterialID2Buffer(frame, newIndex);
        }

        public static int readFrameWindowIDBuffer(ArrayView1D<int, Stride1D.Dense> frame, int index)
        {
            return (frame[index]);
        }

        public static Vec3 readFrameMaterialID2Buffer(ArrayView1D<int, Stride1D.Dense> frame, int index)
        {
            return new Vec3(frame[index], frame[index + 1], frame[index + 2]);
        }


        //Material ID

        public static int readFrameMaterialIDBuffer(ArrayView1D<int, Stride1D.Dense> frame, int index)
        {
            return (frame[index]);
        }


        //Distance
        public static float readFrameDistanceBuffer(ArrayView1D<float, Stride1D.Dense> frame, int index)
        {
            return (frame[index]);
        }
    }
}
