using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace Framework.FindRoad
{
    [System.Serializable]
    public struct FFPFloorData : IComponentData
    {
        /// <summary>
        /// 实际的地块数值：floorValue + FloorType
        /// </summary>
        public int moveValue;

        /// <summary>
        /// 记录刷新count
        /// </summary>
        public int refreshCount;

        public int floorValue;
    }

    public class FFPFloor : Floor
    {
        private FFPFloorData _ffpFloorData;
        public FFPFloorData getFFPData => _ffpFloorData;
        public FFPFloor() : base()
        {
            _ffpFloorData = new FFPFloorData();
            _ffpFloorData.moveValue = 0;
            _ffpFloorData.floorValue = 0;
            _ffpFloorData.refreshCount = 0;
        }

        public bool UpdateFloorValue(int myValue,int refreshCount = -1)
        {
            if(CheckFloorNeedUpdate(myValue,refreshCount))
            {
                _ffpFloorData.moveValue = myValue + (int)getFloorData.cellType + (getFloorData.canCross ? 0 : (int)ECellMatType.Player);
                _ffpFloorData.floorValue = myValue;
                _ffpFloorData.refreshCount = myValue == 0 ? _ffpFloorData.refreshCount + 1 : refreshCount;
                return true;
            }
            return false;
        }

        public bool CheckFloorNeedUpdate(int refreshValue, int refreshCount)
        {
            if(refreshCount == -1)
            {
                return true;
            }
            if(refreshCount != _ffpFloorData.refreshCount)
            {
                return true;
            }
            return _ffpFloorData.floorValue > refreshValue;
        }

        public override void SetCross(bool canCross)
        {
            if(canCross == getFloorData.canCross)
            {
                return;
            }
            base.SetCross(canCross);
            if (canCross)
            {

                    _ffpFloorData.moveValue -= (int)ECellMatType.Player;

            }
            else
            {
                
                    _ffpFloorData.moveValue += (int)ECellMatType.Player;

                
            }
        }

        /// <summary>
        /// 获取真实的没有地块数值，没有角色存在的值
        /// </summary>
        /// <returns></returns>
        public int GetRealMoveValue()
        {
            if(!getFloorData.canCross && getFloorData.cellType != ECellMatType.Max)
            {
                return _ffpFloorData.moveValue - (int)ECellMatType.Player;
            }
            return _ffpFloorData.moveValue;
        }
        
    }
}