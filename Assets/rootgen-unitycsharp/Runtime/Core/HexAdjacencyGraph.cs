using QuikGraph;
using System.Collections.Generic;

public class HexAdjacencyGraph : AdjacencyGraph<HexCell, HexEdge> {

/// <summary>
///     Returns true if the given cell in the graph has a river.
/// </summary>
/// <param name="cell">
///     The cell which is the subject of the query.
/// </param>
/// <returns>
///     A boolean value representing whether the query cell has a river.
/// </returns>
    public bool CellHasRiver(HexCell cell) {
        IEnumerable<HexEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            foreach(HexEdge edge in edges) {
                if (edge.HasRiver)
                    return true;
            }
        }

        return false;
    }

/// <summary>
///     Returns true if the given cell in the graph has a road.
/// </summary>
/// <param name="cell">
///     The cell which is the subject of the query.
/// </param>
/// <returns>
///     A boolean value representing whether the query cell has a road.
/// </returns>
    public bool CellHasRoad(HexCell cell) {
        IEnumerable<HexEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            foreach(HexEdge edge in edges) {
                if (edge.HasRoad)
                    return true;
            }
        }

        return false;
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
    public bool CellHasRiverTerminus(
        HexCell cell
    ) {
        IEnumerable<HexEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            int count = 0;
            foreach(HexEdge edge in edges) {
                if (edge.HasRiver)
                    count++;
                
                if (count > 2) {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CellHasIncomingRiver(
        HexCell cell
    ) {
        IEnumerable<HexEdge> edges;

        if (TryGetOutEdges(cell, out edges)) {
            foreach(HexEdge edge in edges) {
                if (edge.HasRoad)
                    return true;
            }
        }

        return false;
    }

    public IEnumerable<HexEdge> TryGetOutEdgesInDirection(
        HexCell cell,
        HexDirection direction
    ) {
        IEnumerable<HexEdge> edges;
        List<HexEdge> result = new List<HexEdge>();

        if (!TryGetOutEdges(cell, out edges)) {

            foreach(HexEdge edge in edges) {
                if (edge.Direction == direction)
                    result.Add(edge);
            }
        }
        else {
            throw new System.ArgumentException(
                "HexCell did not exist in graph or had no out edges."
            );
        }

        return result;
    }

    public HexEdge GetHexDirectionOppositeEdge(
        HexEdge edge
    ) {
        IEnumerable<HexEdge> edges;
        
        if (!TryGetOutEdges(edge.Source, out edges)) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.Opposite()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionNextEdge(
        HexEdge edge
    ) {
        IEnumerable<HexEdge> edges;
        
        if (!TryGetOutEdges(edge.Source, out edges)) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.Next()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionPreviousEdge(
        HexEdge edge
    ) {
        IEnumerable<HexEdge> edges;
        
        if (!TryGetOutEdges(edge.Source, out edges)) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.Previous()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionNext2Edge(
        HexEdge edge
    ) {
        IEnumerable<HexEdge> edges;

        if (!TryGetOutEdges(edge.Source, out edges)) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.Next2()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }

    public HexEdge GetHexDirectionPrevious2Edge(
        HexEdge edge
    ) {
        IEnumerable<HexEdge> edges;
        
        if (!TryGetOutEdges(edge.Source, out edges)) {
            foreach(HexEdge currentEdge in edges) {
                if (currentEdge.Direction == edge.Direction.Previous2()) {
                    return currentEdge;
                }
            }
        }

        return null;
    }
}