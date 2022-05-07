using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System.IO;

namespace HpGPU
{
    public static class Program
    {
        /*----------------Kernel Example-------------
        static void Kernel(Index1D i, ArrayView<int> data, ArrayView<int> output)
        {
            output[i] = data[i % data.Length];
        }
        */
        
        /*----------------MemoryBuffer------------
         * public static readonly bool debug = false;
         */


        
        public static void Main()
    {
        Context context = Context.Create(builder => builder.Default().EnableAlgorithms());
        Accelerator device = context.GetPreferredDevice(preferCPU: false)
                                  .CreateAccelerator(context);

        int width = 500;
        int height = 500;
        
        // my GPU can handle around 10,000 when using the struct of arrays
        int particleCount = 100; 

        byte[] h_bitmapData = new byte[width * height * 3];

        using MemoryBuffer2D<Vec3, Stride2D.DenseY> canvasData = device.Allocate2DDenseY<Vec3>(new Index2D(width, height));
        using MemoryBuffer1D<byte, Stride1D.Dense> d_bitmapData = device.Allocate1D<byte>(width * height * 3);

        CanvasData c = new CanvasData(canvasData, d_bitmapData, width, height);

        using HostParticleSystem h_particleSystem = new HostParticleSystem(device, particleCount, width, height);

        var frameBufferToBitmap = device.LoadAutoGroupedStreamKernel<Index2D, CanvasData>(CanvasData.CanvasToBitmap);
        var particleProcessingKernel = device.LoadAutoGroupedStreamKernel<Index1D, CanvasData, ParticleSystem>(ParticleSystem.particleKernel);

        //process 100 N-body ticks
        for (int i = 0; i < 100; i++)
        {
            particleProcessingKernel(particleCount, c, h_particleSystem.deviceParticleSystem);
            device.Synchronize();
        }

        frameBufferToBitmap(canvasData.Extent.ToIntIndex(), c);
        device.Synchronize();

        d_bitmapData.CopyToCPU(h_bitmapData);

        //bitmap magic that ignores bitmap striding, be careful some sizes will mess up the striding
        using Bitmap b = new Bitmap(width, height, width * 3, PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(h_bitmapData, 0));
        b.Save("out.bmp");
        Console.WriteLine("Wrote 100 iterations of N-body simulation to out.bmp");
            /*------------Kernel Example----------
            // Initialize ILGPU.
            Context context = Context.CreateDefault();
            Accelerator accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);

            // Load the data
            MemoryBuffer1D<int, Stride1D.Dense> deviceData = accelerator.Allocate1D(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            MemoryBuffer1D<int, Stride1D.Dense> deviceOutput = accelerator.Allocate1D<int>(10_000);

            // Load/ precompile the kernel
            Action<Index1D, ArrayView<int>, ArrayView<int>> loadedKernel =
                accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<int>, ArrayView<int>>(Kernel);

            // finish compiling and tell the accelertor to start computing the kernel
            loadedKernel((int)deviceOutput.Length, deviceData.View, deviceOutput.View);

            // wait for the accelerator to be finished with whatever it's doing
            accelerator.Synchronize();

            // moved output data from the GPU to the CPU for output to console
            int[] hostOutput = deviceOutput.GetAsArray1D();

            for(int i = 0; i < 50; i++)
            {
                Console.WriteLine(hostOutput[i]);
            }

            accelerator.Dispose();
            context.Dispose();
            */
            
            
            
            /*------------MemoryBuffer------------
            // We still need the Context and Accelerator boiler plate.
            Context context = Context.CreateDefault();
            Accelerator accelerator = context.CreateCPUAccelerator(0);

            // Gets array of 1000 doubles on host.
            double[] doubles = new double[1000];

            // Gets memorybuffer on device with same size and contents as doubles.
            MemoryBuffer1D<double, Stride1D.Dense> doublesOnDevice = accelerator.Allocate1D(doubles);

            // What if we change the doubles on the host and need to update the device side memory?
            for (int i = 0; i < doubles.Length; i++) { doubles[i] = i * Math.PI; }

            // We call MemoryBuffer.CopyFrom which copies any linear slice of doubles into the device side memory.
            doublesOnDevice.CopyFromCPU(doubles);

            // What if we change the doublesOnDevice and need to write that data into host memory?
            doublesOnDevice.CopyToCPU(doubles);

            // You can copy data to and from MemoryBuffers into any array/span/memeorybuffer that allocates the same
            // type. for example:
            double[] doubles2 = new double[doublesOnDevice.Length];
            doublesOnDevice.CopyFromCPU(doubles2);

            // There are also helper functions, but be aware of what a function does.
            // As an example this function is shorthand for the above two lines.
            // This completely allocates a new double[] on the host. This is slow.
            double[] doubles3 = doublesOnDevice.GetAsArray1D();

            // Notice that you cannot access memory in a memorybuffer or an arrayview from host code.
            // If you uncomment the following lines they should crash.
            // doublesOnDevice[1] = 0;
            // double d  = doublesOnDevice[1];

            // There is not much we can show with ArrayViews currently, but in the 
            // Kernels Tutorial it will go over much more.
            ArrayView1D<double, Stride1D.Dense> doublesArrayView = doublesOnDevice.View;

            //do not forget to dispose of everything in the reverse order you constructed it.
            doublesOnDevice.Dispose();
            // note the doublesArrayView is now invalid, but does not need to be disposeds.
            accelerator.Dispose();
            context.Dispose();

            */
            
            /*
            // Initialize ILGPU.
            Context context = Context.CreateDefault();
            Accelerator accelerator = context.CreateCPUAccelerator(0);

            // Load the Data.
            var deviceData = accelerator.Allocate1D(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            var deviceOutput = accelerator.Allocate1D<int>(10_000);

            // Load/ compile the kernel
            var loadedKernel = accelerator.LoadAutoGroupedStreamKernel((
                Index1D i, ArrayView<int> data, ArrayView<int> output) =>
            {
                output[i] = data[i % data.Length];
            });

            // tell the accelerator to start computing the kernel
            loadedKernel((int)deviceOutput.Length, deviceData.View, deviceOutput.View);

            // wait for the accelerator to be finished with whatever it's doing
            // in this case it just waits for the kernel to finish.

            accelerator.Synchronize();

            accelerator.Dispose();
            context.Dispose();
            */
        }



