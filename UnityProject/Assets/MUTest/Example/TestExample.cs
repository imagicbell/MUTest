using System.Collections;
using UnityEngine;

namespace MUTest
{
    [TestUnitInfo("测试自定义View")]
    public class TestExample : TestUnit
    {
        private string mContent;
        private string mText;

        [TestSetup]
        IEnumerator Setup()
        {
            Debug.Log("MUTestExample---Setup".ColorFormat(Color.green));
            mText = "";
            yield return 0;
        }

        [TestRun]
        IEnumerator RunCase1()
        {
            Debug.Log("MUTestExample---RunCase1".ColorFormat(Color.green));
            mContent = "Please Enter <color=yellow>Hello Test</color> to Pass RunCase1";
            while (string.IsNullOrEmpty(mText) || !mText.Equals("Hello Test"))
            {
                yield return 0;
            }
            Debug.Log("MUTestExample---FinishCase1".ColorFormat(Color.green));
        }

        [TestRun]
        IEnumerator RunCase2()
        {
            Debug.Log("MUTestExample---RunCase2".ColorFormat(Color.green));
            yield return new WaitForSeconds(0.5f);
            mText = null;
            Debug.Log("MUTestExample---FinishCase2".ColorFormat(Color.green));
        }

        [TestDispose]
        IEnumerator Dispose()
        {
            Debug.Log("MUTestExample---Dispose".ColorFormat(Color.green));
            yield return 0;
        }

        [TestGUI(x = 200, y = 1000, width = 500, height = 200)]
        void View()
        {
            GUI.Label(new Rect(0, 0, 500, 50), mContent, TestViewStyle.LabelStyle);
            mText = GUI.TextField(new Rect(0, 50, 500, 50), mText, TestViewStyle.TextStyle);
        }
    }
}