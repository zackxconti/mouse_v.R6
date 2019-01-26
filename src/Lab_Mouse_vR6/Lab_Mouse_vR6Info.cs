using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Lab_Mouse_vR6
{
    public class Lab_Mouse_vR6Info : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Lab_Mouse_vR6";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("7adf8c6d-484b-4ef3-aa10-72793e82fd1c");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
