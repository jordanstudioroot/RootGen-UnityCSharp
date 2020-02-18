using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleApp : MonoBehaviour
{
    void Awake() {
        
    }

    void OnEnable() {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        RootGen.GenerateMap(
            this,
            Resources.Load("defaultconfig") as RootGenConfig
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}