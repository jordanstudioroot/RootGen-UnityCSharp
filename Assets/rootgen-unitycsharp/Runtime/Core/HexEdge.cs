using QuikGraph;

public class HexEdge : Edge<HexCell> {
    public bool HasRiver {
        get; private set;
    }

    public bool HasRoad {
        get; private set;
    }

    public HexDirection Direction {
        get; private set;
    }

    public HexEdge(
        HexCell source,
        HexCell target
    ) : base(source, target) {
           
    }

    public HexEdge(
        HexCell source,
        HexCell target,
        bool hasRiver,
        bool hasRoad
    ) : base(source, target) {
        HasRiver = hasRiver;
        HasRoad = hasRoad;
    }
}