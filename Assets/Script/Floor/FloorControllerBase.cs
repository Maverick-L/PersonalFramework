using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.FindRoad
{






    /// <summary>
    /// 地块系统，地块系统采用 多个大的范围，每一个范围内生成固定数量的地块数据，用来减少运算量
    /// </summary>
    public abstract class FloorControllerBase : ControlBase
    {
        public static readonly string LAND_MAT_PATH = "Floor/FloorLandMat";
        public static readonly string SEA_MAT_PATH = "Floor/FloorSeaMat";
        public static readonly string MOUNTAIN_MAT_PATH = "Floor/FloorMountainMat";
        public static readonly string MAX_MAT_PATH = "Floor/FloorUnMoveMat";
        public static readonly string Transmission_MAT_PATH = "Floor/TransmissionMat";
        public static readonly string FLOOR_HUD_PATH = "Floor/FloorHud";

        public bool isInit;

        private FloorCreateBase _floorCreate;
        public FloorCreateBase getFloorCreate => _floorCreate;

        private Dictionary<int, List<Floor>> _floorMap;

        protected Dictionary<int, List<Floor>> getFloorMap
        {
            get { return _floorMap; }
        }

        /// <summary>
        /// 地块行列二位数组
        /// </summary>
        private Floor[,] _floorArray;
        protected Floor[,] getFloorArray
        {
            get
            {
                if (_floorArray == null)
                {
                    _floorArray = new Floor[FloorDataConf.FLOOR_LIMIT.x, FloorDataConf.FLOOR_LIMIT.y];
                    foreach (var floorList in _floorMap.Values)
                    {
                        for (int i = 0; i < floorList.Count; i++)
                        {
                            var index = floorList[i].getFloorData.index;
                            var coordinate = GetFloorCoordinates(index);
                            _floorArray[coordinate[0], coordinate[1]] = floorList[i];
                        }
                    }
                }
                return _floorArray;
            }
        }


        private Floor[] _baseFloorBlock;

        protected Floor[] getBaseFloorBlock
        {
            get { return _baseFloorBlock; }
        }
        protected FindRoadManager _findRoadManager;

        public int getFloorCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _baseFloorBlock.Length; i++)
                {
                    count += _floorMap[_baseFloorBlock[i].getFloorData.bigCellIndex].Count;
                }
                return count;
            }
        }

        private GameObject _floorRootObj;
        public GameObject getFloorRoot
        {
            get
            {
                if (_floorRootObj == null)
                {
                    GameObject go = new GameObject(this.GetType().Name);
                    _floorRootObj = go;
                }
                return _floorRootObj;
            }
        }

        /// <summary>
        /// 不可穿越的缓存,每帧更新使用
        /// </summary>
        protected List<Floor> _unCrossCache = new List<Floor>();

        public Action<Floor> clickPoint;
        #region Static
        public static Material GetFloorMat(ECellMatType floortype)
        {
            switch (floortype)
            {
                case ECellMatType.Land:
                    return Resources.Load<Material>(LAND_MAT_PATH);
                case ECellMatType.Sea:
                    return Resources.Load<Material>(SEA_MAT_PATH);
                case ECellMatType.Mountain:
                    return Resources.Load<Material>(MOUNTAIN_MAT_PATH);
                case ECellMatType.Max:
                    return Resources.Load<Material>(MAX_MAT_PATH);
                case ECellMatType.Min:
                default:
                    return new Material(Shader.Find("Standard"));
            }
        }

        #endregion

        #region Public
        public override void OnInitlization()
        {
            _floorCreate = new FloorCreateSquare(FloorDataConf.FLOOR_WIDTH, (int)FloorDataConf.FLOOR_AXIS);
            _floorMap = new Dictionary<int, List<Floor>>();
        }



        public void InitFloorData<T>() where T : Floor, new()
        {
            Vector2 point = new Vector2(0, 0);
            var floors = _floorCreate.CreateFloor<T>(point, FloorDataConf.FLOOR_LIMIT.x, FloorDataConf.FLOOR_LIMIT.y);
            int baseIndex = 0;
            int rawMaxBaseIndex = 0;
            for (int i = 0; i < FloorDataConf.FLOOR_LIMIT.x; i++)
            {
                for (int j = 0; j < FloorDataConf.FLOOR_LIMIT.y; j++)
                {
                    int index = i * FloorDataConf.FLOOR_LIMIT.y + j;
                    int realBaseIndex = j / FloorDataConf.RAY_CHECK_BOUND.y + baseIndex;
                    if (!_floorMap.ContainsKey(realBaseIndex))
                    {
                        _floorMap.Add(realBaseIndex, new List<Floor>(FloorDataConf.RAY_CHECK_LENGTH));
                        rawMaxBaseIndex = Mathf.Max(rawMaxBaseIndex, realBaseIndex);
                    }
                    floors[index].SetFloorDataBaseIndex(realBaseIndex,_floorMap[realBaseIndex].Count);
                    floors[index].controller = this;
                    _floorMap[realBaseIndex].Add(floors[index]);
                }
                if ((i + 1) % FloorDataConf.RAY_CHECK_BOUND.x == 0)
                {
                    baseIndex = rawMaxBaseIndex + 1;
                }
            }
            _baseFloorBlock = new Floor[rawMaxBaseIndex + 1];
            //创建分区地块信息
            foreach (var floor in _floorMap)
            {
                int floorBaseIndex = floor.Key;
                //获得左下角第一个地块的创建坐标，为起始坐标
                float2 floorCreatePos = floor.Value[0].getFloorData.createPos;
                _baseFloorBlock[floorBaseIndex] = _floorCreate.CreateFloorWithRayCheck(floorCreatePos, FloorDataConf.RAY_CHECK_BOUND.x * FloorDataConf.FLOOR_WIDTH, FloorDataConf.RAY_CHECK_BOUND.y * FloorDataConf.FLOOR_WIDTH);
                _baseFloorBlock[floorBaseIndex].controller = this;
                _baseFloorBlock[floorBaseIndex].SetFloorDataBaseIndex(floorBaseIndex);
            }
        }




        public IEnumerator CreateGameObject()
        {

            foreach (var _floorList in _floorMap.Values)
            {
                for (int i = 0; i < _floorList.Count; i++)
                {
                    _floorList[i].CreateGameObject();
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return null;
        }

        #region HUD 展示相关内容
        public IEnumerator CreateFloorHUD(Floor floor)
        {
            var HUDGameObject = GameObject.Instantiate(Resources.Load<GameObject>(FLOOR_HUD_PATH));
            var allAttributes = this.GetType().GetCustomAttributes(true);
            var HUDName = "";
            for (int i = 0; i < allAttributes.Length; i++)
            {
                if (allAttributes[i] is ControllerAttrubite)
                {
                    HUDName = (allAttributes[i] as ControllerAttrubite).type.ToString() + "HUD";
                    break;
                }
            }
            var hudController = HUDGameObject.AddComponent<HUDController>();
            hudController.OnInitData(floor.getFloorObj, Global.getFindRoadManagerInstance.getRenderCamera, "Floor", HUDName);
            hudController.OnInitlization();
            floor.hudController = hudController;
            ShowFloorCoordinate(floor, false);
            yield return null;
        }

        public IEnumerator FloorCoordinate(bool enable)
        {
            if (isInit)
            {
                foreach (var list in getFloorMap.Values)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        ShowFloorCoordinate(list[i], enable);
                    }
                }
            }
            yield return null;
        }

        private void ShowFloorCoordinate(Floor floor, bool enable = true)
        {
            GameObject baseHUD;
            if (floor.hudController == null)
            {
                return;
            }
            baseHUD = floor.hudController.transform.Find("FFPHUD/@Text_Base").gameObject;
            baseHUD.SetActive(enable);
            if (!enable)
            {
                return;
            }

            var coordinate = GetFloorCoordinates(floor);
            var text = baseHUD.GetComponent<Text>();
            text.text = string.Format("{0}:({1},{2})", floor.getFloorData.index, coordinate[0], coordinate[1]);
        }
        #endregion
        public override void OnDestoryControl()
        {

        }

        /// <summary>
        /// 获取一个可以穿过的随机位置
        /// </summary>
        /// <returns></returns>
        public Floor GetRandomCanCrossPoint()
        {
            Dictionary<int, List<int>> canCrossMap = new Dictionary<int, List<int>>();
            foreach (var floorValue in _floorMap)
            {
                var index = floorValue.Key;
                var floorList = floorValue.Value;
                canCrossMap.Add(index, new List<int>());
                for (int i = 0; i < floorList.Count; i++)
                {
                    if (floorList[i].getFloorData.canCross)
                    {
                        canCrossMap[index].Add(floorList[i].getFloorData.cellIndex);
                    }
                }
                if (canCrossMap[index].Count == 0)
                {
                    canCrossMap.Remove(index);
                }
            }
            if (canCrossMap.Count == 0)
            {
                return null;
            }
            int bigIndex = -1;
            while (!canCrossMap.ContainsKey(bigIndex))
            {
                bigIndex = UnityEngine.Random.Range(0, _baseFloorBlock.Length - 1);
            }
            int floorIndex = UnityEngine.Random.Range(0, canCrossMap[bigIndex].Count - 1);
            return GetFloor(bigIndex,canCrossMap[bigIndex][floorIndex]);
        }

        public Floor GetFloor(int baseIndex, int floorIndex)
        {
            try
            {
                return _floorMap[baseIndex][floorIndex];
            }
            catch (Exception ex)
            {
                Debug.LogError("获取FloorData错误:" + ex.Message+"baseIndex:"+baseIndex+"\tfloorIndex:"+floorIndex);
            }
            return null;
        }

        public Floor GetFloor(int index)
        {
            int[] pos = GetFloorCoordinates(index);
            Floor floor;
            GetFloorWithCoordinates<Floor>(pos[0], pos[1], out floor);

            return floor;
        }

        public T GetFloor<T>(int baseIndex,int floorIndex) where T : Floor
        {
            return GetFloor(baseIndex, floorIndex) as T;
        }

        public abstract bool GetNextFloor(int nowBaseIndex, int nowIndex, bool isFindRoad, out Floor nextFloor, int targetBaseIndex = -1, int targetIndex = -1);

        /// <summary>
        /// 获取在目标点周围距离当前点最近的一个可以穿越的位置
        /// </summary>
        /// <param name="nowBaseIndex"></param>
        /// <param name="nowIndex"></param>
        /// <param name="targetBaseIndex"></param>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        public abstract Floor GetGridWithTargetArea(int nowBaseIndex, int nowIndex, int targetBaseIndex, int targetIndex,int checkArea =-1);


        public bool CheckFloorCanCross(int bigIndex,int index,bool checkPlayerInFloor)
        {
            return GetFloor(bigIndex, index).GetCrossState(checkPlayerInFloor);
        }

        /// <summary>
        /// 创建 大地块 展示
        /// </summary>
        public void CreateBigGameObject()
        {
            for (int i = 0; i < _baseFloorBlock.Length; i++)
            {
                _baseFloorBlock[i].CreateGameObject();

            }
        }

        public override void OnLogicUpdate()
        {
            //更新地块状态
            UpdateCrossCache();
        }

        internal void UpdateCrossCache()
        {
            int[][] map = _findRoadManager.GetAllPlayerFloor(Util.GetControllerType(this));
            List<Floor> list = new List<Floor>();
            for (int i = 0; i < map.Length; i++)
            {
                Floor floor = GetFloor(map[i][0], map[i][1]);

                list.Add(floor);
            }
            for (int i = _unCrossCache.Count - 1; i >= 0; i--)
            {
                Floor floor = _unCrossCache[i];
                if (list.Contains(floor))
                {
                    list.Remove(floor);
                }
                else
                {
                    floor.SetCross(true);
                    _unCrossCache.RemoveAt(i);
                    UpdatePlayerCrossFloor(floor);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                _unCrossCache.Add(list[i]);
                list[i].SetCross(false);
                UpdatePlayerCrossFloor(list[i]);
            }
        }

        public override void OnRenderUpdate()
        {
        }
        #endregion

        #region Protected

        /// <summary>
        /// 检测点击
        /// </summary>
        /// <param name="postion">点击的世界坐标</param>
        /// <param name="floor"></param>
        /// <returns></returns>
        protected bool CheckClick(Ray cameraRay, out Floor floor)
        {
            floor = null;
            int baseIndex = -1;
            //获取在整个大平面的点
            Vector3 clickPoint;
            if (Util.CheckRaycastPlane(cameraRay, _baseFloorBlock[0].getFloorData.aabb.Center, _floorCreate.GetNomalVector(), out clickPoint))
            {
                for (int i = 0; i < _baseFloorBlock.Length; i++)
                {
                    if (_baseFloorBlock[i].CheckPoint(clickPoint))
                    {
                        baseIndex = i;
                        break;
                    }
                }
                if (baseIndex == -1)
                {
                    return false;
                }
                var floorList = _floorMap[baseIndex];
                for (int i = 0; i < floorList.Count; i++)
                {
                    if (floorList[i].CheckPoint(clickPoint))
                    {
                        floor = floorList[i];
                        return true;
                    }
                }
            }

            return false;
        }


        protected bool CheckClickFloor(MyInput _input, out Floor target)
        {
            //获取点击到的具体的地板
            target = null;
            var realClickPoint = Camera.main.ScreenToWorldPoint(_input.pointPos);
            Ray ray = Camera.main.ScreenPointToRay(_input.pointPos);
            if (CheckClick(ray, out target))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取地块的行列坐标,不支持大地块查询
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        protected int[] GetFloorCoordinates(Floor floor)
        {
            return GetFloorCoordinates(floor.getFloorData.index);
        }

        /// <summary>
        /// 获取地块的行列坐标
        /// </summary>
        /// <param name="floor">Floor Index</param>
        /// <returns></returns>
        internal int[] GetFloorCoordinates(int index)
        {
            var line = (int)(index / (FloorDataConf.FLOOR_LIMIT.y));
            if(index % FloorDataConf.FLOOR_LIMIT.y == 0)
            {
                line--;
            }
            var row = (int)((index - 1) % FloorDataConf.FLOOR_LIMIT.x);
            return new int[] { line, row };
        }

        internal bool  GetFloorWithCoordinates<T>(int x,int y,out T  floor) where T:Floor
        {
            floor = null;

            if(!CheckPointInFloorMap(x,y))
            {
                return false;
            }
            floor = getFloorArray[x,y] as T;
            return true;
        }

        /// <summary>
        /// 检测地块索引是否在二维坐标内部
        /// </summary>
        /// <returns></returns>
        internal bool CheckPointInFloorMap(int x ,int y)
        {
            return (y < FloorDataConf.FLOOR_LIMIT.x && y >= 0) && (x < FloorDataConf.FLOOR_LIMIT.y && x >= 0);
        }


        protected virtual void UpdatePlayerCrossFloor(Floor floor) { }

        #endregion


        #region Entity
        public void ConvertToEntity()
        {
            ConvertAllFloorToEntity(CustomBootStrap.GetWorld(Global.LOGIC_WORLD_NAME).EntityManager);
        }
        public void ConvertAllFloorToEntity(EntityManager entityManager)
        {
            foreach (var floorList in _floorMap.Values)
            {
                for (int i = 0; i < floorList.Count; i++)
                {
                    var floor = floorList[i];
                    if (floor.getFloorObj == null)
                    {
                        ConvertToEntity(entityManager, floor.getFloorData);
                    }
                    else
                    {
                        ConvertToEntity(entityManager, floor.getFloorData, floor.getFloorObj.GetComponent<MeshFilter>().mesh);
                    }
                    floor.Destory();

                }
            }

            for (int i = 0; i < _baseFloorBlock.Length; i++)
            {
                var floor = _baseFloorBlock[i];
                if (floor.getFloorObj == null)
                {
                    ConvertToEntity(entityManager, floor.getFloorData);
                }
                else
                {
                    ConvertToEntity(entityManager, floor.getFloorData, floor.getFloorObj.GetComponent<MeshFilter>().mesh);
                }
                floor.Destory();
            }

            _floorMap.Clear();
        }


        public void ConvertToEntity(EntityManager entityManage, CellData floor, Mesh mesh = null)
        {
            var archage = entityManage.CreateArchetype(typeof(CellData), typeof(LocalToWorld),
                typeof(RenderBounds));
            var entity = entityManage.CreateEntity(archage);
            entityManage.SetComponentData<LocalToWorld>(entity, new LocalToWorld()
            {
                Value = new float4x4(rotation: quaternion.identity, translation: floor.aabb.Center)
            });
            if (!floor.isBigCell)
            {
                mesh = mesh == null ? _floorCreate.CreateFloorMesh(floor) : mesh;

                entityManage.AddSharedComponentData<RenderMesh>(entity, new RenderMesh()
                {
                    mesh = mesh,
                    material = GetFloorMat(floor.cellType),
                    layer = 0,
                    subMesh = 0,
                    castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
                    receiveShadows = true,
                    needMotionVectorPass = true
                });
                entityManage.SetComponentData<RenderBounds>(entity, new RenderBounds() { Value = mesh.bounds.ToAABB() });
            }
        }
        #endregion
    }
}