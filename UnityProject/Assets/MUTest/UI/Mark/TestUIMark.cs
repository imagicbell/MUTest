using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MUTest
{
    [ExecuteInEditMode, DisallowMultipleComponent]
    public abstract class TestUIMark : MonoBehaviour
    {
        [SerializeField][HideInInspector] public string RelativePath;
        [SerializeField][HideInInspector] public string RootName;
        
        #if UNITY_EDITOR
        private void Reset()
        {
            RelativePath = gameObject.name;
            RootName = gameObject.name;
            Transform trans = transform;
            while (true)
            {
                if (PrefabUtility.GetPrefabType(trans.gameObject) != PrefabType.None
                    && PrefabUtility.FindRootGameObjectWithSameParentPrefab(trans.gameObject) == trans.gameObject)
                    break;
                
                trans = trans.parent;
                if (trans == null)
                {
                    Debug.LogError("TestUIMark---Mark must be added to a gameobject under a prefab!!!");
                    break;
                }
                
                RelativePath = trans.name + "/" + RelativePath;
                RootName = trans.name;
            }

            if (RelativePath.Equals(RootName))
                RelativePath = "";
            else
                RelativePath = RelativePath.Substring(RootName.Length + 1);
            
            Debug.Log(this.ToString());
        }
        #endif

        public override string ToString()
        {
            return string.Format("gameobject: {0}, rootname: {1}, relative path: {2}", gameObject.name, RootName, RelativePath);
        }
    }   
}

