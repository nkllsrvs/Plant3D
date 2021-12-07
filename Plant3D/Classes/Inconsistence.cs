using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant3D.Classes
{
    public class Inconsistence
    {
        public Inconsistence(String pTAG, String pType, String pMessage)
        {
            TAG = pTAG;
            Type = pType;
            Message = pMessage;
        }

        public String TAG { get; set; }
        public string Type { get; set; }

        public string Message { get; set; }
    }
}
