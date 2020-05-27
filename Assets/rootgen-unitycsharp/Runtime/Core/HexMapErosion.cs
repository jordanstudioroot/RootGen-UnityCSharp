using System.Collections.Generic;
using UnityEngine;
using RootCollections;

public class HexMapErosion {
    /// <summary>
    /// A constant representing the required difference in distance
    /// between a hex and one, some, or all of its neighbors in order
    /// for that hex to be considered erodible.
    /// <summary>
    private const int DELTA_ERODIBLE_THRESHOLD = 2;

    private HexMap _hexMap;
    public List<Hex> ErodibleHexes { get; private set; } 

    public HexMapErosion(HexMap hexMap) {
        _hexMap = hexMap;
        ErodibleHexes = new List<Hex>();

        foreach (Hex erosionCandidate in _hexMap.Hexes) {
                List<Hex> erosionCandidateNeighbors;

                _hexMap.TryGetNeighbors(
                    erosionCandidate,
                    out erosionCandidateNeighbors
                );

                if (
                    IsErodible(
                        erosionCandidate,
                        erosionCandidateNeighbors
                    )
                ) {
                    ErodibleHexes.Add(erosionCandidate);
                }
            }
    }

    public void Step(float hexOuterRadius) {
        // Select a random hex from the erodible hexes.
        int index = Random.Range(0, ErodibleHexes.Count);
        Hex originHex = ErodibleHexes[index];

        // Get the candidates for erosion runoff for the selected hex.
        List<Hex> originNeighborHexes;

        if(
            _hexMap.TryGetNeighbors(
                originHex,
                out originNeighborHexes
            )
        ) {
            Hex runoffHex =
                GetErosionRunoffTarget(
                    originHex,
                    originNeighborHexes
                );

            // Lower the elevation of the hex being eroded.
            originHex.SetElevation(
                originHex.elevation - 1,
                hexOuterRadius,
                _hexMap.WrapSize
            );

            // Raise the elevation of the hex selected for runoff.
            runoffHex.SetElevation(
                runoffHex.elevation + 1,
                hexOuterRadius,
                _hexMap.WrapSize
            );

            // If the hex is not erodible after this erosion step,
            // remove it from the list of erodible hexes.
            if (
                !IsErodible(
                    originHex,
                    originNeighborHexes
                )
            ) {
                ErodibleHexes[index] =
                    ErodibleHexes[ErodibleHexes.Count - 1];

                ErodibleHexes.RemoveAt(ErodibleHexes.Count - 1);
            }
            
            // For each neighbor of the current hex...
            foreach(Hex originNeighbor in originNeighborHexes) {
                // If the elevation of the hexes neighbor is is exactly
                // 2 steps higher than the current hex, and is not an
                // erodible hex...
                if (
                    (
                        originNeighbor.elevation ==
                        originHex.elevation + DELTA_ERODIBLE_THRESHOLD
                    ) &&
                    !ErodibleHexes.Contains(originNeighbor)
                ) {
                    // ...this erosion step has modified the map so
                    // that the hex is now erodible, so add it to the
                    // list of erodible hexes.
                    ErodibleHexes.Add(originNeighbor);
                }
            }

            List<Hex> runoffNeighborHexes;

            // If the target of the runoff is now erodible due to the
            // change in elevation, add it to the list of erodible
            // hexes.
            if (
                _hexMap.TryGetNeighbors(
                    runoffHex,
                    out runoffNeighborHexes
                )
            ) {
                if (
                    IsErodible(
                        runoffHex,
                        runoffNeighborHexes
                    ) && 
                    !ErodibleHexes.Contains(runoffHex)
                ) {
                    ErodibleHexes.Add(runoffHex);
                }

                foreach (
                    Hex runoffNeighbor in
                    runoffNeighborHexes
                ) {
                    List<Hex> runoffNeighborNeighborHexes;

                    if (
                        _hexMap.TryGetNeighbors(
                            runoffNeighbor,
                            out runoffNeighborNeighborHexes
                        ) &&
                        runoffNeighbor != originHex &&
                        (
                            runoffNeighbor.elevation ==
                            runoffHex.elevation + 1 
                        ) &&
                        !IsErodible(
                            runoffNeighbor,
                            runoffNeighborNeighborHexes
                        )
                    ) {
                        ErodibleHexes.Remove(runoffNeighbor);
                    }   
                }
            }
        }
    }

