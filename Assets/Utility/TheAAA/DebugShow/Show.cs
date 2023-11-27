using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This Static script will help you to debug using variant kind of premade debug log.
/// </summary>
public static class Show
{
    public static void Log(object obj)
    {
#if UNITY_EDITOR 
        Debug.Log(obj);
#endif

    }
    public static void LogRed(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj.Color(Color.red));
#endif


    }
    public static void LogGreen(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj.Color(Color.green));
#endif

    }
    public static void LogBlue(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj.Color(Color.blue));
#endif

    }
    public static void LogBlack(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj.Color(Color.black));
#endif

    }
    public static void LogCyan(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj.Color(Color.cyan));
#endif

    }
    public static void LogGray(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj.Color(Color.gray));
#endif

    }
    public static void LogMagenta(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj.Color(Color.magenta));
#endif

    }
    public static void LogYellow(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj.Color(Color.yellow));
#endif

    }
    
    public static void LogError(object obj)
    {
#if UNITY_EDITOR
        Debug.LogError(obj);
#endif

    }
    public static void LogWarning(object obj)
    {
#if UNITY_EDITOR
        Debug.LogWarning(obj);
#endif

    }
}
public static class StringExtension
{
    public static string Bold(this object str) => "<b>" + str + "</b>";
    public static string Color(this object str, Color clr) => string.Format("<color={0}>{1}</color>", "#"+ ColorUtility.ToHtmlStringRGBA(clr), str);
    public static string Italic(this object str) => "<i>" + str + "</i>";
    public static string Size(this object str, int size) => string.Format("<size={0}>{1}</size>", size, str);

}
