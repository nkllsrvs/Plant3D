using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Plant3D.Classes
{
    public class DocumentInfo : IEnumerable
    {
        public string Document { get; set; }
        public List<DocumentObject> DocumentObjects { get; set; }
        public List<DocumentObject> DocumentObjectsRT { get; set; }
        public List<DocumentObject> DocumentObjectsFT { get; set; }

        public DocumentInfo(string document)
        {
            this.Document = document;
            this.DocumentObjectsRT = new List<DocumentObject>();
            this.DocumentObjectsFT = new List<DocumentObject>();
            this.DocumentObjects = new List<DocumentObject>();
        }

        public DocumentInfo()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            DocumentInfo objAsDocumentInfo = obj as DocumentInfo;
            if (objAsDocumentInfo == null)
                return false;
            else
                return Equals(objAsDocumentInfo.Document);
        }
        public bool Equals(DocumentInfo other)
        {
            if (other == null) return false;
            return (this.Document.Equals(other.Document));
        }
        public override int GetHashCode()
        {
            int hashCode = 1256543236;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Document);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<DocumentObject>>.Default.GetHashCode(DocumentObjects);
            return hashCode;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
