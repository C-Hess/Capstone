using DFAGraph;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class DFANodeTransitionTest
    {
        [UnityTest]
        // Start is called before the first frame update
        public IEnumerator NextNodeTest()
        {
            DFANode node1 = new DFANode();
            DFANode node2 = new DFANode();

            DFAEdge edge = new DFAEdge("red");

            yield return null;

            var nextNode = node1.NextNode("red");
            var nullNode = node1.NextNode("green");
            Assert.AreEqual(nextNode, node2);
            Assert.AreEqual(nullNode, null);

            


        }
    }
}
