using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.CPU;
using System;
using System.Collections.Generic;
using System.Text;
using NullEngine.Rendering.DataStructures;
using NullEngine.Rendering.DataStructures.BVH;

namespace NullEngine.Rendering.Implementation
{
    public class GPU
    {
        public Context context;
        public Accelerator device;
        public Action<Index1D, Camera, dFrameData> generatePrimaryRays;
        public Action<Index1D, dFrameData, dTLAS, dRenderData> hitRays;
        public Action<Index1D, dByteFrameBuffer, dFloatFrameBuffer, dFrameData> generateFrame;

        public GPU(bool forceCPU)
        {
            context = Context.Create(builder => builder.Cuda().EnableAlgorithms().Assertions());

            if (forceCPU)
            {
                device = context.GetPreferredDevice(preferCPU: forceCPU)
            .CreateAccelerator(context);
            }
            else
            {
                //Jaeha's personal setup for Nvidia GPU select as an accelerator
                device = context.GetCudaDevice(0).CreateAccelerator(context);
            }


            initRenderKernels();
        }

        private void initRenderKernels()
        {
            generateFrame = device.LoadAutoGroupedStreamKernel<Index1D, dByteFrameBuffer, dFloatFrameBuffer, dFrameData>(GPUKernels.GenerateFrame);
            hitRays = device.LoadAutoGroupedStreamKernel<Index1D, dFrameData, dTLAS, dRenderData>(GPUKernels.HitRays);
            generatePrimaryRays = device.LoadAutoGroupedStreamKernel<Index1D, Camera, dFrameData>(GPUKernels.GeneratePrimaryRays);
        }

        public void Dispose()
        {
            device.Dispose();
            context.Dispose();
        }

        public void Render(Camera camera, Scene scene, dByteFrameBuffer output, dFloatFrameBuffer output2, dFrameData frameData)
        {
            generatePrimaryRays(output.width * output.height, camera, frameData);
            hitRays(output.width * output.height, frameData, scene.tlas.GetDTLAS(), scene.tlas.renderDataManager.getDeviceRenderData());// error!
            generateFrame(output.height * output.width, output, output2, frameData);
            device.Synchronize();
        }
    }

    public static class GPUKernels
    {
        public static void GeneratePrimaryRays(Index1D pixel, Camera camera, dFrameData frameData)
        {
            float x = (pixel % camera.width);
            float y = (pixel / camera.width);


            frameData.rayBuffer[pixel] = camera.GetRay(x, y);
        }

