using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Plant3D.Classes
{
    public class DocumentObject : IEnumerable
    {
        public ObjectId Id { get; set; }
        public ObjectId RelatedId { get; set; }
        public string Tag { get; set; }
        public string BelongingDocument { get; set; }
        public String Layer { get; set; }
        public ObjectId LayerId { get; set; }
        public bool RelatedTo { get; set; }
        public bool FromTo { get; set; }
        public bool Equipment { get; set; }
        public bool Instrument { get; set; }
        public bool Line { get; set; }
        public bool FromOtherDWG { get; set; }
        public string OtherDWGDocument { get; set; }

        public DocumentObject()
        {
        }
        public DocumentObject(ObjectId id, string tag, string belongingDocument)
        {
            this.Id = id;
            this.Tag = tag;
            this.BelongingDocument = belongingDocument;
            this.RelatedTo = false;
        }
        public DocumentObject(ObjectId id, string tag, string belongingDocument, String layer, ObjectId layerId)
        {
            this.Id = id;
            this.Tag = tag;
            this.BelongingDocument = belongingDocument;
            this.Layer = layer;
            this.LayerId = layerId;
            this.RelatedTo = false;

        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "ID: " + Id.ToString() + "   Tag: " + Tag + "   Doc: " + BelongingDocument;
        }

        public override int GetHashCode()
        {
            int hashCode = -1633389621;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Tag);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BelongingDocument);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Layer);
            hashCode = hashCode * -1521134295 + LayerId.GetHashCode();
            return hashCode;
        }
        public override bool Equals(object obj)
        {
            return obj is DocumentObject @object &&
                   EqualityComparer<ObjectId>.Default.Equals(Id, @object.Id) &&
                   Tag == @object.Tag &&
                   BelongingDocument == @object.BelongingDocument;
        }
    }
}
