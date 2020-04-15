using UnityEngine;
using RootUtils.Randomization;

public class FeatureContainer : MonoBehaviour
{

// _featureThresholds represents different thresholds for
// different levels of development of a given feature. For a
// given featureThresholds[n] the appearance of more pronounced
// features increases as n decreases. Specifically,
// _featureThresholds represents a range of values for which
// certain features will be selected. For _featureThresholds[2],
// the range of the most developed feature appearing is between
// 0.4f and 0.6f.
    private static float[][] _featureThresholds = {
        new float[] { 0.0f, 0.0f, 0.4f},
        new float[] { 0.0f, 0.4f, 0.6f},
        new float[] { 0.4f, 0.6f, 0.8f}
    };

    public FeatureCollection[] urbanCollections;
    public FeatureCollection[] farmCollections;
    public FeatureCollection[] plantCollections;

    public Transform[] special;

    public HexMesh walls;
    public Transform wallTower;
    public Transform bridge;    

    private Transform _container;

    public static FeatureContainer GetFeatureContainer(HexMesh walls) {
        GameObject resultObj = new GameObject("Feature Container");
        FeatureContainer resultMono = resultObj.AddComponent<FeatureContainer>();
        resultMono.urbanCollections = new FeatureCollection[3];
        resultMono.farmCollections = new FeatureCollection[3];
        resultMono.plantCollections = new FeatureCollection[3];
        resultMono.special = new Transform[3];
        resultMono.walls = walls;

        resultMono.wallTower = Resources.Load<Transform>("Wall Tower");
        resultMono.bridge = Resources.Load<Transform>("Bridge");
        
        resultMono.urbanCollections[0] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Urban High 1"),
                Resources.Load<Transform>("Urban High 2")
            }
        );

        resultMono.urbanCollections[1] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Urban Medium 1"),
                Resources.Load<Transform>("Urban Medium 2")
            }
        );

        resultMono.urbanCollections[2] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Urban Low 1"),
                Resources.Load<Transform>("Urban Low 2")
            }
        );

        resultMono.farmCollections[0] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Farm High 1"),
                Resources.Load<Transform>("Farm High 2")
            }
        );

        resultMono.farmCollections[1] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Farm Medium 1"),
                Resources.Load<Transform>("Farm Medium 2")
            }
        );

        resultMono.farmCollections[2] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Farm Low 1"),
                Resources.Load<Transform>("Farm Low 2")
            }
        );

        resultMono.plantCollections[0] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Plant High 1"),
                Resources.Load<Transform>("Plant High 2")
            }
        );

        resultMono.plantCollections[1] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Plant Medium 1"),
                Resources.Load<Transform>("Plant Medium 2")
            }
        );

        resultMono.plantCollections[2] = new FeatureCollection(
            new Transform[] {
                Resources.Load<Transform>("Plant Low 1"),
                Resources.Load<Transform>("Plant Low 2")
            }
        );

        resultMono.special[0] = Resources.Load<Transform>("Castle");
        resultMono.special[1] = Resources.Load<Transform>("Ziggurat");
        resultMono.special[2] = Resources.Load<Transform>("Megaflora");

        return resultMono;
    }

    public void Clear() {
        if (_container) {
            Destroy(_container.gameObject);
        }
        _container = new GameObject("Features Container").transform;
        _container.SetParent(transform, false);
        walls.Clear();
    }

    public void Apply() {
        walls.Apply();
    }

    public void AddFeature(
        HexCell cell,
        Vector3 position,
        float cellOuterRadius
    ) {

        if (cell.IsSpecial) {
            return;
        }

/* Randomness of rotation is obtained by sampling a Hash
* World rather than using Random, so the rotation of objects
* will not be changed when the cell is refreshed.
*/

        RandomHash randomHash = HexagonPoint.SampleHashGrid(position);
        float a = randomHash.GetValue(0);
        float b = randomHash.GetValue(1);
        float c = randomHash.GetValue(2);
        float d = randomHash.GetValue(3);
        float e = randomHash.GetValue(4);

        Transform prefab = PickPrefab(
            urbanCollections, cell.UrbanLevel, a, d
        );

        Transform otherPrefab = PickPrefab(
            farmCollections, cell.FarmLevel, b, d
        );

        float usedHash = randomHash.GetValue(0);
        if (prefab) {
            if (otherPrefab && b < a) {
                prefab = otherPrefab;
                usedHash = b;
            }
        }
        else if (otherPrefab) {
            prefab = otherPrefab;
            usedHash = b;
        }

        otherPrefab = PickPrefab (
            plantCollections, cell.PlantLevel, c, d
        );

        if (prefab) {
            if (otherPrefab && c < usedHash) {
                prefab = otherPrefab;
            }
        }
        else if (otherPrefab) {
            prefab = otherPrefab;
        }
        else {
            return;
        }

        if (!prefab) {
            return;
        }

        Transform instance = Instantiate(prefab);

        instance.localPosition = HexagonPoint.Perturb(position, cellOuterRadius);
        instance.localRotation = Quaternion.Euler(0f, 360f * e, 0f);

        instance.SetParent(_container, false);
    }

    private Transform PickPrefab (
        FeatureCollection[] collection, 
        int level, 
        float hash, 
        float choice
    ) {
        if (level > 0) {
            float[] thresholds = GetFeatureThresholds(level - 1);

            for (int i = 0; i < thresholds.Length; i++) {
                if (hash < thresholds[i]) {
                    return collection[i].Pick(choice);
                }
            }
        }

        return null;
    }

    public void AddWall(
        EdgeVertices near, HexCell nearCell,
        EdgeVertices far, HexCell farCell,
        bool hasRiver, bool hasRoad,
        float cellOuterRadius
    ) {
        if  (
            nearCell.HasWalls != farCell.HasWalls &&
            !nearCell.IsUnderwater && !farCell.IsUnderwater &&
            nearCell.GetEdgeType(farCell) != EdgeType.Cliff
        ) {
            AddWallSegment(
                near.vertex1,
                far.vertex1,
                near.vertex2,
                far.vertex2,
                cellOuterRadius
            );

            if (hasRiver || hasRoad) {
                AddWallCap(
                    near.vertex2,
                    far.vertex2,
                    cellOuterRadius
                );

                AddWallCap(
                    far.vertex4,
                    near.vertex4,
                    cellOuterRadius
                );
            }
            else {
                AddWallSegment(
                    near.vertex2,
                    far.vertex2,
                    near.vertex3,
                    far.vertex3,
                    cellOuterRadius
                );

                AddWallSegment(
                    near.vertex3,
                    far.vertex3,
                    near.vertex4,
                    far.vertex4,
                    cellOuterRadius
                );
            }

            AddWallSegment(
                near.vertex4,
                far.vertex4,
                near.vertex5,
                far.vertex5,
                cellOuterRadius
            );
        }
    }

    public void AddWall(
        Vector3 corner1, HexCell cell1,
        Vector3 corner2, HexCell cell2,
        Vector3 corner3, HexCell cell3,
        float cellOuterRadius
    ) {
        if (cell1.HasWalls) {
            if (cell2.HasWalls) {
                if (!cell3.HasWalls) {
                    AddWallSegment(
                        corner3,
                        cell3,
                        corner1,
                        cell1,
                        corner2,
                        cell2,
                        cellOuterRadius
                    );
                }
            }
            else if (cell3.HasWalls) {
                AddWallSegment(
                    corner2,
                    cell2,
                    corner3,
                    cell3,
                    corner1,
                    cell1,
                    cellOuterRadius
                );
            }
            else {
                AddWallSegment(
                    corner1,
                    cell1,
                    corner2,
                    cell2,
                    corner3,
                    cell3,
                    cellOuterRadius
                );
            }
        }
        else if (cell2.HasWalls) {
            if (cell3.HasWalls) {
                AddWallSegment(
                    corner1,
                    cell1,
                    corner2,
                    cell2,
                    corner3,
                    cell3,
                    cellOuterRadius
                );
            }
            else {
                AddWallSegment(
                    corner2,
                    cell2,
                    corner3,
                    cell3,
                    corner1,
                    cell1,
                    cellOuterRadius
                );
            }
        }
        else if (cell3.HasWalls) {
            AddWallSegment(
                corner3,
                cell3,
                corner1,
                cell1,
                corner2,
                cell2,
                cellOuterRadius
            );
        }
    }


    private void AddWallSegment(
        Vector3 nearLeft,
        Vector3 farLeft,
        Vector3 nearRight,
        Vector3 farRight,
        float cellOuterRadius,
        bool addTower = false
    ) {
        nearLeft = HexagonPoint.Perturb(nearLeft, cellOuterRadius);
        farLeft = HexagonPoint.Perturb(farLeft, cellOuterRadius);
        nearRight = HexagonPoint.Perturb(nearRight, cellOuterRadius);
        farRight = HexagonPoint.Perturb(farRight, cellOuterRadius);

        Vector3 left = HexagonPoint.WallLerp(nearLeft, farLeft);
        Vector3 right = HexagonPoint.WallLerp(nearRight, farRight);

        Vector3 leftThicknessOffset = 
            HexagonPoint.WallThicknessOffset(nearLeft, farLeft);
        Vector3 rightThicknessOffset = 
            HexagonPoint.WallThicknessOffset(nearRight, farRight);

        float leftTop = left.y + HexagonPoint.wallHeight;
        float rightTop = right.y + HexagonPoint.wallHeight;

        Vector3 vertex1;
        Vector3 vertex2;
        Vector3 vertex3;
        Vector3 vertex4;

        vertex1 = vertex3 = left - leftThicknessOffset;
        vertex2 = vertex4 = right - rightThicknessOffset;
        vertex3.y = leftTop;
        vertex4.y = rightTop;
        walls.AddQuadUnperturbed(vertex1, vertex2, vertex3, vertex4);

        Vector3 top1 = vertex3;
        Vector3 top2 = vertex4;

        vertex1 = vertex3 = left + leftThicknessOffset;
        vertex2 = vertex4 = right + rightThicknessOffset;
        vertex3.y = leftTop;
        vertex4.y = rightTop;
        walls.AddQuadUnperturbed(vertex2, vertex1, vertex4, vertex3);

        walls.AddQuadUnperturbed(top1, top2, vertex3, vertex4);

        if (addTower) {
            Transform towerInstance = Instantiate(wallTower);
            towerInstance.transform.localPosition = (left + right) * 0.5f;
            Vector3 rightDirection = right - left;
            rightDirection.y = 0f;
            towerInstance.transform.right = rightDirection;
            towerInstance.SetParent(_container, false);
        }
    }

    private void AddWallSegment (
        Vector3 pivot, HexCell pivotCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell,
        float cellOuterRadius
    ) {
        if (pivotCell.IsUnderwater) {
            return;
        }

        bool hasLeftWall = !leftCell.IsUnderwater &&
                            pivotCell.GetEdgeType(leftCell) != EdgeType.Cliff;

        bool hasRightWall = !rightCell.IsUnderwater &&
                            pivotCell.GetEdgeType(rightCell) != EdgeType.Cliff;

        if (hasLeftWall) {
            if (hasRightWall) {
                bool hasTower = false;

                if (leftCell.Elevation == rightCell.Elevation) {
                    RandomHash rootHash = HexagonPoint.SampleHashGrid (
                        (pivot + left + right) * 1f / 3f
                    );

                    float e = rootHash.GetValue(4);

                    hasTower = e < HexagonPoint.wallTowerThreshold;
                }

                AddWallSegment(
                    pivot,
                    left,
                    pivot,
                    right,
                    cellOuterRadius,
                    hasTower
                );
            }
            else if (leftCell.Elevation < rightCell.Elevation) {
                AddWallWedge(pivot, left, right, cellOuterRadius);
            }
            else {
                AddWallCap(pivot, left, cellOuterRadius);
            }
        }
        else if (hasRightWall) {
            if (rightCell.Elevation < leftCell.Elevation) {
                AddWallWedge(right, pivot, left, cellOuterRadius);
            }
            else
            {
                AddWallCap(right, pivot, cellOuterRadius);
            }
        }
    }

    private void AddWallCap(
        Vector3 near,
        Vector3 far,
        float cellOuterRadius
    ) {
        near = HexagonPoint.Perturb(near, cellOuterRadius);
        far = HexagonPoint.Perturb(far, cellOuterRadius);

        Vector3 center = HexagonPoint.WallLerp(near, far);
        Vector3 thickness = HexagonPoint.WallThicknessOffset(near, far);

        Vector3 vertex1;
        Vector3 vertex2;
        Vector3 vertex3;
        Vector3 vertex4;

        vertex1 = vertex3 = center - thickness;
        vertex2 = vertex4 = center + thickness;
        vertex3.y = vertex4.y = center.y + HexagonPoint.wallHeight;
        walls.AddQuadUnperturbed(vertex1, vertex2, vertex3, vertex4);
    }

    private void AddWallWedge(
        Vector3 near,
        Vector3 far,
        Vector3 point,
        float cellOuterRadius
    ) {
        near = HexagonPoint.Perturb(near, cellOuterRadius);
        far = HexagonPoint.Perturb(far, cellOuterRadius);
        point = HexagonPoint.Perturb(point, cellOuterRadius);

        Vector3 center = HexagonPoint.WallLerp(near, far);
        Vector3 thickness = HexagonPoint.WallThicknessOffset(near, far);

        Vector3 vertex1;
        Vector3 vertex2;
        Vector3 vertex3;
        Vector3 vertex4;

        Vector3 pointTop = point;
        point.y = center.y;

        vertex1 = vertex3 = center - thickness;
        vertex2 = vertex4 = center + thickness;
        vertex3.y = vertex4.y = pointTop.y = center.y + HexagonPoint.wallHeight;
        
        walls.AddQuadUnperturbed(vertex1, point, vertex3, pointTop);
        walls.AddQuadUnperturbed(point, vertex2, pointTop, vertex4);
        walls.AddTriangleUnperturbed(pointTop, vertex3, vertex4);
    }

    public void AddBridge(
        Vector3 roadCenter1,
        Vector3 roadCenter2,
        float cellOuterRadius
    ) {
        roadCenter1 = HexagonPoint.Perturb(
            roadCenter1,
            cellOuterRadius
        );

        roadCenter2 = HexagonPoint.Perturb(
            roadCenter2,
            cellOuterRadius
        );

        Transform instance = Instantiate(bridge);
        instance.localPosition = (roadCenter1 + roadCenter2) * 0.5f;
        instance.forward = roadCenter2 - roadCenter1;
        float length = Vector3.Distance(roadCenter1, roadCenter2);

        instance.localScale = 
        new Vector3
        (
            1f, 
            1f, 
            length * (1f / HexagonPoint.bridgeDesignLength)
        );

        instance.SetParent(_container, false);
    }

    public void AddSpecialFeature(
        HexCell cell,
        Vector3 position,
        float cellOuterRadius) {
        Transform instance = Instantiate(special[cell.SpecialIndex - 1]);

        instance.localPosition = HexagonPoint.Perturb(
            position,
            cellOuterRadius
        );

        RandomHash rootHash = HexagonPoint.SampleHashGrid(position);
        float e = rootHash.GetValue(4);

        instance.localRotation = Quaternion.Euler(0f, 360f * e, 0f);
        instance.SetParent(_container, false);
    }

    public static float[] GetFeatureThresholds(int level) {
        return _featureThresholds[level];
    }
}
