using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Linq;
using System.Collections;
using System.Globalization;

using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Attributes;
using Lab_Mouse.Components;




namespace Lab_Mouse.Components
{

    public class PSlider : Grasshopper.Kernel.Special.GH_NumberSlider
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        /// 

        public List<double> probabilities;
        public List<double> priors;
        // Rui 
        // Temporary storage for PD
        public string tempPD = "0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1"; // default starting string, need to be the same as the default starting probability distribution
        public float max;
        public float min;
        public string draw_flag;
        public bool evidence = false;
        public List<List<double>> binRange = new List<List<double>>();

        public List<double> Probabilities
        {
            get { return probabilities; }
            set { probabilities = value; }
        }

        public Guid MBguid;

        public PSlider()
          : base()
        {
            base.Name = "PDF Slider";
            base.NickName = "PSlider";
            base.Description = "bla bla ";
            base.Category = "Lab Mouse";
            base.SubCategory = "Parameters";

            
            this.Probabilities = new List<double> { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }; // default starting distribution
            this.priors = new List<double>();

            this.MBguid = new Guid ();

            max = (float)(this.Slider.Maximum);
            min = (float)(this.Slider.Minimum);

            this.draw_flag = "h";

            this.binRange.Add(new List<double> { 0, 1 });
            this.binRange.Add(new List<double> { 1, 2 });
            this.binRange.Add(new List<double> { 2, 3 });
            this.binRange.Add(new List<double> { 3, 4 });
            this.binRange.Add(new List<double> { 4, 5 });
            this.binRange.Add(new List<double> { 5, 6 });
            this.binRange.Add(new List<double> { 6, 7 });
            this.binRange.Add(new List<double> { 7, 8 });
        }


        public override GH_Exposure Exposure
        {
            get;
            // { return GH_Exposure.primary; }
        }

        public override void CreateAttributes()
        {
            {
                this.m_attributes = (IGH_Attributes)new PSliderAttributes(this, this.Probabilities);
            }
        }

        //protected override void SolveInstance(IGH_DataAccess DA)
        //{}

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            menu.Items.Add("Histogram", null, menuItemHisto);
            menu.Items.Add("Smooth", null, menuItemSmooth);
            // Rui
            // additional dropdown menu for custom PD
            ToolStripMenuItem PDDropdown = GH_DocumentObject.Menu_AppendItem(menu, "Custom PD");

            string displayText = "";
            for (int i = 0; i < this.Probabilities.Count; i++)
            {
                displayText += this.Probabilities[i].ToString();
                if (i != this.Probabilities.Count - 1)
                {
                    displayText += " , ";
                }

            }

            Menu_AppendTextItem(PDDropdown.DropDown,
                                displayText,
                                null,
                                new GH_MenuTextBox.TextChangedEventHandler(updatePD),
                                true);

            PDDropdown.DropDown.Items[1].Click += (obj, e) => OK_Click(obj, e);
            PDDropdown.DropDown.Items[2].Click += (obj, e) => Cancel_Click(obj, e);

