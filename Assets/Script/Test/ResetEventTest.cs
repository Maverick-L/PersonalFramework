using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Framework;


namespace Test.ThreadTest
{
    using L = Framework.L;
    public class ResetEventTest :MonoBehaviour
    {
        ManualResetEvent _threadState = new ManualResetEvent(false);
        AutoResetEvent _autoThreadState = new AutoResetEvent(false);

        Object _testLock;

        List<Thread> _threadMap = new List<Thread>();

        private void Awake()
        {
            CreateThread();
        }

        [ContextMenu("Init")]
        public void CreateThread()
        {
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    Thread thread = new Thread(ThreadTestCallback);
                    thread.IsBackground = true;
                    thread.Start("ManualResetEvent_" + i);
                    _threadMap.Add(thread);
                }

                for (int i = 0; i < 2; i++)
                {
                    Thread thread = new Thread(ThreadTestCallback1);
                    thread.IsBackground = true;
                    thread.Start("AutoResetEvent_" + i);
                    _threadMap.Add(thread);
                }
            }
            catch
            {

            }
           
        }

        [ContextMenu("Close")]
        public void CloseThread()
        {
            for (int i = 0; i < _threadMap.Count; i++)
            {
                try
                {
                    _threadMap[i].Abort();
                }
                catch { }
            }
        }

        [ContextMenu("Set")]
        public void Set()
        {
            _threadState.Set();
            _autoThreadState.Set();
        }

        [ContextMenu("Reset")]
        public void Reset()
        {
            _threadState.Reset();
            _autoThreadState.Reset();
        }

        private void ThreadTestCallback(object index)
        {
            while (true)
            {
                L.Log("ResetEventTest", "线程[" + (string)index + "]调用 1");
                _threadState.WaitOne();
                L.Log("ResetEventTest", "线程[" + (string)index + "]调用 2");
            }
        }

        private void ThreadTestCallback1(object index)
        {
            while (true)
            {
                L.Log("ResetEventTest", "线程[" + (string)index + "]调用 1");
                _autoThreadState.WaitOne();
                L.Log("ResetEventTest", "线程[" + (string)index + "]调用 2");
            }
        }
    }
}

