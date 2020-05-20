#region Using

using System.Collections.Generic;
using QuikGraph;

#endregion

public class NeighborGraph : AdjacencyGraph<HexCell, HexEdge> {
    #region ConstantFields
    #endregion

    #region Fields
    #endregion

    #region Constructors
    
    public NeighborGraph() : base(false) { }

    #endregion

    #region Finalizers (Destructors)
    #endregion

    #region Delegates
    #endregion

    #region Events
    #endregion

    #region Enums
    #endregion

    #region Interfaces
    #endregion

    #region Properties
    #endregion

    #region Indexers
    #endregion
    
    #region Methods
    
    /// <summary>
    /// Gets all of the edges for a particular 
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public List<HexEdge> GetVertexEdges(HexCell cell) {
        IEnumerable<HexEdge> outEdge;

        TryGetOutEdges(cell, out outEdge);

        if (outEdge == null) {
            return null;
        }
        
        return outEdge as List<HexEdge>;            
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

            foreach(
                HexCell neighbor in hexGrid.GetNeighbors(i)
            ) {
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
    #endregion

    #region Structs
    #endregion

    #region Classes
    #endregion
}