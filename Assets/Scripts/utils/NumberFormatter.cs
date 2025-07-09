using System;
using UnityEngine;

public static class NumberFormatter
{
    /// <summary>
    /// Formats numbers >= 1000 with K suffix
    /// Examples: 1000 -> 1.0K, 15111 -> 15.1K, 999 -> 999
    /// </summary>
    public static string FormatNumber(int number)
    {
        if (number < 1000)
        {
            return number.ToString();
        }
        
        float thousands = number / 1000f;
        string result = $"{thousands:F1}K";
        UnityEngine.Debug.Log($"[NumberFormatter] Formatted {number} -> {result}");
        return result;
    }
    
    /// <summary>
    /// Formats numbers >= 1000 with K suffix (long version)
    /// </summary>
    public static string FormatNumber(long number)
    {
        if (number < 1000)
        {
            return number.ToString();
        }
        
        float thousands = number / 1000f;
        return $"{thousands:F1}K";
    }
    
    /// <summary>
    /// Formats numbers >= 1000 with K suffix (float version)
    /// </summary>
    public static string FormatNumber(float number)
    {
        if (number < 1000)
        {
            return ((int)number).ToString();
        }
        
        float thousands = number / 1000f;
        return $"{thousands:F1}K";
    }
}