using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DFAGraph {

    public class Wire
    {
        public Color color { get; set; }
        public bool cut = false;

        public Wire(Color col)
        {
            color = col;
        }

    }
}