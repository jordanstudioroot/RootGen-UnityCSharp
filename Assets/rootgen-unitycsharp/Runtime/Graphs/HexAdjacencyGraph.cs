using System.Collections.Generic;
using QuikGraph;
using System.Linq;
using UnityEngine;

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
    /// Gets all out edges for the provided hex. 
    /// </summary>
    /// <param name="hex">
    /// The hex with the desired out edges.
    /// </param>
    /// <returns>
    /// The out edges of the specified hex or null if there are no out edges
    /// for the specified hex.
    /// </returns>
    public List<HexEdge> GetOutEdgesList(Hex hex) {
        IEnumerable<HexEdge> result;

        TryGetOutEdges(hex, out result);

        if (result == null) {
            return null;
        }
        
        return result as List<HexEdge>;            
    }

    public HexEdge[] GetOutEdgesArray(Hex hex) {
        IEnumerable<HexEdge> result;

        TryGetOutEdges(hex, out result);

        if (result == null)
            return null;

        return result.ToArray();
    }

    public bool IsEdgeInDirection(
        Hex hex,
        HexDirections direction
    ) {
        List<HexEdge> edges = GetOutEdgesList(hex);
        
        for (int i = 0; i < edges.Count; i++) {
            if (edges[i].Direction == direction)
                return true;
        }

        return false;
    }

    public HexDirections[] GetEdgeDirections(
        Hex hex
    ) {
        HexEdge[] edges = GetOutEdgesArray(hex);
        
        if (edges == null)
            return null;

        HexDirections[] result = new HexDirections[edges.Length];

        

        for (int i = 0; i < edges.Length; i++) {
            result[i] = edges[i].Direction;    
        }

        return result;
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
        List<HexEdge> edges = GetOutEdgesList(hex);
        
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
                    CubeVector.HexDirectionWrapping(
                        current.CubeCoordinates,
                        neighbor.CubeCoordinates,
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

            foreach (HexEdge edge in GetOutEdgesList(vertex)) {
                result += edge + "\n";
            }

            result += "\n";
        }

        return result;
    }

    public List<HexDirections> GetBorderDirections(Hex hex) {
        List<HexDirections> result = new List<HexDirections> {
            HexDirections.East,
            HexDirections.Northeast,
            HexDirections.Northwest,
            HexDirections.Southeast,
            HexDirections.Southwest,
            HexDirections.West
        };

        HexDirections[] nonBorderDirections = GetEdgeDirections(hex);

        for (
            int i = 0;
            i < nonBorderDirections.Length;
            i++
        ) {
            if (result.Contains(nonBorderDirections[i])) {
                result.Remove(nonBorderDirections[i]);
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