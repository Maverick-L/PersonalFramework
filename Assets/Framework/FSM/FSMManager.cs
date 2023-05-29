using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Framework.FSM
{

    public class FSMManager : ManagerBase
    {

        private Dictionary<string, ControlBase> _controllerMap;

        public override void Initialization()
        {
            _controllerMap = new Dictionary<string, ControlBase>();
        }

        public override void OnDestoryManager()
        {
            foreach (var value in _controllerMap.Values)
            {
                value.OnDestoryControl();
            }
            _controllerMap.Clear();
            base.OnDestoryManager();
        }

        protected override void OnLogicUpdate()
        {
            foreach(var control in _controllerMap.Values)
            {
                control.OnLogicUpdate();
            }
        }

        /// <summary>
        /// 初始化FSMController
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mainBody"></param>
        public void OnInitFSMController<T>(IStateMainBody<T> mainBody, bool useCache = true) where T : System.Enum
        {
            var name = mainBody.GetControlName();
            if (_controllerMap.ContainsKey(name))
            {
                Debug.LogError("FSMConttoller初始化有相同的名字:" + name);
                return;
            }
            if (useCache)
            {
                foreach (var control in _controllerMap.Values)
                {

                    if (control is StateController<T>)
                    {
                        _controllerMap.Add(name, (control as StateController<T>).CopyTo(mainBody));
                        return;
                    }
                }

            }

            var stateControl = new StateController<T>();
            _controllerMap.Add(name, stateControl);
            stateControl.OnInitlization(mainBody);
        }

        /// <summary>
        /// 移除FSMController
        /// </summary>
        /// <param name="mainBody"></param>
        public void OnDestoryFSMController(string contronName)
        {
            var name = contronName;
            if (_controllerMap.ContainsKey(name))
            {
                _controllerMap[name].OnDestoryControl();
                _controllerMap.Remove(name);
            }
        }

        /// <summary>
        /// 状态跳转
        /// </summary>
        public bool TryStateChange<T>(string controlName, T state) where T : System.Enum
        {
            StateController<T> control;
            StateBase<T> stateFunc;
            if (TryGetController(controlName, out control) && control.TryGetStateInMap(state,out stateFunc))
            {
                //判断当前需要跳转的状态和现在存续的状态是不是互斥状态
                if (!control.GetMutexList(state).Contains(control.getNowState))
                {
                    if (stateFunc.TryEnter())
                    {
                        control.ChangeState(state);
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 状态强制跳转，无视跳转条件
        /// </summary>
        public void OnStateMandatoryChange<T>(string controlName, T state) where T : System.Enum
        {
            StateController<T> control;
            if (TryGetController(controlName, out control))
            {
                control.ChangeState(state);
            }
        }


        private bool TryGetController<T>(string controlName, out StateController<T> control) where T : System.Enum
        {
            control = null;
            if (_controllerMap.ContainsKey(controlName))
            {
                control = _controllerMap[controlName] as StateController<T>;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取上一个状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controlName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool GetLastState<T>(string controlName,ref T state) where T : System.Enum
        {
            if (_controllerMap.ContainsKey(controlName))
            {
                state = (_controllerMap[controlName] as StateController<T>).getLastState;
                return true;
            }
            return false;
        }

        //public override void FixedUpdate()
        //{
        //    foreach (var control in _controllerMap.Values)
        //    {
        //        control.OnLogicUpdate();
        //    }
        //}
    }
}