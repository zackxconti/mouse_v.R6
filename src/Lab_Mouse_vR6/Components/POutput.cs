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
using Lab_Mouse_vR6.Properties;
using Lab_Mouse_vR6.Components;

namespace Lab_Mouse_vR6.Components
{
    public class POutput : GH_Param<GH_Integer>
    {
        /// Initializes a new instance of the MyComponent1 class.
        public List<double> probabilities;
        // default starting string, need to be the same as the default starting probability distribution
        public string tempPD = "0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1";
        public float max;
        public float min;
        public string draw_flag;

        public POutput()
          : base(new GH_InstanceDescription("PDF Output", "POutput",
              "bla bla",
              "Lab Mouse vR6", "Modeling"))
        {
            // default starting distribution
            this.probabilities = new List<double> { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 };
            this.draw_flag = "h";
        }

        public override GH_Exposure Exposure
        {
            get;
        }

        /// Provides an Icon for the component.
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// Gets the unique ID for this component. Do not change this ID after release.
        public override Guid ComponentGuid
        {
            get { return new Guid("05b6c687-72c1-470e-8618-bb17fd7b02c6"); }
        }

        // create component display attributes
        public override void CreateAttributes()
        {
            {
                this.m_attributes = new POutputAttributes(this);
            }
        }

        // update PD value
        private void updatePD(object sender, string text)
        {
            this.tempPD = text;
        }

        // Call to update the PDF of this PSlider
        public void updatePDF(List<double> p)
        {
            this.probabilities = p;
        }

        // histo grapgh toggle
        public void menuItemHisto(object sender, EventArgs e)
        {
            this.draw_flag = "h";
        }

        // smooth graph toggle
        public void menuItemSmooth(object sender, EventArgs e)
        {
            this.draw_flag = "s";
        }

        // append additional items in menu
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            menu.Items.Add("Histogram", null, menuItemHisto);
            menu.Items.Add("Smooth", null, menuItemSmooth);
            // Rui
            // additional dropdown menu for custom PD
            ToolStripMenuItem PDDropdown = GH_DocumentObject.Menu_AppendItem(menu, "Custom PD");

            string displayText = "";
            for (int i = 0; i < this.probabilities.Count; i++)
            {
                displayText += this.probabilities[i].ToString();
                if (i != this.probabilities.Count - 1)
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

        // parse input PD and update probability
        private void OK_Click(object sender, EventArgs e)
        {
            string[] values = this.tempPD.Split(',');
            List<double> tempPDList = new List<double>();

            for (int i = 0; i < values.Length; i++)
            {
                tempPDList.Add(Double.Parse(values[i], CultureInfo.InvariantCulture));
            }

            this.probabilities = tempPDList;

            ExpireSolution(true);
        }

        // cancel input event
        private void Cancel_Click(object sender, EventArgs e)
        {
        }

        // initialization 
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            //this.Attributes.Bounds = new RectangleF(this.Attributes.Pivot, new SizeF(160, 20));
        }
    }


    // POutput component attributes
    public class POutputAttributes : GH_ResizableAttributes<POutput>
    {
        private List<double> probabilities;
        POutput own;

        public POutputAttributes(POutput owner) : base(owner)
        {
            own = owner;
        }

        // minimum size for resizing
        protected override Size MinimumSize
        {
            get { return new Size(160, 20); }
        }


        protected override Size MaximumSize
        {
            get { return new Size(500, 20); }
        }

        // padding area for mouse responsing in order to resize
        protected override Padding SizingBorders
        {
            get { return new Padding(0, 0, 10, 0); }
        }

        // get points to draw polygon
        private PointF[] getPts(List<double> Probabilities)

        {
            int n = probabilities.Count + 4;
            PointF[] points = new PointF[n];

            points[0] = new PointF(this.Pivot.X, this.Pivot.Y - 7);
            points[1] = new PointF(this.Pivot.X + (Bounds.Width) - 2, this.Pivot.Y - 7);


            if (probabilities.Count != 0)
            {
                // routine to get drawing coordinates based on bin  probabilities
                for (int i = 0; i < Probabilities.Count; i++)
                {
                    float rail_width = (int)(points[1].X - points[0].X);
                    float bin_width = rail_width / Probabilities.Count;
                    float t = (Probabilities.Count - i) / Probabilities.Count;

                    points[i + 3] = new PointF((float)((this.Pivot.X) + (bin_width * (Probabilities.Count - i)) - bin_width * 0.5), this.Pivot.Y - 7 - (glob.max_ht * (float)Probabilities[Probabilities.Count - i - 1]));
                }
            }

            points[2] = new PointF(points[1].X, points[3].Y);
            points[n - 1] = new PointF(points[0].X, points[n - 2].Y);

            return points;
        }

        // Calculate the background bays for the graph based on Probabilities 
        // return a list of equal size rectangles as background for the render
        private RectangleF[] getBackgroundBins(List<double> Probabilities)
        {
            int n = Probabilities.Count;
            RectangleF[] backgroundBins = new RectangleF[n];

            PointF[] points = new PointF[n];


            points[0] = new PointF(this.Pivot.X, this.Pivot.Y - 7);
            points[1] = new PointF(this.Pivot.X + (Bounds.Width) - 2, this.Pivot.Y - 7);

            if (Probabilities.Count != 0)
            {
                for (int i = 0; i < Probabilities.Count; i++)
                {
                    float rail_width = (int)(points[1].X - points[0].X);
                    float bin_width = rail_width / Probabilities.Count;

                    backgroundBins[i] = new System.Drawing.RectangleF(
                        (float)((this.Pivot.X) + (bin_width * (Probabilities.Count - i - 1))),
                        (float)(this.Pivot.Y - 7 - glob.max_ht),
                        (float)bin_width,
                        (float)(glob.max_ht));
                }
            }

            return backgroundBins;
        }

