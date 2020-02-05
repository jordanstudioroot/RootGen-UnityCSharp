using UnityEngine;
using UnityEngine.EventSystems;
using Camera = UnityEngine.Camera;

public class GameInputHandler : MonoBehaviour
{
    private HexGrid _grid;
    private HexCell _currentCell;
    private HexUnit _selectedUnit;

    public void SetMode(int mode)
    {
        /*if (mode == 1)
            return;

        enabled = true;
        _grid.ShowUI(true);
        _grid.ClearPath();

        if (mode == 1)
        {
            Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        }
        else
        {
            Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
        }*/
    }

    private bool UpdateCurrentCell(HexGrid grid)
    {
        HexCell cell =
            grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));

        if (cell != _currentCell)
        {
            _currentCell = cell;
            return true;
        }

        return false;
    }

    private void DoSelection(HexGrid grid)
    {
        grid.ClearPath();
        UpdateCurrentCell(grid);
        if (_currentCell)
        {
            _selectedUnit = _currentCell.Unit;
        }
    }

    private void DoPathfinding(HexGrid grid)
    {
        if (UpdateCurrentCell(grid))
        {
            if (_currentCell && _selectedUnit.IsValidDestination(_currentCell))
            {
                grid.FindPath(_selectedUnit.Location, _currentCell, _selectedUnit);
            }
            else
            {
                grid.ClearPath();
            }
        }
    }

    private void DoMove(HexGrid grid)
    {
        if (grid.HasPath)
        {
            _selectedUnit.Travel(grid.GetPath());
            grid.ClearPath();
        }
    }

    private void Update()
    {
        if (!(_grid = GameObject.FindObjectOfType<HexGrid>()))
            return;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoSelection(_grid);
            }
            else if (_selectedUnit)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    DoMove(_grid);
                }
                else
                {
                    DoPathfinding(_grid);
                }
            }
        }
    }
}
