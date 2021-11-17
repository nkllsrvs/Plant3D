using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Plant3D.Classes
{
    public class DocumentObject : IEnumerable
    {
        public ObjectId Id { get; set; }
        public string Tag { get; set; }
        public string BelongingDocument { get; set; }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
