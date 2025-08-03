using System;
using UnityEngine;

public static class NumberFormatter
{
    /// <summary>
    /// Formats numbers >= 1000 with K suffix, smart decimal handling
    /// Examples: 1000 -> 1K, 1500 -> 1.5K, 2030 -> 2K, 2100 -> 2.1K
    /// </summary>
    public static string FormatNumber(int number)
    {
        if (number < 1000)
        {
            return number.ToString();
        }
        
        float thousands = number / 1000f;
        
        // Check if the decimal part is effectively zero
        if (Math.Abs(thousands - Math.Round(thousands)) < 0.05f)
        {
            // Round to whole number, no decimal
            return $"{Math.Round(thousands)}K";
        }
        else
        {
            // Show one decimal place
            return $"{thousands:F1}K";
        }
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
        
        // Check if the decimal part is effectively zero
        if (Math.Abs(thousands - Math.Round(thousands)) < 0.05f)
        {
            // Round to whole number, no decimal
            return $"{Math.Round(thousands)}K";
        }
        else
        {
            // Show one decimal place
            return $"{thousands:F1}K";
        }
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
        
        // Check if the decimal part is effectively zero
        if (Math.Abs(thousands - Math.Round(thousands)) < 0.05f)
        {
            // Round to whole number, no decimal
            return $"{Math.Round(thousands)}K";
        }
        else
        {
            // Show one decimal place
            return $"{thousands:F1}K";
        }
    }
}