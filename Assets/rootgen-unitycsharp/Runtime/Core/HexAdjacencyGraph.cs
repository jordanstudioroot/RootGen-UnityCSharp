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
    /// <param name="hex"></param>
    /// <returns></returns>
    public List<HexEdge> GetOutEdges(Hex hex) {
        IEnumerable<HexEdge> result;

        TryGetOutEdges(hex, out result);

        if (result == null) {
            return null;
        }
        
        return result as List<HexEdge>;            
    }

    public bool TryGetEdgeInDirection(
        Hex hex,
        HexDirections direction,
        out HexEdge hexEdge
    ) {
        IEnumerable<HexEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            foreach (HexEdge edge in edges) {
                if (edge.Direction == direction) {
                    hexEdge = edge;
                    return true;
                }
            }
        }

        hexEdge = null;
        return false;
    }

    public Hex TryGetNeighborInDirection(
        Hex hex,
        HexDirections direction
    ) {
        IEnumerable<HexEdge> edges;

        TryGetOutEdges(hex, out edges);
            
        if (edges != null) {
            foreach(HexEdge edge in edges) {
                if (edge.Direction == direction)
                    return edge.Target;
            } 
        }

        return null;
    }

    public HexEdge GetHexDirectionOppositeEdge(
        Hex hex,
        HexEdge edge
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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
        Hex hex,
        HexDirections direction
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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
        Hex hex,
        HexEdge edge
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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
        Hex hex,
        HexDirections direction
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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
        Hex hex,
        HexEdge edge
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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
        Hex hex,
        HexDirections direction
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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
        Hex hex,
        HexEdge edge
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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
        Hex hex,
        HexDirections direction
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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
        Hex hex,
        HexDirections direction
    ) {
        List<HexEdge> edges = GetOutEdges(hex);
        
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

            foreach (HexEdge edge in GetOutEdges(vertex)) {
                result += edge + "\n";
            }

            result += "\n";
        }

        return result;
    }

    public List<HexDirections> GetBorderDirections(Hex hex) {
        List<HexDirections> result = new List<HexDirections>();
        int numDirections =
            System.Enum.GetValues(typeof(HexDirections)).Length;

        for (
            HexDirections i = 0; (int)i < numDirections; i = i.NextClockwise()
        ) {
            HexEdge found;

            if (!TryGetEdgeInDirection(hex, i, out found)) {
                result.Add(i);
            }
        }

        return result;
    }

    #endregion

    #region Structs
    #endregion

    #region Classes
    #endregion
}