        // Calculate the background bays for the graph based on Probabilities 
        // return a list of equal size rectangles as background for the render
        private RectangleF[] getTextholder(List<double> Probabilities)
        {
            int n = Probabilities.Count;
            RectangleF[] textholder = new RectangleF[n];

            PointF[] points = new PointF[n];

            float bin_width = (Bounds.Width-4 )/ Probabilities.Count;

            Rhino.RhinoApp.WriteLine(bin_width.ToString());

            points[0] = new PointF(this.Pivot.X+2, this.Pivot.Y + 62);
            points[1] = new PointF(this.Pivot.X + (Bounds.Width) - bin_width, this.Pivot.Y + 62);

            if (Probabilities.Count != 0)
            {
                for (int i = 0; i < Probabilities.Count; i++)
                {
                    textholder[i] = new System.Drawing.RectangleF(
                        (float)((this.Pivot.X+3) + (bin_width * (Probabilities.Count - i - 1))),
                        (float)(this.Pivot.Y + 2),
                        (float)(bin_width-2),
                        (float)(16));
                }
            }

            return textholder;
        }

        // this function gets the coordinates from the probabilities, (same as getPts) but draws a HISTOGRAM shape an irregular polygon from points
        private PointF[] getHistoPts(List<double> Probabilities)

        {
            int n = (Probabilities.Count * 2) + 2;
            PointF[] points = new PointF[n];

            points[0] = new PointF(this.Pivot.X, this.Pivot.Y - 7);
            points[1] = new PointF(this.Pivot.X + (Bounds.Width) - 2, this.Pivot.Y - 7);


            if (Probabilities.Count != 0)
            {
                int count = 0;
                // routine to get drawing coordinates based on bin  probabilities
                for (int i = 0; i < Probabilities.Count; i++)
                {
                    float rail_width = (int)(points[1].X - points[0].X);
                    float bin_width = rail_width / Probabilities.Count;
                    float t = (Probabilities.Count - i) / Probabilities.Count;

                    points[count + 2] = new PointF((float)((this.Pivot.X) + (bin_width * (Probabilities.Count - i))), this.Pivot.Y - 7 - (glob.max_ht * (float)Probabilities[Probabilities.Count - i - 1]));
                    points[count + 2 + 1] = new PointF((float)((this.Pivot.X) + (bin_width * (Probabilities.Count - i - 1))), this.Pivot.Y - 7 - (glob.max_ht * (float)Probabilities[Probabilities.Count - i - 1]));
                    count += 2;
                }
            }

            return points;
        }

        // double click event handler
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
                        own.probabilities[pos] = 0;
                    }
                    else
                    {
                        own.probabilities[pos] = 1;
                    }
                    Owner.OnDisplayExpired(true);
                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDoubleClick(sender, e);
        }

        // component bound wrapper
        private System.Drawing.RectangleF[] backgroundBinBounds { get; set; }
        private System.Drawing.RectangleF[] textholderBounds { get; set; }

        private RectangleF _baseBounds;
        private RectangleF _extraBounds;

        protected override void Layout()
        {
            base.Layout();
            _baseBounds = Bounds;

            _extraBounds = Bounds;
            _extraBounds.Y = Bounds.Top - 60;
            _extraBounds.Height = 100;

            Bounds = _extraBounds;
            // Overwrite the Bounds property to include our external button.
        }

        // render
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != Grasshopper.GUI.Canvas.GH_CanvasChannel.Objects)
                return;

            RenderIncomingWires(canvas.Painter, Owner.Sources, Owner.WireDisplay);

            GH_Palette palette = GH_Palette.Normal;

            //base.Render(canvas, graphics, channel);
            //Bounds = new RectangleF(this.Pivot, new SizeF(Bounds.Width, 20));

            // Create a new Capsule 
            GH_Capsule capsule = GH_Capsule.CreateCapsule(new RectangleF(this.Pivot, new SizeF(Bounds.Width, 20)), palette);
            capsule.AddInputGrip(this.InputGrip.X, this.InputGrip.Y);
            capsule.AddOutputGrip(this.OutputGrip.X, this.OutputGrip.Y);

            GH_Capsule message = GH_Capsule.CreateTextCapsule(
                new RectangleF(new PointF(this.Pivot.X, this.Pivot.Y + 20), new SizeF(Bounds.Width, 20)),
                new RectangleF(new PointF(this.Pivot.X, this.Pivot.Y + 20), new SizeF(Bounds.Width, 20)),
                GH_Palette.Hidden,
                "Probabilities"
                );

            message.Render(graphics, Selected, Owner.Locked, false);
            message.Dispose();
            message = null;

            //Render the capsule using the current Selection, Locked and Hidden states.
            //Integer parameters are always hidden since they cannot be drawn in the viewport.
            capsule.Render(graphics, Selected, Owner.Locked, true);

            //Always dispose of a GH_Capsule when you're done with it.
            capsule.Dispose();
            capsule = null;

            //Bounds = _extraBounds;

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

            // draw text capsules
            RectangleF[] textholderBounds = getTextholder(own.probabilities);
            GH_Capsule[] textCapsules = new GH_Capsule[textholderBounds.Length];

            for (int i =0; i < textCapsules.Length; i++)
            {
                textCapsules[i] = GH_Capsule.CreateTextCapsule(textholderBounds[i], textholderBounds[i], GH_Palette.Hidden, "<=" + "48.3",3,3);
                textCapsules[i].Render(graphics, Selected, Owner.Locked, false);
            }
        }
    }
}