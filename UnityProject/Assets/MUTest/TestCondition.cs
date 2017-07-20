using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MUTest
{
    public abstract class TestCondition : CustomYieldInstruction
    {
        protected string param;
        protected string objectName;

        protected TestCondition()
        {
        }

        protected TestCondition(string param)
        {
            this.param = param;
        }

        protected TestCondition(string objectName, string param)
        {
            this.param = param;
            this.objectName = objectName;
        }
        
        const int WAIT_INTERVAL_FRAME = 10;//每10帧检查一次
        private float mWaitDuration = -1f;
        private float mWaitTime = 0;
        private int mWaitFrame = WAIT_INTERVAL_FRAME;

        public sealed override bool keepWaiting
        {
            get
            {
                if (mWaitDuration > 0)
                {
                    if (mWaitTime >= mWaitDuration)
                    {
                        mWaitTime = 0;
                        throw new Exception(String.Format("TestCondition---Operation timed out: {0}\n{1}", this.ToString(),
                            Environment.StackTrace));
                    }
                    mWaitTime += Time.unscaledDeltaTime;    
                }

                if (mWaitFrame == WAIT_INTERVAL_FRAME)
                {
                    mWaitFrame = 0;
                    return !Satisfied();
                }
                mWaitFrame++;
                return true;
            }
        }

        public void SetWaitTime(float wait)
        {
            mWaitDuration = wait;
        }

        protected abstract bool Satisfied();

        public override string ToString()
        {
            return GetType() + " \'" + param + "\'";
        }
    }
    
    public class TestObjectAppeared : TestCondition
    {
        protected string path;
        public GameObject o;

        public TestObjectAppeared(string path)
        {
            this.path = path;
        }

        protected override bool Satisfied()
        {
            o = GameObject.Find(path);
            return o != null && o.activeInHierarchy;
        }

        public override string ToString()
        {
            return "ObjectAppeared(" + path + ")";
        }
    }
    
    public class TestObjectAppeared<T> : TestCondition where T : Component
    {
        public T Obj { get; private set; }

        protected override bool Satisfied()
        {
            Obj = Object.FindObjectOfType<T>();
            return Obj != null && Obj.gameObject.activeInHierarchy;
        }
    }
    
    public class TestObjectDisappeared : TestObjectAppeared
    {
        public TestObjectDisappeared(string path) : base(path) {}
        
        protected override bool Satisfied()
        {
            return !base.Satisfied();
        }

        public override string ToString()
        {
            return "ObjectDisappeared(" + path + ")";
        }
    }
    
    public class TestObjectDisappeared<T> : TestObjectAppeared<T> where T : Component
    {
        protected override bool Satisfied()
        {
            return !base.Satisfied();
        }
    }

	public class TestBoolCondition : TestCondition
	{
		private Func<bool> _getter;

		public TestBoolCondition(Func<bool> getter)
		{
			_getter = getter;
		}

		protected override bool Satisfied()
		{
			if (_getter == null) return false;
			return _getter();
		}

		public override string ToString()
		{
			return "BoolCondition(" + _getter + ")";
		}
	}
}