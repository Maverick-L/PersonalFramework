using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using Unity.Rendering;

namespace Framework.FindRoad
{
    /// <summary>
    /// 地块基类，不同的模式可以继承 重写
    /// </summary>
    public class Floor : IComparable<Floor>
    {
        private CellData m_floorData;


        public CellData getFloorData
        {
            get { return m_floorData; }
        }

        internal CellData setFloorData
        {
             set { m_floorData = value; }
        }

        #region Render
        private GameObject m_floorObj;

        public GameObject getFloorObj
        {
            get { return m_floorObj; }
        }

        private HUDController m_hudControl;
        public HUDController hudController { get { return m_hudControl; } internal set { m_hudControl = value; } }
        #endregion
        private FloorControllerBase m_controller;
        public FloorControllerBase controller { get { return m_controller; } internal set { m_controller = value; } }

        public Floor()
        {

        }

        public Floor(CellData data, FloorControllerBase controller = null, GameObject obj = null)
        {
            m_floorData = data;
            m_floorObj = obj;
            m_controller = controller;
            if (obj != null)
            {
                obj.transform.SetParent(m_controller.getFloorRoot.transform);

            }
        }

        public void CreateGameObject()
        {
            m_floorObj = m_controller.getFloorCreate.CreateFloorGameObject(m_floorData);
            if (m_floorObj)
            {
                m_floorObj.transform.SetParent(m_controller.getFloorRoot.transform);
            }
        }

        public void Destory()
        {
            GameObject.Destroy(m_floorObj);
            hudController?.OnDestoryControl();
            hudController = null;
            m_floorObj = null;
        }

        internal void SetFloorDataBaseIndex(int index,int floorIndex = -1)
        {
            m_floorData.bigCellIndex = index;
            m_floorData.cellIndex = floorIndex;
        }


        /// <summary>
        /// 检测点是不是在这个范围内
        /// </summary>
        /// <param name="point">平面的点击点</param>
        /// <returns></returns>
        public bool CheckPoint(Vector3 point)
        {
            return m_floorData.aabb.Contains(point);
        }

        public virtual void SetCross(bool canCross)
        {
            if (canCross == getFloorData.canCross)
            {
                return;
            }
            m_floorData.canCross = canCross && getFloorData.cellType != ECellMatType.Max;
        }

        /// <summary>
        /// 检测地块是否可以通过
        /// </summary>
        /// <param name="checkPlayerInFloor">是否检测当前地块是否有人物占用</param>
        /// <returns></returns>
        public bool GetCrossState(bool checkPlayerInFloor)
        {
            if (checkPlayerInFloor)
            {
                return m_floorData.canCross;
            }
            return getFloorData.cellType != ECellMatType.Max;
        }

        public int CompareTo(Floor other)
        {
            return getFloorData.index - other.getFloorData.index;
        }

        public static bool operator == (Floor a,Floor b)
        {
            if(a is null && b is null)
            {
                return true;
            }
            else if(a is null || b is null)
            {
                return false;
            }
            return a.getFloorData.index == b.getFloorData.index;
        }

        public static bool operator !=(Floor a, Floor b)
        {
            return !(a == b);

        }
    }


    public class FloorEntities
    {


    }

}