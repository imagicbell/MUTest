using UnityEngine;
using UnityEngine.UI;

namespace MUTest
{
    [RequireComponent(typeof(Button))]
    public class TestUIMarkButton : TestUIMark
    {
        public override string ToString()
        {
            return "TestUIMark---Button---" + base.ToString();
        }
    }
}