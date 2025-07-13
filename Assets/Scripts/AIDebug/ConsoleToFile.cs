using UnityEngine;
using System.IO;

public class ConsoleToFile : MonoBehaviour
{
    void OnEnable()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Application.logMessageReceived += LogToFile;
        #endif
    }

    void OnDisable()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Application.logMessageReceived -= LogToFile;
        #endif
    }

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    void LogToFile(string logString, string stackTrace, LogType type)
    {
        string logPath = Path.Combine(Application.dataPath, "Logs", "console.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath));

        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logEntry = $"[{timestamp}] {type}: {logString}\n";

        File.AppendAllText(logPath, logEntry);
    }
    #endif

    void Start()
    {
        #if UNITY_EDITOR
        string logPath = Path.Combine(Application.dataPath, "Logs", "console.log");
        if (File.Exists(logPath))
            File.Delete(logPath); // Clear on game start
        #endif
    }
}