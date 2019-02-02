using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using System.Windows.Forms;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.IO;
using System.Text;
using System.Drawing.Printing;
using Newtonsoft.Json;

namespace Lab_Mouse.Components
{




    public class ModelBuilder : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ModelBuilder class.
        /// </summary>
        /// 

        Dictionary<string, string> model;
        Dictionary<string, string> numBinsDict;
        Dictionary<string, string> allPDs;

        public ModelBuilder()
          : base("ModelBuilder", "BN Model",
              "Description",
              "Lab Mouse", "Modeling")
        {

            this.model = null;
            this.numBinsDict = null;
            this.allPDs = null;

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CSV", "csv", "input-output csv data", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Metamodel", "metamodel", "bayesian network metamodel", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        public void RunSolver_BuildBN() // TODO: one 'runsolver()' function needed for each button 

        {
            // Get the document this component belongs to.
            GH_Document doc = OnPingDocument();
            if (doc == null) return;

            //CSVtype csvinput = this.Params.Input[0].Sources[0] as CSVtype;

            ///// Prepare arguments to pass to Python process to build Bayesian Network metamode l
            // Instaniate empty list of arguments
            List<System.Object> Arguments = new List<System.Object>();

            // Gain access to datgen component that is connected to this component
            IGH_Param source = Params.Input[0].Sources[0];
            DataGenerator datagencomponent = source as DataGenerator;

            // Gain access to inputs of datagen component
            List<PSlider> PSliders = datagencomponent.Params.Input[0].Sources as List<PSlider>;
            List<POutput> POutputs = datagencomponent.Params.Input[1].Sources as List<POutput>;
            //List<Param_Number> outputs = datagencomponent.Params.Input[1].Sources as List<Param_Number>;

            // NOTE: The only way to access data in a GH_Panel is to get it from VolatileData.AllData(true) which can only 
            // be accessed using foreach loop, even though we only want one string. Hence the following code:
            List<string> directory = new List<string> ();
            foreach (GH_String dat in datagencomponent.Params.Input[2].Sources[0].VolatileData.AllData(true))
            {
                directory.Add(dat.Value);
            }

            string IPCbuildPath = Path.Combine(directory[0], "IPC_Modelbuilder_build.py");
            string csvfilepath = Path.Combine(directory[0], "SimulationData.txt");

            // Add csvfilepath to list of Arguments
            Arguments.Add(csvfilepath);

            // get target names from POutput compnent nicknames and store as targetnames
            //List<string> targetnames = new List<string>();
            string[] targetnames = new string[POutputs.Count];
            
            int index = 0;
            foreach (POutput POutput in POutputs)
            {
                string name = POutput.NickName as string;
                targetnames[index] = name;
                index++;
            }

            // Add targetnames to list of Arguments
            Arguments.Add(targetnames);

            
            // Now, that we have all the necessary information to build the Bayesian Network, we can run the python script
            // runPythonScript will return 3 arguments: [0] this.E, this.V, this.Vdata [1] numbinsDict, [2] priors
            List<string> arguments = runPythonScript(IPCbuildPath, Arguments); // will return 3 arguments

            
            this.model = JsonConvert.DeserializeObject<Dictionary<string, string>>(arguments[0]); 
            Dictionary <string, string> allPDs = JsonConvert.DeserializeObject<Dictionary<string, string>>(arguments[1]);
            // call funtion to update all PSlider and Poutput probabilities accordig to priors 
            Dictionary<string, List<int>> zack = null;

            foreach (PSlider slider in PSliders)
            {
                string name = slider.NickName;
                //List<double> pd = allPDs[name]; 
                //slider.updatePDF()
            }
             
        }


        //public List<double> updatePSliderPDF (Dictionary<string,string> model, List<PSlider> sliders)
        //{
        //    //return sliders;
        //}


        public List<string> runPythonScript(string scriptpath, List<System.Object> Arguments)
        {

            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = @"C:\Python27\python.exe";
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.Arguments = "\"" + scriptpath + "\"";
            List<string> outputs = new List<string>();
            foreach (var argument in Arguments)
            {
                info.Arguments += " \"" + argument.ToString() + "\"";
                // here is where the parameters of the python script are passed
                // as system agruments. They are converted to strings
                // We can replace these arguments with a filepath to the actualy csv file
                // then read from the filepath as a parameter in python
                //info.Arguments += " \"" + argument.ToString() + "\"";
                // convert inputs into csv file and then IPC filepath as system input, then open in python as filepath and read inputs from csv
            }

            Console.Write("Send: {0}", info.Arguments);

            System.Diagnostics.Process python = new System.Diagnostics.Process();
            python.StartInfo = info;
            python.Start();

            //string returned;

            try
            {
                // using (StreamReader output = python.StandardOutput)
                using (var output = python.StandardOutput)
                {
                    Console.Write("Standard Output");
                    //Print(output.ReadToEnd());
                    //int o = int.Parse(output.ReadToEnd());
                    //string o = output.ReadToEnd();
                    string o = output.ReadLine();
                    outputs.Add(o);
                }
            }
            catch { }

            try
            {
                using (var output = python.StandardError)
                {
                    Console.Write("Standard Error");
                    Console.Write(output.ReadToEnd());
                }
            }
            catch { }

            python.WaitForExit();

            return outputs; // will return a list of arguments as strings 
        }
        
        


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        public override void CreateAttributes()
        {
            //this.m_attributes = (IGH_Attributes)new DataGeneratorAttributes(this, probabilities);
            //int num = this.Params.Input[0].SourceCount;
            this.m_attributes = new ModelBuilderAttributes(this);
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e517019d-8abe-4880-8581-019caa6e1011"); }
        }
    }




