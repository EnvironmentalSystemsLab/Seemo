using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace SeemoPredictor
{
    public class SeemoPredictorComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SeemoPredictorComponent()
          : base("SeemoPredictor", "0211Predictor",
            "Predict View Satisfaction : Overall Rating, Content, Access, Privacy",
              "SeEmo", "Predictor")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ViewResult", "ViewResult", "ViewResult", GH_ParamAccess.tree);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Overall Rating", "Overall Rating", "Overall Rating", GH_ParamAccess.list);
            pManager.AddGenericParameter("ViewResult&Prediction", "ViewResult&Prediction", "ViewResult&Prediction", GH_ParamAccess.tree);
            //pManager.AddNumberParameter("Content", "Content", "Content", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Access", "Access", "Access", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Privacy", "Privacy", "Privacy", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //DataTree<ResultDataSet> dataTree1 = new DataTree<ResultDataSet>();
            GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> dataTree1;
            DataTree<ResultDataSet> dataTree2 = new DataTree<ResultDataSet>();
            List<double> overallRatings = new List<double>();   


            if (!DA.GetDataTree(0, out dataTree1)) { return; }
            //DA.GetDataTree(0, out dataTree1);

            foreach(GH_Path p in dataTree1.Paths)
            {
                foreach(Grasshopper.Kernel.Types.IGH_Goo o in dataTree1.get_Branch(p))
                {
                    if(o != null)
                    {
                        ResultDataSet r1;

                        //if (item.CastTo(out r1)) { continue; }
                        o.CastTo(out r1);


                        ModelInput sampleData = new ModelInput()
                        {

                            WindowNumber = (float)r1.WindowNumber,
                            WindowAreaSum = (float)r1.WindowAreaSum,
                            Z1PtsCountRatio = (float)r1.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)r1.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)r1.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)r1.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)r1.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)r1.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)r1.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)r1.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)r1.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)r1.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)r1.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)r1.SkyPtsCountRatio,
                            ElementNumber = (float)r1.ElementNumber,
                            FloorHeights = (float)r1.FloorHeights,
                            BuildingClosestDist = (float)r1.BuildingClosestDist,
                            EquipmentClosestDist = (float)r1.EquipmentClosestDist,
                            TreeClosestDist = (float)r1.TreeClosestDist,
                            GrassClosestDist = (float)r1.GrassClosestDist,
                            WaterClosestDist = (float)r1.WaterClosestDist,
                            DynamicClosestDist = (float)r1.DynamicClosestDist,
                            SkyCondition = (float)r1.SkyCondition,

                        };

                        // Make a single prediction on the sample data and print results
                        var overallRating = ConsumeOverallRating.Predict(sampleData);
                        //var heating = ConsumeModelHeating.Predict(input);
                        //var plugloads = ConsumeModelPlugLoads.Predict(input);
                        //var light = ConsumeModelLighting.Predict(input);

                        overallRatings.Add(overallRating.OverallRatingB);

                        r1.PredictedOverallRating = overallRating.OverallRatingB;
                        GH_Path r1path = p;
                        dataTree2.Add(r1, r1path);

                    }
                }
            }



            DA.SetDataList(0, overallRatings);
            //DA.SetData(1, cooling.Score);
            //DA.SetData(2, light.Score);
            //DA.SetData(3, plugloads.Score);

            DA.SetDataTree(1, dataTree2);


            //GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> dataTree1 = new GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();

            //if (!DA.GetDataTree<Grasshopper.Kernel.Types.IGH_Goo>(0, out dataTree1)) return;





        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("FAE58A8B-885F-4ADD-981C-948B2BE5C23A");
    }
}