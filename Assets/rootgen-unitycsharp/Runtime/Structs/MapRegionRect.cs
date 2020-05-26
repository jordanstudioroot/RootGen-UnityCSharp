using System.Collections.Generic;
using RootLogging;

/// <summary>
/// Struct defining the bounds of a rectangular map region.
/// </summary>
public struct MapRegionRect {
    private int _offsetXMin;
    private int _offsetXMax;
    private int _offsetZMin;
    private int _offsetZMax;

    /// <summary>
    /// The minimum X axis offset coordinate in the region.
    /// </summary>
    /// <value>
    /// The provided value if <= OffsetXMax, otherwise OffsetXMax.
    /// </value>
    public int OffsetXMin {
        get { return _offsetXMin; }
        set {
            if (value < 0) {
                _offsetXMin = 0;
            }
            else {
                _offsetXMin = value <= _offsetXMax ?
                    value : _offsetXMax;
            }
        } 
    }

    /// <summary>
    /// The maximum X axis offset coordinate in the region.
    /// </summary>
    /// <value>
    /// The provided value if >= OffsetXMin, otherwise OffsetXMin.
    /// </value>
    public int OffsetXMax {
        get { return _offsetXMax; }
        set {
            _offsetXMin = value >= _offsetXMin ?
                value : _offsetXMin;
        }
    }

    /// <summary>
    /// The minimum Z axis offset coordinate in the region.
    /// </summary>
    /// <value>
    /// The provided value if <= OffsetZMax, otherwise OffsetZMax.
    /// </value>
    public int OffsetZMin {
        get { return _offsetZMin; }
        set {
            if (value < 0) {
                _offsetZMin = 0;
            }
            else {
                _offsetZMin = value <= _offsetZMax ?
                    value : _offsetZMax;
            }
        }
    }

    /// <summary>
    /// The maximum Z axis offset coordinate in the region.
    /// </summary>
    /// <value>
    /// The provided value if >= than OffsetZMin, otherwise OffsetZMin.
    /// </value>
    public int OffsetZMax {
        get { return _offsetZMax; }
        set {
            _offsetZMax = value >= _offsetZMin ?
                value : _offsetZMin;
        }
    }

    /// <summary>
    /// The area of the region using offset coordinates.
    /// </summary>
    public int OffsetArea {
        get {
            return OffsetSizeX * OffsetSizeZ;
        }
    }

    /// <summary>
    /// The size of the region along the x axis using offset coordinates.
    /// </summary>
    public int OffsetSizeX {
        get {
            return (OffsetXMax - OffsetXMin);
        }
    }

    /// <summary>
    /// The size of the region along the z axis using offset coordinates.
    /// </summary>
    public int OffsetSizeZ {
        get {
            return (OffsetZMax - OffsetZMin);
        }
    }

    /// <summary>
    /// The middle offset coordinate along the x axis.
    /// </summary>
    public int OffsetXCenter {
        get {
            return
                OffsetXMin + (OffsetSizeX / 2);
        }
    }

    /// <summary>
    /// The middle offset coordinate along the z axis.
    /// </summary>
    public int OffsetZCenter {
        get {
            return
                OffsetZMin + (OffsetSizeZ / 2);
        }
    }

    public override string ToString() {
        return 
            "xMin: " + _offsetXMin +
            ", xMax: " + _offsetXMax + 
            ", zMin: " + _offsetZMin +
            ", zMax: " + _offsetZMax; 
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="offsetXMin">
    /// The minimum X axis offset coordinate of the region. Will be set to
    /// offsetXMax if greater than offsetXMax. If set to a negative value,
    /// will be set to 0.
    /// </param>
    /// <param name="offsetXMax">
    /// The maximum X axis offset coordinate of the region. Will be set to
    /// offsetXMin if less than offsetXMin.
    /// </param>
    /// <param name="offsetZMin">
    /// The minimum Z axis offset coordinate of the region. Will be set to
    /// offsetZMax is greater than offsetZMax. if set to a negative value,
    /// will be set to 0.
    /// </param>
    /// <param name="offsetZMax">
    /// The maximum Z axis offset coordinate of the region. Will be set of
    /// offsetZMin if greater than offsetZMin.
    /// </param>
    public MapRegionRect(
        int offsetXMin,
        int offsetXMax,
        int offsetZMin,
        int offsetZMax
    ) {
        offsetXMin = offsetXMin < 0 ? 0 : offsetXMin;
        offsetZMin = offsetXMin < 0 ? 0 : offsetZMin;

        _offsetXMin =
            offsetXMin <= offsetXMax ? 
            offsetXMin : offsetXMax;

        _offsetXMax =
            offsetXMax >= offsetXMin ?
            offsetXMax : offsetXMin;

        _offsetZMin =
            offsetZMin <= offsetZMax ?
            offsetZMin : offsetZMax;

        _offsetZMax =
            offsetZMax >= offsetZMin ?
            offsetZMax : offsetZMin;
    }

    /// <summary>
    /// Subdivide the border along the z axis.
    /// </summary>
    /// <param name="border">
    /// (Optional) place a border between the two regions. If border is 
    /// greater than or equal to the size of the X dimension, border will be 
    /// set to the x dimension - 2.
    /// </param>
    public List<MapRegionRect> SubdivideHorizontal(int border = 0) {
        if (this.OffsetSizeX - border < 3) {
            RootLog.Log(
                "Border cannot reduce x dimension below 3 or divison will" +
                " be impossible. Setting border to 0.",
                Severity.Debug,
                "MapGenerator"
            );

            border = 0;
        }

        List<MapRegionRect> result = new List<MapRegionRect>();

        result.Add(
            new MapRegionRect(
                this.OffsetXMin,
                this.OffsetXMax,
                this.OffsetZMin,
                this.OffsetZCenter - border
            )
        );

        result.Add(
            new MapRegionRect(
                this.OffsetXMin,
                this.OffsetXMax,
                this.OffsetZCenter + border,
                this.OffsetZMax
            )
        );

        return result;
    }

    public List<MapRegionRect> SubdivideVertical(int border = 0) {
        if (this.OffsetSizeZ - border < 3) {
            RootLog.Log(
                "Border cannot reduce z dimension below 3 or divison " +
                "will be impossible. Setting border to 0.",
                Severity.Debug,
                "MapGenerator"
            );

            border = 0;
        }

        List<MapRegionRect> result = new List<MapRegionRect>();

        result.Add(
            new MapRegionRect(
                this.OffsetXMin,
                this.OffsetXCenter - border,
                this.OffsetZMin,
                this.OffsetZMax
            )
        );

        result.Add(
            new MapRegionRect(
                this.OffsetXCenter + border,
                this.OffsetXMax,
                this.OffsetZMin,
                this.OffsetZMax
            )
        );

        return result;
    }
}        