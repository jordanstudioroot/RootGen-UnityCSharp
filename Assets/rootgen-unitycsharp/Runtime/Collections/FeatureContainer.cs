using UnityEngine;

public class FeatureContainer : MonoBehaviour
{
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

    public void AddFeature(HexCell cell, Vector3 position) {

        if (cell.IsSpecial) {
            return;
        }

/* Randomness of rotation is obtained by sampling a Hash
* World rather than using Random, so the rotation of objects
* will not be changed when the cell is refreshed.
*/

        RootGenHash rootHash = HexMetrics.SampleHashGrid(position);

        Transform prefab = PickPrefab(
            urbanCollections, cell.UrbanLevel, rootHash.a, rootHash.d
        );

        Transform otherPrefab = PickPrefab(
            farmCollections, cell.FarmLevel, rootHash.b, rootHash.d
        );

        float usedHash = rootHash.a;
        if (prefab) {
            if (otherPrefab && rootHash.b < rootHash.a) {
                prefab = otherPrefab;
                usedHash = rootHash.b;
            }
        }
        else if (otherPrefab) {
            prefab = otherPrefab;
            usedHash = rootHash.b;
        }

        otherPrefab = PickPrefab (
            plantCollections, cell.PlantLevel, rootHash.c, rootHash.d
        );

        if (prefab) {
            if (otherPrefab && rootHash.c < usedHash) {
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

        instance.localPosition = HexMetrics.Perturb(position);
        instance.localRotation = Quaternion.Euler(0f, 360f * rootHash.e, 0f);

        instance.SetParent(_container, false);
    }

    private Transform PickPrefab (
        FeatureCollection[] collection, 
        int level, 
        float hash, 
        float choice
    ) {
        if (level > 0) {
            float[] thresholds = HexMetrics.GetFeatureThresholds(level - 1);

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
        bool hasRiver, bool hasRoad
    ) {
        if  (
            nearCell.HasWalls != farCell.HasWalls &&
            !nearCell.IsUnderwater && !farCell.IsUnderwater &&
            nearCell.GetEdgeType(farCell) != EdgeType.Cliff
        ) {
            AddWallSegment(near.vertex1, far.vertex1, near.vertex2, far.vertex2);

            if (hasRiver || hasRoad) {
                AddWallCap(near.vertex2, far.vertex2);
                AddWallCap(far.vertex4, near.vertex4);
            }
            else {
                AddWallSegment(near.vertex2, far.vertex2, near.vertex3, far.vertex3);
                AddWallSegment(near.vertex3, far.vertex3, near.vertex4, far.vertex4);
            }
            AddWallSegment(near.vertex4, far.vertex4, near.vertex5, far.vertex5);
        }
    }

    public void AddWall(
        Vector3 corner1, HexCell cell1,
        Vector3 corner2, HexCell cell2,
        Vector3 corner3, HexCell cell3
    ) {
        if (cell1.HasWalls) {
            if (cell2.HasWalls) {
                if (!cell3.HasWalls) {
                    AddWallSegment(corner3, cell3, corner1, cell1, corner2, cell2);
                }
            }
            else if (cell3.HasWalls) {
                AddWallSegment(corner2, cell2, corner3, cell3, corner1, cell1);
            }
            else {
                AddWallSegment(corner1, cell1, corner2, cell2, corner3, cell3);
            }
        }
        else if (cell2.HasWalls) {
            if (cell3.HasWalls) {
                AddWallSegment(corner1, cell1, corner2, cell2, corner3, cell3);
            }
            else {
                AddWallSegment(corner2, cell2, corner3, cell3, corner1, cell1);
            }
        }
        else if (cell3.HasWalls) {
            AddWallSegment(corner3, cell3, corner1, cell1, corner2, cell2);
        }
    }


    private void AddWallSegment(
        Vector3 nearLeft,
        Vector3 farLeft,
        Vector3 nearRight,
        Vector3 farRight,
        bool addTower = false
    ) {
        nearLeft = HexMetrics.Perturb(nearLeft);
        farLeft = HexMetrics.Perturb(farLeft);
        nearRight = HexMetrics.Perturb(nearRight);
        farRight = HexMetrics.Perturb(farRight);

        Vector3 left = HexMetrics.WallLerp(nearLeft, farLeft);
        Vector3 right = HexMetrics.WallLerp(nearRight, farRight);

        Vector3 leftThicknessOffset = 
            HexMetrics.WallThicknessOffset(nearLeft, farLeft);
        Vector3 rightThicknessOffset = 
            HexMetrics.WallThicknessOffset(nearRight, farRight);

        float leftTop = left.y + HexMetrics.wallHeight;
        float rightTop = right.y + HexMetrics.wallHeight;

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
        Vector3 right, HexCell rightCell
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
                    RootGenHash rootHash = HexMetrics.SampleHashGrid (
                        (pivot + left + right) * 1f / 3f
                    );

                    hasTower = rootHash.e < HexMetrics.wallTowerThreshold;
                }

                AddWallSegment(pivot, left, pivot, right, hasTower);
            }
            else if (leftCell.Elevation < rightCell.Elevation) {
                AddWallWedge(pivot, left, right);
            }
            else {
                AddWallCap(pivot, left);
            }
        }
        else if (hasRightWall) {
            if (rightCell.Elevation < leftCell.Elevation) {
                AddWallWedge(right, pivot, left);
            }
            else
            {
                AddWallCap(right, pivot);
            }
        }
    }

