using System;
using System.Collections.Generic;
using System.IO;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
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
            pManager.AddTextParameter("String", "ClusterPath", "Path To Cluster", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Activate", "Activate", "Activate to emulate clsuter", GH_ParamAccess.item, false);
            pManager.AddGeometryParameter("Input Geometry", "InputGeo", "InputGeometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Output Geometry", "OutputGeo", "OutputGeometry", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Generic Output", "GenericOutput", "GenericOutput", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Declare a variable for the input String
            string filename = null;
            bool activate = false;
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            // 1. Declare placeholder variables and assign initial invalid data.
            //    This way, if the input parameters fail to supply valid data, we know when to abort.

            // 2. Retrieve input data, exit if non-existent
            if (!DA.GetData(0, ref filename)) { return; }
            if (!DA.GetData(1, ref activate)) { return; }
            if (!DA.GetData(2, ref point)) { return; }

            // If the retrieved data is Nothing, we need to abort.
            if (filename == null) { return; }
            if (!File.Exists(filename)) { return; }

            if (activate)
            {
                GH_Cluster cluster = new GH_Cluster();
                cluster.CreateFromFilePath(filename);

                GH_Document doc = OnPingDocument();
                doc.AddObject(cluster, false);

                Grasshopper.Kernel.Parameters.Param_Point paramIn = new Grasshopper.Kernel.Parameters.Param_Point();
                Grasshopper.Kernel.Parameters.Param_Geometry paramOut = new Grasshopper.Kernel.Parameters.Param_Geometry();

                paramIn.SetPersistentData(point);

                cluster.Params.RegisterInputParam(paramIn);
                cluster.Params.RegisterOutputParam(paramOut);

                cluster.CollectData();

                cluster.ComputeData();


                //Grasshopper.DataTree<object> test = new DataTree<object>();
                //test.Add(paramIn, 0);



                DA.SetData(1, paramOut);







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
