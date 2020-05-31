using System.Collections.Generic;
using UnityEngine;
using RootLogging;

public class HexMapRivers {
    private RiverDigraph _riverDigraph;
    public RiverDigraph RiverDigraph {
        get {
            return _riverDigraph;
        }
    }
    private HexMap _hexMap;
    private List<RiverList> _riverLists;
    private List<Hex> _riverOriginCandidates;
    private HexAdjacencyGraph _adjacencyGraph;
    public HexMapRivers(
        HexMap hexMap,
        List<ClimateData> climate,
        int waterLevel,
        int elevationMax
    ) {
        _hexMap = hexMap;
        _riverLists = new List<RiverList>();
        _riverOriginCandidates = new List<Hex>();
        _riverDigraph = new RiverDigraph();
        _adjacencyGraph = hexMap.AdjacencyGraph;

        for (int i = 0; i < _hexMap.SizeSquared; i++) {
            Hex hex = _hexMap.GetHex(i);

            if (hex.IsUnderwater) {
                continue;
            }

            ClimateData data = climate[i];

            float weight =
                data.moisture * (hex.elevation - waterLevel) /
                (elevationMax - waterLevel);

            if (weight > 0.75) {
                _riverOriginCandidates.Add(hex);
                _riverOriginCandidates.Add(hex);
            }

            if (weight > 0.5f) {
                _riverOriginCandidates.Add(hex);
            }

            if (weight > 0.25f) {
                _riverOriginCandidates.Add(hex);
            }
        }
    }

    public void StartRiver() {
        Hex riverOrigin = ConsumeNextRiverOrigin();

        _riverLists.Add(
            new RiverList(
                _hexMap,
                riverOrigin
            )
        );
    }

    private Hex ConsumeNextRiverOrigin() {
        Hex result = _riverOriginCandidates[
            Random.Range(0, _riverOriginCandidates.Count - 1)
        ];

        _riverOriginCandidates.RemoveAll(
            (a) => { return a == result; }
        );

        return result;
    }

    public void Step(
        int waterLevel,
        int elevationMax,
        float extraLakeProbability,
        float hexOuterRadius,
        List<ClimateData> climate
    ) {
        foreach(RiverList riverList in _riverLists) {        
            Hex branch =
                riverList.Step(
                    _adjacencyGraph,
                    ref _riverDigraph,
                    extraLakeProbability,
                    hexOuterRadius,
                    waterLevel
                );

            if (branch)
                _riverOriginCandidates.Remove(branch);
        }

        /*string logString = "River Stats This Step\n";

        foreach (RiverList list in _riverLists) {
            foreach (Hex hex in list.River) {
                logString += hex + " -> ";
            }

            logString += "\n";
        }

        RootLog.Log(logString, Severity.Information, "HexMapRivers");*/
    }

    public static bool SetOutgoingRiver (
        Hex source,
        Hex target,
        HexDirections direction,
        ref RiverDigraph riverDigraph
    ) {
		if (
            riverDigraph.HasOutgoingRiverInDirection(
                source,
                direction
            )
        ) {
			return false;
		}

		if (
            !IsValidRiverDestination(
                source,
                target
            )
        ) {
			return false;
		}

        riverDigraph.RemoveOutgoingRivers(
            source
        );

        if (
            riverDigraph.HasIncomingRiverInDirection(
                source,
                direction
            )
        ) {
            riverDigraph.RemoveIncomingRivers(
                source
            );
        }

        riverDigraph.RemoveIncomingRivers(target);


        riverDigraph.AddVerticesAndEdge(
            new RiverEdge(
                source,
                target,
                direction
            )
        );

        return true;
        
//		SetRoad((int)direction, false);
	}

    public static bool IsValidRiverDestination(Hex source, Hex target) {
		return target && (
			source.elevation >= target.elevation ||
            source.WaterLevel == target.elevation
		);
	}

    private class RiverList {
        public List<Hex> River { get; private set; }
        private HexMap _hexMap;

        public int Length {
            get {
                return River.Count;
            }
        }

        public Hex RiverHead {
            get {
                if (River.Count > 0)
                    return River[River.Count - 1];
                return null;
            }
        }

        public RiverList(HexMap hexMap, Hex riverOrigin) {
            _hexMap = hexMap;
            River = new List<Hex>();
            River.Add(riverOrigin);
        }

