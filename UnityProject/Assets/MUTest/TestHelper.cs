using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace MUTest
{
    public static class TestHelper
    {
        public static T FindElement<T>(string name) where T : Component
        {
            T e = FindElementOrNull<T>(name);
            if (e == null) throw new Exception(typeof(T) + " not found: " + name);
            return e;
        }

        public static T FindElementOrNull<T>(string name) where T : Component
        {
            var children = Object.FindObjectsOfType<T>();
            foreach (T element in children)
            {
                if (element != null && element.name.Equals(name))
                    return element;
            }
            return null;
        }
        
        private static Random rng = new Random();  
        
        public static void Shuffle<T>(this IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
        
        public static bool UnityInternalError(string condition)
        {
            return condition.StartsWith("The profiler has run out of samples") ||
                   condition.StartsWith("Multiple plugins with the same name") ||
                   condition.StartsWith("String too long for TextMeshGenerator");
        }
        
        public static string XmlEscapeFailedMessage(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            var idx = str.IndexOf("\n", StringComparison.Ordinal);
            if (idx != -1)
                str = str.Substring(0, idx);
            return str.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public static string XmlFormatFailedStack(string str)
        {
            return str.Replace("\n", "\n\t\t\t");
        }
    }
}