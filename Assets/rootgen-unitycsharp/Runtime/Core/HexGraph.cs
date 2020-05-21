using QuikGraph;
using RootLogging;
using System;
using System.Collections.Generic;
using UnityEngine;



public class RiverDigraph : BidirectionalGraph<Hex, RiverEdge> {

    public RiverDigraph() : base(false) { }
    
/// <summary>
///     Returns true if the given hex in the graph has a river.
/// </summary>
/// <param name="hex">
///     The hex which is the subject of the query.
/// </param>
/// <returns>
///     A boolean value representing whether the query hex has a river.
/// </returns>
    public bool HasRiver(Hex hex) {
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            return true;
        }

        return false;
    }

    public List<HexDirections> IncomingRiverDirections(Hex hex) {
        IEnumerable<RiverEdge> edges;
        List<HexDirections> result = null;

        if (!TryGetInEdges(hex, out edges)) {
            result = new List<HexDirections>();

            foreach(RiverEdge currentEdge in edges) {
                result.Add(currentEdge.Direction);
            }
        }

        return result;
    }

    public List<HexDirections> OutgoingRiverDirections(Hex hex) {
        IEnumerable<RiverEdge> edges;
        List<HexDirections> result = null;

        if (!TryGetOutEdges(hex, out edges)) {
            result = new List<HexDirections>();

            foreach(RiverEdge currentEdge in edges) {
                result.Add(currentEdge.Direction);
            }
        }

        return result;
    }

    /// <summary>
///     Returns true if the given hex in the graph contains
///     the termination of a river.
/// </summary>
/// <param name="hex">
///     The hex which is the subject of the query.
/// </param>
/// <returns>
///     A boolean value representing whether the query hex
///     contains the termination of a river.
/// </returns>
    public bool HasRiverEnd(
        Hex hex
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            return false;
        }

        if (TryGetInEdges(hex, out edges)) {
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
        Hex hex
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetInEdges(hex, out edges)) {
            return false;
        }

        if (TryGetOutEdges(hex, out edges)) {
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
        Hex hex
    ) {
        IEnumerable<RiverEdge> edges;
        int count = 0;

        if (TryGetInEdges(hex, out edges)) {
            foreach(RiverEdge edge in edges) {
                count++;
                
                if (count > 1) {
                    return false;
                }
            }
        }

        if (TryGetOutEdges(hex, out edges)) {
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
        Hex hex
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetInEdges(hex, out edges)) {
            return true;
        }

        return false;
    }

    public bool HasIncomingRiverInDirection(
        Hex hex,
        HexDirections direction
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetInEdges(hex, out edges)) {
            foreach(RiverEdge edge in edges) {
                if (edge.Direction == direction) {
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasOutgoingRiver(
        Hex hex
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            return true;
        }

        return false;
    }

    public bool HasOutgoingRiverInDirection(
        Hex hex,
        HexDirections direction
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            foreach(RiverEdge edge in edges) {
                if (edge.Direction == direction) {
                    return true;
                }
            }
        }

        return false;
    }
    
    public bool HasRiverInDirection(
        Hex hex,
        HexDirections direction
    ) {
        if (HasIncomingRiverInDirection(hex, direction))
            return true;

        if (HasOutgoingRiverInDirection(hex, direction))
            return true;

        return false;
    }

    public bool HasStraightRiver(
        Hex hex
    ) {
        if (
            IncomingRiverDirections(hex)[0] ==
            OutgoingRiverDirections(hex)[0].Opposite()
        ) {
            return true;
        }

        return false;
    }

    public bool HasClockwiseCornerRiver(
        Hex hex
    ) {
        if(
            IncomingRiverDirections(hex)[0] ==
            OutgoingRiverDirections(hex)[0].ClockwiseRotation(1)
        ) {
            return true;
        }

        return false;
    }

    public bool HasCounterClockwiseCornerRiver(
        Hex hex
    ) {
        if(
            IncomingRiverDirections(hex)[0] ==
            OutgoingRiverDirections(hex)[0].ClockwiseRotation(-1)
        ) {
            return true;
        }

        return false;
    }

    public bool HasClockwiseRiverBend(
        Hex hex
    ) {
        if(
            IncomingRiverDirections(hex)[0] ==
            OutgoingRiverDirections(hex)[0].ClockwiseRotation(2)
        ) {
            return true;
        }

        return false;
    }

    public bool HasCounterClockwiseRiverBend(
        Hex hex
    ) {
        if(
            IncomingRiverDirections(hex)[0] ==
            OutgoingRiverDirections(hex)[0].ClockwiseRotation(-2)
        ) {
            return true;
        }

        return false;
    }

    public HexDirections RiverStartOrEndDirection(
        Hex hex
    ) {
        if (HasRiverStartOrEnd(hex)) {
            return AnyRiverDirection(hex);
        }

        throw new ArgumentException(
            "The hex has no incoming or outgoing rivers."
        );
    }

    public HexDirections AnyRiverDirection(
        Hex hex
    ) {
        List<RiverEdge> edges = (List<RiverEdge>)Edges;
        if (edges != null && edges.Count > 0) {
            return edges[0].Direction;
        }
        
        throw new ArgumentException(
            "The hex has no incoming or outgoing rivers."
        );
    }
// TODO: STUB
    public static RiverDigraph FromHexGrid(HexGrid<Hex> hexGrid) {
        return new RiverDigraph();
    }
}

