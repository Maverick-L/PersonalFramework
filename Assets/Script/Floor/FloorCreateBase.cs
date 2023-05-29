using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace Framework.FindRoad
{
    public abstract class FloorCreateBase
    {

        protected AxisType _axis;
        public FloorCreateBase(int axis)
        {
            _axis = (AxisType)axis;
        }
        /// <summary>
        /// ͨ���������ȡһ���ؿ������
        /// </summary>
        /// <returns></returns>
        public ECellMatType GetFloorType()
        {
            var rodamValue = (int)Random.Range(0, (int)ECellMatType.Count);
            return (ECellMatType)(1 << rodamValue);
        }

        /// <summary>
        /// ���ɵ����߼�����
        /// </summary>
        /// <param name="startPostion">��ʼ��</param>
        /// <param name="line">����</param>
        /// <param name="list">����</param>
        /// <returns></returns>
        public abstract Floor[] CreateFloor<T>(Vector2 startPostion, int line, int row, int bigIndex = -1) where T : Floor, new();

        /// <summary>
        /// ���������ؿ�
        /// </summary>
        /// <param name="startPostion"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public abstract Floor CreateFloorWithRayCheck(Vector2 startPostion, float width, float height);

        /// <summary>
        /// ����Mesh
        /// </summary>
        /// <param name="floorValue"></param>
        /// <returns></returns>
        public abstract Mesh CreateFloorMesh(CellData floorValue);

        /// <summary>
        /// ������������
        /// </summary>
        /// <param name="floorValue"></param>
        public GameObject CreateFloorGameObject(CellData floorValue)
        {
            GameObject go = new GameObject();
            var renderer = go.AddComponent<MeshRenderer>();
            var filter = go.AddComponent<MeshFilter>();
            Vector3 center = floorValue.aabb.Center;
            go.transform.position = center;
            go.transform.name = "BigFloorIndex_" + floorValue.bigCellIndex + "_Floor_" + floorValue.index;

            filter.mesh = CreateFloorMesh(floorValue);
            renderer.material = FloorControllerBase.GetFloorMat(floorValue.cellType);
            return go;
        }



        public Vector3 GetPoint(Vector2 point)
        {
            switch (_axis)
            {
                case AxisType.X: return new Vector3(0, point.x, point.y);
                case AxisType.Y: return new Vector3(point.x, 0, point.y);
            }
            return point;
        }

        public Vector2 GetPointVector2(Vector3 point)
        {
            switch (_axis)
            {
                case AxisType.X: return new Vector2(point.y, point.z);
                case AxisType.Y: return new Vector2(point.x, point.z);

            }
            return point;
        }

        /// <summary>
        /// ��ȡƽ��ķ�����
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNomalVector()
        {
            switch (_axis)
            {
                case AxisType.X: return new Vector3(1, 0, 0);
                case AxisType.Y: return new Vector3(0, 1, 0);
                case AxisType.Z: return new Vector3(0, 0, 1);
            }
            return Vector3.zero;
        }
    }
}