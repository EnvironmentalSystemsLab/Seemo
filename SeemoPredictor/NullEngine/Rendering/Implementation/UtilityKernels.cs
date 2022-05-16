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
        public static void ClearByteFramebuffer(Index1D index, dByteFrameBuffer frameBuffer, dFloatFrameBuffer frameDistanceBuffer, byte r, byte g, byte b, float t)
        {
            //FLIP Y
            //int x = (frameBuffer.width - 1) - ((index) % frameBuffer.width);
            int y = (frameBuffer.height - 1) - ((index) / frameBuffer.width);

            //NORMAL X
            int x = ((index) % frameBuffer.width);
            //int y = ((index) / frameBuffer.width);

            int newIndex = ((y * frameBuffer.width) + x);
            frameBuffer.writeFrameBuffer(newIndex * 3, r, g, b);

            //FLIP Y
            //int x = (frameBuffer.width - 1) - ((index) % frameBuffer.width);
            int yd = (frameDistanceBuffer.height - 1) - ((index) / frameDistanceBuffer.width);

            //NORMAL X
            int xd = ((index) % frameDistanceBuffer.width);
            //int y = ((index) / frameBuffer.width);

            int newIndexD = ((yd * frameDistanceBuffer.width) + xd);
            frameDistanceBuffer.writeFrameDistanceBuffer(newIndexD, t);
        }

        public static Vec3 readFrameBuffer(ArrayView1D<float, Stride1D.Dense> frame, int width, int x, int y)
        {
            int newIndex = ((y * width) + x) * 3;
            return readFrameBuffer(frame, newIndex);
        }

        public static Vec3 readFrameBuffer(ArrayView1D<float, Stride1D.Dense> frame, int index)
        {
            return new Vec3(frame[index], frame[index + 1], frame[index + 2]);
        }


        public static int readFrameMaterialIDBuffer(ArrayView1D<int, Stride1D.Dense> frame, int index)
        {
            return (frame[index]);
        }

        public static float readFrameDistanceBuffer(ArrayView1D<float, Stride1D.Dense> frame, int index)
        {
            return (frame[index]);
        }
    }
}