        public struct CanvasData
        {
            public ArrayView2D<Vec3, Stride2D.DenseY> canvas;
            public ArrayView1D<byte, Stride1D.Dense> bitmapData;
            public int width;
            public int height;

            public CanvasData(ArrayView2D<Vec3, Stride2D.DenseY> canvas, ArrayView1D<byte, Stride1D.Dense> bitmapData, int width, int height)
            {
                this.canvas = canvas;
                this.bitmapData = bitmapData;
                this.width = width;
                this.height = height;   

            }

            public void setColor(Index2D index, Vec3 c)
            {
                if ((index.X >= 0 ) && (index.X < canvas.IntExtent.X) && (index.Y >= 0 ) && (index.Y < canvas.IntExtent.Y))
                {
                    canvas[index] = c;
                }
            }

            public static void CanvasToBitmap(Index2D index, CanvasData c)
            {
                Vec3 color = c.canvas[index];

                int bitmapIndex = ((index.Y * c.width) + index.X) * 3;

                c.bitmapData[bitmapIndex] = (byte)(255.99f * color.x);
                c.bitmapData[bitmapIndex + 1] = (byte)(255.99f * color.y);
                c.bitmapData[bitmapIndex + 2] = (byte)(255.99f * color.z);

                c.canvas[index] = new Vec3(0, 0, 0);
            }


        }

        public class HostParticleSystem : IDisposable
        {
            public MemoryBuffer1D<Particle, Stride1D.Dense> particleData;
            public ParticleSystem deviceParticleSystem;

            public HostParticleSystem(Accelerator device, int particleCount, int width, int height)
            {
                Particle[] particles = new Particle[particleCount];
                Random rng = new Random();

                for (int i = 0; i < particleCount; i++)
                {
                    Vec3 pos = new Vec3((float)rng.NextDouble() * width, (float)rng.NextDouble() * height, 1);
                    particles[i] = new Particle(pos);
                }

                particleData = device.Allocate1D(particles);
                deviceParticleSystem = new ParticleSystem(particleData, width, height);
            }

            public void Dispose()
            {
                particleData.Dispose();
            }
        }

        public struct ParticleSystem
        {
            public ArrayView1D<Particle, Stride1D.Dense> particles;
            public float gc;
            public Vec3 centerPos;
            public float centerMass;

            public ParticleSystem(ArrayView1D<Particle, Stride1D.Dense> particles, int width, int height)
            {
                this.particles = particles;

                gc = 0.001f;

                centerPos = new Vec3(0.5f * width, 0.5f * height, 0);
                centerMass = (float)particles.Length;
            }

            public Vec3 update(int ID)
            {
                particles[ID].update(this, ID);
                return particles[ID].position;
            }

            public static void particleKernel(Index1D index, CanvasData c, ParticleSystem p)
            {
                Vec3 pos = p.update(index);
                Index2D position = new Index2D((int)pos.x, (int)pos.y);
                c.setColor(position, new Vec3(1, 1, 1));
            }
        }

        public struct Particle
        {
            public Vec3 position;
            public Vec3 velocity;
            public Vec3 acceleration;

            public Particle(Vec3 position)
            {
                this.position = position;
                velocity = new Vec3();
                acceleration = new Vec3();
            }

            private void updateAcceleration(ParticleSystem d, int ID)
            {
                acceleration = new Vec3();

                for (int i = 0; i < d.particles.Length; i++)
                {
                    Vec3 otherPos;
                    float mass;

                    if (i == ID)
                    {
                        //creates a mass at the center of the screen
                        otherPos = d.centerPos;
                        mass = d.centerMass;
                    }
                    else
                    {
                        otherPos = d.particles[i].position;
                        mass = 1f;
                    }

                    float deltaPosLength = (position - otherPos).length();
                    float temp = (d.gc * mass) / XMath.Pow(deltaPosLength, 3f);
                    acceleration += (otherPos - position) * temp;
                }
            }

            private void updatePosition()
            {
                position = position + velocity + acceleration * 0.5f;
            }

            private void updateVelocity()
            {
                velocity = velocity + acceleration;
            }

            public void update(ParticleSystem particles, int ID)
            {
                updateAcceleration(particles, ID);
                updatePosition();
                updateVelocity();
            }
        }
    }
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

        }

        public static Vec3 operator +(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);

        }

        public static Vec3 operator -(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);

        }

        public static Vec3 operator *(Vec3 v1, float v)
        {
            return new Vec3(v1.x * v, v1.y * v, v1.z * v);
        }

        public float length()
        {
            return XMath.Sqrt(x * x + y * y + z * z);
        }



    }
}