public class RoadUndirectedGraph : UndirectedGraph<Hex, RoadEdge> {
/// <summary>
///     Returns true if the given hex in the graph has a road.
/// </summary>
/// <param name="hex">
///     The hex which is the subject of the query.
/// </param>
/// <returns>
///     A boolean value representing whether the query hex has a road.
/// </returns>
    public bool HasRoad(Hex hex) {
        if (!ContainsVertex(hex))
            return false;

        List<RoadEdge> edges = (List<RoadEdge>)AdjacentEdges(hex);

        if (edges != null && edges.Count > 0) {
            return true;
        }

        return false;
    }

    public bool HasRoadInDirection(
        Hex hex,
        HexDirections direction
    ) {
        if (!ContainsVertex(hex))
            return false;
            
        List<RoadEdge> edges = (List<RoadEdge>)AdjacentEdges(hex);

        if (edges != null && edges.Count > 0) {
            foreach (RoadEdge edge in edges) {
                if (edge.Direction == direction)
                    return true;
            }
        }

        return false;
    }

// TODO: STUB
    public static RoadUndirectedGraph FromHexGrid(HexGrid<Hex> array) {
        return new RoadUndirectedGraph();
    }
}

public class ElevationBidirectionalGraph : BidirectionalGraph<Hex, ElevationEdge> {
    public ElevationEdgeTypes GetEdgeTypeInDirection(
        Hex hex,
        HexDirections direction
    ) {
        IEnumerable<ElevationEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Direction == direction) {
                    return edge.EdgeType;
                }
            }
        }

        return ElevationEdgeTypes.Flat;
    }

    public bool AnyDirectionVisible(
        Hex hex
    ) {
        IEnumerable<ElevationEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Delta <= 0)
                    return true;
            }
        }

        return false;
    }

    public List<HexEdge> GetVisibleEdges(
        Hex hex
    ) {

        IEnumerable<ElevationEdge> edges;
        List<HexEdge> result = new List<HexEdge>();

        if (TryGetOutEdges(hex, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Delta <= 0)
                    result.Add(edge);
            }
        }

        return result;
    }

    public static ElevationBidirectionalGraph FromHexGrid(
        HexGrid<Hex> denseArray
    ) {
        ElevationBidirectionalGraph result = new ElevationBidirectionalGraph();
        return result;
    }
}