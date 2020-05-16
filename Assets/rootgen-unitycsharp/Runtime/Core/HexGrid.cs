using System.Collections.Generic;
using UnityEngine;
using RootLogging;

public class HexGrid<T> where T : IHexPoint {
    private Dictionary<int, T> _dictionary;
    public int Rows {
        get; private set;
    }

    public int Columns {
        get; private set;
    }

    public bool IsWrapping {
        get; private set;
    }

    public int WrapSize {
        get {
            return IsWrapping ?  Rows : 0;
        }
    }

    public HexGrid(
        int rows,
        int columns,
        bool wrapping
    ) {
        Rows = rows;
        Columns = columns;
        _dictionary = new Dictionary<int, T>();
        IsWrapping = wrapping;

        for (int index = 0, row = 0; row < rows; row++) {
            for (int column = 0; column < columns; column++) {
                index = RowAndColumnToIndex(row, column);
                _dictionary.Add(
                    index,
                    default(T)
                );
            }
        }
    }

    public T Center {
        get {
            return GetElement(Rows/2, Columns/2);
        }
    }

    public int Size {
        get {
            return Rows * Columns;
        }
    }

    public void SetElement(T element, int row, int column) {
        int index = RowAndColumnToIndex(row, column);
        SetElement(element, index);
    }

    public T GetElement(int row, int column) {
        int index = RowAndColumnToIndex(row, column);

        return GetElement(index);
    }

    public void SetElement(T element, int index) {
        Debug.Log(index);
        if (IsInBounds(index)) {
            _dictionary[index] = element;
            Debug.Log(_dictionary[index]);
        }
        else {
            throw new OutOfGridBoundsException();
        }
    }

    public T GetElement(int index) {
        if (IsInBounds(index)) {
            T value;

            if (_dictionary.TryGetValue(index, out value)) {
                if (value == null) 
                    throw new System.NullReferenceException();
            }
            else {
                throw new System.IndexOutOfRangeException();
            }

            return value;
        }
        else {
            throw new OutOfGridBoundsException();
        }
    }

    public List<T> Neighbors(int index) {
        List<T> result = new List<T>();

        if (IsInBounds(index)) {
            int row = RowFromIndex(index);
            int column = ColumnFromIndex(index);
            
            T origin = GetElement(index);

            for (int i = row - 1; i <= row + 1; i++) {
                for (int j = column - 1; j <= column + 1; j++) {
                    try {
                        T neighborCandidate =
                            GetElement(row, column);

                        if (
                            origin.HexCoordinates.IsNeighborOf(
                                neighborCandidate.HexCoordinates,
                                WrapSize
                            )
                        ) {
                            RootLog.Log(
                                GetElement(row, column).ToString(),
                                Severity.Information,
                                "Neighbors"
                            );
                            result.Add(
                                GetElement(row, column)
                            );
                        }
                    }
                    catch(System.ArgumentException e) {

                    }
                }
            }
        }

        return result;
    }

    public T[] ToArray() {
        T[] result = new T[_dictionary.Keys.Count];

        for (int i = 0; i < result.Length; i++) {
            result[i] = _dictionary[i];
        }

        return result;
    }

    public int RowFromIndex(int index) {
        if (Rows == 0) {
            return 0;
        }

        return (index / Rows);
    }

    public int ColumnFromIndex(int index) {
        return index - ((RowFromIndex(index)) * Columns);
    }

    private bool IsInBounds(int row, int column) {
        return(
            row >= 0 &&
            column >= 0 &&
            row < Rows &&
            column < Columns
        );
    }

    private bool IsInBounds(int index) {
        return (
            index >= 0 &&
            index < (Rows * Columns)
        );
    }

    private int RowAndColumnToIndex(int row, int column) {
        return (row * Columns) + column;
    }
}