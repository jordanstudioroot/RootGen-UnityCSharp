using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootLogging;

public class RootGenExampleApp : MonoBehaviour {
    public RootGenConfig config;
    private RootGen _rootGen;

    private HexMap _hexMap;

    void Awake() {
        _rootGen = new RootGen();
    }

    void OnEnable() {
        
    }

    // Start is called before the first frame update
    void Start() {
        if (config) {
            _hexMap = _rootGen.GenerateMap(config);
            _hexMap.Draw(config.hexSize);
        }
        else {
            _hexMap = GenerateDefaultMap(_rootGen);
            _hexMap.Draw(HexMeshConstants.DEFAULT_HEX_OUTER_RADIUS);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) {
            if (config) {
                _hexMap = _rootGen.GenerateMap(config);
                _hexMap.Draw(config.hexSize);
            }
            else {
                _hexMap = GenerateDefaultMap(_rootGen);
                _hexMap.Draw(
                    HexMeshConstants.DEFAULT_HEX_OUTER_RADIUS
                );
            }
        }
    }

    private HexMap GenerateDefaultMap(RootGen rootGen) {
        return rootGen.GenerateEmptyMap(
            new Vector2(
                HexMeshConstants.CHUNK_SIZE_X * 5,
                HexMeshConstants.CHUNK_SIZE_Z * 4
            ),
            0,
            HexMeshConstants.DEFAULT_HEX_OUTER_RADIUS,
            true,
            true
        );
    }
}
