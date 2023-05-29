using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Framework.FindRoad
{
    public enum ECellMatType
    {
        Min = 0,
        Land = 0X0001,//陆地
        Sea = 0X0002,//海洋
        Mountain = 0X0004,//山地
        Max = 0X0080,//限制最大
        Player = 0X0080,//玩家占用
        Count = 3,
    }

    public enum ECellType
    {
        Square = 1, //正方形
    }

    [SerializeField]
    public struct CellData : IComponentData
    {
        public float2 createPos;
        public ECellMatType cellType;
        public AABB aabb;
        public bool canCross;
        public bool isBigCell;
        public int index; //整体索引的index
        public int bigCellIndex;
        public int cellIndex; //二维坐标索引的index和bigFloorIndex 合并使用
    }

    /// <summary>
    /// 地块实例
    /// </summary>
    public class CellEntity
    {
        public List<CellData> cellMap;
        public AxisType axis;


    }
}

