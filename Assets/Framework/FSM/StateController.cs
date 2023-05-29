using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace Framework.FSM
{
    /// <summary>
    /// 状态主体，用来继承，在对应的状态中可以读取到这里的数据
    /// </summary>
    public interface IStateMainBody <T> where T:Enum
    {
        string GetControlName();

        void SetNextState(T state) ;
        void SetLastState(T state) ;
        void SetNowState(T state) ;

        T GetNextState();
        T GetNowState();
        T GetLastState();

    }


    public class StateController<T> : ControlBase where T : Enum
    {
        public T getNowState => _mainBody.GetNowState();

        public T getLastState => _mainBody.GetLastState();

        public T getNextState => _mainBody.GetNextState();

        private IStateMainBody<T> _mainBody;
        public IStateMainBody<T> getMainBody => _mainBody;
        private Dictionary<T, StateBase<T>> _stateMap;

        private Dictionary<T, List<T>> _mutexMap = new Dictionary<T, List<T>>();

        public bool TryGetStateInMap(T type, out StateBase<T> stateBase)
        {
            if (!HasState(type))
            {
                stateBase = null;
                return false;
            }
            stateBase = _stateMap[type];
            return true;
        }

        public bool HasState(T state)
        {
            return _stateMap != null && _stateMap.ContainsKey(state);
        }

        public virtual void ChangeState(T type)
        {
            _stateMap[getNowState].OnExit();
            _mainBody.SetLastState(getNowState);
            _mainBody.SetNowState(type);
            if (!_stateMap.ContainsKey(type))
            {
                return;
            }
            _stateMap[type].OnEnter();
        }

        public override void OnLogicUpdate()
        {
            StateBase<T> stateFunc;
            if (TryGetStateInMap(getNowState, out stateFunc))
            {
                stateFunc.OnUpdate();
            }
        }


        public override void OnInitlization()
        {
            _stateMap = new Dictionary<T, StateBase<T>>();
            var allType = Util.GetAllChildClass(typeof(StateBase<T>));
            for (int i = 0; i < allType.Count; i++)
            {
                
                var type = allType[i];
                var attrubate = type.GetCustomAttribute(typeof(StateFuncAttrubite)) as StateFuncAttrubite;
                T t = (T)Enum.ToObject(attrubate.stateEnumType, attrubate.state);
                _stateMap.Add(t, Activator.CreateInstance(type, this) as StateBase<T>);
            }
        }

        public void OnInitlization(IStateMainBody<T> mainBody)
        {
            _mainBody = mainBody;
            OnInitlization();
            //标记0号为初始状态 自动跳入初始状态
            T initState = (T)Enum.ToObject(typeof(T), 0);
            _mainBody.SetNowState( initState);
            _stateMap[getNowState].OnEnter();
        }

        public void OnInitlization(IStateMainBody<T> mainBody, Dictionary<T, List<T>> mutexMap, Dictionary<T, StateBase<T>> stateMap)
        {
            _mainBody = mainBody;
            _mutexMap = mutexMap;
            _stateMap = stateMap;

            //标记0号为初始状态 自动跳入初始状态
            T initState = (T)Enum.ToObject(typeof(T), 0);
            _mainBody.SetNowState( initState);
            _stateMap[getNowState].OnEnter();
        }

        public void OnModifyMainBody(IStateMainBody<T> mainBody)
        {
            _mainBody = mainBody;
            foreach(var state in _stateMap.Values)
            {
                state.OnModifyStateMainBody();
            }
        }



        public List<T> GetMutexList(T type)
        {
            if (_mutexMap.ContainsKey(type))
            {
                return _mutexMap[type];
            }
            List<T> mutexList = new List<T>();
            FindMutex(type, mutexList);
            _mutexMap.Add(type, mutexList);
            return mutexList;
        }

        /// <summary>
        /// 复制一个相同的Controller
        /// </summary>
        /// <returns></returns>
        public StateController<T> CopyTo(IStateMainBody<T> body)
        {
            StateController<T> control = new StateController<T>();
            Dictionary<T, List<T>> mutexMap = new Dictionary<T, List<T>>(_mutexMap.Count);
            Dictionary<T, StateBase<T>> stateMap = new Dictionary<T, StateBase<T>>(_stateMap.Count);
            foreach (var mutex in _mutexMap)
            {
                T[] mutexList = new T[mutex.Value.Count];
                mutex.Value.CopyTo(mutexList);
                mutexMap.Add(mutex.Key, mutexList.ToList());
            }
            foreach (var state in _stateMap)
            {
                Type t = state.Value.GetType();
                stateMap.Add(state.Key, Activator.CreateInstance(t, control) as StateBase<T>);
            }
            control.OnInitlization(body, mutexMap, stateMap);
            return control;
        }

        public override void OnRenderUpdate()
        {
        }

        public override void OnDestoryControl()
        {
            if (_mainBody != null)
            {
                _stateMap[getNowState].OnExit();
            }
            _stateMap.Clear();
            _mainBody = null;
        }

        private void FindMutex(T type, List<T> mutexList)
        {
            var stateBase = _stateMap[type];
            var stateAttrubt = stateBase.GetType().GetCustomAttribute(typeof(StateFuncAttrubite)) as StateFuncAttrubite;
            var stateMutexAttrubt = stateBase.GetType().GetCustomAttribute(typeof(StateMutexAttrubite)) as StateMutexAttrubite;
            if (stateMutexAttrubt != null)
            {
                for (int i = 0; i < stateMutexAttrubt.mutexAttrubite.Length; i++)
                {
                    mutexList.Add((T)Enum.ToObject(stateAttrubt.stateEnumType, stateMutexAttrubt.mutexAttrubite[i]));
                }
            }
            if (stateAttrubt.rootState != -1)
            {
                mutexList.AddRange(GetMutexList((T)Enum.ToObject(stateAttrubt.stateEnumType, stateAttrubt.rootState)));
            }
        }
    }
}