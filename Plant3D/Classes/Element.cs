using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.ProjectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant3D.Classes
{
    public class Element
    {
        public ObjectId id { get; set; }
        public string TAG { get; set; }
        public string Type { get; set; }
        public string RelatedTo { get; set; }
        public string PipeRunFrom { get; set; }
        public string PipeRunTo { get; set; }
        public Boolean OtherDWGRT { get; set; }
        public Boolean OtherDWGFTTo { get; set; }
        public Boolean OtherDWGFTFrom { get; set; }
        public string DWGNameRelatedTo { get; set; }
        public string DWGNameFromToOrigin { get; set; }
        public string DWGNameFromToDestiny { get; set; }
        public Boolean HaveRT { get; set; }
        public Boolean HaveFTTo { get; set; }
        public Boolean HaveFTFrom { get; set; }
        public string ClassName { get; set; }
        public int RowIdRelated { get; set; }
        public int RowIdRelatedTo { get; set; }
        public int RowIdFromToOrigin { get; set; }
        public int RowIdFromToDestiny { get; set; }

    }
}

