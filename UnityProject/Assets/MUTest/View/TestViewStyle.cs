using UnityEngine;

namespace MUTest
{
    public class TestViewStyle
    {
        public const int FontSize = 24;
        public const float LineHeight = 30;
        
        public static GUIStyle LabelStyle;
        public static GUIStyle BtnStyle;
        public static GUIStyle TextStyle;
        
        public static void Init()
        {
            LabelStyle = new GUIStyle(GUI.skin.label);
            LabelStyle.fontSize = FontSize;
            
            BtnStyle = new GUIStyle(GUI.skin.button);
            BtnStyle.fontSize = FontSize;
            
            TextStyle = new GUIStyle(GUI.skin.textField);
            TextStyle.fontSize = FontSize;
        }
    }
    
    public static class StringHelper
    {
        public static string ColorFormat(this string text, Color color)
        {
            return System.String.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(color), text);
        }

        public static string ColorFormat(this string text, string HexColorStr)
        {
            if (HexColorStr.StartsWith("#"))
                return System.String.Format("<color={0}>{1}</color>", HexColorStr, text);
            return System.String.Format("<color=#{0}>{1}</color>", HexColorStr, text);
        }
    }
}