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

    public int SizeSquared {
        get {
            return Rows * Columns;
        }
    }

    public bool IsWrapping {
        get; private set;
    }

    public int WrapSize {
        get {
            return IsWrapping ?  Columns : 0;
        }
    }

    public T this[int rowMajorIndex] {
        get {
            return GetElement(rowMajorIndex);
        }

        set {
            SetElement(value, rowMajorIndex);
        }
    }

    public T this[int offsetX, int offsetZ] {
        get {
            return GetElement(offsetX, offsetZ);
        }

        set {
            SetElement(value, offsetX, offsetZ);
        }
    }

    public T this[int cubicX, int cubicY, int cubicZ] {
        get {
            return GetElement(cubicX, cubicY, cubicZ);
        }

        set {
            SetElement(value, cubicX, cubicY, cubicZ);
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
                index = OffsetToIndex(row, column);
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

    public T GetElement(int x, int y, int z) {
        throw new System.NotImplementedException();
    }

    public T SetElement(T element, int x, int y, int z) {
        throw new System.NotImplementedException();
    }

    public T GetElement(int offsetX, int offsetZ) {
        int index = OffsetToIndex(offsetX, offsetZ);
        return GetElement(index);
    }

    public void SetElement(T element, int offsetX, int offsetZ) {
        int index = OffsetToIndex(offsetX, offsetZ);
        SetElement(element, index);
    }

    public void SetElement(T element, int rowMajorIndex) {
        if (IsInBounds(rowMajorIndex)) {
            _dictionary[rowMajorIndex] = element;
        }
        else {
            throw new OutOfGridBoundsException();
        }
    }

    public T GetElement(int rowMajorIndex) {
        if (IsInBounds(rowMajorIndex)) {
            T value;

            if (_dictionary.TryGetValue(rowMajorIndex, out value)) {
                if (value == null) 
                    throw new System.NullReferenceException(
                        "index: " + rowMajorIndex
                    );
            }
            else {
                throw new OutOfGridBoundsException();
            }

            return value;
        }
        else {
            throw new OutOfGridBoundsException();
        }
    }

    public List<T> Neighbors(int index) {
        List<T> result = new List<T>();
        T origin = default(T);

        if (IsInBounds(index)) {
            int row = RowIndex(index);
            int column = ColumnIndex(index);
            
            origin = GetElement(index);        

            for (int i = row - 1; i <= row + 1; i++) {
                for (int j = column - 1; j <= column + 1; j++) {
                    try {
                        int neighborIndex = OffsetToIndex(
                            j, i
                        );

                        T neighborCandidate =
                            this[neighborIndex];

                        if (
                            CubeVector.HexTileDistance(
                                origin.CubeCoordinates,
                                neighborCandidate.CubeCoordinates,
                                WrapSize
                            ) == 1
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
//            for (int i = row; i > 0; i--)
//                result += leftPad;
            if (row % 2 != 0)
                result += leftPad;    
            for (int column = 0; column < Columns; column++) {
                result +=
                    GetElement(column, row).CubeCoordinates +
                    innerPad;
            }

            result += "\n\n";
        }

        return result;
    }

    private bool IsInBounds(int index) {
        return (
            index >= 0 &&
            index < SizeSquared
        );
    }

    private int OffsetToIndex(int x, int z) {
        if (x > Columns - 1)
            x -= WrapSize;
        else if (x < 0) {
            x += WrapSize;
        }
        
        return (z * Columns) + x;
    }
}