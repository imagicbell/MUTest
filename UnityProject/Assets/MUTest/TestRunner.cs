using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MUTest
{
    public class TestRunner : MonoBehaviour
    {
        /// <summary>
        /// 应用层调用初始化
        /// </summary>
        public static void Create()
        {
            var runner = FindObjectOfType<TestRunner>();
            if (runner != null)
            {
                runner.gameObject.name = "[MUTestRunner]";
                runner.transform.parent = null;
            }
            else
            {
                GameObject runnerObj = new GameObject("[MUTestRunner]");
                runner = runnerObj.AddComponent<TestRunner>();
            }
        }

        private List<UnitState> mTestUnits;
        public List<UnitState> TestUnits { get { return mTestUnits; } }

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            
            mTestUnits = new List<UnitState>();
            foreach (Type type in Assembly.GetAssembly(typeof(TestUnit)).GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(TestUnit))))
            {
                UnitState unit = new UnitState();
                unit.unitType = type;
                unit.unitCases = new Dictionary<MethodInfo, CaseState>();
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (Attribute.IsDefined(method, typeof(TestRunAttribute)))
                        unit.unitCases.Add(method, new CaseState());
                }
                mTestUnits.Add(unit);
            }
            
            gameObject.AddComponent<TestView>();
        }

        public void StartTestUnit(UnitState testUnit)
        {
            if (testUnit.isRunning) return;
            StartCoroutine(RunTestUnit(testUnit));
        }

        public void StartTestUnit(Type testUnitType)
        {
            UnitState testUnit = mTestUnits.Find(u => u.unitType == testUnitType);
            if (testUnit != null)
                StartTestUnit(testUnit);
        }

        public void StopTestUnit(UnitState testUnit)
        {
            if (!testUnit.isRunning) return;
            //first stop current running test case
            testUnit.runner.StopTestCase(testUnit.runner.RunningCase);
            //then set the running state false, and then wait the coroutine to finish
            testUnit.isRunning = false;
        }

        public void StopTestUnit(Type testUnitType)
        {
            UnitState testUnit = mTestUnits.Find(u => u.unitType == testUnitType);
            if (testUnit != null)
                StopTestUnit(testUnit);
        }

        private IEnumerator RunTestUnit(UnitState testUnit)
        {
            var go = new GameObject(testUnit.unitType.ToString());
            go.transform.parent = this.transform;
            var runner = (TestUnit)go.AddComponent(testUnit.unitType);

            testUnit.OnRun(runner);

            Application.LogCallback logReceived = (condition, stackTrace, type) =>
            {
                if ((type == LogType.Error || type == LogType.Exception) && !TestHelper.UnityInternalError(condition))
                {
                    testUnit.unitCases[runner.RunningCase].failedMsg = condition;
                    testUnit.unitCases[runner.RunningCase].failedStack = stackTrace;
                    runner.StopTestCase(runner.RunningCase);
                }
            };
            Application.logMessageReceived += logReceived;
            
            //run test cases
            foreach (var method in testUnit.unitCases.Keys)
            {
                var timeStarted = DateTime.Now;
                runner.StartTestCase(method);

                var state = testUnit.unitCases[method];
                state.result = CaseState.Result.Run;

                while (runner.IsRunning) yield return null;

                state.duration = (float) (DateTime.Now - timeStarted).TotalSeconds;
                state.result = state.failedMsg == null ? CaseState.Result.Success : CaseState.Result.Fail;

                //stopped by interruption
                if (!testUnit.isRunning) break;
            }
            yield return new WaitForSeconds(0.5f);

            Application.logMessageReceived -= logReceived;

            //finish test cases
            GameObject.Destroy(go);
            testUnit.OnEnd();
            
            //output report
            ReportXml(testUnit);
        }
        
        private void ReportXml(UnitState testUnit)
        {
            #if UNITY_EDITOR
            string ReportPath = Application.dataPath + "/../MUTest";
            #elif UNITY_STANDALONE_WIN
            string ReportPath = Application.dataPath + "/MUTest";
            #elif UNITY_STANDALONE_OSX
            string ReportPath = Application.dataPath + "/MUTest";
            #else
            string ReportPath = Application.persistentDataPath + "/MUTest";
            #endif

            if (!Directory.Exists(ReportPath))
                Directory.CreateDirectory(ReportPath);
            
            using (var writer = File.CreateText(ReportPath + "/TEST-" + testUnit.unitType + ".xml"))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine(string.Format(
                    "<testunit name=\"{0}\" tests=\"{1}\" failures=\"{2}\" timestamp=\"{3}\" time=\"{4}\">",
                    testUnit.unitType,
                    testUnit.unitCases.Count,
                    testUnit.FailCount,
                    testUnit.TimeStarted.ToString("yyyy-MM-dd'T'HH:mm:ss"),
                    testUnit.Duration.ToString("0.00")));
                foreach (var c in testUnit.unitCases.Keys)
                {
                    CaseState testCase = testUnit.unitCases[c];
                    writer.WriteLine(string.Format(
                        "\t<testcase name=\"{0}\" time=\"{1}\">",
                        c.Name,
                        testCase.duration.ToString("0.00")));
                    if (testCase.result == CaseState.Result.Fail)
                    {
                        writer.WriteLine(string.Format("\t\t<failure message=\"{0}\">", TestHelper.XmlEscapeFailedMessage(testCase.failedMsg)));
                        writer.Write(string.Format("\t\t\t<![CDATA[{0}]]>\n", TestHelper.XmlFormatFailedStack(testCase.failedStack)));
                        writer.WriteLine("\t\t</failure>");
                    }
                    writer.WriteLine("\t</testcase>");
                }
                writer.WriteLine("</testunit>");
            }
        }
        

        public class CaseState
        {
            public Result result = Result.Idle;
            public string failedMsg;
            public string failedStack;
            public float duration;

            public enum Result
            {
                Idle, Run, Success, Fail,
            }

            public void Reset()
            {
                result = Result.Idle;
                failedMsg = null;
                failedStack = null;
                duration = 0;
            }
        }
        
        public class UnitState
        {
            public Type unitType;
            public Dictionary<MethodInfo, CaseState> unitCases;
            public bool isRunning;
            public TestUnit runner;

            public bool Successed { get; private set; }
            public bool Failed { get; private set; }
            public int FailCount { get; private set; }
            public float Duration { get; private set; }
            public DateTime TimeStarted { get; private set; }

            public void OnRun(TestUnit unitCom)
            {
                this.isRunning = true;
                this.runner = unitCom;
                this.Successed = false;
                this.Failed = false;
                this.TimeStarted = DateTime.Now;
                foreach (CaseState state in unitCases.Values)
                    state.Reset();
            }

            public void OnEnd()
            {
                this.isRunning = false;
                this.runner = null;

                this.FailCount = 0;
                foreach (CaseState state in unitCases.Values)
                {
                    if (state.result == CaseState.Result.Fail)
                    {
                        this.Failed = true;
                        this.FailCount++;
                    }
                }
                if (!this.Failed)
                    this.Successed = true;

                this.Duration = (float) (DateTime.Now - this.TimeStarted).TotalSeconds;
            }
        }
    }
}