        //real raycasting first place of making hitrecord
        public static void HitRays(Index1D pixel, dFrameData frameData, dTLAS tlas, dRenderData renderData)
        {
            int unset = 13; // later instead of 13 =(smoface.viewContentType.Count -1)

            HitRecord hit = new HitRecord();
            hit.t = float.MaxValue;
            hit.materialID = unset; 

            HitRecord second = new HitRecord();
            second.t = float.MaxValue;
            second.materialID = unset;

            //Vec3 windowN = new Vec3(0, 0, 0);
            int materialIndex = 0;

            for (int i = 0; i < tlas.meshes.Length; i++)
            {
                dMesh mesh = tlas.meshes[i];
                for(int j = 0; j< mesh.triangleLength; j++)
                {
                    HitRecord temp = new HitRecord();
                    temp.t = float.MaxValue;
                    temp.materialID = unset;

                    float dist = mesh.GetTriangle(j, renderData).GetTriangleHit(frameData.rayBuffer[pixel], j, ref temp); //안부딪혓는데, 부딪힌 것처럼 temp에 저장됨.절대값 때문인듯,, mesh face 가 flip 된 것과, t가 음수, 가 관려닝 잇는지...
                    if (temp.t < float.MaxValue && temp.t > 0 && dist!= float.NaN)
                    {
                        temp.materialID = renderData.rawMaterialID2Buffers[j];

                        if (temp.materialID != 2 && temp.materialID != unset) // 2:glazing
                        {
                            if (temp.t > 0 && temp.t < hit.t)
                            {
                                second.t = hit.t;
                                second.materialID = hit.materialID;
                                hit.t = temp.t;
                                hit.materialID = temp.materialID;
                            }
                            else if (temp.t > hit.t && temp.t < second.t)
                            {
                                second.t = temp.t;
                                second.materialID = temp.materialID;
                            }
                        }
                        else if (temp.materialID == 2) //temp.material is glazing
                        {
                            if (hit.materialID == 2) //closest material is glazing
                            {
                                if (temp.t > 0 && temp.t < hit.t)
                                {

                                }
                                else if (temp.t > hit.t && temp.t < second.t)
                                {
                                    hit.t = temp.t;
                                    hit.materialID = temp.materialID;
                                    //windowN = mesh.GetTriangle(j, renderData).faceNormal();
                                }
                            }
                            else if (hit.materialID == 0) //closest  material is  intieror wall
                            {
                                if (temp.t > 0 && temp.t < hit.t)
                                {

                                }
                                else if (temp.t > hit.t && temp.t < second.t)
                                {
                                    second.t = temp.t;
                                    second.materialID = temp.materialID;
                                }
                            }
                            else
                            {
                                if (temp.t > 0 && temp.t < hit.t)
                                {
                                    second.t = hit.t;
                                    second.materialID = hit.materialID;
                                    hit.t = temp.t;
                                    hit.materialID = temp.materialID;
                                    //windowN = mesh.GetTriangle(j, renderData).faceNormal();
                                }
                                else if (temp.t > hit.t && temp.t < second.t)
                                {
                                    second.t = temp.t;
                                    second.materialID = temp.materialID;
                                }
                            }
                        }

                    }
                    materialIndex++;

                }
            }

            //check all mesh faces 

            if(hit.materialID == unset) { hit.t = 0; }
            if(second.materialID == unset) { second.t = 0; }

            if(hit.materialID == 2 || hit.materialID == unset)
            {
                if(second.materialID != 0) //0: interior
                {
                    
                    frameData.outputMaterialIDBuffer[pixel] = second.materialID;
                    frameData.outputMaterialID2Buffer[(pixel * 3)] = second.materialID;
                    frameData.outputMaterialID2Buffer[(pixel * 3) + 1] = second.materialID;
                    frameData.outputMaterialID2Buffer[(pixel * 3) + 2] = second.materialID;

                    frameData.outputBuffer[(pixel * 3)] = second.t;
                    frameData.outputBuffer[(pixel * 3) + 1] = second.t;
                    frameData.outputBuffer[(pixel * 3) + 2] = second.t;
                    frameData.depthBuffer[pixel] = second.t;

                }
                else
                {
                    
                    frameData.outputMaterialIDBuffer[pixel] = second.materialID;
                    frameData.outputMaterialID2Buffer[(pixel * 3)] = second.materialID;
                    frameData.outputMaterialID2Buffer[(pixel * 3) + 1] = second.materialID;
                    frameData.outputMaterialID2Buffer[(pixel * 3) + 2] = second.materialID;

                    frameData.outputBuffer[(pixel * 3)] = second.t;
                    frameData.outputBuffer[(pixel * 3) + 1] = second.t;
                    frameData.outputBuffer[(pixel * 3) + 2] = second.t;
                    frameData.depthBuffer[pixel] = second.t;
                }

            }else if(hit.materialID == 0) // closest material is interior
            {
                
                frameData.outputMaterialIDBuffer[pixel] = hit.materialID;
                frameData.outputMaterialID2Buffer[(pixel * 3)] = hit.materialID;
                frameData.outputMaterialID2Buffer[(pixel * 3) + 1] = hit.materialID;
                frameData.outputMaterialID2Buffer[(pixel * 3) + 2] = hit.materialID;

                frameData.outputBuffer[(pixel * 3)] = hit.t;
                frameData.outputBuffer[(pixel * 3) + 1] = hit.t;
                frameData.outputBuffer[(pixel * 3) + 2] = hit.t;
                frameData.depthBuffer[pixel] = hit.t;
            }
            else
            {
                
                frameData.outputMaterialIDBuffer[pixel] = hit.materialID;
                frameData.outputMaterialID2Buffer[(pixel * 3)] = hit.materialID;
                frameData.outputMaterialID2Buffer[(pixel * 3) + 1] = hit.materialID;
                frameData.outputMaterialID2Buffer[(pixel * 3) + 2] = hit.materialID;

                frameData.outputBuffer[(pixel * 3)] = hit.t;
                frameData.outputBuffer[(pixel * 3) + 1] = hit.t;
                frameData.outputBuffer[(pixel * 3) + 2] = hit.t;
                frameData.depthBuffer[pixel] = hit.t;
            }


            

        }

        public static void GenerateFrame(Index1D pixel, dByteFrameBuffer output, dFloatFrameBuffer output2, dFrameData frameData)
        {
            

            Vec3 color = UtilityKernels.readFrameBuffer(frameData.outputBuffer, pixel * 3);
            output.writeFrameBuffer(pixel * 3, color.x, color.y, color.z);

            Vec3 materialID2 = UtilityKernels.readFrameMaterialID2Buffer(frameData.outputMaterialID2Buffer, pixel * 3); //problem
            switch(materialID2.x)
            {
                case 0:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 100, 100, 100);
                    break;
                case 1:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 255, 0, 0);
                    break;
                case 2:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 125, 0);
                    break;
                case 3:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 50, 50, 50);
                    break;
                case 4:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 255, 0);
                    break;
                case 5:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 255, 255, 0);
                    break;
                case 6:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 200, 100, 0);
                    break;
                case 7:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 125, 125);
                    break;
                case 8:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 50, 0, 50);
                    break;
                case 9:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 255, 255);
                    break;
                case 10:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 90, 255, 90);
                    break;
                case 11:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 160, 160);
                    break;
                case 12:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 255, 70, 0);
                    break;
                case 13:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 0, 0);
                    break;
                default:
                    output.writeFrameMaterialID2Buffer(pixel * 3, 0, 0, 0);
                    break;

            }
            int materialID = UtilityKernels.readFrameMaterialIDBuffer(frameData.outputMaterialIDBuffer, pixel);
            output.writeFrameMaterialIDBuffer(pixel, materialID);

            float distance = UtilityKernels.readFrameDistanceBuffer(frameData.depthBuffer, pixel);
            output2.writeFrameDistanceBuffer(pixel, distance);

            
        }

    }
}
