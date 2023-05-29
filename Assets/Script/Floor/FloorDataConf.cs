using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public enum AxisType
{
    X = 1,
    Y = 2,
    Z = 3,
}


public class  FloorDataConf
{

    public static readonly float FLOOR_WIDTH = 2; //地块宽度
    public static readonly AxisType FLOOR_AXIS = AxisType.Y; //地块轴

    public static readonly int2 FLOOR_LIMIT = new int2(20,20);//地块数量显示

    public static readonly int FLOOR_LIMIT_LENGTH = FLOOR_LIMIT.x * FLOOR_LIMIT.y;

    public static readonly int2 RAY_CHECK_BOUND = new int2(5, 5);//射线检测分区

    public static readonly int RAY_CHECK_LENGTH = RAY_CHECK_BOUND.x * RAY_CHECK_BOUND.y;

}

public class PlayerDataConf
{
    public static readonly int CREATE_PLAYER_COUNT = 10;


}
