using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant3D.Classes
{
    public class Instruments : IEnumerable
    {
        public Instruments() { }
        public Instruments(ObjectId Id, String Layer, ObjectId LayerId)
        {
            this.Id = Id;
            this.Layer = Layer;
            this.LayerId = LayerId;
        }
        public Autodesk.AutoCAD.DatabaseServices.ObjectId Id { get; set; }
        public String Layer { get; set; }
        public Autodesk.AutoCAD.DatabaseServices.ObjectId LayerId { get; set; }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
