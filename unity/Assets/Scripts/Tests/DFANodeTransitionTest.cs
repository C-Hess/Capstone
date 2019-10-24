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
        public IEnumerator NextNodeTest()
        {
            var gameObject = new GameObject();
            
            DFANode node1 = gameObject.AddComponent(typeof (DFANode)) as DFANode;
            DFANode node2 = gameObject.AddComponent(typeof(DFANode)) as DFANode;
            DFAEdge edge = gameObject.AddComponent(typeof(DFAEdge)) as DFAEdge;
            edge.SetColor("red");
            edge.parent = node1;
            edge.child = node2;

            yield return null;

            var nextNode = node1.NextNode("red");
            var nullNode = node1.NextNode("red");
           // Assert.AreEqual(nextNode, node2);
            Assert.AreEqual(nullNode, null);
        }
    }
}
