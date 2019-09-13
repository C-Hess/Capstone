using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MoveSpriteTest
    {
        [UnityTest]
        public IEnumerator UpdatesDestination()
        {

            var movingObj = new GameObject().AddComponent<MoveSprite>();
            movingObj.StartPosition = new Vector2(0, 0);
            movingObj.EndPosition = new Vector2(10, 10);
            movingObj.Speed = 0.0f;
    

            yield return null;

            movingObj.transform.position = movingObj.EndPosition;

            movingObj.UpdateDesition();


            Assert.AreEqual(0.0, Vector2.Distance(movingObj.CurrentGoal, new Vector2(0, 0)), 0.1, "Set to start goal after destination reached");

            movingObj.transform.position = movingObj.StartPosition;

            movingObj.UpdateDesition();

            Assert.AreEqual(0.0, Vector2.Distance(movingObj.CurrentGoal, new Vector2(10, 10)), 0.1, "Set to end position goal after start reached");
        }
    }
}
