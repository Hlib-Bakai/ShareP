using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShareP.Server
{
    [DataContract]
    public class Presentation
    {
        [DataMember]
        public string Author
        {
            get;
            set;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public int SlidesTotal
        {
            get;
            set;
        }

        [DataMember]
        public int CurrentSlide
        {
            get;
            set;
        }
    }
}
