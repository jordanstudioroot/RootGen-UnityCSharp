using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootGenExampleApp : MonoBehaviour
{
    public enum GenerationType {
        Standard,
        TwoThreeAlgorithm
    }

    public GenerationType generationType;

    private RootGen _rootGen;
    void Awake() {
        
    }

    void OnEnable() {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        _rootGen = new RootGen();


        if (generationType == GenerationType.Standard) {
            _rootGen.GenerateMap(this, Resources.Load("defaultconfig") as RootGenConfig);
        }
        else if (generationType == GenerationType.TwoThreeAlgorithm) {  
            _rootGen.GenerateHistoricalBoard(75, 75);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) {

            if (generationType == GenerationType.Standard) {
                _rootGen.GenerateMap(this, Resources.Load("defaultconfig") as RootGenConfig);
            }
            else if (generationType == GenerationType.TwoThreeAlgorithm) {  
                _rootGen.GenerateHistoricalBoard(75, 75);
            }
        }
    }
}
