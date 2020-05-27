using System.Collections.Generic;
using UnityEngine;

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

        _riverOriginCandidates.Remove(riverOrigin);
    }

    private Hex ConsumeNextRiverOrigin() {
        Hex result = _riverOriginCandidates[
            Random.Range(0, _riverOriginCandidates.Count - 1)
        ];

        _riverOriginCandidates.Remove(result);

        return result;
    }

    public void Step(
        int waterLevel,
        int elevationMax,
        float extraLakeProbability,
        float hexOuterRadius,
        List<ClimateData> climate
    ) {
        List<RiverList> cache = new List<RiverList>();
        
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
            else {
                Hex riverOrigin = ConsumeNextRiverOrigin();
                cache.Add(
                    new RiverList(
                        _hexMap,
                        riverOrigin
                    )
                );
            }
        }

        foreach (RiverList cachedRiverList in cache) {
            cachedRiverList.Step(
                _adjacencyGraph,
                ref _riverDigraph,
                extraLakeProbability,
                hexOuterRadius,
                waterLevel
            );

            _riverLists.Add(cachedRiverList);
        }
    }

    public static void SetOutgoingRiver (
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
			return;
		}

		if (
            !IsValidRiverDestination(
                source,
                target
            )
        ) {
			return;
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

//		SetRoad((int)direction, false);
	}

    public static bool IsValidRiverDestination(Hex source, Hex target) {
		return target && (
			source.elevation >= target.elevation ||
            source.WaterLevel == target.elevation
		);
	}

    private class RiverList {
        private List<Hex> _river;
        private HexMap _hexMap;

        public Hex RiverHead {
            get {
                if (_river.Count > 0)
                    return _river[_river.Count - 1];
                return null;
            }
        }

        public RiverList(HexMap hexMap, Hex riverOrigin) {
            _hexMap = hexMap;
            _river = new List<Hex>();
            _river.Add(riverOrigin);
        }

        public Hex Step(
            HexAdjacencyGraph adjacencyGraph,
            ref RiverDigraph riverDigraph,
            float extraLakeProbability,
            float hexOuterRadius,
            int waterLevel
        ) {
            if (RiverHead.elevation < waterLevel)
                return null;
                
            int minNeighborElevation = int.MaxValue;

            List<HexDirections> flowDirections =
                new List<HexDirections>();

            HexDirections direction = HexDirections.Northeast;

            for (
                HexDirections directionCandidate = direction;
                directionCandidate <= HexDirections.Northwest;
                directionCandidate++
            ) {
                Hex neighborInDirection =
                      adjacencyGraph.TryGetNeighborInDirection(
                          RiverHead,
                          directionCandidate
                      );

                if (!neighborInDirection) {
                    continue;
                }

                if (neighborInDirection.elevation < minNeighborElevation) {
                    minNeighborElevation = neighborInDirection.elevation;
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

                    _river.Add(neighborInDirection);
                    return neighborInDirection;
                    /*riverDigraph.RemoveOutgoingRivers(RiverHead);
                    RiverEdge mergeEdge = new RiverEdge(
                        RiverHead,
                        riverBranch,
                        directionCandidate
                    );

                    riverDigraph.AddVerticesAndEdge(mergeEdge);
                    return riverBranch;*/
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
                    _river.Count == 1 ||
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
                if (_river.Count == 1) {
                    // Do nothing and return null
                    return null;
                }

                // If the hex is surrounded by hexes at a higher elevation,
                // set the water level of the hex to the minium elevation
                // of all neighbors.
                if (minNeighborElevation >= RiverHead.elevation) {
                    RiverHead.WaterLevel = minNeighborElevation;

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

            /*RiverEdge randomEdge = new RiverEdge(
                RiverHead,
                adjacencyGraph.TryGetNeighborInDirection(
                    RiverHead,
                    direction
                ),
                direction
            );*/

            // riverDigraph.AddVerticesAndEdge(randomEdge);

            // If the hex is lower than the minimum elevation of its
            // neighbors assign a lake based on a specified probability.
            if (
                minNeighborElevation >= RiverHead.elevation &&
                Random.value < extraLakeProbability
            ) {
                RiverHead.WaterLevel = RiverHead.elevation;

                RiverHead.SetElevation(
                    RiverHead.elevation - 1,
                    hexOuterRadius,
                    _hexMap.WrapSize
                );
            }

            _river.Add(neighborInRandomDirection);    
            return neighborInRandomDirection;
        }

        public bool HasLandTerminus(HexAdjacencyGraph adjacencyGraph) {
            IEnumerable<HexEdge> edges;

            if (adjacencyGraph.TryGetOutEdges(RiverHead, out edges)) {
                foreach (HexEdge edge in edges) {
                    if (edge.Target.elevation <= RiverHead.elevation) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}