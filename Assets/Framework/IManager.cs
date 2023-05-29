using System.Collections;
using System.Collections.Generic;

namespace Framework
{
    public interface  IManager
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void OnInitlization();
        /// <summary>
        /// patch模式的初始化
        /// </summary>
        void OnPatchInitlization();
        /// <summary>
        /// 低内存模式
        /// </summary>
        void OnLowMenery();//低内存模式
        /// <summary>
        /// 关闭
        /// </summary>
        void OnDestory();
    }
}