            base.AppendAdditionalMenuItems(menu);
        }


        // Rui
        // parse input PD and update probability
        private void OK_Click(object sender, EventArgs e)
        {
            string[] values = this.tempPD.Split(',');
            List<double> tempPDList = new List<double>();

            for (int i = 0; i < values.Length; i++)
            {
                tempPDList.Add(Double.Parse(values[i], CultureInfo.InvariantCulture));
            }

            this.Probabilities = tempPDList;
            this.evidence = true;

            ExpireSolution(true);
        }

        // Rui
        // cancel input even
        private void Cancel_Click(object sender, EventArgs e)
        {

        }

        // Rui
        // store PD input
        private void updatePD(object sender, string text)
        {
            this.tempPD = text;
        }

        public void menuItemHisto(object sender, EventArgs e)
        {
            this.draw_flag = "h";
        }

        public void menuItemSmooth(object sender, EventArgs e)
        {
            this.draw_flag = "s";
        }

        // Call to update the PDF of this PSlider
        public void updatePDF(List<double> p)
        {
            this.Probabilities = p;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("00535b1c-f1f0-4760-aec6-4f3a521716b0"); }
        }

    }

    // sets maximum height of histogram window above slider (global variable)
    static class glob
    {
        public static int max_ht = 50;
    }


    // this class overrides the classic slider to inherit it's properties and draw over it
    public class PSliderAttributes : Grasshopper.Kernel.Special.GH_NumberSliderAttributes
    {

        private List<double> probabilities;
        PSlider own;
        // private float minimum;


        public PSliderAttributes(PSlider owner, List<double> probabs) :
          base(owner)
        {
            //probabilities = owner.probabilities;
            own = owner;
        }


        private float bins(float val)
        {

            List<List<float>> bin_ranges = new List<List<float>>();
            float max = (float)this.Owner.Slider.Maximum;
            float min = (float)this.Owner.Slider.Minimum;
            float bin_size = (max - min) / own.probabilities.Count;
            float returned_p = 0;

            float counter = min;
            for (int i = 0; i < own.probabilities.Count; i++)
            {

                List<float> t = new List<float> { counter, counter + bin_size };
                bin_ranges.Add(t);

                counter = t[1];
            }


            for (int i = 0; i < bin_ranges.Count; i++)
            {
                if (val > bin_ranges[i][0] && val <= bin_ranges[i][1])
                {
                    returned_p = (float)own.probabilities[i];
                }
            }
            return returned_p;
        }

        // this function gets the coordinates from the probabilities, for drawing an irregular polygon from points 
        private PointF[] getPts(List<double> Probabilities)

        {
            int n = probabilities.Count + 4;
            PointF[] points = new PointF[n];

            int width_nickname = GH_FontServer.StringWidth(Owner.NickName, GH_FontServer.Standard);


            points[0] = new PointF(this.Pivot.X + width_nickname, this.Pivot.Y - 7);
            points[1] = new PointF(this.Pivot.X + (Bounds.Width) - 11, this.Pivot.Y - 7);


            if (probabilities.Count != 0)
            {
                // routine to get drawing coordinates based on bin  probabilities
                for (int i = 0; i < Probabilities.Count; i++)
                {
                    float rail_width = (int)(points[1].X - points[0].X);
                    float bin_width = rail_width / Probabilities.Count;
                    float t = (Probabilities.Count - i) / Probabilities.Count;

                    points[i + 3] = new PointF((float)((this.Pivot.X + width_nickname) + (bin_width * (Probabilities.Count - i)) - bin_width * 0.5), this.Pivot.Y - 7 - (glob.max_ht * (float)Probabilities[Probabilities.Count - i - 1]));
                }
            }

            points[2] = new PointF(points[1].X, points[3].Y);
            points[n - 1] = new PointF(points[0].X, points[n - 2].Y);

            return points;
        }


        // Rui
        // Calculate the background bays for the graph based on Probabilities 
        // return a list of equal size rectangles as background for the render
        private RectangleF[] getBackgroundBins(List<double> Probabilities)
        {
            int n = Probabilities.Count;
            RectangleF[] backgroundBins = new RectangleF[n];

            PointF[] points = new PointF[n];

            int width_nickname = GH_FontServer.StringWidth(Owner.NickName, GH_FontServer.Standard);

            points[0] = new PointF(this.Pivot.X + width_nickname, this.Pivot.Y - 7);
            points[1] = new PointF(this.Pivot.X + (Bounds.Width) - 11, this.Pivot.Y - 7);

            if (Probabilities.Count != 0)
            {
                for (int i = 0; i < Probabilities.Count; i++)
                {
                    float rail_width = (int)(points[1].X - points[0].X);
                    float bin_width = rail_width / Probabilities.Count;

                    backgroundBins[i] = new System.Drawing.RectangleF(
                        (float)((this.Pivot.X + width_nickname) + (bin_width * (Probabilities.Count - i - 1))),
                        (float)(this.Pivot.Y - 7 - glob.max_ht),
                        (float)bin_width,
                        (float)(glob.max_ht));
                }
            }

            return backgroundBins;
        }


        // this function gets the coordinates from the probabilities, (same as getPts) but draws a HISTOGRAM shape an irregular polygon from points
        private PointF[] getHistoPts(List<double> Probabilities)

        {
            int n = (Probabilities.Count * 2) + 2;
            PointF[] points = new PointF[n];

            int width_nickname = GH_FontServer.StringWidth(Owner.NickName, GH_FontServer.Standard);


            points[0] = new PointF(this.Pivot.X + width_nickname, this.Pivot.Y - 7);
            points[1] = new PointF(this.Pivot.X + (Bounds.Width) - 11, this.Pivot.Y - 7);


            if (Probabilities.Count != 0)
            {
                int count = 0;
                // routine to get drawing coordinates based on bin  probabilities
                for (int i = 0; i < Probabilities.Count; i++)
                {
                    float rail_width = (int)(points[1].X - points[0].X);
                    float bin_width = rail_width / Probabilities.Count;
                    float t = (Probabilities.Count - i) / Probabilities.Count;

                    points[count + 2] = new PointF((float)((this.Pivot.X + width_nickname) + (bin_width * (Probabilities.Count - i))), this.Pivot.Y - 7 - (glob.max_ht * (float)Probabilities[Probabilities.Count - i - 1]));
                    points[count + 2 + 1] = new PointF((float)((this.Pivot.X + width_nickname) + (bin_width * (Probabilities.Count - i - 1))), this.Pivot.Y - 7 - (glob.max_ht * (float)Probabilities[Probabilities.Count - i - 1]));
                    count += 2;
                }
            }

            return points;
        }


        // Rui 
        // double click handler on graph
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            System.Drawing.RectangleF[] rec = backgroundBinBounds;

            for (int i = 0; i < rec.Length; i++)
            {
                if (rec[i].Contains(e.CanvasLocation))
                {
                    int pos = rec.Length - i - 1;

                    for (int j = 0; j < own.probabilities.Count; j++)
                    {
                        if (j != pos)
                        {
                            own.probabilities[j] = 0;
                        }
                    }

                    if (own.probabilities[pos] == 1)
                    {
                      
                        own.probabilities = new List<double>(own.priors);
                        own.evidence = false;
                        own.ExpireSolution(true);
                    }
                    else
                    {
                        own.probabilities[pos] = 1;
                    }
                    Owner.OnDisplayExpired(true);
                    return GH_ObjectResponse.Handled;
                }
            }

            own.evidence = true;
            return base.RespondToMouseDoubleClick(sender, e);
        }

        // Rui
        // new layout 
        private RectangleF _baseBounds;
        private RectangleF _thisBounds;
        private RectangleF _extraBounds;

        protected override void Layout()
        {
            base.Layout();
            _baseBounds = Bounds;

            _extraBounds = Bounds;
            _extraBounds.Y -= 60;
            _extraBounds.Height += 60;

            _thisBounds = RectangleF.Union(_baseBounds, _extraBounds);

            Bounds = _thisBounds;
        }

        // Rui
        // list of background bin Bounds
        private System.Drawing.RectangleF[] backgroundBinBounds { get; set; }

        // this function takes care of the drawing routines 
        protected override void Render(Grasshopper.GUI.Canvas.GH_Canvas canvas, Graphics graphics,
          Grasshopper.GUI.Canvas.GH_CanvasChannel channel)
        {

            if (channel != Grasshopper.GUI.Canvas.GH_CanvasChannel.Objects)
                return;

            // render of original component 
            //Bounds = _baseBounds;
            base.Render(canvas, graphics, channel);
            //Bounds = _thisBounds;

            int width = GH_FontServer.StringWidth(Owner.NickName, GH_FontServer.Standard);
            PointF p = new PointF(this.Pivot.X + width + 19, this.Pivot.Y - 7);

            List<double> probs = own.probabilities;

            PointF[] pts = getHistoPts(own.probabilities);

            if (own.draw_flag == "s")
            {
                pts = getPts(probs);
            }

            PointF maxPt = new PointF(0, 0);

            float ptY = pts[0].Y;
            int index_max = 0;
            for (int i = 0; i < pts.Length; i++)
            {
                if (pts[i].Y < ptY)
                {
                    ptY = pts[i].Y;
                    index_max = i;
                }
            }

            maxPt = pts[index_max];


            // Create Pens
            System.Drawing.Pen pen2 = new System.Drawing.Pen(Brushes.Red, 2);
            System.Drawing.Pen pen = new System.Drawing.Pen(Brushes.Black, 2);
            System.Drawing.Pen pen3 = new System.Drawing.Pen(Brushes.GhostWhite, 1);

            // Create Gradient Brush
            System.Drawing.Drawing2D.LinearGradientBrush lb = new System.Drawing.Drawing2D.LinearGradientBrush
              (new PointF(maxPt.X, maxPt.Y), new PointF(maxPt.X, this.Pivot.Y), Color.FromArgb(255, 0, 0),
              Color.FromArgb(255, 255, 255));
            System.Drawing.SolidBrush sb = new System.Drawing.SolidBrush(Color.FromArgb(70, 255, 255, 255));

            // Rui 
            // render background bins
            RectangleF[] backgroundBins = getBackgroundBins(own.probabilities);
            graphics.DrawRectangles(pen3, backgroundBins);
            graphics.FillRectangles(sb, backgroundBins);

            backgroundBinBounds = backgroundBins;

            //Draw Polygon ouline and fill
            graphics.DrawPolygon(pen, pts);
            graphics.FillPolygon(lb, pts);

            //Owner.NickName = "Variable";

            // Draw probability value
            // string evidence = "Y<5

            string s = "P(MINIMIZE Y)=" + (this.bins((float)Owner.CurrentValue) * 100).ToString() + "%";
            //string s = (Owner.CurrentValue + 1).ToString();
            //double cv = Owner.CurrentValue;
            graphics.DrawString(s, GH_FontServer.Standard, Brushes.Black, pts[1].X + 15, (int)(this.Pivot.Y - 8 - glob.max_ht));
        }
    }
}
