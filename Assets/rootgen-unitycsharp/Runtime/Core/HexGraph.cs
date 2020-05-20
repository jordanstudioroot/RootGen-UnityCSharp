using QuikGraph;
using RootLogging;
using System;
using System.Collections.Generic;
using UnityEngine;



public class RiverGraph : BidirectionalGraph<HexCell, RiverEdge> {

    public RiverGraph() : base(false) { }
    
/// <summary>
///     Returns true if the given cell in the graph has a river.
/// </summary>
/// <param name="cell">
///     The cell which is the subject of the query.
/// </param>
/// <returns>
///     A boolean value representing whether the query cell has a river.
/// </returns>
    public bool HasRiver(HexCell cell) {
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            return true;
        }

        return false;
    }

    public List<HexDirections> IncomingRiverDirections(HexCell cell) {
        IEnumerable<RiverEdge> edges;
        List<HexDirections> result = null;

        if (!TryGetInEdges(cell, out edges)) {
            result = new List<HexDirections>();

            foreach(RiverEdge currentEdge in edges) {
                result.Add(currentEdge.Direction);
            }
        }

        return result;
    }

    public List<HexDirections> OutgoingRiverDirections(HexCell cell) {
        IEnumerable<RiverEdge> edges;
        List<HexDirections> result = null;

        if (!TryGetOutEdges(cell, out edges)) {
            result = new List<HexDirections>();

            foreach(RiverEdge currentEdge in edges) {
                result.Add(currentEdge.Direction);
            }
        }

        return result;
    }

    /// <summary>
///     Returns true if the given cell in the graph contains
///     the termination of a river.
/// </summary>
/// <param name="cell">
///     The cell which is the subject of the query.
/// </param>
/// <returns>
///     A boolean value representing whether the query cell
///     contains the termination of a river.
/// </returns>
    public bool HasRiverEnd(
        HexCell cell
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            return false;
        }

        if (TryGetInEdges(cell, out edges)) {
            int count = 0;
            foreach(RiverEdge edge in edges) {
                count++;
                
                if (count > 1) {
                    return false;
                }
            }
        }

        return true;
    }

    public bool HasRiverStart(
        HexCell cell
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetInEdges(cell, out edges)) {
            return false;
        }

        if (TryGetOutEdges(cell, out edges)) {
            int count = 0;
            foreach(RiverEdge edge in edges) {
                count++;
                
                if (count > 1) {
                    return false;
                }
            }
        }

        return true;
    }

    public bool HasRiverStartOrEnd(
        HexCell cell
    ) {
        IEnumerable<RiverEdge> edges;
        int count = 0;

        if (TryGetInEdges(cell, out edges)) {
            foreach(RiverEdge edge in edges) {
                count++;
                
                if (count > 1) {
                    return false;
                }
            }
        }

        if (TryGetOutEdges(cell, out edges)) {
            foreach(RiverEdge edge in edges) {
                count++;
                
                if (count > 1) {
                    return false;
                }
            }
        }

        return true;
    }

    public bool HasIncomingRiver(
        HexCell cell
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetInEdges(cell, out edges)) {
            return true;
        }

        return false;
    }

    public bool HasIncomingRiverInDirection(
        HexCell cell,
        HexDirections direction
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetInEdges(cell, out edges)) {
            foreach(RiverEdge edge in edges) {
                if (edge.Direction == direction) {
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasOutgoingRiver(
        HexCell cell
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            return true;
        }

        return false;
    }

    public bool HasOutgoingRiverInDirection(
        HexCell cell,
        HexDirections direction
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            foreach(RiverEdge edge in edges) {
                if (edge.Direction == direction) {
                    return true;
                }
            }
        }

        return false;
    }
    
    public bool HasRiverInDirection(
        HexCell cell,
        HexDirections direction
    ) {
        if (HasIncomingRiverInDirection(cell, direction))
            return true;

        if (HasOutgoingRiverInDirection(cell, direction))
            return true;

        return false;
    }

    public bool HasStraightRiver(
        HexCell cell
    ) {
        if (
            IncomingRiverDirections(cell)[0] ==
            OutgoingRiverDirections(cell)[0].Opposite()
        ) {
            return true;
        }

        return false;
    }

    public bool HasClockwiseCornerRiver(
        HexCell cell
    ) {
        if(
            IncomingRiverDirections(cell)[0] ==
            OutgoingRiverDirections(cell)[0].ClockwiseRotation(1)
        ) {
            return true;
        }

        return false;
    }

    public bool HasCounterClockwiseCornerRiver(
        HexCell cell
    ) {
        if(
            IncomingRiverDirections(cell)[0] ==
            OutgoingRiverDirections(cell)[0].ClockwiseRotation(-1)
        ) {
            return true;
        }

        return false;
    }

    public bool HasClockwiseRiverBend(
        HexCell cell
    ) {
        if(
            IncomingRiverDirections(cell)[0] ==
            OutgoingRiverDirections(cell)[0].ClockwiseRotation(2)
        ) {
            return true;
        }

        return false;
    }

    public bool HasCounterClockwiseRiverBend(
        HexCell cell
    ) {
        if(
            IncomingRiverDirections(cell)[0] ==
            OutgoingRiverDirections(cell)[0].ClockwiseRotation(-2)
        ) {
            return true;
        }

        return false;
    }

    public HexDirections RiverStartOrEndDirection(
        HexCell cell
    ) {
        if (HasRiverStartOrEnd(cell)) {
            return AnyRiverDirection(cell);
        }

        throw new ArgumentException(
            "Cell has no incoming or outgoing rivers."
        );
    }

    public HexDirections AnyRiverDirection(
        HexCell cell
    ) {
        List<RiverEdge> edges = (List<RiverEdge>)Edges;
        if (edges != null && edges.Count > 0) {
            return edges[0].Direction;
        }
        
        throw new ArgumentException(
            "Cell has no incoming or outgoing rivers."
        );
    }
// TODO: STUB
    public static RiverGraph FromHexGrid(HexGrid<HexCell> hexGrid) {
        return new RiverGraph();
    }
}

