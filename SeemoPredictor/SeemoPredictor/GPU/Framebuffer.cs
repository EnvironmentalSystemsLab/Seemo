using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SeemoPredictor.Geometry;


namespace SeemoPredictor.GPU
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct dFramebuffer
    {
        public ArrayView1D<float, Stride1D.Dense> RayBuffer;
        public ArrayView1D<float, Stride1D.Dense> HitsBuffer;
        public ArrayView1D<float, Stride1D.Dense> DepthBuffer;
        public ArrayView1D<int, Stride1D.Dense> LabelIDBuffer;

        public dFramebuffer(hFramebuffer _Framebuffer)
        {
            RayBuffer = _Framebuffer.RayBuffer;
            HitsBuffer = _Framebuffer.HitsBuffer;
            DepthBuffer = _Framebuffer.DepthBuffer;
            LabelIDBuffer = _Framebuffer.LabelBuffer;
        }
    }

    public class hFramebuffer
    {
        
        public MemoryBuffer1D<float, Stride1D.Dense> RayBuffer;
        public MemoryBuffer1D<float, Stride1D.Dense> HitsBuffer;
        public MemoryBuffer1D<float, Stride1D.Dense> DepthBuffer;
        public MemoryBuffer1D<int, Stride1D.Dense> LabelBuffer;
        public dFramebuffer D;

        public hFramebuffer(Accelerator device, int width, int height)
        {
            RayBuffer = device.Allocate1D<float>(width * height * 3);
            HitsBuffer = device.Allocate1D<float>(width * height * 3);
            DepthBuffer = device.Allocate1D<float>(width * height);
            LabelBuffer = device.Allocate1D<int>(width * height);
            D = new dFramebuffer(this);
        }

        public dFramebuffer GetRef()
        {
            return D;
        }

        public void Dispose()
        {
            RayBuffer.Dispose();
            HitsBuffer.Dispose();
            DepthBuffer.Dispose();
            LabelBuffer.Dispose();
        }
    }
}
