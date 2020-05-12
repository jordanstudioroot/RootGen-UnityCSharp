using UnityEngine;

public class HexGridPresentation : MonoBehaviour {
/// <summary>
///     Sets the distance labels and enables highlights for the
///     currently active path with the given speed.
/// </summary>
/// <param name="speed">
///     The travelling speed for the given path.
/// </param>
    private void SetPathDistanceLabelAndEnableHighlights(
        HexCell currentPathTo,
        int speed
    ) {
        if (_currentPathExists) {
            HexCell current = currentPathTo;

            while (current != _currentPathFrom) {
                int turn = (current.Distance - 1) / speed;
                current.SetLabel(turn.ToString());
                current.EnableHighlight(Color.white);
                current = current.PathFrom;
            }
        }

        _currentPathFrom.EnableHighlight(Color.blue);
        currentPathTo.EnableHighlight(Color.red);
    }
}