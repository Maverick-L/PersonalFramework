using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.FindRoad
{
    public static class FloorCreateMesh
    {
        public static Mesh CreateFloorMeshPlane(this FloorCreateBase floorCreate, float width, float height)
        {
            var halfWidth = width / 2;
            var halfHeight = height / 2;
            Vector3[] vertices = new Vector3[] { floorCreate.GetPoint(new Vector2( - halfWidth, -halfHeight)), //左下
                                             floorCreate.GetPoint(new Vector2( - halfWidth, + halfHeight)), //左上
                                            floorCreate. GetPoint(new Vector2(+ halfWidth,-halfHeight)), // 右下
                                            floorCreate. GetPoint( new Vector2(+halfWidth,+halfHeight)),//右上
                                             };
            int[] triangles = new int[] {0,1,2,
                                    2,1,3};



            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.SetTriangles(triangles, 0);
            return mesh;
        }
    }
}