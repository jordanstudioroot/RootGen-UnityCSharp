using UnityEngine;

[System.Serializable]
public struct Biome {
    [SerializeField]
    public Terrains terrain;
    [SerializeField]
    public int plant;

    public Biome(Terrains terrain, int plant) {
        this.terrain = terrain;
        this.plant = plant;
    }

    public override string ToString() {
        return "Terrain Type: " + terrain + "\n Plant Level: " + plant;
    }
}