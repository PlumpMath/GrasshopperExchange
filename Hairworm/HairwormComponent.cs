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
		private int fixedParamNumInput = 1;
		private int fixedParamNumOutput = 1;

        const string HairwormBaseName = "Hairworm";
		string HairwormClusterName = null; //name of the parasite cluster
		string HairwormClusterNickName = ""; //nickname of the parasite cluster

		string clusterFileUrl = null;
		string fullTempFilePath = null;
        string loadedClusterFileUrl = null;

		string debugText = "";
        GH_ObjectWrapper[] clusterInputs = null;
//        GH_Number[] clusterInputs = null;

        GH_Cluster wormCluster = null;
        GH_Document wormDoc = null;

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
                "Hairworm", "Hairworm")
        {
        }

        public override void RemovedFromDocument(GH_Document document)
        {
			// let's be polite and pick up our garbage
            if (wormDoc != null)
            {
                wormDoc.Enabled = false;
                wormDoc.RemoveObject(wormCluster, false);
                wormDoc.Dispose();
                wormDoc = null;
            }
            base.RemovedFromDocument(document);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("String", "ClusterURL", "URL To Cluster", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Debug", "Debug", "This is debug output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

			////////////////////////
            //  Retrieve crucial (fixed) input data, exit if non-existent
			////////////////////////

            if (!DA.GetData(0, ref clusterFileUrl)) { return; }


			////////////////////////
            // check if cluster was properly loaded, and if parameters are correct
			// and if not, do something about it!
			////////////////////////

            if (loadedClusterFileUrl == null ||
				loadedClusterFileUrl != clusterFileUrl)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cluster not loaded properly - click on 'Reload Cluster' button!");
//                MessageBox.Show("hey, don't we have a parameter mismatch?");
                //we've got a parameter mismatch
                // urge user to click on buttom to match paramcount to cluster param count
/*                (this.m_attributes as Attributes_Custom).button.Palette = GH_Palette.Pink;
                (this.m_attributes as Attributes_Custom).ExpireLayout();
                (this.m_attributes as Attributes_Custom).PerformLayout(); 
				NEXT STEP - HIGHLIGHT BUTTON */

            }
            else
            {
                //successful! parameters match. so - let's run this thing:

                //get data from hairworm inputs, put into array
                for (int i = fixedParamNumInput; i < (fixedParamNumInput + clusterParamNumInput); i++)
                {
                    if (!DA.GetData(i, ref clusterInputs[i - fixedParamNumInput])) { return; }
                    debugText += "okay, input # " + i + " is: " + clusterInputs[i - fixedParamNumInput].ToString() + "\n";
					DA.SetData(0, debugText);
                }

                // get data from array, input into cluster
                for (int i = fixedParamNumInput; i < (fixedParamNumInput + clusterParamNumInput); i++)
                {
                    wormCluster.Params.Input[i - fixedParamNumInput].AddVolatileData(new GH_Path(0), 0, clusterInputs[i - fixedParamNumInput]);
                }

				// RUN CLUSTER AND RECOMPUTE THIS 
                wormCluster.ExpireSolution(true);
					
                // get computed data from cluster outputs, push into hairworm outputs
                for (int i = fixedParamNumOutput; i < (fixedParamNumOutput + clusterParamNumOutput); i++)
                {
					// Create a copy of this data (the original data will be wiped)
					DataTree<object> copy = new DataTree<object>();
					copy.MergeStructure(wormCluster.Params.Output[i - fixedParamNumOutput].VolatileData, new Grasshopper.Kernel.Parameters.Hints.GH_NullHint());

					// push into hairworm component outputs
					DA.SetDataTree(i, copy);
                }



				DA.SetData(0, debugText);

            }

            DA.SetData(0, debugText);

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

        public override void CreateAttributes()
        {
            base.m_attributes = new Attributes_Custom(this);
        }

    #endregion

		#region Methods of IGH_VariableParameterComponent interface


        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
			// we want this to be false, because we don't want those pesky users adding their own parameters
			// (but what if a cluster is a variable-input one?)
			// well, we'll deal with that later.
			return false;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
			// see above.
            return false;
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            Param_GenericObject param = new Param_GenericObject();

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
			//OKAY, ONLY if we're loading a new cluster altogether, or if the parameter number differs,
           //  we have to remove all parameters, because the data type may change.
			// we want to remove all but fixedParamNumOutput / fixedParamNumInput
			// so we just want to keep on iterating as long as Params.Input.Count > fixedParamNumInput, etc.

            if (clusterFileUrl != loadedClusterFileUrl ||
				Params.Input.Count != fixedParamNumInput + clusterParamNumInput ||
				Params.Output.Count != fixedParamNumOutput + clusterParamNumOutput)
            {

                while (Params.Input.Count > fixedParamNumInput)
                {
                    // delete the last one
                    Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1]);
                }
                while (Params.Output.Count > fixedParamNumOutput)
                {
                    // delete the last one
                    Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
                }

				// now, we should add as many input/output params as we need.
				for (int i = fixedParamNumInput; i < fixedParamNumInput + clusterParamNumInput; i++)
				{
						// even though this is generic, somehow it magically synces up with the type of the cluster type. huh.
						Params.RegisterInputParam(new Param_GenericObject());
				}
				for (int i = fixedParamNumOutput; i < fixedParamNumOutput + clusterParamNumOutput; i++)
				{
						// even though this is generic, somehow it magically synces up with the type of the cluster type. huh.
						Params.RegisterOutputParam(new Param_GenericObject());
				}

				//instantiate an array to hold the values of cluster inputs, since  we have to size it based on.. well, the number of cluster inputs
				clusterInputs = new GH_ObjectWrapper[clusterParamNumInput];

            }

			// we should do this regardless, since the names of inputs could have changed.

			// detect cluster input names and set hairworm input names
			for (int i = 0; i < clusterParamNumInput; i++)
			{
				Params.Input[i + fixedParamNumInput].Name = wormCluster.Params.Input[i].Name;
				Params.Input[i + fixedParamNumInput].NickName = wormCluster.Params.Input[i].NickName;
				Params.Input[i + fixedParamNumInput].Optional = wormCluster.Params.Input[i].Optional;
			}

			// detect cluster output names and set hairworm output names
			for (int i = 0; i < clusterParamNumOutput; i++)
			{
				Params.Output[i + fixedParamNumOutput].Name = wormCluster.Params.Output[i].Name;
				Params.Output[i + fixedParamNumOutput].NickName = wormCluster.Params.Output[i].NickName;
				Params.Output[i + fixedParamNumOutput].Optional = wormCluster.Params.Output[i].Optional;
			}

            //refresh parameters! since they changed.
            Params.OnParametersChanged();
        }

        public void InitCluster()
        {
            debugText = "";
			
			// if we had a previous document, then let's delete it and start over
            if (wormDoc != null)
            {
                wormDoc.Enabled = false;
                wormDoc.RemoveObject(wormCluster, false);
                wormDoc.Dispose();
                wormDoc = null;
            }

			////////////////////////
            // get clusterFileURL param again, since inputs may not have run if invalid
			////////////////////////
            clusterFileUrl = Params.Input[0].VolatileData.get_Branch(0)[0].ToString();

			////////////////////////
            // set path for temporary file location
			////////////////////////
            string tempPath = System.IO.Path.GetTempPath();
            Uri uri = new Uri(clusterFileUrl);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            fullTempFilePath = tempPath + filename; 

			////////////////////////
            // attempt to downloadCluster file
			////////////////////////
  
			using (WebClient Client = new WebClient())
			{
				try {
					Client.DownloadFile(clusterFileUrl, fullTempFilePath);
                }
				catch(WebException webEx)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Network error: " + webEx.Message);

                }
			}
			debugText += "Downloaded file " + clusterFileUrl + ", " + filename + "\n";
            debugText += "into " + fullTempFilePath + "\n";

            // if gh file doesn't exist in temporary location, abort 
            if (!File.Exists(fullTempFilePath)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File does not exist!"); }

			////////////////////////
            // Create a cluster
			////////////////////////

			// create cluster
            wormCluster = new GH_Cluster();
            wormCluster.CreateFromFilePath(fullTempFilePath);

			// set cluster parameter count
            clusterParamNumInput = wormCluster.Params.Input.Count;
			clusterParamNumOutput = wormCluster.Params.Output.Count;
            debugText += "\ncluster input params # = " + clusterParamNumInput;
            debugText += "\ncluster output params # = " + clusterParamNumOutput;

			// add/remove/rename parameters to match cluster parameter count.
            MatchParameterCount();

			// change hairworm name to match cluster name
			if(wormCluster.Name == "Cluster") {
				HairwormClusterName = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
                HairwormClusterNickName = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
            } else {
				HairwormClusterName = wormCluster.Name;
                HairwormClusterNickName = wormCluster.NickName;
            }
            Name = HairwormBaseName + " (" + HairwormClusterName + ")";
            NickName = HairwormBaseName + " (" + this.HairwormClusterNickName + ")";
			debugText += "cluster is named = " + wormCluster.Name;
			debugText += "cluster is nicknamed = " + wormCluster.NickName;

			//get new document, enable it, and add cluster to it
			wormDoc = new GH_Document();
			wormDoc.Enabled = true;
			wormDoc.AddObject(wormCluster, true, 0);

			// loading cluster worked. (it's important that this is almost last, because MatchParameterCount scans this to know when to disconnect params)
            loadedClusterFileUrl = clusterFileUrl;

            ExpireSolution(true);

        }


        #endregion
    }
    #region GH_ComponentAttributes interface

    public class Attributes_Custom : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        GH_Component thisowner = null;
        public Attributes_Custom(GH_Component owner) : base(owner) { thisowner = owner; }
        public GH_Capsule button = null;

        protected override void Layout()
        {
            base.Layout();

			// Draw a button 
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
                //GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Reload Cluster", 2, 0);
                button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Reload Cluster", 2, 0);
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
                    (base.Owner as HairwormComponent).InitCluster();

                    //MessageBox.Show("The button was clicked, and we want " + (base.Owner as HairwormComponent).clusterParamNumInput + " inputs and " + (base.Owner as HairwormComponent).clusterParamNumOutput + " output params", "Button", MessageBoxButtons.OK);

                    return GH_ObjectResponse.Handled;

                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
    #endregion GH_ComponentAttributes interface
}
