using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Drawing;

namespace SeemoPredictor
{
    public class ImagePreviewComponent : GH_Component
    {

        // fields
        internal int PlotterWidth = 600;
        internal int PlotterHeight = 300;
        public Bitmap Bitmap;



        public ImagePreviewComponent()
          : base("ImageViewer", "ImageViewer",
              "Show bitmap",
            "RCEnergy", "Model")
        {
        }


        public override GH_Exposure Exposure => GH_Exposure.primary;


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Results", "Res", "Direction Result", GH_ParamAccess.item);
           


        }




        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Data", "Data", "Data of current result view type", GH_ParamAccess.list);
        }




        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

             DirectionResult results = new DirectionResult();

            DA.GetData(0, ref results);

            
            //current img to be shown
            this.Bitmap = results.Image.GetDepthBitmap();


        }

        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                // return Properties.Resources.Viewer_24;
                return null;
            }
        }


        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.

        public override Guid ComponentGuid
        {
            get { return new Guid("{7DC2729B-43EF-4B01-B9A0-607B4633760B}"); }
        }



        public override void CreateAttributes()
        {
            var newAttri = new ImagePreviewComponentAttributes(this);
            //newAttri.mouseImgClickEvent += OnMouseImgClickEvent;
            //newAttri.mouseNavClickEvent += OnMouseNavClickEvent;
            m_attributes = newAttri;

        }



 
    }



    public class ImagePreviewComponentAttributes : GH_ComponentAttributes
    {
        private float border = 3;
        private float plotWidth = 1;
        private float plotHeight = 1;

        private Bitmap imgBitmap;

        private Graphics MyGraphics;
        private ImagePreviewComponent ImagePreviewComponent;

        public ImagePreviewComponentAttributes(ImagePreviewComponent owner)
            : base(owner)
        {
            this.imgBitmap = owner.Bitmap;
            this.ImagePreviewComponent = (ImagePreviewComponent)this.Owner;
        }

        protected override void Layout()
        {
            base.Layout();

            this.imgBitmap = this.ImagePreviewComponent.Bitmap;

            // get chart dimensions
            plotHeight = ImagePreviewComponent.PlotterHeight;
            plotWidth = ImagePreviewComponent.PlotterWidth;
            float minHeight = m_innerBounds.Height + border;
            float minWidth = m_innerBounds.Width + border;
            if (plotHeight < minHeight) plotHeight = minHeight;
            if (plotWidth < minWidth) plotWidth = minWidth;

            // resize component
            var bounds = Bounds;
            bounds.Height = plotHeight + 2 * border;
            bounds.Width = plotWidth + ImagePreviewComponent.Params.InputWidth + 2 * border + ImagePreviewComponent.Params.OutputWidth;
            Bounds = bounds;

            // shift inner bounds for border
            m_innerBounds.Y += border;
            LayoutInputParams(Owner, m_innerBounds);

            // shift output region
            m_innerBounds.X = Bounds.Right - border - ImagePreviewComponent.Params.OutputWidth - m_innerBounds.Width;
            LayoutOutputParams(Owner, m_innerBounds);



        }

        public RectangleF PlotterRegion()
        {
            // get image bounds
            float left = Bounds.Left + border + ImagePreviewComponent.Params.InputWidth;
            float right = left + plotWidth;
            float top = Bounds.Top + border;
            float bottom = top + plotHeight;

            // get image rectangle
            var loc = new PointF(left, top);
            var size = new SizeF(plotWidth, plotHeight);
            return new RectangleF(loc, size);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {

            MyGraphics = graphics;


            // render normally
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                // get image rectangle
                var rect = PlotterRegion();

                // get palette from current state
                var palette = (Owner.Hidden) ? GH_Palette.Hidden : GH_Palette.Normal;
                if (Owner.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error) palette = GH_Palette.Error;
                if (Owner.RuntimeMessageLevel == GH_RuntimeMessageLevel.Warning) palette = GH_Palette.Warning;
                if (Owner.Locked) palette = GH_Palette.Locked;

                // render empty capsule
                var capsule = GH_Capsule.CreateCapsule(rect, palette, 0, 0);
                capsule.Render(graphics, Selected, Owner.Locked, false);

                // draw white background
                rect = new RectangleF(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                graphics.FillRectangle(Brushes.White, rect);

                // draw plot
                RectangleF rec = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);

                if (this.imgBitmap != null)
                {
                    MyGraphics.DrawImage(imgBitmap, rec);
                }
                else
                {
                    DisplayDefaultComponent(rec);
                }
            }
        }




        private void DisplayDefaultComponent(RectangleF rec)
        {
            //reset the comonent
            imgBitmap = null;
            this.Owner.Message = null;

            var bitmap = new Bitmap((int)plotWidth, (int)plotHeight);
            using (Graphics graph = Graphics.FromImage(bitmap))
            {
                Rectangle ImageSize = new Rectangle(0, 0, (int)plotWidth, (int)plotHeight);
                graph.FillRectangle(Brushes.White, ImageSize);
            }

            Pen pen = new Pen(Color.Gray, 3);
            SolidBrush myBrush = new SolidBrush(Color.Gray);

            Font standardFont = GH_FontServer.Standard; //29
            Font standardFontAdjust = GH_FontServer.NewFont(standardFont, (float)Math.Round(120M / standardFont.Height));

            StringFormat myFormat = new StringFormat();

            MyGraphics.FillRectangle(myBrush, Rectangle.Round(rec));
            MyGraphics.DrawImage(bitmap, new RectangleF(rec.X, rec.Y, rec.Width, rec.Height));
            MyGraphics.DrawString("No results to display", standardFontAdjust, Brushes.Black, new Point((int)rec.X + 12, (int)rec.Y + ((int)rec.Height) - 20), myFormat);

            myBrush.Dispose();
            myFormat.Dispose();
        }


    }

}
