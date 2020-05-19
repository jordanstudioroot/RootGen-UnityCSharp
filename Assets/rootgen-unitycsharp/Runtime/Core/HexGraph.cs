using QuikGraph;
using RootLogging;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NeighborGraph : AdjacencyGraph<HexCell, HexEdge> {
    public NeighborGraph() : base(false) { }

    public List<HexEdge> GetVertexEdges(HexCell cell) {
        IEnumerable<HexEdge> outEdge;

        TryGetOutEdges(cell, out outEdge);

        if (outEdge == null) {
            return null;
        }
        
        return outEdge as List<HexEdge>;            
    }

    public List<HexCell> Neighbors(HexCell cell) {
        List<HexEdge> edges = GetVertexEdges(cell);

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
        HexDirections direction
    ) {
        List<HexEdge> edges = GetVertexEdges(cell);
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
        HexDirections direction
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
        List<HexEdge> edges = GetVertexEdges(cell);
        
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
        HexDirections direction
    ) {
        List<HexEdge> edges = GetVertexEdges(cell);
        
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
        List<HexEdge> edges = GetVertexEdges(cell);
        
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
        HexDirections direction
    ) {
        List<HexEdge> edges = GetVertexEdges(cell);
        
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
        List<HexEdge> edges = GetVertexEdges(cell);
        
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
        HexDirections direction
    ) {
        List<HexEdge> edges = GetVertexEdges(cell);
        
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
        List<HexEdge> edges = GetVertexEdges(cell);
        
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
        HexDirections direction
    ) {
        List<HexEdge> edges = GetVertexEdges(cell);
        
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
        HexDirections direction
    ) {
        List<HexEdge> edges = GetVertexEdges(cell);
        
        if (edges != null && edges.Count > 0) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == direction.PreviousClockwise2()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public static NeighborGraph FromHexGrid(
        HexGrid<HexCell> hexGrid
    ) {
        List<HexEdge> edges = new List<HexEdge>();

        for (
            int i = 0;
            i < hexGrid.Rows * hexGrid.Columns;
            i++
        ) {
            HexCell current = hexGrid[i];

            foreach(HexCell neighbor in hexGrid.GetNeighbors(i)) {
                HexEdge newEdge = new HexEdge(
                    current,
                    neighbor,
                    CubeVector.HexDirection(
                        current.Coordinates,
                        neighbor.Coordinates,
                        hexGrid.WrapSize
                    )
                );

                edges.Add(newEdge);
            }
        }

        NeighborGraph result = new NeighborGraph();
        result.AddVerticesAndEdgeRange(edges);
        return result;
    }

    public override string ToString() {
        string result = "Neighbor Graph Edges \n\n";
        
        foreach (HexCell vertex in Vertices) {
            result += vertex + " edges: \n";

            foreach (HexEdge edge in GetVertexEdges(vertex)) {
                result += edge + "\n";
            }

            result += "\n";
        }

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