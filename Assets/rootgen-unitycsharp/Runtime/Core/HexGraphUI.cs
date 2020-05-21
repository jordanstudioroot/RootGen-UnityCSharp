using UnityEngine;
using System.Collections.Generic;

public class HexGraphUI : MonoBehaviour {
/// <summary>
///     Sets the distance labels and enables highlights for the
///     currently active path with the given speed.
/// </summary>
/// <param name="speed">
///     The travelling speed for the given path.
/// </param>
    private void SetPathDistanceLabelAndEnableHighlights(
        List<HexEdge> path,
        int speed
    ) {
        int distance = 0;

        for (int i = 0; i < path.Count; i++) {
            Hex current = path[i].Target;
            int turn = distance / speed;
            current.SetLabel(turn.ToString());

            if (i == 0) {
                current.EnableHighlight(Color.blue);
            }
            else if (i == path.Count - 1) {
                current.EnableHighlight(Color.red);
            }
            else {
                current.EnableHighlight(Color.white);
            }
            
            distance++;
        }
    }
}