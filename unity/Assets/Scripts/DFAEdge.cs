using UnityEngine;
using UnityEngine.UI;

namespace DFAGraph
{
    public class DFAEdge : MonoBehaviour
    {
        public DFANode child;
        public DFANode parent;
        private string colorName;

        public Color GetColor()
        {
            return this.GetComponent<Image>().color;
        }

        public string GetColorStr()
        {
            return colorName;
        }

        public void SetColor(Color value, string strName)
        {
            this.GetComponent<Image>().color = value;
            this.colorName = strName;
        }

    }



}