public class RoadGraph : UndirectedGraph<HexCell, RoadEdge> {
/// <summary>
///     Returns true if the given cell in the graph has a road.
/// </summary>
/// <param name="cell">
///     The cell which is the subject of the query.
/// </param>
/// <returns>
///     A boolean value representing whether the query cell has a road.
/// </returns>
    public bool HasRoad(HexCell cell) {
        if (!ContainsVertex(cell))
            return false;

        List<RoadEdge> edges = (List<RoadEdge>)AdjacentEdges(cell);

        if (edges != null && edges.Count > 0) {
            return true;
        }

        return false;
    }

    public bool HasRoadInDirection(
        HexCell cell,
        HexDirections direction
    ) {
        if (!ContainsVertex(cell))
            return false;
            
        List<RoadEdge> edges = (List<RoadEdge>)AdjacentEdges(cell);

        if (edges != null && edges.Count > 0) {
            foreach (RoadEdge edge in edges) {
                if (edge.Direction == direction)
                    return true;
            }
        }

        return false;
    }

// TODO: STUB
    public static RoadGraph FromHexGrid(HexGrid<HexCell> array) {
        return new RoadGraph();
    }
}

public class ElevationGraph : BidirectionalGraph<HexCell, ElevationEdge> {
    public ElevationEdgeTypes GetEdgeTypeInDirection(
        HexCell cell,
        HexDirections direction
    ) {
        IEnumerable<ElevationEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Direction == direction) {
                    return edge.EdgeType;
                }
            }
        }

        return ElevationEdgeTypes.Flat;
    }

    public bool AnyDirectionVisible(
        HexCell cell
    ) {
        IEnumerable<ElevationEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Delta <= 0)
                    return true;
            }
        }

        return false;
    }

    public List<HexEdge> GetVisibleEdges(
        HexCell cell
    ) {

        IEnumerable<ElevationEdge> edges;
        List<HexEdge> result = new List<HexEdge>();

        if (TryGetOutEdges(cell, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Delta <= 0)
                    result.Add(edge);
            }
        }

        return result;
    }

    public static ElevationGraph FromHexGrid(HexGrid<HexCell> denseArray) {
        ElevationGraph result = new ElevationGraph();
        return result;
    }
}