        public Hex Step(
            HexAdjacencyGraph adjacencyGraph,
            ref RiverDigraph riverDigraph,
            float extraLakeProbability,
            float hexOuterRadius,
            int waterLevel
        ) {
            if (RiverHead.IsUnderwater)
                return null;
                
            int minNeighborElevation = int.MaxValue;
            int minNeighborWaterLevel = int.MaxValue;

            List<HexDirections> flowDirections = new List<HexDirections>();
            HexDirections direction = HexDirections.Northeast;

            for (
                HexDirections directionCandidate = HexDirections.Northeast;
                directionCandidate <= HexDirections.Northwest;
                directionCandidate++
            ) {
                Hex neighborInDirection =
                      adjacencyGraph.TryGetNeighborInDirection(
                          RiverHead,
                          directionCandidate
                      );

                if (!neighborInDirection || River.Contains(neighborInDirection)) {
                    continue;
                }

                if (neighborInDirection.elevation < minNeighborElevation) {
                    minNeighborElevation = neighborInDirection.elevation;
                    minNeighborWaterLevel = neighborInDirection.WaterLevel;
                }

                // If the direction points to the river origin, or to a
                // neighbor which already has an incoming river, continue.
                if (
                    neighborInDirection == RiverHead ||
                    riverDigraph.HasIncomingRiver(neighborInDirection)
                ) {
                    continue;
                }

                int delta =
                    neighborInDirection.elevation - RiverHead.elevation;
                    
                // If the elevation in the given direction is positive,
                // continue.
                if (delta > 0) {
                    
                    continue;
                }

                // If the direction points away from the river origin and
                // any neighbors which already have an incoming river, and
                // the elevation in the given direction is negative or
                // zero, and the neighbor has an outgoing river, branch
                // river in this direction.
                if (
                    riverDigraph.HasOutgoingRiver(neighborInDirection)
                ) {
                    SetOutgoingRiver(
                        RiverHead,
                        neighborInDirection,
                        directionCandidate,
                        ref riverDigraph
                    );

                    River.Add(neighborInDirection);
                    return neighborInDirection;
                }

                // If the direction points away from the river origin and
                // any neighbors which already have an incoming river, and
                // the elevation in the given direction is not positive,
                // and the neighbor does not have an outgoing river in the
                // given direction...

                // If the direction is a decline, make the probability for
                // the branch 4 / 5.
                if (delta < 0) {
                    flowDirections.Add(directionCandidate);
                    flowDirections.Add(directionCandidate);
                    flowDirections.Add(directionCandidate);
                }

                // If the rivers local length is 1, and the direction does
                // not result in a slight river bend, but rather a straight
                // river or a corner river, make the probability of the
                // branch 2 / 5
                if (
                    River.Count == 1 ||
                    (directionCandidate != direction.NextClockwise2() &&
                    directionCandidate != direction.PreviousClockwise2())
                ) {
                    flowDirections.Add(directionCandidate);
                }

                flowDirections.Add(directionCandidate);
            }

            // If there are no candidates for branching the river...
            if (flowDirections.Count == 0) {
                // If the river contains only the river origin...
                if (River.Count == 1) {
                    // Do nothing and return null
                    return null;
                }

                // If the hex is surrounded by hexes at a higher elevation,
                // set the water level of the hex to the minium elevation
                // of all neighbors.
                if (minNeighborElevation >= RiverHead.elevation) {
                    RiverHead.WaterLevel = RiverHead.elevation;

                    // If the hex is of equal elevation to a neighbor with
                    // a minimum elevation, lower the current hexes
                    // elevation to one below the minimum elevation of all
                    // of its neighbors so that it becomes a small lake
                    // that the river feeds into, and then break out of the
                    // while statement terminating the river in a lake
                    // rather than into the ocean.
                    if (minNeighborElevation == RiverHead.elevation) {
                        RiverHead.SetElevation(
                            minNeighborElevation - 1,
                            hexOuterRadius,
                            _hexMap.WrapSize
                        );
                    }
                }

                return null;
            }

            direction = flowDirections[
                Random.Range(0, flowDirections.Count)
            ];

            Hex neighborInRandomDirection =
                adjacencyGraph.TryGetNeighborInDirection(
                    RiverHead,
                    direction
                );
            
            SetOutgoingRiver(
                RiverHead,
                neighborInRandomDirection,
                direction,
                ref riverDigraph
            );

            // If the hex is lower than the minimum elevation of its
            // neighbors assign a lake based on a specified probability.
            if (
                minNeighborElevation >= RiverHead.elevation &&
                Random.value < extraLakeProbability
            ) {
                RiverHead.WaterLevel = RiverHead.elevation;

                RiverHead.SetElevation(
                    minNeighborElevation - 1,
                    hexOuterRadius,
                    _hexMap.WrapSize
                );
            }

            River.Add(neighborInRandomDirection);    
            return neighborInRandomDirection;
        }

        private void VisualizeRiverOrigins(
            HexMap hexMap,
            int hexCount,
            int waterLevel,
            int elevationMax,
            List<ClimateData> climate
        ) {
            for (int i = 0; i < hexCount; i++) {
                Hex hex = hexMap.GetHex(i);

                float data = climate[i].moisture * (hex.elevation - waterLevel) /
                                (elevationMax - waterLevel);

                if (data > 0.75f) {
                    hex.SetMapData(1f);
                }
                else if (data > 0.5f) {
                    hex.SetMapData(0.5f);
                }
                else if (data > 0.25f) {
                    hex.SetMapData(0.25f);
                }
            }
        }
    }
}