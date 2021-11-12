using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Setup.Model
{
    public class InstallParameters
    {
        public string InstallPath { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleSecretId { get; set; }
        public int IFCFileVersion { get; set; }
        public string RevitVersion { get; set; }
        public string RevitPath { get; set; }
        public bool ExportRevitSetProperties { get; set; }
        public bool ExportIFCCommonPropertiesSet { get; set; }

        public bool ExportElementsFrom2DPlanView { get; set; }

        public bool ExportLinkedFilesAsSeparateIFCs { get; set; }

        public bool ExportOnlyTheElementsVisibleInTheView { get; set; }
        public bool ExportBaseQuantities { get; set; }
        public string LevelOfDetail { get; set; }


        public static int GetIFCVersionId(string iFCVersion)
        {
            switch (iFCVersion)
            {
                case "IFC 2X3 Coordination View":
                    return 10;
                case "IFC 2X3 Coordination View 2.0":
                    return 21;
                case "IFC 2X3 Basic FM Handover View":
                    return 27;
                case "IFC 2X3 Singapore BCA e-Plan Check":
                    return 8;
                case "IFC 2X3 COBie 2.4 Design Deliverable View":
                    return 17;
                case "IFV 4 Reference View":
                    return 25;
                case "IFC 4 Design Transfer View":
                    return 26;
                default:
                    return 0;
            }
        }
        public static string GetIFCVersionName(int iFCVersion)
        {
            switch (iFCVersion)
            {
                case 10 :
                    return "IFC 2X3 Coordination View";
                case 21 :
                    return "IFC 2X3 Coordination View 2.0";
                case 27 :
                    return "IFC 2X3 Basic FM Handover View";
                case 8:
                    return "IFC 2X3 Singapore BCA e-Plan Check";
                case 17:
                    return "IFC 2X3 COBie 2.4 Design Deliverable View";
                case 25:
                    return "IFV 4 Reference View";
                case 26:
                    return "IFC 4 Design Transfer View";
                default:
                    return "";
            }
        }
    }
}
