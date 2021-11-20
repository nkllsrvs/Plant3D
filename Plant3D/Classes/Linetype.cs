using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant3D.Classes
{
    public class Linetype : IEnumerable
    {
        public Linetype() { }
        public Linetype(ObjectId Id, String Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
        public Autodesk.AutoCAD.DatabaseServices.ObjectId Id { get; set; }
        public String Name { get; set; }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
