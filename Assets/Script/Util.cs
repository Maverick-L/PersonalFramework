using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

public class Util
{

    public static List<Type> GetAllChildClass(Type baseType)
    {
        var allType = Assembly.GetExecutingAssembly().ExportedTypes.GetEnumerator();
        List<Type> returnTypes = new List<Type>();
        while (allType.MoveNext())
        {
            var type = allType.Current;
            if(type.BaseType == baseType && type != baseType)
            {
                returnTypes.Add(type);
                returnTypes.AddRange(GetAllChildClass(type));
            }
        }
        return returnTypes;
    }


    public static ControlBase GetControlInstanveWithAttrubate(ControllerType type)
    {
        var allType = GetAllChildClass(typeof(ControlBase));

        foreach (var controlType in allType)
        {
            var attrabute = controlType.GetCustomAttribute<ControllerAttrubite>();
            if (attrabute!=null && attrabute.type == type)
            {
                return Activator.CreateInstance(controlType) as ControlBase;
            }
        }
        return null;
    }

    public static ControllerType GetControllerType(ControlBase control)
    {
        ControllerAttrubite attrubite = control.GetType().GetCustomAttribute<ControllerAttrubite>();
        return attrubite.type;
    }

    public static ControllerType GetControllerType<TControl>() where TControl : ControlBase
    {
        ControllerAttrubite attrubite = typeof(TControl).GetCustomAttribute<ControllerAttrubite>();
        return attrubite.type;
    }

    public static Vector3 float3ToVector3(float3 value)
    {
        return new Vector3(value.x, value.y, value.z);
    }
    #region 射线检测与平面相交
    /// <summary>
    /// 检测射线与平面是否相交
    /// </summary>
    /// <returns></returns>
    public static bool CheckRaycastPlane(Ray ray,AABB aabb, float distance)
    {
        var startPoint = ray.origin;
        var dir = ray.direction;
        return CheckRaycastPlane(startPoint,dir, aabb, distance);
    }



    public static bool CheckRaycastPlane(Vector3 startPoint,Vector3 dir, AABB aabb, float distance =10000f)
    {
        //检测aabb包围圈穿过中心的两个平面，用来确定是否穿过这个平面
        var up0 = aabb.Center + aabb.Extents * new float3(-1, 1, -1);
        var up1 = aabb.Center + aabb.Extents * new float3(-1, 1, 1);
        var up2 = aabb.Max;
        var up3 = aabb.Center + aabb.Extents * new float3(1, 1, -1);
        var down0 = aabb.Min;
        var down1 = aabb.Center + aabb.Extents * new float3(-1, -1, 1);
        var down2 = aabb.Center + aabb.Extents * new float3(1, -1, 1);
        var down3 = aabb.Center + aabb.Extents * new float3(1, -1, -1);
        var n1 =  float3ToVector3(math.cross(up2 - aabb.Center, down1 - aabb.Center));
        var n2 = float3ToVector3(math.cross(up1 - aabb.Center, down2 - aabb.Center));

        Vector3 point1;
        Vector3 point2;
        
        if(CheckRaycastPlane(startPoint, dir, float3ToVector3(aabb.Center), n1,out point1, distance) && CheckRaycastPlane(startPoint, dir, float3ToVector3(aabb.Center), n2,out point2, distance))
        {
            //判断是否点在平面内
            GameObject go1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject go2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go1.transform.name = "Cube:" + point1;
            go2.transform.name = "Cube:" + point2;
            go1.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            go2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            go1.transform.position = point1;
            go2.transform.position = point2;
            return aabb.Contains(point1) || aabb.Contains(point2);
        }
        return  false ;
    }

    public static bool CheckRaycastPlane(Ray ray,Vector3 planePoint, Vector3 normalVector,out Vector3 targetPoint,float distance = 10000f)
    {
        return CheckRaycastPlane(ray.origin, ray.direction, planePoint, normalVector,out targetPoint,distance);
    }
    /// <summary>
    /// 具体实现方法
    /// 射线方程:P(t)=P 0′+tu
    /// </summary>
    /// <param name="startPoint">起始点</param>
    /// <param name="dir">方向向量</param>
    /// <param name="distance">距离</param>
    /// <param name="planePoint">平面上的点</param>
    /// <param name="normalVector">法向量</param>
    /// <param name="targetPoint">射线与平面相交的点</param>
    /// <returns></returns>
    public static bool CheckRaycastPlane(Vector3 startPoint,Vector3 dir,Vector3 planePoint,Vector3 normalVector,out Vector3 targetPoint, float distance = 10000f)
    {
        targetPoint =Vector3.zero;
        float member = Vector3.Dot(normalVector, planePoint - startPoint);
        float denominator = Vector3.Dot(normalVector, dir);
        if(denominator == 0)
        {
            return false;
        }
        var t = member / denominator;
        targetPoint = startPoint + t * dir;
        return t >= 0 && t <= distance;
    }
    #endregion

    #region 数学
    public static float GetAngle(Vector3 lv,Vector3 rv,Vector3 normal)
    {
        float angel = Vector3.Angle(lv, rv);
        float dot = Vector3.Dot(lv, rv);
        Vector3 cross = Vector3.Cross(lv, rv);
        float angel2 = Vector3.Angle(cross, normal);
        if(angel2 == 0)
        {
            return angel;
        }
        return 360 - angel;
    }
    #endregion
}
