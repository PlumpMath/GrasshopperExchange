using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Hairworm
{
    public class HairwormComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public HairwormComponent()
            : base("Hairworm", "Hairworm",
                "Description",
                "Extra", "Hairworm")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
//            pManager.AddTextParameter("String", "ClusterURL", "URL To Cluster", GH_ParamAccess.item);
//            pManager.AddBooleanParameter("Activate", "Activate", "Activate to emulate clsuter", GH_ParamAccess.item, false);
//            pManager.AddGeometryParameter("Input Geometry", "InputGeo", "InputGeometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Output Geometry", "OutputGe32o", "OutputGeometry", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Generic Output", "GenericOutput", "GenericOutput", GH_ParamAccess.tree);
            pManager.AddTextParameter("Debug", "Debug", "This is debug output", GH_ParamAccess.item); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Declare a variable for the input String
            string fileurl = null;
            bool activate = false;
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            // 1. Declare placeholder variables and assign initial invalid data.
            //    This way, if the input parameters fail to supply valid data, we know when to abort.

            // 2. Retrieve input data, exit if non-existent
//            if (!DA.GetData(0, ref fileurl)) { return; }
//            if (!DA.GetData(1, ref activate)) { return; }
//            if (!DA.GetData(2, ref point)) { return; }

			//temporary fire url
            fileurl = "https://github.com/provolot/GrasshopperExchange/raw/master/Hairworm/_example_files/SphereMaker.ghcluster";
            activate = true;

			// get temp. get filename.
            string tempPath = System.IO.Path.GetTempPath();
            Uri uri = new Uri(fileurl);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);

            string debugText = "";
            debugText += "client.downloadfile( " + fileurl + ", " + filename + " );\n";
            debugText += tempPath;

            DA.SetData(2, debugText);

            // If the retrieved data is Nothing, we need to abort.
            if (fileurl == null) { return; }

			// actually run this.
            if (activate)
            {
/*				// attempt to download file
				using (WebClient Client = new WebClient())
				{
					Client.DownloadFile(fileurl, tempPath + filename);
				}
*/
				// if gh file doesn't exist, abort 
				if (!File.Exists(tempPath + filename)) { return; }

				// create a cluster
                GH_Cluster thiscluster = new GH_Cluster();
                thiscluster.CreateFromFilePath(tempPath + filename);

 //               Grasshopper.Kernel.Parameters.Param_Geometry paramOut = new Grasshopper.Kernel.Parameters.Param_Geometry();
//                thiscluster.Params.RegisterOutputParam(paramOut);

				//get new document, enable it, and add cluster to it
                GH_Document newdoc = new GH_Document();
                newdoc.Enabled = true;
                newdoc.AddObject(thiscluster, true, 0);

//                newdoc.CreateAutomaticClusterHooks();
//                newdoc.ExpireSolution();
/*                debugText += "\nfindclusters comp = " + string.Join(", ", newdoc.FindClusters(thiscluster.ComponentGuid));
                debugText += "\nfindclusters = " + string.Join(", ",newdoc.FindClusters(thiscluster.InstanceGuid));
                debugText += string.Join(", ",newdoc.EnabledObjects());
                debugText += newdoc.ClusterOutputHooks();
                debugText += string.Join(", ", newdoc.ContainsClusterHooks()); */

//                debugText += "yoy";

//                Grasshopper.Kernel.Parameters.Param_Point paramIn = new Grasshopper.Kernel.Parameters.Param_Point();

//                Grasshopper.Kernel.GH_Param paramIn = new Grasshopper.Kernel.GH_Param();
				// make a 'param out' geometry item

				// NOTE TO MY SELF
				// I DON'T KNOW WHAT'S GOING WRONG HERE
				// AND I SHOULD FIX THIS THING
				// SO I CAN GET DATA FROM THIS PARAMETER
				// THIS IS SO THAT THIS WIL BREKA
//				GH_Param<GH_Brep> newparam = new GH_Param<GH_Brep>;

//                Grasshopper.Kernel.Parameters.Param_Geometry paramOut = new Grasshopper.Kernel.Parameters.Param_Geometry();
//                new Grasshopper.Kernel.GH_Param<Grasshopper.Kernel.Types.GH_Brep>();
//                Grasshopper.Kernel.GH_Param<Grasshopper.Kernel.Types.GH_Brep> paramOut = new Grasshopper.Kernel.GH_Param<Grasshopper.Kernel.Types.GH_Brep>();

//                paramIn.SetPersistentData(point);

//                cluster.Params.RegisterInputParam(paramIn);
//                thiscluster.Params.RegisterOutputParam(paramOut);

                debugText += "\noutputcount = " + thiscluster.Params.Output.Count;

				// Get a pointer to the data inside the first cluster output.
                IGH_Structure data = thiscluster.Params.Output[0].VolatileData;

                // Create a copy of this data (the original data will be wiped)
                DataTree<object> copy = new DataTree<object>();
                copy.MergeStructure(data, new Grasshopper.Kernel.Parameters.Hints.GH_NullHint());
                //A = copy;

                //GH_Structure<object> newcopy = new GH_Structure<object>();
                IGH_Structure newcopy = new GH_Structure <Grasshopper.Kernel.Types.GH_Brep> ();
                newcopy = data;

                // Cleanup!
                newdoc.Enabled = false;
                newdoc.RemoveObject(thiscluster, false);
                newdoc.Dispose();
                newdoc = null;



 //               Brep temp;
//                paramOut.CastTo<Brep>(out temp);


//                debugText += "instanceguid = " + paramOut.InstanceGuid;
//                debugText += "datamappingk = " + paramOut.DataMapping;
//                debugText += paramOut.SubCategory;

	            DA.SetData(2, debugText);

                DA.SetDataTree(0, copy); //new Rhino.Geometry.Circle(4.3));
                DA.SetDataTree(1, copy);





            }
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
                //return Resources.IconForThisComponent;
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
            get { return new Guid("{99170264-7e33-48c9-81b9-33d56842aaec}"); }
        }
    }
}
