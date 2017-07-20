using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MUTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestRunAttribute : Attribute
    {
        [Description("test case method, should return type of IEnumerator or IEnumerable")]
        public TestRunAttribute() {}
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestSetupAttribute : Attribute
    {
        [Description("setup before any test case, should return type of IEnumerator or IEnumerable")]
        public TestSetupAttribute() {}
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestDisposeAttribute : Attribute
    {
        [Description("dispose after any test case, should return type of IEnumerator or IEnumerable")]
        public TestDisposeAttribute() {}
    }
    
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TestUnitInfoAttribute : Attribute
    {
        public TestUnitInfoAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }
        
        public string DisplayName { get; private set; }
        public string Description { get; set; }
    }
    
    public abstract class TestUnit : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            CheckViewMethod();
        }

        #region Test Cases
        
        private MethodInfo mRunningCase;
        private IEnumerator mCaseCoroutine;
        
        public bool IsRunning
        {
            get { return mRunningCase != null; }
        }

        public bool IsRunningCase(MethodInfo method)
        {
            return mRunningCase == method;
        }

        public MethodInfo RunningCase
        {
            get { return mRunningCase; }
        }
        
        /// <summary>
        /// 外部调用：begin running a test case
        /// </summary>
        public void StartTestCase(MethodInfo method)
        {
            if (IsRunningCase(method))
                return;

            mRunningCase = method;
            mCaseCoroutine = RunTestCase(method);
            StartCoroutine(mCaseCoroutine);
        }

        /// <summary>
        /// 外部调用：force stop running a test case
        /// </summary>
        public void StopTestCase(MethodInfo method)
        {
            if (!IsRunningCase(method))
                return;

            StopCoroutine(mCaseCoroutine);
            mCaseCoroutine = null;
            mRunningCase = null;
        }

        private IEnumerator RunTestCase(MethodInfo method)
        {
            yield return null; // wait for destroy to be executed

            yield return StartCoroutine(Run(typeof(TestSetupAttribute)));
            yield return StartCoroutine(InvokeMethod(method));
            yield return StartCoroutine(Run(typeof(TestDisposeAttribute)));

            yield return null;
            
            mCaseCoroutine = null;
            mRunningCase = null;
        }
        
        private IEnumerator InvokeMethod(MethodInfo method)
        {
            var enumerator = method.Invoke(this, null) as IEnumerator;
            if (enumerator != null)
            {
                yield return enumerator;
            }
            else
            {
                var enumerable = method.Invoke(this, null) as IEnumerable;
                if (enumerable != null)
                {
                    foreach (YieldInstruction y in enumerable)
                        yield return y;
                }
            }
        }

        private IEnumerator Run(Type type)
        {
            foreach (MethodInfo method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(method, type))
                    yield return StartCoroutine(InvokeMethod(method));
            }
        }

        #endregion

        #region Test View

        private MethodInfo mViewMethod;

        private void CheckViewMethod()
        {
            mViewMethod = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .First(m => Attribute.IsDefined(m, typeof(TestGUIAttribute)));
        }

        public bool HasView
        {
            get { return mViewMethod != null; }
        }

        public Rect ViewRect
        {
            get { return (mViewMethod.GetCustomAttributes(typeof(TestGUIAttribute), false)[0] as TestGUIAttribute).ViewRect; }
        }

        public bool ShowView()
        {
            if (!HasView) return false;
            mViewMethod.Invoke(this, null);
            return true;
        }
        
        #endregion
    }
}