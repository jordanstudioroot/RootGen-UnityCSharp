using QuikGraph;
using System.Collections.Generic;

public class ElevationDigraph :
    BidirectionalGraph<Hex, ElevationEdge> {
        
    public ElevationEdgeTypes GetEdgeTypeInDirection(
        Hex hex,
        HexDirections direction
    ) {
        IEnumerable<ElevationEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Direction == direction) {
                    return edge.EdgeType;
                }
            }
        }

        return ElevationEdgeTypes.Flat;
    }

    public bool AnyDirectionVisible(
        Hex hex
    ) {
        IEnumerable<ElevationEdge> edges;

        if (TryGetOutEdges(hex, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Delta <= 0)
                    return true;
            }
        }

        return false;
    }

    public List<HexEdge> GetVisibleEdges(
        Hex hex
    ) {

        IEnumerable<ElevationEdge> edges;
        List<HexEdge> result = new List<HexEdge>();

        if (TryGetOutEdges(hex, out edges)) {
            foreach(ElevationEdge edge in edges) {
                if (edge.Delta <= 0)
                    result.Add(edge);
            }
        }

        return result;
    }

    public static ElevationDigraph FromHexGrid(
        HexGrid<Hex> hexGrid
    ) {
        List<ElevationEdge> edges =
            new List<ElevationEdge>();

        for (
            int i = 0;
            i < hexGrid.Rows * hexGrid.Columns;
            i++
        ) {
            Hex current = hexGrid[i];

            foreach(
                Hex neighbor in hexGrid.GetNeighbors(i)
            ) {
                ElevationEdge outEdge = new ElevationEdge(
                    current,
                    neighbor,
                    CubeVector.HexDirectionWrapping(
                        current.CubeCoordinates,
                        neighbor.CubeCoordinates,
                        hexGrid.WrapSize
                    )
                );

                edges.Add(outEdge);

                ElevationEdge inEdge = new ElevationEdge(
                    neighbor,
                    current,
                    CubeVector.HexDirectionWrapping(
                        neighbor.CubeCoordinates,
                        current.CubeCoordinates,
                        hexGrid.WrapSize
                    )
                );

                edges.Add(inEdge);

            }
        }

        ElevationDigraph result =
            new ElevationDigraph();

        result.AddVerticesAndEdgeRange(edges);
        return result;
    }

    public Dictionary<HexDirections, ElevationEdgeTypes> GetNeighborEdgeTypes(
        Hex hex
    ) {
        Dictionary<HexDirections, ElevationEdgeTypes> result =
                new Dictionary<HexDirections, ElevationEdgeTypes>();
        
        for (int i = 0; i < 6; i++) {
            result.Add((HexDirections)i, ElevationEdgeTypes.Flat);
        }

        IEnumerable<ElevationEdge> edges;

        if(TryGetOutEdges(hex, out edges)) {
            

            foreach(ElevationEdge edge in edges) {
                result[edge.Direction] = edge.EdgeType;
            }

            return result;
        }

        return result;
    }
}