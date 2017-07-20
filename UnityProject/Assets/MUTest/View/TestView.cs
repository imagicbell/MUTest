using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MUTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestGUIAttribute : Attribute
    {
        [Description("custom test GUI method, only allowed one in a class, will be called in Monobehavior OnGUI(), ")]
        public TestGUIAttribute() {}

        [Description("view window rect on screen")]
        public Rect ViewRect
        {
            get { return new Rect(x, y, width, height); }
        }

        public int x;
        public int y;
        public int width;
        public int height;
    }
    
    public class TestView : MonoBehaviour
    {   
        private TestRunner mTestRunner;
        
        private const int WINDOW_ID = 1024;
        private bool mIsShow = false;
        private bool mIsInitStyle = false;
        
        void Awake()
        {
            mTestRunner = GetComponent<TestRunner>();
        }

        void Update()
        {
            #if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            if (Input.GetKeyUp(KeyCode.F5))
                mIsShow = !mIsShow;
            
            #elif UNITY_ANDROID || UNITY_IOS
			if (Input.touchCount == 5)
			{
			    for (int i = 0; i < Input.touchCount; i++)
			    {
			        if (Input.GetTouch(i).phase != TouchPhase.Began)
			            break;
			        if (i == Input.touchCount - 1)
			        {
			            mIsShow = !mIsShow;
			        }
			    }
			}
            #endif
        }

        void OnGUI()
        {
            if (!mIsInitStyle)
            {
                TestViewStyle.Init();
                mIsInitStyle = true;
            }

            if (!mIsShow) return;

            mMainWindow = GUI.Window(WINDOW_ID, mMainWindow, MainView, "");

            if (mUnitWindows != null)
            {
                foreach (var window in mUnitWindows.Values)
                {
                    if (window.isShow)
                        window.rect = GUI.Window(window.id, window.rect, UnitView, "");
                }
            }
        }


        #region MainWindow
        
        private Rect mMainWindow = new Rect(20, 20, 600, 600);
        private Vector2 mScrollPos = Vector2.zero;
        
        private float mScrollViewHeight = -1;
        private float scrollViewHeight
        {
            get
            {
                if (mScrollViewHeight < 0)
                {
                    mScrollViewHeight = 0;
                    foreach (TestRunner.UnitState unit in mTestRunner.TestUnits)
                    {
                        mScrollViewHeight += TestViewStyle.LineHeight * (1 + unit.unitCases.Count);
                    }
                    mScrollViewHeight = Mathf.Max(Screen.height, mScrollViewHeight);
                }
                return mScrollViewHeight;
            }
        }
        
        void MainView(int windowId)
        {
            mScrollPos = GUI.BeginScrollView(new Rect(0, 0, mMainWindow.width, mMainWindow.height), mScrollPos, new Rect(0, 0, mMainWindow.width, scrollViewHeight));

            float y = 10;
            for (int i = 0; i < mTestRunner.TestUnits.Count; i++)
            {
                var unit = mTestRunner.TestUnits[i];
                if (!unit.isRunning)
                {
                    if (GUI.Button(new Rect(mMainWindow.width - 80, y, 60, 60), "Run", TestViewStyle.BtnStyle))
                        mTestRunner.StartTestUnit(unit);

                    if (mUnitWindows != null)
                        mUnitWindows.Remove(unit);
                }
                else
                {
                    if (GUI.Button(new Rect(mMainWindow.width - 80, y, 60, 60), "Stop", TestViewStyle.BtnStyle))
                        mTestRunner.StopTestUnit(unit);

                    if (unit.runner.HasView)
                    {
                        if (GUI.Button(new Rect(mMainWindow.width - 80, y + 60, 60, 60), "View", TestViewStyle.BtnStyle))
                        {
                            if (mUnitWindows == null) mUnitWindows = new Dictionary<TestRunner.UnitState, UnitWindow>();
                            UnitWindow window;
                            if (!mUnitWindows.TryGetValue(unit, out window))
                            {
                                window = new UnitWindow(WINDOW_ID + i + 1, unit.runner.ViewRect);
                                mUnitWindows[unit] = window;
                            }
                            window.isShow = !window.isShow;
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(unit.unitType.ToString().ColorFormat(Color.green));
                var info = unit.unitType.GetCustomAttributes(typeof(TestUnitInfoAttribute), false);
                if (info.Length > 0)
                {
                    sb.Append("\n").Append(((TestUnitInfoAttribute)info[0]).DisplayName.ColorFormat(Color.green));
                    //sb.Append(": ").Append(((TestUnitInfoAttribute)info[0]).Description.ColorFormat(Color.green));
                }
                foreach (MethodInfo unitCase in unit.unitCases.Keys)
                {
                    sb.Append("\n  ");
                    switch (unit.unitCases[unitCase].result)
                    {
                        case TestRunner.CaseState.Result.Idle:
                            sb.Append(("+++" + unitCase.Name).ColorFormat("#6AFF00FF"));
                            break;
                        case TestRunner.CaseState.Result.Run:
                            sb.Append((">>>" + unitCase.Name).ColorFormat(Color.cyan));
                            break;
                        case TestRunner.CaseState.Result.Success:
                            sb.Append(("###" + unitCase.Name).ColorFormat(Color.yellow));
                            break;
                        case TestRunner.CaseState.Result.Fail:
                            sb.Append(("###" + unitCase.Name).ColorFormat(Color.red));
                            break;
                    }
                }
                sb.Append("\n------".ColorFormat(Color.blue));
                if (unit.Failed) sb.Append("Failed".ColorFormat(Color.red));
                else if (unit.Successed) sb.Append("Success".ColorFormat(Color.yellow));
                
                GUIContent unitContent = new GUIContent(sb.ToString());
                float width = mMainWindow.width - 90;
                float height = TestViewStyle.LabelStyle.CalcHeight(unitContent, width);
                GUI.Label(new Rect(10, y, width, height), unitContent, TestViewStyle.LabelStyle);
                
                y += height + 5;
            }
           
            GUI.EndScrollView();
        }
        
        #endregion
        
        #region TestUnitWindows
        
        public class UnitWindow
        {
            public int id;
            public Rect rect;
            public bool isShow;

            public UnitWindow(int wId, Rect wRect)
            {
                this.id = wId;
                this.rect = wRect;
                this.isShow = false;
            }
        }

        private Dictionary<TestRunner.UnitState, UnitWindow> mUnitWindows;

        void UnitView(int windowId)
        {
            foreach (TestRunner.UnitState unit in mUnitWindows.Keys)
            {
                if (mUnitWindows[unit].id == windowId && unit.isRunning)
                {
                    unit.runner.ShowView();
                    break;
                }
            }
            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        }

        #endregion
    }
}
