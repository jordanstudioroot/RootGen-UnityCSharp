using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RootExtensions;
using RootUtils.Math;

public class HexUnit : MonoBehaviour
{
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
    private HexCell _location;
    private HexCell _currentDestination;
    private float _orientation;
    private List<HexCell> _pathToTravel;
    private const float _travelSpeed = 4f;
    private const float _rotationSpeed = 180f;
    private const int _visionRange = 3;

// CONSTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DESTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DELEGATES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// EVENTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// ENUMS

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INTERFACES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// PROPERTIES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INDEXERS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// METHODS ~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public
    public HexCell Location
    {
        get { return _location; }
        set {
            if (_location) {
// TODO: This is a circular dependency that needs to be fixed.
//                Grid.DecreaseVisibility(Location, _visionRange);
                _location.Unit = null;
            }

            _location = value;
            value.Unit = this;
// TODO: This is a circular dependency that needs to be fixed.
//   Grid.IncreaseVisibility(Location, _visionRange);
//            transform.StandOn(value.Position);
//            Grid.MakeChildOfColumn(transform, value.ColumnIndex);
        }
    }

    public float Orientation
    {
        get { return _orientation; }
        set {
            _orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    public HexMap Grid { get; set; }

    public int Speed {
        get { return 24; }
    }

    public int VisionRange {
        get { return 3; }
    }

    public void ValidateLocation() {
        transform.localPosition = Location.Position;
    }

    public void Die() {
        if (_location) {
            Location.DecreaseVisibility();
        }
        _location.Unit = null;
        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer) {
        _location.Coordinates.Save(writer);
        writer.Write(_orientation);
    }

    public bool IsValidDestination(HexCell cell) {
        return cell.IsExplored && !cell.IsUnderwater && !cell.Unit;
    }

    public void Travel(
        List<HexCell> path,
        float cellOuterRadius,
        HexMap hexMap
    ) {
        _location.Unit = null;
        _location = path[path.Count - 1];
        _location.Unit = this;
        _pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(
            TravelPath(
                cellOuterRadius,
                hexMap
            )
        );
    }

// TODO: This should not be the responsibilty of this class. Should
//       probably be located in the HexMap.
//    public int GetMoveCost
//    (
//        HexCell fromCell, 
//       HexCell toCell, 
//        HexDirection direction
//    ) {
//        int moveCost;
//
//        ElevationEdgeTypes edgeType = fromCell.GetEdgeType(toCell);
//        if (edgeType == ElevationEdgeTypes.Cliff) {
//            return -1;
//        }
//
//        if (fromCell.HasRoadThroughEdge(direction)) {
//            moveCost = 1;
//        }
//        else if (fromCell.HasWalls != toCell.HasWalls) {
//            return -1;
//        }
//        else {
//            moveCost = edgeType == ElevationEdgeTypes.Flat ? 5 : 10;
//            moveCost +=
//                toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
//        }
//
//        return moveCost;
//    }
    
// ~~ private
    private IEnumerator TravelPath(float cellOuterRadius, HexMap hexMap) {
        float innerDiameter = 
            HexagonPoint.GetOuterToInnerRadius(cellOuterRadius) * 2f;
        Vector3 pointA, pointB, pointC = _pathToTravel[0].Position;
        
        yield return LookAt(
            _pathToTravel[1].Position,
            cellOuterRadius,
            hexMap
        );

        if (!_currentDestination) {
            _currentDestination = _pathToTravel[0];
        }
// TODO: This is a circular dependency and needs to be fixed.
//        Grid.DecreaseVisibility(_currentDestination, VisionRange);

        int currentColumn = _currentDestination.ColumnIndex;

        float t = Time.deltaTime * _travelSpeed;
        for (int i = 1; i < _pathToTravel.Count; i++) {
            _currentDestination = _pathToTravel[i];

// Start or previous cell midpoint.
            pointA = pointC;

// Current cell position.
            pointB = _pathToTravel[i - 1].Position;

            int nextColumn = _currentDestination.ColumnIndex;
            if (currentColumn != nextColumn) {
                if (nextColumn < currentColumn - 1) {
                    pointA.x -= innerDiameter * hexMap.WrapSize;
                    pointB.x -= innerDiameter * hexMap.WrapSize;
                }
                else if (nextColumn > currentColumn + 1) {
                    pointA.x += innerDiameter * hexMap.WrapSize;
                    pointB.x += innerDiameter * hexMap.WrapSize;
                }
//                Grid.MakeChildOfColumn(transform, nextColumn);
                currentColumn = nextColumn;
            }

            pointC = (pointB + _currentDestination.Position) * 0.5f;
// TODO: This is a circular dependency and needs to be fixed.
//            Grid.IncreaseVisibility(_pathToTravel[i], VisionRange);

            for (; t < 1f; t += Time.deltaTime * _travelSpeed) {
                transform.StandOn(
                    Bezier.GetQuadradicPoint(pointA, pointB, pointC, t)
                );

                Vector3 direction = Bezier.GetDerivative(pointA, pointB, pointC, t);

                // Lock y rotation.
                direction.y = 0f;

                transform.localRotation = Quaternion.LookRotation(direction);
                yield return null;
            }

            _currentDestination = null;

// TODO: This is a circular dependency and needs to be fixed.
//            Grid.DecreaseVisibility(_pathToTravel[i], _visionRange);

/* Subtract 1 from each time "segment" to carry over the remaining
* time into the next loop, which will prevent stuttering if the
* frame rate is low.
*/
            t -= 1f;
        }

        pointA = pointC;

/* Can simply use destination, as the unit is at the
* last cell before the end of the path.
*/
        pointB = Location.Position;

        pointC = pointB;

// TODO: This is a circular dependency and needs to be fixed.
//        Grid.IncreaseVisibility(_location, _visionRange);

        for (; t < 1f; t += Time.deltaTime * _travelSpeed) {
            transform.StandOn(
                Bezier.GetQuadradicPoint(pointA, pointB, pointC, t)
            );
            Vector3 direction = Bezier.GetDerivative(pointA, pointB, pointC, t);
            direction.y = 0f;
            transform.localRotation = Quaternion.LookRotation(direction);
            yield return null;
        }

/* Because the amount of movement depends on t, the unit may be short of its
* destination by a small amount. When the unit has completed its animation,
* snap it into the correct position by assigning its position explicitly.
*/
        transform.StandOn(_location.Position);
        _orientation = transform.localRotation.eulerAngles.y;
    }

    private void OnDrawGizmos() {
        if (_pathToTravel == null || _pathToTravel.Count == 0) {
            return;
        }

        Vector3 pointA;
        Vector3 pointB;
        Vector3 pointC;

        pointA = pointB = pointC = _pathToTravel[0].Position;

        for (int i = 1; i < _pathToTravel.Count; i++) {
// Start or previous cell midpoint.
            pointA = pointC;
            
// Current cell position.
            pointB = _pathToTravel[i - 1].Position;

// Next cell midpoint.
            pointC = (pointB + _pathToTravel[i].Position) * 0.5f;

            for (float t = 0f; t < 1f; t += Time.deltaTime * _travelSpeed) {
                Gizmos.DrawSphere(Bezier.GetQuadradicPoint(pointA, pointB, pointC, t), 2f);
            }
        }

        pointA = pointC;
        pointB = _pathToTravel[_pathToTravel.Count - 1].Position;
        pointC = pointB;

        for (float t = 0f; t < 1f; t += 0.1f) {
            Gizmos.DrawSphere(Bezier.GetQuadradicPoint(pointA, pointB, pointC, t), 2f);
        }
    }

    private IEnumerator LookAt(
        Vector3 point,
        float cellOuterRadius,
        HexMap hexMap
    ) {
        
        float innerRadius = HexagonPoint.GetOuterToInnerRadius(cellOuterRadius);
        float innerDiameter = innerRadius * 2f;

        if (hexMap.IsWrapping) {
            float xDistance = point.x - transform.localPosition.x;
            if (xDistance < -innerRadius * hexMap.WrapSize) {
                point.x += innerDiameter * hexMap.WrapSize;
            }
            else if (xDistance > innerRadius * hexMap.WrapSize) {
                point.x -= innerDiameter * hexMap.WrapSize;
            }
        }

        point.y = transform.localPosition.y;
        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation =
            Quaternion.LookRotation(point - transform.localPosition);

        float angle = Quaternion.Angle(fromRotation, toRotation);

        if (angle > 0f) {
            float speed = _rotationSpeed / angle;

            for (
                float t = Time.deltaTime * speed;
                t < 1f;
                t += Time.deltaTime * speed
            ) {
                transform.localRotation =
                    Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }

            transform.LookAt(point);
            _orientation = transform.localRotation.eulerAngles.y;
        }
    }

    private void Awake() { }

    private void OnEnable() {
        if (_location) {
            transform.localPosition = Location.Position;

            if (_currentDestination) {
// TODO: These are both circular dependencies and they need to be fixed.
//                Grid.IncreaseVisibility(_location, _visionRange);
//                Grid.DecreaseVisibility(_currentDestination, _visionRange);
                _currentDestination = null;
            }
        }
    }
    
// STRUCTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// CLASSES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
}
