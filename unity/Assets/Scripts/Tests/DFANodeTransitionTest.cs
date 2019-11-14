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
            var nextNode = node1.NextNode("red");

            yield return null;
            Assert.AreEqual(nextNode, null);



        }
    }
}
