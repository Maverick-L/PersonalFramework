using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Framework.FindRoad
{


    [ControllerAttrubite(ControllerType.FFP)]
    public class FFPController : FloorControllerBase
    {
        private MyInput _input;
        private Floor _lastRefreshFloor;
        public override void OnDestoryControl()
        {
            base.OnDestoryControl();
            _lastRefreshFloor = null;
        }

        public override void OnInitlization()
        {
            _findRoadManager = Global.getFindRoadManagerInstance;
            base.OnInitlization();
            InitFloorData<FFPFloor>();
        }

        public override IEnumerator AsynInitlization()
        {
            yield return InstanceFloor();
        }

        public IEnumerator InstanceFloor()
        {
            yield return CreateGameObject();

            //给每一个地块加上HUD
            foreach (var floorList in getFloorMap.Values)
            {
                for (int i = 0; i < floorList.Count; i++)
                {
                    yield return CreateFloorHUD(floorList[i]);
                }
            }


            isInit = true;
        }


        public override void OnLogicUpdate()
        {
            if (!isInit) return;

            base.OnLogicUpdate();
            var input = _findRoadManager.getFrameInput;
            if (input.click == 1)
            {
                _input = input;
                Floor floor;
                if (CheckClickFloor(_input, out floor))
                {
                    UpdateMap(floor);
                    clickPoint?.Invoke(floor);
                }
            }
           
            
        }

        public override void OnRenderUpdate()
        {
            if (!isInit) return;

            base.OnRenderUpdate();

            int[][] map = _findRoadManager.GetAllPlayerFloor(Util.GetControllerType(this));
            List<Floor> playerList = new List<Floor>();
            Floor target = _findRoadManager.GetControl(Util.GetControllerType(this)).GetTargetFloor();
            for (int i = 0; i < map.Length; i++)
            {
                Floor floor = GetFloor(map[i][0], map[i][1]);
                playerList.Add(floor);
            }
            //更新HUD
            foreach (var floorList in getFloorMap.Values)
            {
                for (int i = 0; i < floorList.Count; i++)
                {
                    //更新HUD位置
                    floorList[i].hudController?.OnRenderUpdate();
                    //更新HUD显示
                    if (floorList[i].hudController)
                    {
                        bool show =  playerList.Contains(floorList[i]) || floorList[i] == target;
                        CellData my = floorList[i].getFloorData;
                        Floor nextFloor;
                        if(_lastRefreshFloor != null)
                        {
                            GetNextFloor(my.bigCellIndex, my.cellIndex, true, out nextFloor);
                            floorList[i].hudController.SetActive("@Text_Count", nextFloor != floorList[i], "FFPHUD");
                            if ( nextFloor != floorList[i])
                            {
                                Vector3 down = getFloorCreate.GetPoint(Vector2.down);
                                Vector3 dir = nextFloor.getFloorObj.transform.position - floorList[i].getFloorObj.transform.position;
                                float angel = Util.GetAngle(dir, down, getFloorCreate.GetNomalVector());
                                floorList[i].hudController.SetActive("@Text_Count", nextFloor != floorList[i], "FFPHUD");
                                floorList[i].hudController.SetRotation("@Text_Count", new Vector3(90f, 0f, angel), "FFPHUD");
                            }
                            else
                            {
                                // floorList[i].hudController.SetRotation("@Text_Count",Vector3.zero, "FFPHUD");

                            }

                        }
                        else
                        {
                            floorList[i].hudController.SetActive("@Text_Count",false, "FFPHUD");

                        }

                    }
                }
            }
        }

        public override bool GetNextFloor(int nowBaseIndex, int nowIndex, bool isFindRoad, out Floor nextFloor,int targetBaseIndex = -1,int targetIndex=-1)
        {
            FFPFloor nowFloor = GetFloor(nowBaseIndex, nowIndex) as FFPFloor;
            if (!isFindRoad )
            {
                FFPFloor targetFloor = GetFloor(targetBaseIndex, targetIndex) as FFPFloor;
                if(_lastRefreshFloor == null || targetFloor != _lastRefreshFloor)
                {
                    UpdateMap(GetFloor(targetBaseIndex, targetIndex));
                }
            }
            FFPFloor temp ;
            bool next = GetNextMoveFloor(nowFloor, out temp);
            nextFloor = temp;
            return next;
        }

        public override Floor GetGridWithTargetArea(int nowBaseIndex, int nowIndex, int targetBaseIndex, int targetIndex,int checkArea = -1)
        {
            FFPFloor nowFloor = GetFloor<FFPFloor>(nowBaseIndex, nowIndex);
            FFPFloor targetFloor = GetFloor<FFPFloor>(targetBaseIndex, targetIndex);
            int checkIndex = 1;
            FFPFloor nextTarget = nowFloor;
            int[] pos = GetFloorCoordinates(targetFloor);
            do
            {
                int minX = pos[0] - checkIndex;
                int minY = pos[1] - checkIndex;
                int maxX = pos[0] + checkIndex;
                int maxY = pos[1] + checkIndex;
                if(!CheckPointInFloorMap(minX,minY) && !CheckPointInFloorMap(minX,maxY) && ! CheckPointInFloorMap(maxX,minY) && !CheckPointInFloorMap(maxX, maxY))
                {
                    break;
                }
                for (int x = (minX<0 ? 0:minX); x <= maxX; x++)
                {
                    FFPFloor temp;
                    if(GetCanCrossFloor(x,minY,nextTarget,out temp, true))
                    {
                        nextTarget = temp;
                    }
                }
                for(int x = (minX <0?0:minX);x<= maxX; x++)
                {
                    FFPFloor temp;
                    if (GetCanCrossFloor(x, maxY, nextTarget, out temp, true))
                    {
                        nextTarget = temp;
                    }
                }
                for(int y = (minY<0? 0 : minY); y <= maxY; y++)
                {
                    FFPFloor temp;
                    if (GetCanCrossFloor(minX, y, nextTarget, out temp, true))
                    {
                        nextTarget = temp;
                    }
                }
                for (int y = (minY < 0 ? 0 : minY); y <= maxY; y++)
                {
                    FFPFloor temp;
                    if (GetCanCrossFloor(maxX, y, nextTarget, out temp,true))
                    {
                        nextTarget = temp;
                    }
                }
                checkIndex++;
            } while (nextTarget != nowFloor &&(checkArea == -1 || checkIndex < checkArea));
            return nextTarget;
        }

        private void UpdateMap(Floor targetFloor)
        {
            //遍历所有的地块，标记当前地块的值
            var pos = GetFloorCoordinates(targetFloor);
            UpdateMapDfs(0, pos[0], pos[1],-1);
            _lastRefreshFloor = targetFloor;
            _unCrossCache.Clear();
            UpdateCrossCache();
        }

        private void UpdateMapDfs(int value,int centerX,int centerY,int refreshCount)
        {
            FFPFloor floor =null;
            int nextValue;
            if(!GetFloorWithCoordinates<FFPFloor>(centerX, centerY, out floor))
            {
                return ;
            }
            //先给自己赋值
            if (!floor.UpdateFloorValue(value, refreshCount))
            {
                return;
            }
            nextValue = floor.getFFPData.moveValue +1;

            UpdateMapDfs(nextValue, centerX - 1, centerY,floor.getFFPData.refreshCount);
            UpdateMapDfs(nextValue, centerX + 1, centerY, floor.getFFPData.refreshCount);
            UpdateMapDfs(nextValue, centerX, centerY + 1, floor.getFFPData.refreshCount);
            UpdateMapDfs(nextValue, centerX, centerY - 1, floor.getFFPData.refreshCount);
           
        }

        /// <summary>
        /// 获取下一个目标方向
        /// </summary>
        /// <param name="nowFloor"></param>
        /// <returns></returns>
        private bool GetNextMoveFloor(FFPFloor nowFloor,out FFPFloor nextFloor)
        {
            CellData floorData = nowFloor.getFloorData;

            int[] pos = GetFloorCoordinates(floorData.index);
            //查询以当前节点为中心点的八个方向的数值，确定最小值，标定当走到这个地块上面之后，下一个目标是哪里
            FFPFloor minFloor = nowFloor;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    
                    if (GetCanCrossFloor(pos[0] + i, pos[1] + j, minFloor, out nextFloor, false))
                    {
                        minFloor = nextFloor;
                        if (nextFloor.getFFPData.floorValue == 0)
                        {
                            return nextFloor.GetCrossState(true);
                        }
                    }
                }
            }
            nextFloor = minFloor;
            return nextFloor.GetCrossState(true);
        }

        private bool GetCanCrossFloor(int x, int y,FFPFloor minFloor,out FFPFloor floor,bool checkPlayerInFloor)
        {
            if (GetFloorWithCoordinates<FFPFloor>(x, y, out floor))
            {
                if (!floor.GetCrossState(checkPlayerInFloor))
                {
                    return false;
                }
                if(minFloor == null)
                {
                    return true;
                }
                if(!checkPlayerInFloor)
                {
                    return minFloor.GetRealMoveValue() > floor.GetRealMoveValue();
                }
                return minFloor.getFFPData.moveValue > floor.getFFPData.moveValue;
            }
            floor = null;
            return false;
        }
    }
}
