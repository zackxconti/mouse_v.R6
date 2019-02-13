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



namespace Lab_Mouse.Components
{
    public class DataGenerator : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public string samplingAlgorithm;
        public bool generate_flag;
        public List<GH_NumberSlider> pluggedSliders;
        public CSVtype csvdata;
        public List<string> pluggedSliderNames;
        public List<string> pluggedOutputNames;
        // public List<Grasshopper.Kernel.IGH_DocumentObject> pluggedSliders;
        //public List<Grasshopper.GUI.GH_Slider> pluggedSliders;

        public DataGenerator()
          : base("DataGenerator", 
                "datagen",
                "Generates CSV data",
                "Lab Mouse", 
                "Data")
        {

            // identify plugged sliders and store them for later reference
            var inputs = this.Params.Input[0].Sources;
            this.pluggedSliderNames = new List<string>();
            this.pluggedOutputNames = new List<string>();
            this.pluggedSliders = new List<GH_NumberSlider>();
            this.samplingAlgorithm = "sobol"; // by default,sobol sequece is used 


        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Inputs", "PSliders", "Plug input sliders", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Outputs", "POutputs", "Plug output numeric values", GH_ParamAccess.tree);
            pManager.AddTextParameter("Directory", "dir", "Specify file directory", GH_ParamAccess.item);

        }



        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CSV", "csv", "input-output csv data", GH_ParamAccess.item);
            // change to GH_ParamAccess.List --> in case of multiple outputs, we want to be able to output (and write to txt file) multiple CSV file types, one for each output. 
            // OR MAYBE NOT -- we want to store output columns in same CSV - this is needed to build metamodel with multiple outputs. 
        }

        public override void CreateAttributes()
        {
            //this.m_attributes = (IGH_Attributes)new DataGeneratorAttributes(this, probabilities);
            //int num = this.Params.Input[0].SourceCount;
            this.m_attributes = new Attributes_Custom(this);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, this.csvdata);
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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("eaab59e6-4392-47d8-a9d2-61485eecb798"); }
        }
        public void menuItemLatin(object sender, EventArgs e)
        {
            this.samplingAlgorithm = "latin";
        }

        public void menuItemSobol(object sender, EventArgs e)
        {
            this.samplingAlgorithm = "sobol";
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem SamplerDropdown = GH_DocumentObject.Menu_AppendItem(menu, "Sampling Algorithm");

            Menu_AppendItem(SamplerDropdown.DropDown, "Sobol Sequence", menuItemSobol);
            Menu_AppendItem(SamplerDropdown.DropDown, "Latin Hypercube", menuItemLatin);
            Menu_AppendItem(SamplerDropdown.DropDown, "MDRM (efficient)", null);
            Menu_AppendItem(SamplerDropdown.DropDown, "Factorial DOE (efficient)", null);


            //Menu_AppendSeperator(menu);
            //menu.Items.Add("Latin Hypercube Sampling", null, menuItemLatin);
           
            //Menu_AppendSeperator(menu);
            //menu.Items.Add("Sobol sampling", null, menuItemSobol);
            base.AppendAdditionalMenuItems(menu);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Run Solver", SolverClicked);
        }
        private void SolverClicked(object sender, EventArgs eventArgs)
        {
            RunSolver();
        }

        public List<List<double>> readcsv(string filename)
        {

            List<List<double>> data = new List<List<double>>();

            // 1
            // Declare new List.
            List<string> lines = new List<string>();

            // 2
            // Use using StreamReader for disposing.
            using (StreamReader r = new StreamReader(filename))
            {
                // 3
                // Use while != null pattern for loop
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    // 4
                    // The "line" value is a line in the file.
                    // Add it to our List.
                    lines.Add(line);
                }
            }

            // 5
            // Print out all the lines.

            foreach (string line in lines)
            {
                List<double> row = new List<double>();

                string [] cols = line.Split(',');

                for (int i = 0; i < cols.Length; i++)
                    
                {
                    row.Add(double.Parse(cols[i]));
                    //data[index][i] = cols[i];
                    //Console.WriteLine(s);
                    //Console.(s);

                    data.Add(row);
                }
            }

            return data;
        }

        
        

        public List<List<double>> runPythonScript(string scriptpath, List<System.Object> Arguments)
        {

            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            //info.FileName = @"C:\Dropbox\Apps\Python3\python.exe";
            info.FileName = @"C:\Python27\python.exe";
            //info.FileName = @"python.exe";
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.Arguments = "\"" + scriptpath + "\"";
            List<string> outputs = new List<string>(); // stores all outputs from python script
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

            return readcsv(outputs[0]);
        }

        public void RunSolver()
        {
            // Get the document this component belongs to.
            GH_Document doc = OnPingDocument();
            if (doc == null) return;

            // First figure out which sliders are to be used as inputs.
            // This means iterating over all sources of the first input parameter,
            // and seeing if they are sliders. If we find something other than a slider,
            // print an error message and abort.

            List<GH_NumberSlider> sliders = new List<GH_NumberSlider>();
            //List<List<string>> slider_ranges = new List<List<string>>(); // to store slider ranges for sampling

            ////// generate sampling data based on plugged sliders //////
            var csv = new StringBuilder();

            List<string> names = new List<string>();

            foreach (IGH_Param source in Params.Input[0].Sources)
            {
                GH_NumberSlider slider = source as GH_NumberSlider;
                if (slider == null)
                {
                    Rhino.RhinoApp.Write("One of the inputs is not a slider.");
                    return;
                }
                sliders.Add(slider);
                this.pluggedSliders.Add(slider);

                

                var nickname = source.NickName;
                names.Add(nickname.ToString() ); // Add slider name to global list 
                var minimum = (slider.Slider.Minimum).ToString("0.00");
                var maximum = (slider.Slider.Maximum).ToString("0.00");
         
                var newLine = string.Format("{0}, {1}, {2}", nickname, minimum, maximum);
                csv.AppendLine(newLine);

                // now write slider_ranges list to csv file and pass as parameter for python system call 
            }

            this.pluggedSliderNames = names;

            //string direc = this.Params.Input[2].Sources[0].VolatileData.ToString(); // need to check what this is returning 
            //Grasshopper.Kernel.Parameters.Param_String param = (Grasshopper.Kernel.Parameters.Param_String)this.Params.Input[2];
            //string direc = param.PersistentData.ToString();

            //Param_String param0 = Params.Input[2] as Param_String;
            //GH_Structure<GH_String> data = param0.VolatileData as GH_Structure<GH_String>;
            
            //GH_String dat = data.AllData(true);

            
            // access directory string 
            foreach (GH_String dat in Params.Input[2].Sources[0].VolatileData.AllData(true))
           
            {
                string directory = dat.Value;
            

                //    Grasshopper.Kernel.IGH_Param sourceZ = this.Params.Input[2].Sources[0].VolatileData; //ref for input where a boolean or a button is connected
                //Grasshopper.Kernel.Types.GH_String text = sourceZ as Grasshopper.Kernel.Types.GH_String;
                //string direc = text.Value;
 
                string ranges_filepath = Path.Combine(directory, "ranges.txt");

                File.WriteAllText(ranges_filepath, csv.ToString());

                /// run python script to generate samples ///
                string samplingscript_filepath = Path.Combine(directory, "intercommunication_script.py");

                //string samplingscript_filepath = "C:\\Users\\zac067\\Desktop\\intercommunication_script.py"; // TODO: internalise this script in the component dll?

                List <System.Object> Arguments = new List<System.Object>();
                Arguments.Add(ranges_filepath);

                //string type = "sobol";
                Arguments.Add(this.samplingAlgorithm);

                int samplesize = 1000;
                Arguments.Add(samplesize.ToString());
            
                // Generate samples by calling sampling Python script //
                List<List<double>> Samples = runPythonScript(samplingscript_filepath, Arguments);

                // Print samples to Rhino Console 
                for (int j=0; j < Samples.Count; j++)
                {
                    for (int k=0; k < Samples[j].Count; k++)
                    {                     
                        Console.WriteLine(Samples[j][k]);
                    }       
                }

                if (sliders.Count == 0)
                {
                    Rhino.RhinoApp.Write("At least one variable slider must be connected to the X input.");
                    return;
                }

                // Similarly, we need to find which number parameter is to be used as the measure.
                // We only accept a single one, and it may only be a Param_Number (you may want to make
                // this more flexible in production code).

                var outputs = Params.Input[1].Sources;
                //List<Param_Number> pluggedOutputs = new List<Param_Number>();
                List<POutput> pluggedPOutputs = new List<POutput>();
                for (int o = 0; o < outputs.Count; o++)
                {
                    pluggedPOutputs.Add(outputs[o] as POutput);
                    this.pluggedOutputNames.Add(outputs[o].NickName.ToString());
                   
                }

                for (int o=0; o < pluggedPOutputs.Count; o++)
                {

                    if (pluggedPOutputs[o] == null)
                    {
                        Rhino.RhinoApp.Write("One of the plugged output parameters is not of type POutput.");
                        return;
                    }
                }

                
                //Param_Number outcome = Params.Input[1].Sources[0] as Param_Number;

                // Now that we have a bunch of sliders and a measure parameter, 
                // we can generate a bunch of solutions.
                // We will also harvest the resulting outcome and print each state to the command line.


                List<List<double>> csvd = new List<List<double>>();

                // Assign Samples to sliders as SliderValues 
                // loop through list of Samples
                for (int i = 0; i < Samples.Count; i++)
                {
                    for (int j = 0; j < sliders.Count; j++)
                    {
                        sliders[j].SetSliderValue((decimal)Samples[i][j]); // does not work 
                    }


                    doc.NewSolution(false);

                    List<double> csvrow = new List<double>();

                    // For each Sample vector, harvest the actual slider values.
                    List<string> sliderValuesTxt = new List<string>();
                    List<double> sliderValues = new List<double>();
                    foreach (GH_NumberSlider slider in sliders)
                    {
                        sliderValuesTxt.Add(slider.Slider.GripText); // save as text for printing to console                
                        csvrow.Add((double)slider.Slider.Value); // cast slider value from decimal to double 
                    }


                    // For each sample vector, harvest response output. This can be made more flexible.

                    string measure = "no values yet";

                    // for each output that is plugged in, store each output value 
                    for (int o = 0; o < pluggedPOutputs.Count; o++)
                    {
                        // access stored output value
                        var volatileData = pluggedPOutputs[o].VolatileData as GH_Structure<GH_Number>;

                        if (volatileData != null)
                        {
                            csvrow.Add((double)volatileData.Branches[0][0].Value); // store output value in csv
                            measure = string.Format("{0:0.0000}", volatileData.Branches[0][0].Value); // for printing to console                   
                        }
                    }

                    // Add csv row to csvdata 
                    csvd.Add(csvrow);

                    Rhino.RhinoApp.WriteLine("({0}) = {1:0.00000}", string.Join(", ", sliderValuesTxt), measure);                    
                }
                // Update CSVtype with generated csvdata
                this.csvdata = new CSVtype(this.pluggedSliderNames, this.pluggedOutputNames, csvd, directory); 
                
                // Write csv data to text file in user-specified directory
                this.csvdata.writeCSV(directory);
            }
        }
    }



    public class Attributes_Custom : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {

        DataGenerator own;


        public Attributes_Custom(DataGenerator owner)

            : base(owner) {
            own = owner;
            
        }

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
            rec0.Height += 22;

            System.Drawing.Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - 22;
            rec1.Height = 22;
            rec1.Inflate(-2, -2);

            Bounds = rec0;
            ButtonBounds = rec1;
        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Generate!", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
        }
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
                    if (numsliders != 0) { 
                        DialogResult result = MessageBox.Show("You have "+numsliders+" input sliders connected. Go ahead?", "Slider Automator", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            datgen.RunSolver();
                        }
                        else if (result == DialogResult.No)
                        {
                            // do nothing
                        }
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show("No sliders are connected!", "Slider Automator");
                    }


                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    
    }


    
}