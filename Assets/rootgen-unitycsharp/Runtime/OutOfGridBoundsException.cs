using System;

public class OutOfGridBoundsException : Exception {
    public override string Message {
        get {
            return "Requested element was outside the bounds of the grid.";
        }
    }
}