    private void AddWallCap(Vector3 near, Vector3 far) {
        near = HexMetrics.Perturb(near);
        far = HexMetrics.Perturb(far);

        Vector3 center = HexMetrics.WallLerp(near, far);
        Vector3 thickness = HexMetrics.WallThicknessOffset(near, far);

        Vector3 vertex1;
        Vector3 vertex2;
        Vector3 vertex3;
        Vector3 vertex4;

        vertex1 = vertex3 = center - thickness;
        vertex2 = vertex4 = center + thickness;
        vertex3.y = vertex4.y = center.y + HexMetrics.wallHeight;
        walls.AddQuadUnperturbed(vertex1, vertex2, vertex3, vertex4);
    }

    private void AddWallWedge(Vector3 near, Vector3 far, Vector3 point) {
        near = HexMetrics.Perturb(near);
        far = HexMetrics.Perturb(far);
        point = HexMetrics.Perturb(point);

        Vector3 center = HexMetrics.WallLerp(near, far);
        Vector3 thickness = HexMetrics.WallThicknessOffset(near, far);

        Vector3 vertex1;
        Vector3 vertex2;
        Vector3 vertex3;
        Vector3 vertex4;

        Vector3 pointTop = point;
        point.y = center.y;

        vertex1 = vertex3 = center - thickness;
        vertex2 = vertex4 = center + thickness;
        vertex3.y = vertex4.y = pointTop.y = center.y + HexMetrics.wallHeight;
        
        walls.AddQuadUnperturbed(vertex1, point, vertex3, pointTop);
        walls.AddQuadUnperturbed(point, vertex2, pointTop, vertex4);
        walls.AddTriangleUnperturbed(pointTop, vertex3, vertex4);
    }

    public void AddBridge(Vector3 roadCenter1, Vector3 roadCenter2) {
        roadCenter1 = HexMetrics.Perturb(roadCenter1);
        roadCenter2 = HexMetrics.Perturb(roadCenter2);
        Transform instance = Instantiate(bridge);
        instance.localPosition = (roadCenter1 + roadCenter2) * 0.5f;
        instance.forward = roadCenter2 - roadCenter1;
        float length = Vector3.Distance(roadCenter1, roadCenter2);

        instance.localScale = 
        new Vector3
        (
            1f, 
            1f, 
            length * (1f / HexMetrics.bridgeDesignLength)
        );

        instance.SetParent(_container, false);
    }

    public void AddSpecialFeature(HexCell cell, Vector3 position) {
        Transform instance = Instantiate(special[cell.SpecialIndex - 1]);
        instance.localPosition = HexMetrics.Perturb(position);
        RootGenHash rootHash = HexMetrics.SampleHashGrid(position);
        instance.localRotation = Quaternion.Euler(0f, 360f * rootHash.e, 0f);
        instance.SetParent(_container, false);
    }
}
