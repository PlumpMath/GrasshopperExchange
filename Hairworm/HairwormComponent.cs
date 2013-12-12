using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;


using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Hairworm
{

    public class HairwormComponent : GH_Component, IGH_VariableParameterComponent
    {

		public int clusterParamNumInput = 0;
		public int clusterParamNumOutput = 0;
		private int fixedParamNumInput = 2;
		private int fixedParamNumOutput = 1;
  //      HairwormComponent self = new HairwormComponent();

		string clusterFileUrl = null;
		bool downloadCluster = false;

		#region Methods of GH_Component interface
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
            pManager.AddTextParameter("String", "ClusterURL", "URL To Cluster", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Download", "Download", "Download clsuter", GH_ParamAccess.item, false);
//            pManager.AddNumberParameter("Input Value", "InputVal", "InputValue", GH_ParamAccess.item);
//            pManager[0].Optional = true;
            //            pManager.AddGeometryParameter("Input Geometry", "InputGeo", "InputGeometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
//            pManager.AddGeometryParameter("Output Geometry", "OutputGe32o", "OutputGeometry", GH_ParamAccess.tree);
//            pManager.AddGenericParameter("Generic Output", "GenericOutput", "GenericOutput", GH_ParamAccess.tree);
            pManager.AddTextParameter("Debug", "Debug", "This is debug output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 2. Retrieve input data, exit if non-existent
            if (!DA.GetData(0, ref clusterFileUrl)) { return; }
            if (!DA.GetData(1, ref downloadCluster)) { return; }


            // find out number of additional parameters and assign input data
            if (Params.Input.Count == (fixedParamNumInput + clusterParamNumInput))
            {
				// Assign input params
                for (int i = fixedParamNumInput; i < (fixedParamNumInput + clusterParamNumInput); i++)
                {
					if (!DA.GetData(1, ref downloadCluster)) { return; }
                }
            }

            radius = 3.0; // blah.Value;

            //temporary fire url
            //            clusterFileUrl = "https://github.com/provolot/GrasshopperExchange/raw/master/Hairworm/_example_files/SphereMakerVariable.ghcluster";

            // get temp. get filename.
            string tempPath = System.IO.Path.GetTempPath();
            Uri uri = new Uri(clusterFileUrl);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);

            string debugText = "";
            debugText += "client.downloadfile( " + clusterFileUrl + ", " + filename + " );\n";
            debugText += tempPath;

            DA.SetData(0, debugText);

            // If the retrieved data is Nothing, we need to abort.
            if (clusterFileUrl == null) { return; }
/*
            // attempt to downloadCluster file
            if (downloadCluster)
            {
                using (WebClient Client = new WebClient())
                {
                    Client.DownloadFile(clusterFileUrl, tempPath + filename);
                }
            }
*/
            // if gh file doesn't exist, abort 
            if (!File.Exists(tempPath + filename)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File does not exist!"); return; }

            // create a cluster
            GH_Cluster thiscluster = new GH_Cluster();
            thiscluster.CreateFromFilePath(tempPath + filename);

            thiscluster.Params.Input[0].AddVolatileData(new GH_Path(0), 0, radius);
            debugText += "\ninputtypename = " + thiscluster.Params.Input[0].TypeName;

			clusterParamNumInput = thiscluster.Params.Input.Count;
			clusterParamNumOutput = thiscluster.Params.Output.Count;
            debugText += "\ncluster input params # = " + clusterParamNumInput;
            debugText += "\ncluster output params # = " + clusterParamNumOutput;

            //GH_Param temptype = new IGH_Param();


            //get new document, enable it, and add cluster to it
            GH_Document newdoc = new GH_Document();
            newdoc.Enabled = true;
            newdoc.AddObject(thiscluster, true, 0);

            debugText += "\nradisu = " + radius;
            debugText += "\noutputcount = " + thiscluster.Params.Output.Count;
            DA.SetData(0, debugText);

            // Get a pointer to the data inside the first cluster output.
            IGH_Structure data = thiscluster.Params.Output[0].VolatileData;

            // Create a copy of this data (the original data will be wiped)
            DataTree<object> copy = new DataTree<object>();
            copy.MergeStructure(data, new Grasshopper.Kernel.Parameters.Hints.GH_NullHint());

            // Cleanup!
            newdoc.Enabled = false;
            newdoc.RemoveObject(thiscluster, false);
            newdoc.Dispose();
            newdoc = null;


            // Output
            DA.SetData(0, debugText);
//            DA.SetDataTree(0, copy); //new Rhino.Geometry.Circle(4.3));
//            DA.SetDataTree(1, copy);

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
            get { return new Guid("99170264-7e33-48c9-81b9-33d56842aaec"); }
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new Attributes_Custom(this);
        }

    #endregion

		#region Methods of IGH_VariableParameterComponent interface


        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
          /*  //We only let input parameters to be added (output number is fixed at one)
            if (side == GH_ParameterSide.Input)
            {
                return true;
            }
            else
            {
                return false;
            }*/
			return true;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return true;
            //We can only remove if we have more than the 'fixed' param numbers
            if(side == GH_ParameterSide.Input) {
                if(Params.Input.Count > fixedParamNumInput)
					return true;
				else
					return false;
            } 
            else {
                if(Params.Output.Count > fixedParamNumOutput)
					return true;
				else
					return false;
            }
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            Param_Number param = new Param_Number();

 /*           param.Name = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input);
            param.NickName = param.Name;
            param.Description = "Param" + (Params.Input.Count + 1);
            param.SetPersistentData(0.0); */

            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            //Nothing to do here by the moment
            return true;
        }


        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
			//Nothing to do here by the moment
		}

        public void MatchParameterCount()
        {
            while (clusterParamNumOutput != (Params.Output.Count - fixedParamNumOutput))
            {
                if (clusterParamNumOutput > (Params.Output.Count - fixedParamNumOutput))
                    Params.RegisterOutputParam(new Param_GenericObject());
				else
                    Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
            }
            while (clusterParamNumInput != (Params.Input.Count - fixedParamNumInput))
            {
                if (clusterParamNumInput > (Params.Input.Count - fixedParamNumInput))
                    Params.RegisterInputParam(new Param_GenericObject());
				else
                    Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1]);
            }
            this.ExpireSolution(true);
        }

        #endregion
    }
    #region GH_ComponentAttributes interface

    public class Attributes_Custom : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        GH_Component thisowner = null;
        public Attributes_Custom(GH_Component owner) : base(owner) { thisowner = owner; }

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
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Param RRERefresh", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
        }
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    MessageBox.Show("The button was clicked, and we want " + (base.Owner as HairwormComponent).clusterParamNumInput + " inputs and " + (base.Owner as HairwormComponent).clusterParamNumOutput + " output params", "Button", MessageBoxButtons.OK);
                    (base.Owner as HairwormComponent).MatchParameterCount();

                    return GH_ObjectResponse.Handled;

                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
    #endregion GH_ComponentAttributes interface
}
