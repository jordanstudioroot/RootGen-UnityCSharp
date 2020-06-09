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
        if (ContainsVertex(hex))
            return true;
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
        Dictionary<HexDirections, bool> incomingRivers =
            new Dictionary<HexDirections, bool>();

        Dictionary<HexDirections, bool> outgoingRivers =
            new Dictionary<HexDirections, bool>();

        for (HexDirections i = 0; (int)i < 6; i++) {
            incomingRivers.Add(i, false);
            outgoingRivers.Add(i, false);
        }
        
        IEnumerable<RiverEdge> inEdges;

        if (TryGetInEdges(hex, out inEdges)) {
            foreach(RiverEdge edge in inEdges) {
                incomingRivers[edge.Direction] = true;
            }
        }

        IEnumerable<RiverEdge> outEdges;

        if (TryGetOutEdges(hex, out outEdges)) {
            foreach(RiverEdge edge in outEdges) {
                outgoingRivers[edge.Direction] = true;
            }
        }

        return new HexRiverData(
            outgoingRivers,
            incomingRivers
        );
    }
}

public struct HexRiverData {
    public int OutgoingRiverCount {
        get; private set;
    }

    public int IncomingRiverCount {
        get; private set;
    }

    public bool HasRiverStart {
        get {
            return
                OutgoingRiverCount == 1 &&
                IncomingRiverCount == 0;
        }
    }

    public bool HasRiverEnd {
        get {
            return
                IncomingRiverCount == 1 &&
                OutgoingRiverCount == 0;
        }
    }

    public bool HasRiverStartOrEnd {
        get {
            return
                HasRiverStart ||
                HasRiverEnd;
        }
    }

    public bool HasIncomingRiver {
        get {
            foreach(
                KeyValuePair<HexDirections, bool> pair in IncomingRivers
            ) {
                if (pair.Value)
                    return true;
            }

            return false;
        }
    }

    public bool HasOutgoingRiver {
        get {
            foreach(
                KeyValuePair<HexDirections, bool> pair in OutgoingRivers
            ) {
                if (pair.Value)
                    return true;
            }

            return false;
        }
    }

    public bool HasRiver {
        get {
            return
                HasIncomingRiver ||
                HasOutgoingRiver;
        }
    }

    public bool HasNextClockwiseCornerRiver {
        get {
            for (int i = 0; i < 6; i++) {
                HexDirections dir = (HexDirections)i;

                if (
                    IncomingRivers[dir] &&
                    OutgoingRivers[dir.NextClockwise()]
                ) {
                    return true;
                }
            }

            return false;
        }
    }

    public bool HasPreviousClockwiseCornerRiver {
        get {
            for (int i = 0; i < 6; i++) {
                HexDirections dir = (HexDirections)i;

                if (
                    IncomingRivers[dir] &&
                    OutgoingRivers[dir.PreviousClockwise()]
                ) {
                    return true;
                }
            }

            return false;
        }
    }

    public HexDirections AnyIncomingRiver {
        get {
            foreach (
                KeyValuePair<HexDirections, bool> pair in IncomingRivers
            ) {
                if (pair.Value)
                    return pair.Key;
            }

            throw new System.NullReferenceException();
        }
    }

    public HexDirections AnyOutgoingRiver {
        get {
            foreach (
                KeyValuePair<HexDirections, bool> pair in OutgoingRivers
            ) {
                if (pair.Value)
                    return pair.Key;
            }

            throw new System.NullReferenceException();
        }
    }

    public HexDirections RiverStartDirection {
        get {
            if (HasRiverStart) {
                foreach (
                    KeyValuePair<HexDirections, bool> direction in
                    OutgoingRivers
                ) {
                    if (direction.Value) {
                        return direction.Key;
                    }
                }
            }

            throw new System.NullReferenceException();
        }
    }

    public bool HasStraightRiver {
        get {
            for (int i = 0; i < 6; i++) {
                HexDirections dir = (HexDirections)i;

                if (
                    IncomingRivers[dir] &&
                    OutgoingRivers[dir.Opposite()]
                ) {
                    return true;
                }
            }

            return false;
        }
    }

    public HexDirections RiverEndDirection {
        get {
            if (HasRiverEnd) {
                foreach (
                    KeyValuePair<HexDirections, bool> direction in
                    IncomingRivers
                ) {
                    if (direction.Value) {
                        return direction.Key.Opposite();
                    }
                }
            }

            throw new System.NullReferenceException();
        }
    }

    public HexDirections RiverStartOrEndDirection {
        get {
            if (HasRiverStart) {
                return RiverStartDirection;
            }
            else if (HasRiverEnd) {
                return RiverEndDirection;
            }

            throw new System.NullReferenceException();
        }
    }

    public Dictionary<HexDirections, bool> OutgoingRivers {
        get; private set;
    }
    public Dictionary<HexDirections, bool> IncomingRivers {
        get; private set;
    }

    public HexRiverData(
        Dictionary<HexDirections, bool> outgoingRivers,
        Dictionary<HexDirections, bool> incomingRivers
    ) {
        OutgoingRivers = outgoingRivers;
        IncomingRivers = incomingRivers;

        int incomingRiverCount = 0;

        foreach(
            KeyValuePair<HexDirections, bool> pair in IncomingRivers
        ) {
            if (pair.Value)
                incomingRiverCount++;
        }

        IncomingRiverCount = incomingRiverCount;

        int outgoingRiverCount = 0;
        
        foreach(
            KeyValuePair<HexDirections, bool> pair in OutgoingRivers
        ) {
            if (pair.Value)
                outgoingRiverCount++;
        }
        
        OutgoingRiverCount = outgoingRiverCount;
    }

    public bool HasRiverInDirection(HexDirections direction) {
        return
            OutgoingRivers[direction] ||
            IncomingRivers[direction.Opposite()]; 
    }

    public bool HasIncomingRiverInDirection(HexDirections direction) {
        return IncomingRivers[direction.Opposite()];
    }

    public bool HasRiverInOppositeDirection(HexDirections direction) {
        return
            IncomingRivers[direction.Opposite()] ||
            OutgoingRivers[direction.Opposite()];
    }

    public bool HasRiverInNextDirection(HexDirections direction) {
        return
            IncomingRivers[direction.NextClockwise()] ||
            OutgoingRivers[direction.NextClockwise()];
    }

    public bool HasRiverInPreviousDirection(HexDirections direction) {
        return
            IncomingRivers[direction.PreviousClockwise()] ||
            OutgoingRivers[direction.PreviousClockwise()];
    }

    public bool HasRiverClockwiseToDirection(
        HexDirections direction,
        int turns
    ) {
        return
            IncomingRivers[direction.ClockwiseRotation(turns)] ||
            OutgoingRivers[direction.ClockwiseRotation(turns)];
    }
}