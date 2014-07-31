using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Catalogs;
using Tekla.Structures;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;

using TSD = Tekla.Structures.Datatype;
using Point = Tekla.Structures.Geometry3d.Point;

namespace HollowCoreSlabFitting
{
    public class StructuresData
    {
        #region Fields

        [Tekla.Structures.Plugins.StructuresField("P1")]
        public double P1;

        #endregion
    }

    [Plugin("HollowCoreSlabFitting")]
    [PluginUserInterface(HollowCoreSlabFittingUI.HollowCoreSlabFitting)]
    [SecondaryType(ConnectionBase.SecondaryType.SECONDARYTYPE_ONE)]
    [AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_GLOBAL_Z)]
    [PositionType(PositionTypeEnum.BOX_PLANE)]
    public class DeltaModelPlugin : ConnectionBase
    {
        #region Fields
        private StructuresData _data;

        private Model _Model;

        private double FittingGap = 0.0;
        private string _PosAtdepthAttribute = string.Empty;

        //Constructor
        public DeltaModelPlugin(StructuresData data)
        {
            this._data = data;
            _Model = new Model();
        }

        #endregion

        #region Overrides

        private static bool CreateFittings(Beam targetBeam, double customGap)
        {
            bool Result = false;

            Fitting fitBeam = new Fitting();
            fitBeam.Plane = new Plane();
            fitBeam.Plane.Origin = new Point(customGap, -160, -500);
            fitBeam.Plane.AxisX = new Vector(0.0, 500.0, 0.0);
            fitBeam.Plane.AxisY = new Vector(0.0, 0.0, 1200.0);
            fitBeam.Father = targetBeam;

            if (fitBeam.Insert())
                Result = true;

            return Result;
        }
  
        public override bool Run()
        {
            bool Result = false;
            double deltaBB_side = 0.0;

            try
            {
                FittingGap = _data.P1;
                //Get primary and secondary
                Beam PrimaryBeam = _Model.SelectModelObject(Primary) as Beam;
                Beam SecondaryBeam = _Model.SelectModelObject(Secondaries[0]) as Beam;
                if (PrimaryBeam != null && SecondaryBeam != null)
                {
                    TransformationPlane originalTransformationPlane = _Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                    CoordinateSystem coordSys = PrimaryBeam.GetCoordinateSystem();

                    Matrix ToThisPlane = _Model.GetWorkPlaneHandler().GetCurrentTransformationPlane().TransformationMatrixToGlobal;

                    //Translaatiot molemmille pisteille
                    Point joint_point1 = ToThisPlane.Transform(new Point(0, 0, 0));
                    Point joint_point2 = ToThisPlane.Transform(new Point(1, 0, 0));
                    Vector joint_vector = new Vector(joint_point2 - joint_point1);
                    joint_vector.Normalize();

                    if (_Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(coordSys)))
                    {
                        Matrix ToThisPlane2 = _Model.GetWorkPlaneHandler().GetCurrentTransformationPlane().TransformationMatrixToGlobal;
                        //Translaatiot molemmille pisteille
                        Point primpart_point1 = ToThisPlane2.Transform(new Point(0, 0, 0));
                        Point primpart_point2 = ToThisPlane2.Transform(new Point(0, 0, 1));
                        Vector primpart_vector = new Vector(primpart_point2 - primpart_point1);
                        primpart_vector.Normalize();

                        string ProfileSubtype = "";

                        if (PrimaryBeam.GetReportProperty("PROFILE.SUBTYPE", ref ProfileSubtype))
                            if (ProfileSubtype == "ABEAM")
                            {
                                double ABEAM_c1 = 0.0;
                                double ABEAM_c3 = 0.0;
                                PrimaryBeam.GetReportProperty("PROFILE.c1", ref ABEAM_c1);
                                PrimaryBeam.GetReportProperty("PROFILE.c3", ref ABEAM_c3);
                                deltaBB_side = ABEAM_c3 - ABEAM_c1;
                            }
                            else
                            {
                                if (joint_vector.X * primpart_vector.X > 0)
                                    // hae deltapalkin right-parametrit
                                    PrimaryBeam.GetReportProperty("PROFILE.bb.right", ref deltaBB_side);
                                else if (joint_vector.Y * primpart_vector.Y > 0)
                                    // hae deltapalkin right-parametrit
                                    PrimaryBeam.GetReportProperty("PROFILE.bb.right", ref deltaBB_side);
                                else
                                    // hae deltapalkin left-parametrit
                                    PrimaryBeam.GetReportProperty("PROFILE.bb.left", ref deltaBB_side);
                            }
                        _Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(originalTransformationPlane);
                    }
                    // siirrä yz-tasoa x-suunnassa
                    // luo fitting liitoksen yz-tasossa  
                    CreateFittings(SecondaryBeam, deltaBB_side + FittingGap);
                    Result = true;
                }
            }

            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }

            return Result;
        }
        #endregion
    }
}