    public void ErodePercetage(
        int erosionPercentage,
        float hexOuterRadius
    ) {
        List<Hex> erodibleHexes = ListPool<Hex>.Get();

        // For each hex in the hex map, check if the hex is erodible.
        // If it is add it to the list of erodible hexes.
        foreach (Hex erosionCandidate in _hexMap.Hexes) {
            List<Hex> erosionCandidateNeighbors;

            _hexMap.TryGetNeighbors(
                erosionCandidate,
                out erosionCandidateNeighbors
            );

            if (
                IsErodible(
                    erosionCandidate,
                    erosionCandidateNeighbors
                )
            ) {
                erodibleHexes.Add(erosionCandidate);
            }
        }

        // Calculate the target number of uneroded hexes.
        int targetUnerodedHexes =
            (int)(
                erodibleHexes.Count *
                (100 - erosionPercentage) *
                0.01f
            );

        // While the number of erodible hexes is greater than the target
        // number of uneroded hexes...
        while (erodibleHexes.Count > targetUnerodedHexes) {

            // Select a random hex from the erodible hexes.
            int index = Random.Range(0, erodibleHexes.Count);
            Hex originHex = erodibleHexes[index];

            // Get the candidates for erosion runoff for the selected hex.
            List<Hex> originNeighborHexes;

            if(
                _hexMap.TryGetNeighbors(
                    originHex,
                    out originNeighborHexes
                )
            ) {
                Hex runoffHex =
                    GetErosionRunoffTarget(
                        originHex,
                        originNeighborHexes
                    );

                // Lower the elevation of the hex being eroded.
                originHex.SetElevation(
                    originHex.elevation - 1,
                    hexOuterRadius,
                    _hexMap.WrapSize
                );

                // Raise the elevation of the hex selected for runoff.
                runoffHex.SetElevation(
                    runoffHex.elevation + 1,
                    hexOuterRadius,
                    _hexMap.WrapSize
                );

                // If the hex is not erodible after this erosion step,
                // remove it from the list of erodible hexes.
                if (
                    !IsErodible(
                        originHex,
                        originNeighborHexes
                    )
                ) {
                    erodibleHexes[index] =
                        erodibleHexes[erodibleHexes.Count - 1];

                    erodibleHexes.RemoveAt(erodibleHexes.Count - 1);
                }
                
                // For each neighbor of the current hex...
                foreach(Hex originNeighbor in originNeighborHexes) {
                    // If the elevation of the hexes neighbor is is exactly
                    // 2 steps higher than the current hex, and is not an
                    // erodible hex...
                    if (
                        (
                            originNeighbor.elevation ==
                            originHex.elevation + DELTA_ERODIBLE_THRESHOLD
                        ) &&
                        !erodibleHexes.Contains(originNeighbor)
                    ) {
                        // ...this erosion step has modified the map so
                        // that the hex is now erodible, so add it to the
                        // list of erodible hexes.
                        erodibleHexes.Add(originNeighbor);
                    }
                }

                List<Hex> runoffNeighborHexes;

                // If the target of the runoff is now erodible due to the
                // change in elevation, add it to the list of erodible
                // hexes.
                if (
                    _hexMap.TryGetNeighbors(
                        runoffHex,
                        out runoffNeighborHexes
                    )
                ) {
                    if (
                        IsErodible(
                            runoffHex,
                            runoffNeighborHexes
                        ) && 
                        !erodibleHexes.Contains(runoffHex)
                    ) {
                        erodibleHexes.Add(runoffHex);
                    }

                    foreach (
                        Hex runoffNeighbor in
                        runoffNeighborHexes
                    ) {
                        List<Hex> runoffNeighborNeighborHexes;

                        if (
                            _hexMap.TryGetNeighbors(
                                runoffNeighbor,
                                out runoffNeighborNeighborHexes
                            ) &&
                            runoffNeighbor != originHex &&
                            (
                                runoffNeighbor.elevation ==
                                runoffHex.elevation + 1 
                            ) &&
                            !IsErodible(
                                runoffNeighbor,
                                runoffNeighborNeighborHexes
                            )
                        ) {
                            erodibleHexes.Remove(runoffNeighbor);
                        }   
                    }
                }
            }
        }

        ListPool<Hex>.Add(erodibleHexes);
    }

    /// <summary>
    /// Gets a boolean value representing whether the specified hex is
    /// erodible based on the state of its neighbors.
    /// </summary>
    /// <param name="candidate">
    /// The provided hex.
    /// </param>
    /// <param name="candidateNeighbors">
    /// The provided hexes neighbors.
    /// </param>
    /// <returns>
    /// A boolean value representing whether the specified hex is erodible
    /// based on the state of its neighbors.
    /// </returns>
    private bool IsErodible(
        Hex candidate,
        List<Hex> candidateNeighbors
    ) {
        // For each neighbor of this hex...
        foreach (Hex neighbor in candidateNeighbors) {

            if (
                neighbor &&
                (

                    // If the neighbors elevation is less than or equal to
                    // the erosion threshold value...
                    neighbor.elevation <=
                    candidate.elevation - DELTA_ERODIBLE_THRESHOLD
                )
            ) {
                return true;
            }
        }

        return false;
    }

    private Hex GetErosionRunoffTarget(
        Hex origin,
        List<Hex> neighbors
    ) {
        List<Hex> candidates = ListPool<Hex>.Get();
        int erodibleElevation =
            origin.elevation - DELTA_ERODIBLE_THRESHOLD;

        foreach (Hex neighbor in neighbors) {
            if (neighbor && neighbor.elevation <= erodibleElevation) {
                candidates.Add(neighbor);
            }
        }

        Hex target = candidates[Random.Range(0, candidates.Count)];
            
        ListPool<Hex>.Add(candidates);
        return target;
    }
}