using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabuSearch
{
    class TabuElement
    {
        public int Cadency { get; set; }
        public Edge Edge { get; set; }

        public TabuElement(int cadency, Edge edge)
        {
            Cadency = cadency;
            Edge = edge;
        }
    }
}
