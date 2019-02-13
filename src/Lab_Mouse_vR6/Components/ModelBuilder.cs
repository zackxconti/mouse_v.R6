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
using System.Linq;
using System.Drawing.Printing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Xaml;
using System.Web.Script.Serialization;



namespace Lab_Mouse.Components
{




    public class ModelBuilder : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ModelBuilder class.
        /// </summary>
        /// 
        

        //Dictionary<string, string> model;
        
        

        public Dictionary<string, List<double>> priors;
        public Dictionary<string, List<List<double>>> BinRanges;
        public string model;
        List<PSlider> PSliders;
        List<POutput> POutputs;
        List<string> targets;
        public List<string> directory;
        IGH_Component datagencomponent;



        public ModelBuilder()
          : base("ModelBuilder", "BN Model",
              "Description",
              "Lab Mouse", "Model")
        {

            this.model = null;
            this.priors = null;
            this.BinRanges = null;
            this.PSliders = null;
            this.POutputs = null;
            this.targets = null;
            this.directory = new List<string>();




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

        public static Dictionary<string, TValue> ToDictionary<TValue>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        public void  getdatagenInputParams ()
        {
            // Get the document this component belongs to.
            GH_Document doc = OnPingDocument();
            if (doc == null) return;

            //CSVtype csvinput = this.Params.Input[0].Sources[0] as CSVtype;

            // Gain access to datgen component that is connected to this component
            IGH_Param source = this.Params.Input[0].Sources[0];


            // Get GUID of the connected datagen component
            Guid guid = source.Attributes.GetTopLevel.DocObject.InstanceGuid;
            this.datagencomponent = doc.FindComponent(guid);

            foreach (GH_String dat in datagencomponent.Params.Input[2].Sources[0].VolatileData.AllData(true))
            {
                this.directory.Add(dat.Value);
            }

            //List<PSlider> PSliders = datagencomponent.Params.Input[0].Sources as List<PSlider>;
            List<PSlider> PSliders = new List<PSlider>();

            //List<POutput> POutputs = datagencomponent.Params.Input[1].Sources as List<POutput>;
            List<POutput> POutputs = new List<POutput>();
            //List<Param_Number> outputs = datagencomponent.Params.Input[1].Sources as List<Param_Number>;

            List<IGH_Param> datagen_input_sources = this.datagencomponent.Params.Input[0].Sources as List<IGH_Param>;
            List<IGH_Param> datagen_output_sources = this.datagencomponent.Params.Input[1].Sources as List<IGH_Param>;

            foreach (IGH_Param s in datagen_input_sources)
            {
                PSliders.Add(s as PSlider);

            }

            foreach (IGH_Param o in datagen_output_sources)
            {
                POutputs.Add(o as POutput);

            }

            this.PSliders = new List<PSlider>(PSliders);
            this.POutputs = new List<POutput>(POutputs);

            // get target names from POutput compnent nicknames and store as targetnames
            //List<string> targetnames = new List<string>();
            string[] targetnames = new string[POutputs.Count];

            int index = 0;
            foreach (POutput POutput in this.POutputs)
            {
                string name = POutput.NickName as string;
                targetnames[index] = name;
                index++;
            }

            this.targets = new List<string>(targetnames);


        }

       

   

        public void RunSolver_BuildBN() // TODO: one 'runsolver()' function needed for each button 

        {
            getdatagenInputParams();
            /*
            // Get the document this component belongs to.
            GH_Document doc = OnPingDocument();
            if (doc == null) return;

            //CSVtype csvinput = this.Params.Input[0].Sources[0] as CSVtype;

            ///// Prepare arguments to pass to Python process to build Bayesian Network metamode l
            // Instaniate empty list of arguments
            //List<System.Object> Arguments = new List<System.Object>();
            
            // Gain access to datgen component that is connected to this component
            IGH_Param source = Params.Input[0].Sources[0];
            //DataGenerator datagencomponent = source as DataGenerator;
            
            // Get GUID of the connected datagen component
            Guid guid = source.Attributes.GetTopLevel.DocObject.InstanceGuid;
            IGH_Component datagencomponent = doc.FindComponent(guid);
            
            
            // Gain access to inputs of datagen component

            //List<PSlider> PSliders = datagencomponent.Params.Input[0].Sources as List<PSlider>;
            List<PSlider> PSliders = new List<PSlider>();
            
            //List<POutput> POutputs = datagencomponent.Params.Input[1].Sources as List<POutput>;
            List<POutput> POutputs = new List<POutput>();
            //List<Param_Number> outputs = datagencomponent.Params.Input[1].Sources as List<Param_Number>;

            List<IGH_Param> datagen_input_sources = this.datagencomponent.Params.Input[0].Sources as List<IGH_Param>;
            List<IGH_Param> datagen_output_sources = this.datagencomponent.Params.Input[1].Sources as List<IGH_Param>;

            foreach (IGH_Param s in datagen_input_sources)
            {
                PSliders.Add(s as PSlider);  
               
            }

            foreach (IGH_Param o in datagen_output_sources)
            {
                POutputs.Add(o as POutput);
                
            }

            this.PSliders = new List<PSlider>(PSliders);
            this.POutputs = new List<POutput>(POutputs);
      
            
            // NOTE: The only way to access data in a GH_Panel is to get it from VolatileData.AllData(true) which can only 
            // be accessed using foreach loop, even though we only want one string. Hence the following code:
            List<string> directory = new List<string> ();
            foreach (GH_String dat in this.datagencomponent.Params.Input[2].Sources[0].VolatileData.AllData(true))
            {
                directory.Add(dat.Value);
            }
            */

            //string IPCbuildPath = Path.Combine(directory[0], "buildButton_IPC.py");
            string IPCbuildPath = Path.Combine(this.directory[0], "buildButton_IPC.py"); 
            string csvfilepath = Path.Combine(this.directory[0], "SimulationData.txt");

            //string csvfilepath = "C:/Users/tij/Desktop/Zack/LabMouse_Dev/SimulationData.txt";

            ///// Prepare arguments to pass to Python process to build Bayesian Network metamode l
            // Instaniate empty list of arguments
            List<System.Object> Arguments = new List<System.Object>();

            // Add csvfilepath to list of Arguments
            Arguments.Add(csvfilepath);

            /*
            // get target names from POutput compnent nicknames and store as targetnames
            //List<string> targetnames = new List<string>();
            string [] targetnames = new string[POutputs.Count];
            
            int index = 0;
            foreach (POutput POutput in this.POutputs)
            {
                string name = POutput.NickName as string;
                targetnames[index] = name;
                index++;
            }

            this.targets = new List<string>(targetnames);
            */

            // convert to json format
            //string[][] names_formatted = targetnames.Select(x => new string[] { x }).ToArray();
            //string targetnames_json = JsonConvert.SerializeObject(names_formatted);

            // Add targetnames to list of Arguments
            //Arguments.Add(targetnames_json);
            Arguments.Add(string.Join(",",this.targets));


            // Now, that we have all the necessary information to build the Bayesian Network, we can run the python script
            // runPythonScript will return 3 arguments: [0] {this.E, this.V, this.Vdata} [1] numbinsDict, [2] priors
            string jsonArguments = runPythonScript(IPCbuildPath, Arguments)[0]; // will return 3 arguments

            // Parse arguments to deserialise json string 
            var json = JObject.Parse(jsonArguments);

            // store json priors into a dict
            Dictionary<string, List<double>> priorpds = json["priors"].ToObject<Dictionary<string, List<double>>>();

            // store json bin ranges into a dict
            Dictionary<string, List<List<double>>> binranges = json["binranges"].ToObject<Dictionary<string, List<List<double>>>>();

            // store json model as string 
            string jsonmodel = json["model"].ToString();

            // update ModelBuilder properties
            this.priors = priorpds;
            this.BinRanges = binranges;
            this.model = jsonmodel;
            
  
            // loop through the PSliders to update priors 
            foreach (PSlider slider in this.PSliders)    
            {
                string name = slider.NickName;

                List<double> pd = this.priors[name].ToList(); // gets corresponding array of probabilities and converts to list 
                slider.updatePDF(new List<double>(pd));
                slider.priors = new List<double>(pd); // permanently store prior PD in PSlider component
                
                ExpireSolution(true);
            }

            // loop through the Poutputs to update their PDs and ranges
            foreach (POutput pout in this.POutputs)    
            {
                string name = pout.NickName;
                List<double> pd = this.priors[name].ToList(); // gets corresponding array of probabilities and converts to list 
                pout.updatePDF(new List<double>(pd));
                pout.Priors = new List<double>(pd);

                List<List<double>> ranges = binranges[name].ToList();
                pout.BinRanges = new List<List<double>>(ranges);

                ExpireSolution(true);
            }

        }

        public void RunSolver_UpdateBN() // TODO: one 'runsolver()' function needed for each button 
        {


            //if no model is loaded, then prompt message


            Dictionary<string, List<double>> evidence = new Dictionary<string, List<double>>();
            string IPCupdatePath = Path.Combine(this.directory[0], "buildButton_IPC.py");
            Dictionary<string, List<double>> allposteriors;



            // search through Psliders to check their evidence flag status 
            foreach (PSlider slider in this.PSliders)
            {
                if (slider.evidence == true)
                {
                    evidence[slider.NickName] = slider.probabilities;
                }
            }
            // search through Poutputs to check their evidence flag status
            foreach (POutput output in this.POutputs)
            {
                if (output.evidence == true)
                {
                    evidence[output.NickName] = output.Probabilities;
                }
            }

            

            if (this.model != null)
            {
               
                List<System.Object> Arguments = new List<System.Object>();
                // note: the order in which the following are added, is order sensitive 
                Arguments.Add(this.model);
                Arguments.Add(this.targets);
                Arguments.Add(this.BinRanges);                
                Arguments.Add(evidence);

                //string jsonArguments = runPythonScript(IPCupdatePath, Arguments)[0];

            }

            // allposteriors = runpythonscript (this.model, this.targets, evidence)

            //update Psliders

            //update Poutputs
        }

        public void RunSolver_ResetBN()
        {
            foreach(PSlider slider in this.PSliders)
            {
                slider.updatePDF(new List<double>(this.priors[slider.NickName]));
            }

            foreach (POutput output in this.POutputs)
            {
                output.updatePDF(new List<double>(this.priors[output.NickName]));
            }

            this.ExpireSolution(true);
        }


        public List<string> runPythonScript(string scriptpath, List<System.Object> Arguments)
        {

            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = @"C:\Python27\python.exe";
            info.UseShellExecute = false;
            info.CreateNoWindow = false;
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


            //info.Arguments = string.Format("")

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
                    //Console.Write("Standard Output");
                    Debug.WriteLine("Standard Output");
                    //Print(output.ReadToEnd());
                    //int o = int.Parse(output.ReadToEnd());
                    //string o = output.ReadToEnd();
                    string o = output.ReadLine();
                    outputs.Add(o);
                    Debug.WriteLine("The value of the output is "+o.ToString());
                }
            }
            catch {

                Debug.WriteLine("issues");
            }

