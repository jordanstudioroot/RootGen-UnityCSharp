using QuikGraph;
using UnityEngine;

public class HexEdge : Edge<HexCell> {
    public HexDirections Direction {
        get; private set;
    }

    public HexEdge(
        HexCell source,
        HexCell target,
        HexDirections direction
    ) : base(source, target) {
        Direction = direction;
    }

    public override string ToString() {
        return Source + " -> " + Direction + " -> " + Target;
    }
}

public class RiverEdge : HexEdge {
    public RiverEdge(
        HexCell source,
        HexCell target,
        HexDirections direction
    ) : base (source, target, direction) { }
}

public class RoadEdge : HexEdge {
    public RoadEdge(
        HexCell source,
        HexCell target,
        HexDirections direction
    ) : base (source, target, direction) { }
}

public class ElevationEdge : HexEdge {

/// <summary>
/// The difference in elevation between the source and target
/// of this edge.
/// </summary>
/// <value>
///     Returns 0 for no change, positive for an increase in
///     elevation, and negative for a decrease in elevation.
/// </value>
    public float Delta {
        get {
            return Target.Elevation - Source.Elevation;
        }
    }

    public ElevationEdgeTypes EdgeType {
        get {
            if (Delta == 0) {
                return ElevationEdgeTypes.Flat;
            }
            else if (Mathf.Abs(Delta) <= 1) {
                return ElevationEdgeTypes.Slope;
            }
            else {
                return ElevationEdgeTypes.Cliff;
            }
        }
    }

    public ElevationEdge(
        HexCell source,
        HexCell target,
        HexDirections direction
    ) : base (source, target, direction) { }

    public override string ToString() {
        return
            Source + " -> " +
            "( " + Direction + ", Delta:" + Delta + ", " +
            EdgeType + " ) -> "
            + Target;
    }
}

public class TraversalEdge : HexEdge {
    public float MovementCost {
        get; private set;
    }

    public TraversalEdge(
        HexCell source,
        HexCell target,
        HexDirections direction
    ) : base(source, target, direction) {
        MovementCost = 0;
    }

    public TraversalEdge(
        HexCell source,
        HexCell target,
        HexDirections direction,
        float movementCost
    ) : base(source, target, direction) {
        MovementCost = movementCost;
    }

    public override string ToString() {
        return
            Source + " -> " +
            "( " + Direction + ", MoveCost:" + MovementCost + ") -> "
            + Target;
    }
}