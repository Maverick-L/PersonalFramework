using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.FSM
{
    //状态类的基类
    public abstract class StateBase<T> where T : Enum
    {
        protected StateController<T> _stateControl;
        public StateBase(StateController<T> control)
        {
            _stateControl = control;
        }

        public virtual bool TryEnter()
        {
            return true;
        }

        public abstract void OnEnter();

        public abstract void OnUpdate();

        public abstract void OnExit();

        public virtual void OnModifyStateMainBody() { }
    }



    public class StateFuncAttrubite : Attribute
    {
        public Type stateEnumType;
        public int state;
        public int rootState;
        public StateFuncAttrubite(Type stateType, int state, int rootState = -1)
        {
            stateEnumType = stateType;
            this.state = state;
            this.rootState = rootState;
        }
    }

    public class StateMutexAttrubite : Attribute
    {
        public int[] mutexAttrubite;

        public StateMutexAttrubite(params int[] mutex)
        {
            mutexAttrubite = mutex;

        }
    }
}