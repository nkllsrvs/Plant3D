using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Plant3D.Classes
{
    public class DocumentInfo: IEnumerable
    {
        public string Document { get; set; }
        public List<DocumentObject> DocumentObjects { get; set; }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
