using UnityEngine;

/* Will only be used in a static list,
* and therefore need not be serialized.
* Should consider creating classes for
* this at some point.
*/
public struct RootHash
{
    public float a;
    public float b;
    public float c;
    public float d;
    public float e;

    public static RootHash Create()
    {
        RootHash rootHash;
        // Multiply by 0.999f to clamp between 0 and 1
        rootHash.a = Random.value * 0.999f;
        rootHash.b = Random.value * 0.999f;
        rootHash.c = Random.value * 0.999f;
        rootHash.d = Random.value * 0.999f;
        rootHash.e = Random.value * 0.999f;
        return rootHash;
    }
}
