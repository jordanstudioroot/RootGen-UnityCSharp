using System;
using QuikGraph;
using System.Collections.Generic;
using DenseArray;

public class NeighborGraph : AdjacencyGraph<HexCell, HexEdge> {

// Default constructor does not allow parralel edges.
    public NeighborGraph() : base(false) { }

    public NeighborGraph(int capacity) :
        base(false, capacity) { }
    
    public NeighborGraph(int vertexCapacity, int edgeCapacity) :
        base(false, vertexCapacity, edgeCapacity) { }

    public List<HexEdge> Edges(HexCell cell) {
        IEnumerable<HexEdge> edges;

        TryGetOutEdges(cell, out edges);

        if (edges == null) {
            return null;
        }
        
        return edges as List<HexEdge>;            
    }

    public List<HexCell> Neighbors(HexCell cell) {
        List<HexEdge> edges = Edges(cell);

        if (edges == null)
            return null;
        
        List<HexCell> result = new List<HexCell>();

        if (edges.Count > 0) {
            foreach(HexEdge edge in edges)
                result.Add(edge.Target);
        }

        return result;
    }

    public IEnumerable<HexEdge> TryGetEdgeInDirection(
        HexCell cell,
        HexDirection direction
    ) {
        List<HexEdge> edges = Edges(cell);
        List<HexEdge> result = new List<HexEdge>();

        if (edges != null && edges.Count > 0) {
            foreach(HexEdge edge in edges) {
                if (edge.Direction == direction)
                    result.Add(edge);
            }
        }

        return result;
    }

    public HexCell TryGetNeighborInDirection(
        HexCell cell,
        HexDirection direction
    ) {
        IEnumerable<HexEdge> edges;

        TryGetOutEdges(cell, out edges);
            
        if (edges != null) {
            foreach(HexEdge edge in edges) {
                if (edge.Direction == direction)
                    return edge.Target;
            } 
        }

        return null;
    }

    public HexEdge GetHexDirectionOppositeEdge(
        HexCell cell,
        HexEdge edge
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.Opposite()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionOppositeEdge(
        HexCell cell,
        HexDirection direction
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == direction.Opposite()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionNextEdge(
        HexCell cell,
        HexEdge edge
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.NextClockwise()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionNextEdge(
        HexCell cell,
        HexDirection direction
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == direction.NextClockwise()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionPreviousEdge(
        HexCell cell,
        HexEdge edge
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.PreviousClockwise()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionPreviousEdge(
        HexCell cell,
        HexDirection direction
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == direction.PreviousClockwise()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionNext2Edge(
        HexCell cell,
        HexEdge edge
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.NextClockwise2()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionNext2Edge(
        HexCell cell,
        HexDirection direction
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == direction.NextClockwise2()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionPrevious2Edge(
        HexCell cell,
        HexDirection direction
    ) {
        List<HexEdge> edges = Edges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == direction.PreviousClockwise2()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public static NeighborGraph FromDenseArray(
        DenseArray<HexCell> denseArray
    ) {
        List<HexEdge> edges = new List<HexEdge>();

        int numCells = denseArray.Columns * denseArray.Rows;
        HexDirection direction = HexDirection.SouthWest;

        for (int i = 0; i < numCells; i++) {
            for (int dz = -1; dz <= 1; dz++) {
                for (int dx = -1; dx <= 1; dx++) {
                    try {
                        if (denseArray[dz, dx] && denseArray[dz, dx] != denseArray[i]) {
                            edges.Add(
                                new HexEdge(
                                    denseArray[i],
                                    denseArray[dz, dx],
                                    direction                        
                                )
                            );

                            direction = direction.NextClockwise();
                        }
                    }
                    catch (IndexOutOfRangeException e) {
                        
                    }
                }
            }
        }

        NeighborGraph result = new NeighborGraph();
        result.AddVerticesAndEdgeRange(edges);
        return result;
    }
}

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

    public List<HexDirection> IncomingRiverDirections(HexCell cell) {
        IEnumerable<RiverEdge> edges;
        List<HexDirection> result = null;

        if (!TryGetInEdges(cell, out edges)) {
            result = new List<HexDirection>();

            foreach(RiverEdge currentEdge in edges) {
                result.Add(currentEdge.Direction);
            }
        }

        return result;
    }

    public List<HexDirection> OutgoingRiverDirections(HexCell cell) {
        IEnumerable<RiverEdge> edges;
        List<HexDirection> result = null;

        if (!TryGetOutEdges(cell, out edges)) {
            result = new List<HexDirection>();

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
        HexDirection direction
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
        HexDirection direction
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
        HexDirection direction
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

    public HexDirection RiverStartOrEndDirection(
        HexCell cell
    ) {
        if (HasRiverStartOrEnd(cell)) {
            return AnyRiverDirection(cell);
        }

        throw new ArgumentException(
            "Cell has no incoming or outgoing rivers."
        );
    }

    public HexDirection AnyRiverDirection(
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
    public static RiverGraph FromDenseArray(DenseArray<HexCell> array) {
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
        HexDirection direction
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
    public static RoadGraph FromDenseArray(DenseArray<HexCell> array) {
        return new RoadGraph();
    }
}

public class ElevationGraph : BidirectionalGraph<HexCell, ElevationEdge> {
    public ElevationEdgeTypes GetEdgeTypeInDirection(
        HexCell cell,
        HexDirection direction
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

    public static ElevationGraph FromDenseArray(
        DenseArray<HexCell> denseArray
    ) {
        List<ElevationEdge> edges = new List<ElevationEdge>();
        List<HexCell> vertices = new List<HexCell>();

        int numCells = denseArray.Columns * denseArray.Rows;
        HexDirection direction = HexDirection.SouthWest;

        for (int i = 0; i < numCells; i++) {
            for (int dz = -1; dz <= 1; dz++) {
                for (int dx = -1; dx <= 1; dx++) {
                    try {
                        if (
                            denseArray[dz, dx] &&
                            denseArray[dz, dx] !=
                            denseArray[i]
                        ) {
                            edges.Add(
                                new ElevationEdge(
                                    denseArray[i],
                                    denseArray[dz, dx],
                                    direction                     
                                )
                            );

                            direction = direction.NextClockwise();
                        }
                    }
                    catch (IndexOutOfRangeException e) {
                        
                    }
                }
            }
        }

        ElevationGraph result = new ElevationGraph();
        result.AddVerticesAndEdgeRange(edges);
        return result;
    }
}