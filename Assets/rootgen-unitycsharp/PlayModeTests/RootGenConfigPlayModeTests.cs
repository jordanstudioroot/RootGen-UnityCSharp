using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class RootGenConfigPlayModeTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void RootGenConfigTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        [Test]
        public void ToJson_DefaultConfig_FileExists() {
            ScriptableObject.CreateInstance<RootGenConfig>();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator RootGenConfigTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
