using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootLogging;

public class RootGenExampleApp : MonoBehaviour {
    public RootGenConfig config;
    private RootGen _rootGen;

    void Awake() {
        _rootGen = new RootGen();
    }

    void OnEnable() {
        
    }

    // Start is called before the first frame update
    void Start() {
        if (config) {
            _rootGen.GenerateMap(config, true);
        }
        else {
            GenerateDefaultMap(_rootGen);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) {
            if (config) {
                _rootGen.GenerateMap(config, true);
            }
            else {
                GenerateDefaultMap(_rootGen);
            }
        }
    }

    private void GenerateDefaultMap(RootGen rootGen) {
        rootGen.GenerateEmptyMap(
            new Vector2(
                MeshConstants.ChunkSizeX * 5,
                MeshConstants.ChunkSizeZ * 4
            ),
            0,
            MeshConstants.DefaulthexOuterRadius,
            true,
            true
        );
    }
}
