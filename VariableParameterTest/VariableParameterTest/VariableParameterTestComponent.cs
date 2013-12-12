using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;

namespace VariableParameterTest
{
    public class VariableParameterTestComponent : GH_Component, IGH_VariableParameterComponent
    {

        public int outputparamno = 0;
        public int inputparamno = 0;

        #region Methods of GH_Component interface

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VariableParameterTestComponent()
            : base("VariableParameterTest", "Nickname",
                "Description",
                "Category", "Subcategory")
        {
        }

        public override void CreateAttributes()
        {
            base.m_attributes = new Attributes_Custom(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("OutputParamNo", "OutputParamNo", "OutputParamNo", GH_ParamAccess.item);
            pManager.AddIntegerParameter("InputParamNo", "InputParamNo", "InputParamNo", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
		    if (!DA.GetData(0, ref inputparamno)) { return; }
		    if (!DA.GetData(1, ref outputparamno)) { return; }
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
            get { return new Guid("{fdc25843-e26e-4c2d-b985-fd7fc13ed282}"); }
        }


        #endregion Methods of GH_Component interface



        #region Methods of IGH_VariableParameterComponent interface


        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
                return true;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
			//leave two inputs
            if (side == GH_ParameterSide.Input) 
            {
                if (Params.Input.Count > 2)
                    return true;
                else
                    return false;
            }
            else
            {
                if (Params.Output.Count > 0)
                    return true;
                else
                    return false;
            }
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {

            Param_Number param = new Param_Number();
            param.Name = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input);
            param.NickName = param.Name;
            param.Description = "Param" + (Params.Output.Count + 1);
            param.SetPersistentData(0.0);

            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            //Nothing to do here by the moment
            return true;
        }


        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
        }

        public void MatchParameterCount() {
            while (outputparamno != Params.Output.Count)
            {
                if (outputparamno > Params.Output.Count)
                {
                    Params.RegisterOutputParam(new Param_GenericObject());
                }
                if (outputparamno < Params.Output.Count)
                {
                    Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
                }
            }
            this.OnAttributesChanged();
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
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Param Refresh", 2, 0);
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
                    MessageBox.Show("The button was clicked, and we want " + (base.Owner as VariableParameterTestComponent).outputparamno + " output params", "Button", MessageBoxButtons.OK);
                    (base.Owner as VariableParameterTestComponent).MatchParameterCount();

                    return GH_ObjectResponse.Handled;
					
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
	#endregion GH_ComponentAttributes interface
}
