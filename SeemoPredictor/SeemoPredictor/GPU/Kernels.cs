using SeemoPredictor.Geometry;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SeemoPredictor.GPU
{
    public class Kernels
    {

        public static void CreatBitmap(Index1D index, ArrayView<float> data, ArrayView<float> flattenmap, SmoImage SmoImage)
        {
            //FLIP Y
            //int x = (SmoImage.xres - 1) - ((index) % SmoImage.xres);
            int y = (SmoImage.yres - 1) - ((index) / SmoImage.xres);

            //NORMAL X
            int x = ((index) % SmoImage.xres);
            //int y = ((index) / SmoImage.xres);

            int newIndex = ((y * SmoImage.xres) + x) * 3;
            int oldIndexStart = index * 3;

            flattenmap[newIndex] = data[oldIndexStart];
            flattenmap[newIndex + 1] = data[oldIndexStart + 1];
            flattenmap[newIndex + 2] = data[oldIndexStart + 2];
        }


        public static float map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
        

        public static (float min, float max) ReduceMax(Accelerator device, ArrayView<float> map)
        {
            
            using (var target = device.Allocate1D<float>(1))
            {
                // This overload requires an explicit output buffer but
                // uses an implicit temporary cache from the associated accelerator.
                // Call a different overload to use a user-defined memory cache.
                device.Reduce<float, MinFloat>(
                    device.DefaultStream,
                    map,
                    target.View);

                device.Synchronize();

                var min = target.GetAsArray1D();

                device.Reduce<float, MaxFloat>(
                device.DefaultStream,
                map,
                target.View);

                device.Synchronize();

                var max = target.GetAsArray1D();
                return (min[0], max[0]);
            }
        }

        public static void NULLTAA(Index1D index,
            dFramebuffer srcFramebuffer,
            dFramebuffer dstFramebuffer,
            float depthFuzz, float exponent, int tick)
        {

            float newDepth = srcFramebuffer.DepthBuffer[index];
            int newID = srcFramebuffer.LabelIDBuffer[index];

            int xIndex = index * 3;
            int yIndex = xIndex + 1;
            int zIndex = yIndex + 1;

            if (/*XMath.Abs(newDepth - lastDepth) > depthFuzz || */tick == 0)
            {
                dstFramebuffer.RayBuffer[xIndex] = srcFramebuffer.RayBuffer[xIndex];
                dstFramebuffer.RayBuffer[yIndex] = srcFramebuffer.RayBuffer[yIndex];
                dstFramebuffer.RayBuffer[zIndex] = srcFramebuffer.RayBuffer[zIndex];

                dstFramebuffer.LabelIDBuffer[index] = newID;
                dstFramebuffer.DepthBuffer[index] = newDepth;
            }
            else
            {
                if (tick < 1 / exponent)
                {
                    dstFramebuffer.RayBuffer[xIndex] = ((1.0f / tick) * srcFramebuffer.RayBuffer[xIndex]) + ((1 - (1.0f / tick)) * dstFramebuffer.RayBuffer[xIndex]);
                    dstFramebuffer.RayBuffer[yIndex] = ((1.0f / tick) * srcFramebuffer.RayBuffer[yIndex]) + ((1 - (1.0f / tick)) * dstFramebuffer.RayBuffer[yIndex]);
                    dstFramebuffer.RayBuffer[zIndex] = ((1.0f / tick) * srcFramebuffer.RayBuffer[zIndex]) + ((1 - (1.0f / tick)) * dstFramebuffer.RayBuffer[zIndex]);

                    dstFramebuffer.LabelIDBuffer[index] = newID;
                    dstFramebuffer.DepthBuffer[index] = newDepth;
                }
                else
                {
                    dstFramebuffer.RayBuffer[xIndex] = (exponent * srcFramebuffer.RayBuffer[xIndex]) + ((1 - exponent) * dstFramebuffer.RayBuffer[xIndex]);
                    dstFramebuffer.RayBuffer[yIndex] = (exponent * srcFramebuffer.RayBuffer[yIndex]) + ((1 - exponent) * dstFramebuffer.RayBuffer[yIndex]);
                    dstFramebuffer.RayBuffer[zIndex] = (exponent * srcFramebuffer.RayBuffer[zIndex]) + ((1 - exponent) * dstFramebuffer.RayBuffer[zIndex]);

                    dstFramebuffer.LabelIDBuffer[index] = newID;
                    dstFramebuffer.DepthBuffer[index] = newDepth;
                }
            }
        }

        public static void NULLLowPassFilter(Index1D index,
            ArrayView<float> srcColor,
            ArrayView<float> dstColor,
            SmoImage SmoImage, int filterWidth)
        {
            int x = ((index) % SmoImage.xres);
            int y = ((index) / SmoImage.xres);

            int xIndex = index * 3;
            int yIndex = xIndex + 1;
            int zIndex = xIndex + 2;

            float filterWidthHalf = filterWidth / 2f;
            Point3 coordinate = new Point3();
            float sampleCounter = 0;

            for (int i = 0; i < filterWidth; i++)
            {
                for (int j = 0; j < filterWidth; j++)
                {
                    int imageX = (x + (i - (int)filterWidthHalf));
                    int imageY = (y + (j - (int)filterWidthHalf));

                    int newIndex = ((imageY * SmoImage.xres) + imageX);

                    if (newIndex >= 0 && newIndex <= srcColor.Length / 3)
                    {
                        int r = newIndex * 3;
                        int g = r + 1;
                        int b = r + 2;

                        if (srcColor[r] > 0)
                        {
                            coordinate += new Point3(srcColor[r], srcColor[g], srcColor[b]);
                            sampleCounter++;
                        }
                    }
                }
            }

            if (sampleCounter > 1)
            {
                dstColor[xIndex] = coordinate.X / sampleCounter;
                dstColor[yIndex] = coordinate.Y / sampleCounter;
                dstColor[zIndex] = coordinate.Z / sampleCounter;
            }
            else
            {
                dstColor[xIndex] = coordinate.X;
                dstColor[yIndex] = coordinate.Y;
                dstColor[zIndex] = coordinate.Z;
            }
        }
    }
}
