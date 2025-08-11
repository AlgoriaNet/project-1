using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteAlways]
public class ClaudeFolderBridge : MonoBehaviour
{
    public string folderPath = "Assets/Scripts/AIFolder";
    private string logPath = "Assets/Logs/console.log";

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (Directory.Exists(folderPath))
            {
                string message = $"ClaudeBridge: Folder exists at {folderPath}";
                Debug.Log(message);
                File.AppendAllText(logPath, message + "\n");
            }
        }
    }
}