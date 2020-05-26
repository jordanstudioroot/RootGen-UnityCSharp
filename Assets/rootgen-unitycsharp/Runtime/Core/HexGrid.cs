using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using RootLogging;

public class HexGrid<T> where T : IHexPoint {
    #region Constant Fields
    #endregion

    #region Fields

    #region Public Fields
    #endregion

    #region Private Fields

    /// <summary>
    /// The value dictionary for elements of the grid, indexed by
    /// row-major coordinates.
    /// </summary>
    private Dictionary<int, T> _elements;
    private Dictionary<T, int> _indicies;

    #endregion

    #endregion
    
    #region Constructors

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="rows">
    /// The desired number of rows.
    /// </param>
    /// <param name="columns">
    /// The desired number of columns.
    /// </param>
    /// <param name="wrapping">
    /// A boolean value specifying whether the longitudinal edges of the grid,
    /// should wrap into one another.
    /// </param>
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
        _elements = new Dictionary<int, T>();
        _indicies = new Dictionary<T, int>();
        IsWrapping = wrapping;

        for (int row = 0; row < rows; row++) {
            for (int column = 0; column < columns; column++) {
                int index = ConvertOffsetToIndex(column, row);

                T element = default(T);

                _elements.Add(
                    index,
                    element
                );
            }
        }
    }

    #endregion
    
    #region Finalizers (Destructors)
    #endregion
    
    #region Delegates
    #endregion
    
    #region Events
    #endregion
    
    #region Enums
    #endregion
    
    #region Interfaces
    #endregion
    
    #region Properties
    
    #region Public Properties
    #endregion

    #region Public Readonly Properties

    /// <summary>
    /// The number of rows in the grid.
    /// </summary>
    /// <value>
    /// The number of rows in the grid.
    /// </value>
    public int Rows {
        get; private set;
    }

    /// <summary>
    /// The number of columns in the grid.
    /// </summary>
    /// <value>
    /// The number of columns in the grid.
    /// </value>
    public int Columns {
        get; private set;
    }

    /// <summary>
    /// The size of the grid squared. Equal to the number of elements
    /// in the grid.
    /// </summary>
    /// <value>
    /// The size of the grid squared. Equal to the number of elements
    /// in the grid.
    /// </value>
    public int SizeSquared {
        get {
            return Rows * Columns;
        }
    }

    /// <summary>
    /// A boolean value representing whether or not elements on the
    /// longitudinal edges of the grid should be considered adjacent.
    /// </summary>
    /// <value>
    /// A boolean value representing whether or not elements on the
    /// longitudinal edges of the grid should be considered adjacent.
    /// </value>
    public bool IsWrapping {
        get; private set;
    }

    /// <summary>
    /// The size of the offset which should be used to reference adjacent
    /// elements on the longitudinal edges of the grid. If wrapping is not
    /// enabled this returns 0.
    /// </summary>
    /// <value>
    /// The size of the offset which should be used to reference adjacent
    /// elements on the longitudinal edges of the grid. If wrapping is not
    /// enabled this returns 0.
    /// </value>
    public int WrapSize {
        get {
            return IsWrapping ?  Columns : 0;
        }
    }

    /// <summary>
    /// The value assigned to the center-most index (rounded down) of the grid,
    /// rounded down.
    /// </summary>
    /// <value>
    /// The value assigned to the center-most index (rounded down) of the grid,
    /// rounded down.
    /// </value>
    public T Center {
        get {
            return GetElement(Columns/2, Rows/2);
        }
    }

    #endregion

    #region Private Properties
    #endregion

    #endregion
    
    #region Indexers

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

    public T this[int cubeX, int cubeY, int cubeZ] {
        get {
            return GetElement(cubeX, cubeY, cubeZ);
        }

        set {
            SetElement(value, cubeX, cubeY, cubeZ);
        }
    }

    #endregion
    
    #region Methods

    #region Public Methods
    /// <summary>
    /// Gets the element at the specified cube coordinates.
    /// </summary>
    /// <param name="element">
    /// The element to be assigned.
    /// </param>
    /// <param name="cubeX">
    /// The specified x (right diagonal longitudinal) coordinates.
    /// </param>
    /// <param name="cubeY">
    /// The specified y (left diagonal longitudinal) coordinates.
    /// </param>
    /// <param name="cubeZ">
    /// The specified z (latitudinal) coordiantes.
    /// </param>
    /// <returns>
    /// The element at the specified cubic coordinates.
    /// </returns>
    public T GetElement(int cubeX, int cubeY, int cubeZ) {
        return GetElement(
            ConvertCubeToIndex(
                cubeX,
                cubeY,
                cubeZ
            )
        );
    }

    /// <summary>
    /// Assigns the specified element to the specified cube coordinates. 
    /// </summary>
    /// <param name="element">
    /// The element to be assigned.
    /// </param>
    /// <param name="cubicX">
    /// The specified x (right diagonal longitudinal) coordinates.
    /// </param>
    /// <param name="cubicY">
    /// The specified y (left diagonal longitudinal) coordinates.
    /// </param>
    /// <param name="cubicZ">
    /// The specified z (latitudinal) coordiantes.
    /// </param>
    /// <returns></returns>
    public void SetElement(T element, int cubicX, int cubicY, int cubicZ) {
        SetElement(
            element,
            ConvertCubeToIndex(
                cubicX,
                cubicY,
                cubicZ
            )
        );
    }

    /// <summary>
    /// Gets the element at the specified offset coordinates.
    /// </summary>
    /// <param name="offsetX">
    /// The specified x (longitudinal) coordinate.
    /// </param>
    /// <param name="offsetZ">
    /// The specified z (latitudinal) coordinate.
    /// </param>
    /// <returns></returns>
    public T GetElement(int offsetX, int offsetZ) {
        int index = ConvertOffsetToIndex(offsetX, offsetZ);
        return GetElement(index);
    }

    /// <summary>
    /// Assigns the specified element to the specified offset coordinates.
    /// </summary>
    /// <param name="element">
    /// The element to be assigned.
    /// </param>
    /// <param name="offsetX">
    /// The specified x (longitudinal) coordiante.
    /// </param>
    /// <param name="offsetZ">
    /// The specified z (latitudinal) coordinate.
    /// </param>
    public void SetElement(T element, int offsetX, int offsetZ) {
        int index = ConvertOffsetToIndex(offsetX, offsetZ);
        SetElement(element, index);
    }

    /// <summary>
    /// Gets the element at the specified row-major index.
    /// </summary>
    /// <param name="rowMajorIndex">
    /// The row-major index of the desired element.
    /// </param>
    /// <returns>
    /// The element at the specified row-major index.
    /// </returns>
    public T GetElement(int rowMajorIndex) {
        if (IsInBounds(rowMajorIndex)) {
            T value;

            if (_elements.TryGetValue(rowMajorIndex, out value)) {
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

    /// <summary>
    /// Gets the index of the specified element, or -1 if the element
    /// is not present in the grid.
    /// </summary>
    /// <param name="element">
    /// The specified element to obtain the index of.
    /// </param>
    /// <returns>
    /// The index of the specified element, or -1 if it is not present.
    /// </returns>
    private bool TryGetRowMajorIndex(T element, out int index) {

        if (_indicies.TryGetValue(element, out index)) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Assigns the specified element to the specified row-major index.
    /// </summary>
    /// <param name="element">
    /// The element to be assigned.
    /// </param>
    /// <param name="rowMajorIndex">
    /// The row-major index to assign the element to.
    /// </param>
    public void SetElement(
        T element,
        int rowMajorIndex
    ) {
        if (IsInBounds(rowMajorIndex)) {
            _elements[rowMajorIndex] = element;

            int index;
            if (_indicies.TryGetValue(element, out index)) {

            }
            else {
                _indicies[element] = rowMajorIndex;
            }
        }
        else {
            throw new OutOfGridBoundsException();
        }
    }

    /// <summary>
    /// Gets a list of elements adjacent to the element at the specified index.
    /// </summary>
    /// <param name="rowMajorIndex">
    /// The index of the element whose neighbors should be returned.
    /// </param>
    /// <returns>
    /// A list of elements adjacent to the specified index.
    /// </returns>
    public List<T> GetNeighbors(int rowMajorIndex) {
        List<T> result = new List<T>();

        if (IsInBounds(rowMajorIndex)) {
            int row = GetRowIndex(rowMajorIndex);
            int column = GetOffsetColumnIndex(rowMajorIndex);
            
            T origin = GetElement(rowMajorIndex);        

            HexDirections currentDirection = HexDirections.Southwest;

            for (
                int neighborRow = row - 1;
                neighborRow <= row + 1;
                neighborRow++
            ) {
                for (
                    int neighborColumn = column - 1;
                    neighborColumn <= column + 1;
                    neighborColumn++
                ) {

                    if (neighborRow == row && neighborColumn == column)
                        continue;

                    try {
                        int neighborIndex = ConvertOffsetToIndex(
                            neighborColumn, neighborRow
                        );

                        T neighborCandidate =
                            this[neighborIndex];

                        if (
                            CubeVector.WrappedHexTileDistance(
                                origin.CubeCoordinates,
                                neighborCandidate.CubeCoordinates,
                                WrapSize
                            ) == 1
                        ) {
                            result.Add(
                                neighborCandidate
                            );

                            currentDirection =
                                currentDirection.NextClockwise();
                        }
                    }
                    catch(OutOfGridBoundsException) {
                        /*RootLog.Log(
                            "Row: " + neighborRow + ", Column: " + neighborColumn + " is " + 
                            "outside of the grid bounds.",
                            Severity.Warning,
                            "HexGrid.Neighbors"
                        );*/
                    }
                }
            }
        }

        return result;
    }

    public bool TryGetNeighbors(T element, out List<T> neighbors) {
        
        int index; 
        
        if (TryGetRowMajorIndex(element, out index)) {
            neighbors = GetNeighbors(index);
            return true;
        }

        neighbors = null;
        return false;
    }

    /// <summary>
    /// Gets a list of elements adjacent to the element at the specified
    /// offset coordinates.
    /// </summary>
    /// <param name="offsetX">
    /// The x (longitudinal) offset coordinate.
    /// </param>
    /// <param name="offsetZ">
    /// The y (latitudinal) offset coordinate.
    /// </param>
    /// <returns>
    /// A list of elements adjacent to the element at the specified offset
    /// coordinates.
    /// </returns>
    public List<T> TryGetNeighbors(int offsetX, int offsetZ) {
        return
            GetNeighbors(
                ConvertOffsetToIndex(
                    offsetX,
                    offsetZ
                )
            );
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
    public int GetRowIndex(int index) {
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
    public int GetOffsetColumnIndex(int index) {
        return IsInBounds(index) ?
            index - (GetRowIndex(index) * Columns) :
            throw new OutOfGridBoundsException();
    }

    /// <summary>
    /// Return a flat array composed of the elements of the grid in
    /// row major order.
    /// </summary>
    /// <returns>
    /// A flat array composed of the elements of the grid in row-major
    /// order
    /// </returns>
    public IEnumerable<T> Hexes {
        get {
            return _elements.Values;
        }
    }

    /// <inhertidoc />
    public override string ToString() {
        string leftPad = "              ";
        string innerPad = "   ";

        string result = "";

        for (int row = Rows - 1; row > -1; row--) {
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

    #endregion
    
    #region Private Methods

    /// <summary>
    /// Gets a boolean value representing whether the specified index is
    /// within the bounds of the grid.
    /// </summary>
    /// <param name="index">
    /// The specified row-major index.
    /// </param>
    /// <returns>
    /// A boolean value representing whether the specified index is within
    /// the bounds of the grid.
    /// </returns>
    private bool IsInBounds(int index) {
        return (
            index >= 0 &&
            index < SizeSquared
        );
    }

    /// <summary>
    /// Get an index converted from the specified cube coordinates.
    /// </summary>
    /// <param name="x">
    /// The x (right diagonal longitudinal) cube coordinate.
    /// </param>
    /// <param name="y">
    /// The y (left diagonal longitudinal) cube coordinate.
    /// </param>
    /// <param name="z">
    /// The z (latitudinal) cube coordinate.
    /// </param>
    /// <returns>
    /// An index converted from the specified cube coordinates.
    /// </returns>
    private int ConvertCubeToIndex(int x, int y, int z) {
        Vector2 offsetIndex = CubeVector.CubeToOffset(x, z);

        return ConvertOffsetToIndex(
            (int)offsetIndex.x,
            (int)offsetIndex.y
        );
    }

    /// <summary>
    /// Get an index converted from the specified offset coordinates.
    /// </summary>
    /// <param name="x">
    /// The x (longitudinal) offset coordinate.
    /// </param>
    /// <param name="z">
    /// The z (latitudinal) offset coordinate.
    /// </param>
    /// <returns>
    /// An index converted from the specified offset coordinates.
    /// </returns>
    private int ConvertOffsetToIndex(int x, int z) {
        if (x >= Columns)
            x -= WrapSize;
        else if (x < 0) {
            x += WrapSize;
        }
        
        return (z * Columns) + x;
    }

    #endregion

    #endregion
    
    #region Structs
    #endregion
    
    #region Classes
    #endregion
}