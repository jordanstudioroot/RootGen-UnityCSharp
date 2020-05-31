using QuikGraph;
using System;
using System.Collections.Generic;

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

        if (
            IncomingRiverCount(hex) > 0 ||
            OutgoingRiverCount(hex) > 0
        ) {
            return true;
        }

        return false;
    }

    public List<HexDirections> IncomingRiverDirections(Hex hex) {
        IEnumerable<RiverEdge> edges;
        List<HexDirections> result = null;

        if (TryGetInEdges(hex, out edges)) {
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

        if (TryGetOutEdges(hex, out edges)) {
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
        if (
            OutgoingRiverCount(hex) == 0 &&
            IncomingRiverCount(hex) == 1
        ) {
            return true;
        }

        return false;
    }

    public bool HasRiverStart(
        Hex hex
    ) {
        if (
            IncomingRiverCount(hex) == 0 &&
            OutgoingRiverCount(hex) == 1
        ) {
            return true;
        }
        return false;
    }

    public int OutgoingRiverCount(Hex hex) {
        int result = 0;
        IEnumerable<RiverEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            foreach (RiverEdge edge in edges) {
                result++;
            }
        }

        return result;
    }

    public int IncomingRiverCount(Hex hex) {
        int result = 0;
        IEnumerable<RiverEdge> edges;

        if (TryGetInEdges(hex, out edges)) {
            foreach (RiverEdge edge in edges) {
                result++;
            }
        }

        return result;
    }

    public bool HasRiverStartOrEnd(
        Hex hex
    ) {
        return HasRiverStart(hex) || HasRiverEnd(hex);
    }

    public bool HasIncomingRiver(
        Hex hex
    ) {
        return IncomingRiverCount(hex) > 0;
    }

    /// <summary>
    /// Returns a boolean value indicating whether the specified hex
    /// has a river flowing in to it opposite the specified direction.
    /// </summary>
    /// <param name="hex">
    /// The specified hex.
    /// </param>
    /// <param name="directionToward">
    /// The direction pointing outward toward the incoming river.
    /// </param>
    public bool HasIncomingRiverInDirection(
        Hex hex,
        HexDirections directionToward
    ) {
        IEnumerable<RiverEdge> edges;

        if (TryGetInEdges(hex, out edges)) {
            foreach(RiverEdge edge in edges) {
                if (edge.Direction.Opposite() == directionToward) {
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasOutgoingRiver(
        Hex hex
    ) {
        return OutgoingRiverCount(hex) > 0;
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
        return
            HasIncomingRiverInDirection(hex, direction) ||
            HasOutgoingRiverInDirection(hex, direction);
    }

    public bool HasStraightRiver(
        Hex hex
    ) {
        List<HexDirections> incomingDirections =
            IncomingRiverDirections(hex);

        List<HexDirections> outgoingDirections =
            OutgoingRiverDirections(hex);
        
        foreach (HexDirections incoming in incomingDirections) {
            foreach (HexDirections outgoing in outgoingDirections) {
                if (incoming == outgoing.Opposite())
                    return true;
            }
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

    public void RemoveOutgoingRivers(Hex hex) {
        List<RiverEdge> toRemove = new List<RiverEdge>();
        
        IEnumerable<RiverEdge> edges;

        if(TryGetOutEdges(hex, out edges)) {
            foreach(RiverEdge edge in edges) {
                toRemove.Add(edge);
            }
        }

        foreach(RiverEdge edge in toRemove) {
            RemoveEdge(edge);
        }
    }

    public void RemoveIncomingRivers(Hex hex) {
        List<RiverEdge> toRemove = new List<RiverEdge>();
        
        IEnumerable<RiverEdge> edges;

        if(TryGetInEdges(hex, out edges)) {
            foreach(RiverEdge edge in edges) {
                toRemove.Add(edge);
            }
        }

        foreach(RiverEdge edge in toRemove) {
            RemoveEdge(edge);
        }
    }

    public HexRiverData GetRiverData(Hex hex) {
        IEnumerable<RiverEdge> inEdges;
        HexDirections incomingRiverDirection = default(HexDirections);
        bool hasIncomingRiver = false;

        if (TryGetInEdges(hex, out inEdges)) {
            List<RiverEdge> inEdgeList = inEdges as List<RiverEdge>;
            if (inEdgeList.Count > 0) {
                hasIncomingRiver = true;
                incomingRiverDirection = inEdgeList[0].Direction;
            }
        }

        IEnumerable<RiverEdge> outEdges;
        HexDirections outgoingRiverDirection = default(HexDirections);
        bool hasOutgoingRiver = false;

        if (TryGetInEdges(hex, out outEdges)) {
            List<RiverEdge> outEdgeList = outEdges as List<RiverEdge>;
            if (outEdgeList.Count > 0) {
                hasOutgoingRiver = true;
                outgoingRiverDirection = outEdgeList[0].Direction;
            }
        }

        return new HexRiverData(
            incomingRiverDirection,
            outgoingRiverDirection,
            hasIncomingRiver,
            hasOutgoingRiver
        );
    }
}

public struct HexRiverData {
    public bool HasIncomingRiver { get; set; }
    public bool HasOutgoingRiver { get; set; }

    public HexDirections IncomingRiverDirection { get; set; }
    public HexDirections OutgoingRiverDirection { get; set; }

    public bool HasClockwiseCornerRiver {
        get {
            return 
            IncomingRiverDirection ==
            OutgoingRiverDirection.ClockwiseRotation(1);
        } 
    }

    public bool HasCounterClockwiseCornerRiver {
        get {
            return
                IncomingRiverDirection ==
                OutgoingRiverDirection.ClockwiseRotation(-1);
        }
    }

    public bool HasClockwiseRiverBend {
        get {
            return
                IncomingRiverDirection ==
                OutgoingRiverDirection.ClockwiseRotation(2);
        }
    }

    public bool HasCounterClockwiseRiverBend {
        get {
            return
                IncomingRiverDirection ==
                OutgoingRiverDirection.ClockwiseRotation(-2);
        }
    }
    
    public bool HasRiverStart {
        get {
            return HasOutgoingRiver && !HasIncomingRiver;
        }
    }

    public bool HasRiverEnd {
        get  {
            return HasIncomingRiver && !HasOutgoingRiver;
        }
    }

    public bool HasRiver {
        get {
            return HasIncomingRiver || HasOutgoingRiver;
        }
    }

    public bool HasStraightRiver {
        get {
            if (
                HasIncomingRiver &&
                HasOutgoingRiver &&
                IncomingRiverDirection ==
                OutgoingRiverDirection.Opposite()
            ) {
                return true;
            }

            return false;
        }
    }

    public bool HasRiverStartOrEnd { 
        get {
            return HasRiverStart || HasRiverEnd;
        }
    }

    public HexRiverData(
        HexDirections incomingRiver,
        HexDirections outgoingRiver,
        bool hasIncomingRiver,
        bool hasOutgoingRiver
    ) {
        HasIncomingRiver = false;
        IncomingRiverDirection = incomingRiver;

        HasOutgoingRiver = true;
        OutgoingRiverDirection = outgoingRiver;
    }

    public bool HasRiverInDirection(HexDirections direction) {
        if (
            direction == IncomingRiverDirection ||
            direction == OutgoingRiverDirection
        ) {
            return true;
        }

        return false;
    }
}