            try
            {
                using (var output = python.StandardError)
                {
                    Console.Write("Standard Error");
                    Debug.WriteLine("Standard Error");

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
        
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ModelBuilder modelbld = Owner as ModelBuilder;

                System.Drawing.RectangleF rec1 = Button1Bounds;
                if (rec1.Contains(e.CanvasLocation))
                {
      
                    modelbld.RunSolver_BuildBN();

                    return GH_ObjectResponse.Handled;
                }

                System.Drawing.RectangleF rec2 = Button2Bounds;
                if (rec2.Contains(e.CanvasLocation))
                {

                    if (modelbld.model == null)
                    {
                        DialogResult result = MessageBox.Show("No model has been built. Please build model before updating.", "Warning");
                    }

                    else
                    {
                        modelbld.RunSolver_UpdateBN();
                    }


                    return GH_ObjectResponse.Handled;
                }

                System.Drawing.RectangleF rec3 = Button3Bounds;
                if (rec3.Contains(e.CanvasLocation))
                {

                    if (modelbld.model == null)
                    {
                        DialogResult result = MessageBox.Show("Nothing to reset. Model has been built.", "Warning");
                    }

                    else
                    {
                        modelbld.RunSolver_ResetBN();
                    }


                    return GH_ObjectResponse.Handled;
                }



            }
            return base.RespondToMouseDown(sender, e);
        }


    }




}
 