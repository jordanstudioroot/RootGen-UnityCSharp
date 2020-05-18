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

    public T this[int index] {
        get {
            return GetElement(index);
        }

        set {
            SetElement(value, index);
        }
    }

    public T this[int row, int column] {
        get {
            return GetElement(row, column);
        }

        set {
            SetElement(value, row, column);
        }
    }

    public HexGrid(
        int rows,
        int columns,
        bool wrapping
    ) {
        if (rows <= 0 || columns <= 0)
            throw new System.ArgumentException(
                "A hex grid must have at least 1 column and 1 row."
            );

        Rows = rows;
        Columns = columns;
        _dictionary = new Dictionary<int, T>();
        IsWrapping = wrapping;

        for (int index = 0, row = 0; row < rows; row++) {
            for (int column = 0; column < columns; column++) {
                index = AxialCoordinatesToIndex(row, column);
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
        int index = AxialCoordinatesToIndex(row, column);
        SetElement(element, index);
    }

    public T GetElement(int row, int column) {
        int index = AxialCoordinatesToIndex(row, column);
        return GetElement(index);
    }

    public void SetElement(T element, int index) {
        if (IsInBounds(index)) {
            _dictionary[index] = element;
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
            int row = RowIndex(index);
            int column = ColumnIndex(index);
            
            T origin = GetElement(index);

            for (int i = row - 1; i <= row + 1; i++) {
                for (int j = column - 1; j <= column + 1; j++) {
                    try {
                        T neighborCandidate =
                            this[row + i, column + j];

                        if (
                            CubeVector.AreAdjacent(
                                origin.CubeCoordinates,
                                neighborCandidate.CubeCoordinates,
                                WrapSize
                            )
                        ) {
                            result.Add(
                                neighborCandidate
                            );
                        }
                    }
                    catch(OutOfGridBoundsException e) {
                        RootLog.Log(
                            "Row: " + i + ", Column: " + j + " is " + 
                            "outside of the grid bounds.",
                            Severity.Warning,
                            "HexGrid.Neighbors"
                        );
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

/// <summary>
/// Return the corresponding zero-indexed column from a row-major array
/// index.
/// </summary>
/// <param name="index">
/// A row-major array index.
/// </param>
/// <returns>
/// The zero-index column corresponding to the zero-indexed row major
/// array index.
/// </returns>
    public int RowIndex(int index) {
        return IsInBounds(index) ?
            (index / Columns) :
            throw new OutOfGridBoundsException();
    }

/// <summary>
/// Return the corresponding zero-indexed row from a row-major array
/// index.
/// </summary>
/// <param name="index">
/// A row-major array index.
/// </param>
/// <returns>
/// The zero-indexed row corresponding to the zero-indexed row-major
/// array index.
/// </returns>
    public int ColumnIndex(int index) {
        return IsInBounds(index) ?
            index - (RowIndex(index) * Columns) :
            throw new OutOfGridBoundsException();
    }

    public override string ToString() {
        string leftPad = "              ";
        string innerPad = "   ";

        string result = "";

        for (int row = Rows - 1; row > -1; row--) {
            for (int i = row; i > 0; i--)
                result += leftPad;

            for (int column = 0; column < Columns; column++) {
                result +=
                    GetElement(row, column).CubeCoordinates.ToString() +
                    innerPad;
            }

            result += "\n\n";
        }

        return result;
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

    private int AxialCoordinatesToIndex(int row, int column) {
        return (row  * Columns) + column;
    }
}