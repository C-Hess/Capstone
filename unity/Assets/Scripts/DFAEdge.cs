using UnityEngine;
using UnityEngine.UI;

namespace DFAGraph
{
    public class DFAEdge : MonoBehaviour
    {
        public DFANode child;
        public DFANode parent;

        public Color Color
        {
            get
            {
                return this.GetComponent<Image>().color;
            }

            set
            {
                this.GetComponent<Image>().color = value;
            }
        }

    }



}

