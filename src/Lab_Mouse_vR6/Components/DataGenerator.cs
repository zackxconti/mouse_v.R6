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
using CsvHelper;
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
        public string sampling;
        public bool generate_flag;
        public List<Grasshopper.Kernel.Special.GH_NumberSlider> pluggedSliders;
       // public List<Grasshopper.Kernel.IGH_DocumentObject> pluggedSliders;
        //public List<Grasshopper.GUI.GH_Slider> pluggedSliders;

        public DataGenerator()
          : base("DataGenerator", "datagen",
              "Generates CSV data",
              "Lab Mouse vR6", "Data")
        {

            // identify plugged sliders and store them for later reference
            var inputs = this.Params.Input[0].Sources;
            for (int i=0; i < inputs.Count; i++)
            {
                GH_NumberSlider s = inputs[i] as GH_NumberSlider;
                this.pluggedSliders.Add(s);
                //this.pluggedSliders.Add(inputs[i].Attributes.GetTopLevel.DocObject);
                //this.pluggedSliders.Add(inputs[i] as Grasshopper.GUI.GH_Slider);

            }

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Inputs", "inputs", "Plug input sliders", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Outputs", "outputs", "Plug output numeric values", GH_ParamAccess.tree);
            pManager.AddTextParameter("Directory", "dir", "Specify file directory", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("CSV", "csv", "input-output csv data", GH_ParamAccess.item);
        }

        public override void CreateAttributes()
        {
            
            //this.m_attributes = (IGH_Attributes)new DataGeneratorAttributes(this, probabilities);
            this.m_attributes = new Attributes_Custom(this, this.pluggedSliders);
            
            
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //this.pluggedSliders[0].Slider.Value = (decimal)0;

            //GH_NumberSlider ghSlider = Params.Input[0].Sources[0] as GH_NumberSlider;
            //ghSlider.SetSliderValue(0);

            // here call sampling function that returns input csv
            // here write logic for calling sliders, batch running simulations and recording input and output values into a csv file 




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
            this.sampling = "latin";
        }

        public void menuItemSobol(object sender, EventArgs e)
        {
            this.sampling = "sobol";
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {

            //Menu_AppendSeperator(menu);
            menu.Items.Add("Latin Hypercube Sampling", null, menuItemLatin);
            //Menu_AppendSeperator(menu);
            menu.Items.Add("Sobol sampling", null, menuItemSobol);
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
            //const string f = "TextFile1.txt";

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
                    // Insert logic here.
                    // ...
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

            foreach (IGH_Param source in Params.Input[0].Sources)
            {
                GH_NumberSlider slider = source as GH_NumberSlider;
                if (slider == null)
                {
                    Rhino.RhinoApp.Write("One of the variable inputs is not a slider.");
                    return;
                }
                sliders.Add(slider);

                //List<string> range = new List<string>();
                //range.Add(source.NickName);
                //range.Add((slider.Slider.Minimum).ToString("0.00"));
                //range.Add((slider.Slider.Maximum).ToString("0.00"));
                //slider_ranges.Add(range);

                var nickname = source.NickName;
                var minimum = (slider.Slider.Minimum).ToString("0.00");
                var maximum = (slider.Slider.Maximum).ToString("0.00");
         
                var newLine = string.Format("{0}, {1}, {2}", nickname, minimum, maximum);
                csv.AppendLine(newLine);

                // now write slider_ranges list to csv file and pass as parameter for python system call 
            }

            //string direc = this.Params.Input[2].Sources[0].VolatileData.ToString(); // need to check what this is returning 
            //Grasshopper.Kernel.Parameters.Param_String param = (Grasshopper.Kernel.Parameters.Param_String)this.Params.Input[2];
            //string direc = param.PersistentData.ToString();

            //Param_String param0 = Params.Input[2] as Param_String;
            //GH_Structure<GH_String> data = param0.VolatileData as GH_Structure<GH_String>;
            
            //GH_String dat = data.AllData(true);

            //string direc = null;
            foreach (GH_String dat in Params.Input[2].Sources[0].VolatileData.AllData(true))
            //GH_String dat = data.AllData(true).GetEnumerator.data;
            {
                string direc = dat.Value;
            

                //    Grasshopper.Kernel.IGH_Param sourceZ = this.Params.Input[2].Sources[0].VolatileData; //ref for input where a boolean or a button is connected
                //Grasshopper.Kernel.Types.GH_String text = sourceZ as Grasshopper.Kernel.Types.GH_String;
                //string direc = text.Value;
                //string ranges_filepath = string.Format("{0}\\{1}", dir, "\\rangesz.txt");
                //string ranges_filepath = string.Concat(direc, "\\rangesz.txt");
                //string ranges_filepath = Path.Combine(direc, "rangesz2.txt");
                string ranges_filepath = "C:\\Users\\zac067\\Desktop\\rangesz2.txt";
                //string combined = Path.Combine(direc, Path.GetFileName(fileName));

                //string ranges_filepath = "C:\\Users\\tij\\Desktop\\testza.txt";

                File.WriteAllText(ranges_filepath, csv.ToString());

                /// run python script to generate samples ///
                //string samplingscript_filepath = string.Format("{0}\\{1}", dir, "\\intercommunication_script.py");
                //string samplingscript_filepath = string.Concat(dir, "\\intercommunication_script.py");
                //string samplingscript_filepath = Path.Combine(direc, "intercommunication_script.py");

                string samplingscript_filepath = "C:\\Users\\zac067\\Desktop\\intercommunication_script.py"; // TODO: internalise this script in the component
                //string samplingscript_filepath = Path.Combine(direc, "intercommunication_script.py");


                List <System.Object> Arguments = new List<System.Object>();
                Arguments.Add(ranges_filepath);
            

                List<List<double>> Samples = new List<List<double>>();
                List<List<double>> outputdata = runPythonScript(samplingscript_filepath, Arguments);

                for (int j=0; j < outputdata.Count; j++)
                {
                    //List<double> l = new List<double>();
                    for (int k=0; k < outputdata[j].Count; k++)
                    {
                       // Samples[j].Add((double)outputdata[0][0]);
                        Console.WriteLine(outputdata[j][k]);
                    }
                    //Samples.Add(l);
                }


                Console.Write(Samples);
            

                if (sliders.Count == 0)
                {
                    Rhino.RhinoApp.Write("At least one variable slider must be connected to the X input.");
                    return;
                }

                // Similarly, we need to find which number parameter is to be used as the measure.
                // We only accept a single one, and it may only be a Param_Number (you may want to make
                // this more flexible in production code).
                if (Params.Input[1].SourceCount != 1)
                {
                    Rhino.RhinoApp.Write("Exactly one parameter must be connected to the Y input.");
                    return;
                }
                Param_Number outcome = Params.Input[1].Sources[0] as Param_Number;
                if (outcome == null)
                {
                    Rhino.RhinoApp.Write("Y input parameter is not of type Param_Number.");
                    return;
                }

                // Now that we have a bunch of sliders and a measure parameter, 
                // we can generate a bunch of solutions.
                // Temporary: We will run 100 solutions, moving all sliders each time.
                // We will also harvest the resulting outcome and print each state to the command line.
                // TO replace with sampling algorithm
                //int csvlen = 
            
                //List<List<double>> Samples = new List<List<double>>();



                for (int i = 0; i < outputdata.Count; i++)
                {
                    //double t = 0.01 * i;
                    //int index = 0;
                    for (int j=0; j<sliders.Count; j++)
                    {
                       // GH_NumberSlider slider = sliders[i]
                        sliders[j].SetSliderValue((decimal)outputdata[i][j]); // does not work 
                    }
                    //foreach (GH_NumberSlider slider in sliders)
                
                        //slider.TickValue = (int)(slider.TickCount * t);
                        //double value = 
                        //Console.Write(Samples[i][index]);

                        //slider.TickValue = (int)outputdata[i][index];
                        //slider.SetSliderValue((decimal)outputdata[i][index]); // does not work 
     
                        //index ++;
                    //he
                
               


                    doc.NewSolution(false);

                    // Now harvest the actual slider values.
                    List<string> values = new List<string>();
                    foreach (GH_NumberSlider slider in sliders)
                        values.Add(slider.Slider.GripText);

                    // And finally harvest the measure value. This can be made more flexible.
                    string measure = "no values";
                    var volatileData = outcome.VolatileData as GH_Structure<GH_Number>;
                    if (volatileData != null)
                    {
                        if (volatileData.DataCount == 1)
                            measure = string.Format("{0:0.0000}", volatileData.Branches[0][0].Value);
                        else
                            measure = "many values";
                    }

                    Rhino.RhinoApp.WriteLine("({0}) = {1:0.00000}", string.Join(", ", values), measure);

                }

            }
        }


    }



    public class Attributes_Custom : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        public List<Grasshopper.Kernel.Special.GH_NumberSlider> pluggedSliders;
        //public List<Grasshopper.Kernel.IGH_DocumentObject> pluggedSliders;
        //public List<Grasshopper.GUI.GH_Slider> pluggedSliders;

        public Attributes_Custom(GH_Component owner, List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders)
        //public Attributes_Custom(GH_Component owner, List<Grasshopper.Kernel.IGH_DocumentObject> sliders)
        //public Attributes_Custom(GH_Component owner, List<Grasshopper.GUI.GH_Slider> sliders)
            //public List<Grasshopper.Kernel.IGH_DocumentObject> pluggedSliders;

            : base(owner) {

            this.pluggedSliders = sliders;
            
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
                    MessageBox.Show("Generating csv data!", "Generate!", MessageBoxButtons.OK);
                    datgen.RunSolver();
                    //this.batchSimulation();
                    //Console.WriteLine(" number of plugged in sliders is ", this.pluggedSliders.Count);


                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        public void batchSimulation()
        {
            // write code to automate sliders and run simulations
            // 

            //GH_Slider slider = this.Params.Input[0].Attributes.GetTopLevel.DocObject;
            
            //for (int i=0; i<this.pluggedSliders.Count; i++)
            for (int i = 0; i <1; i++)
                {


                //GH_NumberSlider ghSlider = 
                //ghSlider.SetSliderValue(0);
                try

                {
                    this.pluggedSliders[i].Slider.Value = (decimal)0;
                }

                catch (Exception e)
                {
                    Console.WriteLine("This is empty", e);
                }
                //if (this.pluggedSliders[i] != null)
                //{
                // this.pluggedSliders[i].SetSliderValue((decimal)0);
                //    this.pluggedSliders[i].Value= (decimal)0;
                //}

            }



            //compInput_simOutputs = ghenv.Component.Params.Input[2]
        }



        //compInput_simOutputs = ghenv.Component.Params.Input[2]
    
    }


    
}