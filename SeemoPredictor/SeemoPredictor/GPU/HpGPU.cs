using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using SeemoPredictor.Geometry;


namespace SeemoPredictor.GPU
{
    public static class HpGPU
    {

        public struct CanvasData
        {
            //public ArrayView2D<Point3, Stride2D.DenseY> canvas;
            public ArrayView1D<double, Stride1D.Dense> flattenDepthMap;
            public int width;
            public int height;

            public CanvasData(/*ArrayView2D<Point3, Stride2D.DenseY> canvas, */ArrayView1D<double, Stride1D.Dense> flattenDepthMap, int width, int height)
            {
                //this.canvas = canvas;
                this.flattenDepthMap = flattenDepthMap;
                this.width = width;
                this.height = height;

            }
            /*
            public void setColor(Index2D index, Point3 c)
            {
                if ((index.X >= 0) && (index.X < canvas.IntExtent.X) && (index.Y >= 0) && (index.Y < canvas.IntExtent.Y))
                {
                    canvas[index] = c;
                }
            }

            public static void CanvasToBitmap(Index2D index, CanvasData c)
            {
                Point3 color = c.canvas[index];

                int flattenDepthMapIndex = ((index.Y * c.width) + index.X) * 3;

                c.flattenDepthMap[flattenDepthMapIndex] = (double)(255.99f * color.X);
                c.flattenDepthMap[flattenDepthMapIndex + 1] = (double)(255.99f * color.Y);
                c.flattenDepthMap[flattenDepthMapIndex + 2] = (double)(255.99f * color.Z);

                c.canvas[index] = new Point3(0, 0, 0);
            }*/

        }

        //key transfer part from host to device
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
                    Point3 pos = new Point3((float)rng.NextDouble() * width, (float)rng.NextDouble() * height, 1);
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
            public Point3 centerPos;
            public float centerMass;



            public ParticleSystem(ArrayView1D<Particle, Stride1D.Dense> particles, int width, int height)
            {
                this.particles = particles;

                gc = 0.001f;

                centerPos = new Point3(0.5f * width, 0.5f * height, 0);
                centerMass = (float)particles.Length;
            }

            public Point3 update(int ID)
            {
                particles[ID].update(this, ID);
                return particles[ID].position;
            }

            //key!!!
            public static void particleKernel(Index1D index, CanvasData c, ParticleSystem p)
            {
                Point3 pos = p.update(index);
                Index2D position = new Index2D((int)pos.X, (int)pos.Y);
                //c.setColor(position, new Point3(1, 1, 1));



            }

        }



        public struct Particle
        {
            public Point3 position;
            public Point3 velocity;
            public Point3 acceleration;

            public Particle(Point3 position)
            {
                this.position = position;
                velocity = new Point3();
                acceleration = new Point3();
            }

            private void updateAcceleration(ParticleSystem d, int ID)
            {
                acceleration = new Point3();

                for (int i = 0; i < d.particles.Length; i++)
                {
                    Point3 otherPos;
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

                    float deltaPosLength = (position - otherPos).Length;
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
}
