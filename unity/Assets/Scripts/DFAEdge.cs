using UnityEngine;
using UnityEngine.UI;

namespace DFAGraph
{
    public class DFAEdge : MonoBehaviour
    {
        public DFANode child;
        public DFANode parent;
        private string colorName;
        /**
        * This method gets the color of the dfa edge
        * 
        * @param none
        */
        public Color GetColor()
        {
            return this.GetComponent<Image>().color;
        }
        /**
        * This method gets the color of the edge
        * 
        * @param 
        */
        public string GetColorStr()
        {
            return colorName;
        }
        /**
        * This method sets the color
        * 
        * @param 
        */
        public void SetColor(Color value, string strName)
        {
            this.GetComponent<Image>().color = value;
            this.colorName = strName;
        }

    }



}

