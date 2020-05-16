using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootLogging;

public class RootGenExampleApp : MonoBehaviour
{
    public int mapSizeX;
    public int mapSizeY;
    public int cellSize;
    private RootGen _rootGen;
    void Awake() {
        
    }

    void OnEnable() {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        _rootGen = new RootGen();
        _rootGen.GenerateEmptyMap(
            new Vector2(mapSizeX, mapSizeY),
            0,
            10,
            true,
            true
        );
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) {
            _rootGen.GenerateEmptyMap(
                new Vector2(mapSizeX, mapSizeY),
                0,
                10,
                true,
                true
            );
        }
    }
}
