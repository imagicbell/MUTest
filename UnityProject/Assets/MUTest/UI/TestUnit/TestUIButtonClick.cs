using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Enumerable = System.Linq.Enumerable;
using Object = UnityEngine.Object;

namespace MUTest
{
    [TestUnitInfo("随机按钮测试", Description = "自动化随机遍历点击游戏UI中的按钮，成功完成所有按钮点击，则测试通过")]
    public class TestUIButtonClick : TestUnit
    {
        private List<ButtonGroup> mOpenList = new List<ButtonGroup>();
        private List<ButtonGroup> mClosedList = new List<ButtonGroup>();

        [TestSetup]
        IEnumerator Setup()
        {
            yield return new TestObjectAppeared<TestUIMarkButton>();
            
            EvaluateOpenList();
        }
        
        [TestRun]
        IEnumerator CrazyTestButtonClick()
        {
            while (mOpenList.Count > 0)
            {
				yield return new TestBoolCondition(RandomClickButton);
				yield return 0;
				EvaluateOpenList();
            }
        }

        /// <summary>
        /// find all buttons int current scene and ensure existance in openlist or closedlist
        /// </summary>
        void EvaluateOpenList()
        {
            var markButtons = Object.FindObjectsOfType<TestUIMarkButton>();
            foreach (var markButton in markButtons)
            {
                ButtonGroup group = mClosedList.Find(g => g.RootName.Equals(markButton.RootName));
                if (group != null) continue;

                group = mOpenList.Find(g => g.RootName.Equals(markButton.RootName));
                if (group != null)
                {
                    if (!group.Contains(markButton.RelativePath))
                        group.Add(markButton.RelativePath);
                }
                else
                {
                    var rootObj = GameObject.Find(markButton.RootName);
                    if (rootObj == null)
                        throw new Exception(string.Format("TestUIMark--- the root name of button: {0} is wrong, please do Reset TestUIMarkButton in Editor"));
                    
                    group = new ButtonGroup(markButton.RootName);
                    var eles = rootObj.GetComponentsInChildren<TestUIMarkButton>(true);
                    foreach (var e in eles)
                    {
                        group.Add(e.RelativePath);
                    }
                    mOpenList.Add(group);
                }
            }
        }

        /// <summary>
        /// randomly find a reachable button, and simulate a click event
        /// </summary>
        /// <returns></returns>
        bool RandomClickButton()
        {
            if (!EventSystem.current.isActiveAndEnabled)
                return false;
            
            //find open list first
            if (mOpenList.Count > 0)
            {
                mOpenList.Shuffle();
                foreach (ButtonGroup group in mOpenList)
                {
                    if (group.RandomReachableMember())
                    {
                        if (group.Close())
                        {
                            Debug.LogFormat("<color=green>TestUIButtonClick---finish test all buttons on group: {0}.</color>", group.RootName);
                            mOpenList.Remove(group);
                            mClosedList.Add(group);
                        }
                        return true;
                    }
                }
            }

            if (mClosedList.Count > 0)
            {
                mClosedList.Shuffle();
                foreach (ButtonGroup group in mClosedList)
                {
                    if (group.RandomReachableMember())
                    {
                        return true;
                    }
                }
            }
            
            //Debug.LogFormat("<color=yellow>TestUIButtonClick---can't find a button to test.</color>");
            return false;
        }
        
        private class ButtonGroup
        {
            public string RootName { get; private set; }
			private List<string> mOpenMembers = new List<string>();
			private List<string> mClosedMembers = new List<string>();

            public ButtonGroup(string root)
            {
                this.RootName = root;
            }

            public bool Contains(string path)
            {
                return mOpenMembers.Contains(path) || mClosedMembers.Contains(path);
            }

            public void Add(string path)
            {
                mOpenMembers.Add(path);
            }

            public bool Active()
            {
                return GameObject.Find(RootName) != null;
            }

            public bool Close()
            {
                return mOpenMembers.Count == 0 && mClosedMembers.Count > 0;
            }

			public bool RandomReachableMember()
            {
                GameObject rootObj = GameObject.Find(RootName);
                if (rootObj == null) return false;
                if (!rootObj.activeInHierarchy) return false;
                
				Canvas canvas = rootObj.GetComponentInParent<Canvas>();
				if (canvas == null)
				{
					throw new Exception(string.Format("TestUIButtonClick---rootObject: {0} must be under a canvas"));
					return false;
				}
                
                //find open list first
                if (mOpenMembers.Count > 0)
                {
                    mOpenMembers.Shuffle();
                    foreach (string s in mOpenMembers)
                    {
                        Transform bTrans = rootObj.transform.Find(s);
                        //may be destroyed sometimes 
                        if (bTrans == null) continue;

                        if (SimulateClickButton(bTrans.gameObject, canvas))
                        {
                            mOpenMembers.Remove(s);
                            mClosedMembers.Add(s);
                            return true;
                        }
                    }
                }

                if (mClosedMembers.Count > 0)
                {
                    mClosedMembers.Shuffle();
                    foreach (string s in mClosedMembers)
                    {
                        Transform bTrans = rootObj.transform.Find(s);
                        //may be destroyed sometimes 
                        if (bTrans == null) continue;

                        if (SimulateClickButton(bTrans.gameObject, canvas))
                        {
                            return true;
                        }
                    }
                }
                
				return false;
            }

            private bool SimulateClickButton(GameObject bObj, Canvas uiCanvas)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.pointerId = -1;
                eventData.position = RectTransformUtility.WorldToScreenPoint(uiCanvas.worldCamera, bObj.GetComponent<RectTransform>().position);
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);
                if (results.Count > 0 && results[0].gameObject == bObj)
                {
                    Debug.LogFormat("<color=green>TestUIButtonClick---click button: {0}, time: {1}.</color>", bObj.name, Time.time);
                    ExecuteEvents.Execute(bObj, eventData,ExecuteEvents.pointerClickHandler);
                    return true;
                }
                return false;
            }
        }
    }
}