using QuikGraph;
using System.Collections.Generic;

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

    public Dictionary<HexDirections, bool> GetNeighborRoads(Hex hex) {
        Dictionary<HexDirections, bool> result =
            new Dictionary<HexDirections, bool>();
        
        for (int i = 0; i < 6; i++) {
            result.Add((HexDirections)i, false);
        }

        if (ContainsVertex(hex)) {
            foreach(RoadEdge edge in AdjacentEdges(hex)) {
                result[edge.Direction] = true;
            }
        }

        return result;
    }
}