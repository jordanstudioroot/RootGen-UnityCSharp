using UnityEngine;

[System.Serializable]
public struct FeatureCollection
{
    public Transform[] prefabs;

    public FeatureCollection(Transform[] prefabs) {
        this.prefabs = prefabs;
    }

/* This method converts the choice value, which is generated
* from a hashed value, into an array index.
*/
    public Transform Pick(float choice)
    {
        return prefabs[(int)(choice * prefabs.Length)];
    }
}

