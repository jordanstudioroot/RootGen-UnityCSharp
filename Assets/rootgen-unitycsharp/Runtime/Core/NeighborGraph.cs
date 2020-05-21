#region Using

using System.Collections.Generic;
using QuikGraph;

#endregion

public class HexAdjacencyGraph : AdjacencyGraph<Hex, HexEdge> {
    #region ConstantFields
    #endregion

    #region Fields
    #endregion

    #region Constructors
    
    public HexAdjacencyGraph() : base(false) { }

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
    public List<HexEdge> GetVertexEdges(Hex cell) {
        IEnumerable<HexEdge> outEdge;

        TryGetOutEdges(cell, out outEdge);

        if (outEdge == null) {
            return null;
        }
        
        return outEdge as List<HexEdge>;            
    }

    public IEnumerable<HexEdge> TryGetEdgeInDirection(
        Hex cell,
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

    public Hex TryGetNeighborInDirection(
        Hex cell,
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
        Hex cell,
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
        Hex cell,
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
        Hex cell,
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
        Hex cell,
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
        Hex cell,
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
        Hex cell,
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
        Hex cell,
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
        Hex cell,
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
        Hex cell,
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

    public static HexAdjacencyGraph FromHexGrid(
        HexGrid<Hex> hexGrid
    ) {
        List<HexEdge> edges = new List<HexEdge>();

        for (
            int i = 0;
            i < hexGrid.Rows * hexGrid.Columns;
            i++
        ) {
            Hex current = hexGrid[i];

            foreach(
                Hex neighbor in hexGrid.GetNeighbors(i)
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

        HexAdjacencyGraph result = new HexAdjacencyGraph();
        result.AddVerticesAndEdgeRange(edges);
        return result;
    }

    public override string ToString() {
        string result = "Neighbor Graph Edges \n\n";
        
        foreach (Hex vertex in Vertices) {
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