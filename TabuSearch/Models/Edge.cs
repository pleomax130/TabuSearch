using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabuSearch
{
    class Edge
    {
        public int Start { get; set; }
        public int End { get; set; }

        public Edge(int start , int end)
        {
            Start = start;
            End = end;
        }
    }
}
