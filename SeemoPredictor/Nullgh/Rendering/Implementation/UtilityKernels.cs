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
        /*
        public static void ClearByteFramebuffer(Index1D index, dByteFrameBuffer frameBuffer, dByteFrameBuffer frameMaterialID2Buffer, dFloatFrameBuffer frameDistanceBuffer, dByteFrameBuffer frameDistance2Buffer, byte r, byte g, byte b, float t)
        {
            //FLIP Y
            //int x = (frameBuffer.width - 1) - ((index) % frameBuffer.width);
            int y = (frameBuffer.height - 1) - ((index) / frameBuffer.width);

            //NORMAL X
            int x = ((index) % frameBuffer.width);
            //int y = ((index) / frameBuffer.width);

            int newIndex = ((y * frameBuffer.width) + x);
            frameBuffer.writeFrameBuffer(newIndex * 3, r, g, b);


            //Distance2
            //FLIP Y
            //int x = (frameBuffer.width - 1) - ((index) % frameBuffer.width);
            int yd2 = (frameDistance2Buffer.height - 1) - ((index) / frameDistance2Buffer.width);

            //NORMAL X
            int xd2 = ((index) % frameDistance2Buffer.width);
            //int y = ((index) / frameBuffer.width);

            int newIndexD2 = ((yd2 * frameDistance2Buffer.width) + xd2);
            frameDistance2Buffer.writeFrameDistance2Buffer(newIndexD2 * 3, r, g, b);


            //Material ID2
            //FLIP Y
            //int x = (frameBuffer.width - 1) - ((index) % frameBuffer.width);
            int ym = (frameMaterialID2Buffer.height - 1) - ((index) / frameMaterialID2Buffer.width);

            //NORMAL X
            int xm = ((index) % frameMaterialID2Buffer.width);
            //int y = ((index) / frameBuffer.width);

            int newIndexM = ((ym * frameMaterialID2Buffer.width) + xm);
            frameMaterialID2Buffer.writeFrameMaterialID2Buffer(newIndexM * 3, r, g, b); //???오류가 있을 수도..


            //Material ID
            //FLIP Y
            //int x = (frameBuffer.width - 1) - ((index) % frameBuffer.width);
            int yd = (frameDistanceBuffer.height - 1) - ((index) / frameDistanceBuffer.width);

            //NORMAL X
            int xd = ((index) % frameDistanceBuffer.width);
            //int y = ((index) / frameBuffer.width);

            int newIndexD = ((yd * frameDistanceBuffer.width) + xd);
            frameDistanceBuffer.writeFrameDistanceBuffer(newIndexD, t);
        }
        */
        
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
