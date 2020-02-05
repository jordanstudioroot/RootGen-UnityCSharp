using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public enum Severity {
    Trace, // Information that is valuable only for debugging.
    Debug, // Information that may be useful in development and debugging.
    Information, // Tracking the general flow of the app. Long term logging.
    Warning, // Abnormal or unexpected events. Errors that should be investigated.
    Error, // Errors and exceptions that cannot be handled.
    Critical // Failures that require immediate attention.
}

public static class Logger {
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private
    private static List<LogItem> _items = new List<LogItem>();

// ~ Non-Static

// ~~ public

// ~~ private

// CONSTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DESTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DELEGATES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// EVENTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// ENUMS

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INTERFACES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// PROPERTIES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INDEXERS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// METHODS ~~~~~~~~~

// ~ Static

// ~~ public
    public static void Log(
        string message, 
        Severity severity = Severity.Debug, 
        string category = "General"
    ) {
        LogItem item = new LogItem(
            DateTime.Now, 
            severity, 
            category, 
            message
        );

        _items.Add(item);
        string logString =
            "@Frame " + Time.frameCount + ": " + 
            severity.ToString() + ", " + 
            category + ", \n" + 
            message;

        switch (severity) {
            case Severity.Critical:
                Debug.LogError(logString);
                break;
            case Severity.Debug:
                Debug.Log(logString);
                break;
            case Severity.Error:
                Debug.LogError(logString);
                break;
            case Severity.Information:
                Debug.Log(logString);
                break;
            case Severity.Trace:
                Debug.Log(logString);
                break;
            case Severity.Warning:
                Debug.LogWarning(logString);
                break;
        }
    }

    public static void ToFile(string target) {
        if (!File.Exists(target)) {
            StreamWriter sw = File.CreateText(target);
            try {
                foreach(LogItem item in _items) {
                    sw.WriteLine(
                        item.time.Millisecond + ": " +
                        item.severity + ", " +
                        item.category + ", " +
                        item.message
                    );
                }
            }
            finally {
                if (sw != null)
                    sw.Dispose();
            }
        }
        else {
            File.Delete(target);
            ToFile(target);
        }
    }

    public static void ToDatabase() {

    }

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// STRUCTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
    private struct LogItem {
        public DateTime time;
        public Severity severity;
        public string category;
        public string message;

        public LogItem (
            DateTime time, 
            Severity severity, 
            string category, 
            string message
        ) {
            this.time = time;
            this.severity = severity;
            this.category = category;
            this.message = message;
        }
    }

// CLASSES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private    
}