using NullEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SeemoPredictor.Geometry;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using SeemoPredictor;

using Grasshopper.Kernel;

namespace NullEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Renderer renderer;

        public MainWindow()
        {

            
            //original null engine

            InitializeComponent(); //default Null

            Closed += MainWindow_Closed; //default Null

            InitRenderer(); //default Null
            

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            renderer.Stop();
        }

        private void InitRenderer()
        {
            renderer = new Renderer(renderFrame, 60, false);
            renderer.Start();
        }
    }
}
