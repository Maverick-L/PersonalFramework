using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
namespace Framework.FindRoad
{
    public class FloorCreateSquare : FloorCreateBase
    {
        private float _width;

        public FloorCreateSquare(float width, int axis) : base(axis)
        {
            _width = width;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPostion">左下角的位置</param>
        /// <param name="line"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public override Floor[] CreateFloor<T>(Vector2 startPostion, int line, int row, int bigIndex = -1)
        {
            Vector2 init = startPostion;
            Floor[] allFloor = new Floor[line * row];
            Vector2 offset = new Vector2(_width / 2, _width / 2);
            int index = 1;
            int halfLine = line / 2;
            int halfRow = row / 2;
            for (int i = 0; i < line; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    allFloor[index - 1] = CreateFloor<T>(startPostion, offset, index++, bigIndex, _width);

                    startPostion.x += _width;
                }
                startPostion.x = init.x;
                startPostion.y += _width;
            }



            return allFloor;
        }


        public override Floor CreateFloorWithRayCheck(Vector2 startPostion, float width, float height)
        {
            Vector2 offset = new Vector2(width / 2, height / 2);
            var point = startPostion + offset;
            return new Floor(new CellData()
            {
                aabb = new AABB()
                {
                    Center = new float3(GetPoint(point)),
                    Extents = new float3(GetPoint(new Vector2(width / 2, height / 2)))
                },
                isBigCell = true
            });

        }


        public override Mesh CreateFloorMesh(CellData floorValue)
        {
            if (floorValue.isBigCell)
            {
                var extent = GetPointVector2(floorValue.aabb.Size);
                return this.CreateFloorMeshPlane(extent.x, extent.y);
            }
            return this.CreateFloorMeshPlane(_width, _width);
        }



        private T CreateFloor<T>(Vector2 point, Vector2 offset, int index, int bigIndex = -1, float width = -1) where T : Floor, new()
        {
            if (width == -1)
            {
                width = _width;
            }
            var centerPoint = point + offset;
            CellData data = new CellData();
            data.index = index;
            data.aabb = new Unity.Mathematics.AABB();
            data.aabb.Center = new float3(GetPoint(centerPoint));
            data.aabb.Extents = new float3(GetPoint(new Vector2(width / 2, width / 2)));
            data.cellType = GetFloorType();
            data.canCross = data.cellType != ECellMatType.Max;
            data.createPos = point;
            data.bigCellIndex = bigIndex;
            T t = new T();
            t.setFloorData = data;
            return t;
        }
    }
}