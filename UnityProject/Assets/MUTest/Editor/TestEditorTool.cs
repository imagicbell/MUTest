using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MUTest
{
    public class TestEditorTool : MonoBehaviour
    {
        [MenuItem("Assets/MUTestTools/Batch Mark Button")]
        static void MarkTestButton()
        {
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets | SelectionMode.TopLevel);
            bool displayProgress = objs.Length > 1;
            if (displayProgress) EditorUtility.DisplayProgressBar("", "Create UIPrefab Code...", 0);
            for (int i = 0; i < objs.Length; i++)
            {
                GameObject asset = objs[i] as GameObject;
                Button[] buttons = asset.GetComponentsInChildren<Button>(true);
                foreach (Button button in buttons)
                {
                    TestUIMarkButton markButton = button.GetComponent<TestUIMarkButton>();
                    if (markButton == null) button.gameObject.AddComponent<TestUIMarkButton>();
                }
            
                if (displayProgress)
                    EditorUtility.DisplayProgressBar("", "Create UIPrefab Code...", (float) (i + 1) / objs.Length);
            }
			
            AssetDatabase.Refresh();
            if (displayProgress) EditorUtility.ClearProgressBar();
        }
    }    
}

