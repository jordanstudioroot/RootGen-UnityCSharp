using System;
public class NullHexGridException : NullReferenceException {
    public override string Message {
        get {
            return "Hex Grid was not initialized.";
        }
    }
} 