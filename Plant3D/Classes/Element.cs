using Autodesk.AutoCAD.DatabaseServices;
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
        public Boolean OtherDWG { get; set; }
        public string OtherDWGName { get; set; }
        public Boolean HaveInOtherDocRT { get; set; }
        public Boolean HaveInOtherDocFT { get; set; }
        public string ClassName { get; set; }
        public int RowIdRelated { get; set; }
    }
}

