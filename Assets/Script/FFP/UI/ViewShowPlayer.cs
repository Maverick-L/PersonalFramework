using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Framework.FindRoad;
using Framework.FSM;
using UnityEngine.UI;
public class ViewShowPlayer : MonoBehaviour
{
    public Text playerNameText;
    public Image lineColor;
    public Text playerNowGridText;
    public Text playerTargetGridText;
    public Text playerNextGridText;
    public Text playerStateText;
    private Color _lineColor;

    public void Update(FloorPlayer player)
    {
        if(_lineColor != Color.white && player.getPlayerObject)
        {
            var lineRender = player.getPlayerObject.GetComponentInChildren<LineRenderer>();
            if (lineRender)
            {
                _lineColor = lineRender.material.color;
                lineColor.color = _lineColor;
            }
        }
        PlayerData data = player.getPlayerData;
        playerNameText.text = player.GetControlName();
        playerNowGridText.text = Global.getFindRoadManagerInstance.GetFloor(player.getType, data.bigFloorIndex, data.bigFloorTarget).getFloorData.index.ToString();
        if(data.bigFloorTarget != -1)
        {
            Floor target = Global.getFindRoadManagerInstance.GetFloor(player.getType, data.bigFloorTarget, data.floorTarget);
            if (target != null)
            {
                playerTargetGridText.text = target.getFloorData.index.ToString();
            }
            else
            {
                playerTargetGridText.text = "暂无";
            }
        }
        else
        {
            playerTargetGridText.text = "暂无";
        }
        if (data.nextBigFloorIndex != -1)
        {
            Floor target = Global.getFindRoadManagerInstance.GetFloor(player.getType, data.nextBigFloorIndex, data.nextFloorIndex);
            if (target != null)
            {
                playerNextGridText.text = target.getFloorData.index.ToString();
            }
            else
            {
                playerNextGridText.text = "暂无";
            }
        }
        else
        {
            playerNextGridText.text = "暂无";
        }
        playerStateText.text = player.GetNowState().ToString();
    }
}