    /// custom attribute class
    /// 

    public class ModelBuilderAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {

        ModelBuilder own;


        public ModelBuilderAttributes(ModelBuilder owner)

            : base(owner)
        {
            own = owner;

        }

        protected override void Layout()
        {
            base.Layout();

            int n = 3;

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
            rec0.Height += 22*n;

            
            System.Drawing.Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - (22*n); // position of button (-ve is upwards)
            rec1.Height = 22; // height of button 
            rec1.Inflate(-2, -2);

            System.Drawing.Rectangle rec2 = rec0;
            rec2.Y = rec2.Bottom -22*(n-1);
            rec2.Height = 22;
            rec2.Inflate(-2, -2);

            System.Drawing.Rectangle rec3 = rec0;
            rec3.Y = rec3.Bottom - 22 * (n - 2);
            rec3.Height = 22;
            rec3.Inflate(-2, -2);

            Bounds = rec0;
            Button1Bounds = rec1;
            Button2Bounds = rec2;
            Button3Bounds = rec3;

        }
        private System.Drawing.Rectangle Button1Bounds { get; set; }
        private System.Drawing.Rectangle Button2Bounds { get; set; }
        private System.Drawing.Rectangle Button3Bounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button1 = GH_Capsule.CreateTextCapsule(Button1Bounds, Button1Bounds, GH_Palette.Black, "Build Model", 2, 0);
                button1.Render(graphics, Selected, Owner.Locked, false);
                button1.Dispose();

                GH_Capsule button2 = GH_Capsule.CreateTextCapsule(Button2Bounds, Button2Bounds, GH_Palette.Black, "Update PDs", 2, 0);
                button2.Render(graphics, Selected, Owner.Locked, false);
                button2.Dispose();

                GH_Capsule button3 = GH_Capsule.CreateTextCapsule(Button3Bounds, Button3Bounds, GH_Palette.Black, "Reset All PDs", 2, 0);
                button3.Render(graphics, Selected, Owner.Locked, false);
                button3.Dispose();
            }
        }
        /*
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DataGenerator datgen = Owner as DataGenerator;

                System.Drawing.RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    //int num  = numSld;

                    int numsliders = own.Params.Input[0].SourceCount;
                    DialogResult result = MessageBox.Show("You have " + numsliders + " input sliders connected. Go ahead?", "Slider Automator", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        datgen.RunSolver();
                    }
                    else if (result == DialogResult.No)
                    {
                        // do nothing
                    }

                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
        */

    }
}