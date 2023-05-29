using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ViewFFP : MonoBehaviour
{
    public Toggle bigIndexToggle;
    public Button createPlayer;
    public InputField createPlayerInput;
    public GameObject playerObj;
    public Transform content;

    private int _playerCount = 0;
    private FindRoadControl _control;
    private Dictionary<string, ViewShowPlayer> _playerMap = new Dictionary<string, ViewShowPlayer>();
    private void OnEnable()
    {
        bigIndexToggle.isOn = false;
        bigIndexToggle.onValueChanged.AddListener(ToggleValueChangeWithBigIndex);
        createPlayerInput.text = _playerCount.ToString();
        createPlayerInput.onValueChanged.AddListener((string value) =>
        {
            _playerCount = int.Parse(value);
        });
        createPlayer.onClick.AddListener(OnClickCreatePlayer);
        _control = Global.getFindRoadManagerInstance.GetControl(ControllerType.FFP);
    }


    private void ToggleValueChangeWithBigIndex(bool isOn)
    {
        Global.getFindRoadManagerInstance.OnShowCoordinate(isOn);
    }

    private void OnClickCreatePlayer()
    {
        Global.getFindRoadManagerInstance.CreatePlayerData(_playerCount);
    }

    private void Update()
    {
        var playerControl = _control.getPlayerControl;
        var allPlayer = playerControl.getPlayerMap;
        for (int i = 0; i < allPlayer.Count; i++)
        {
            FloorPlayer player = allPlayer[i];
            GameObject playerGo;
            ViewShowPlayer show;
            if (!_playerMap.TryGetValue(player.GetControlName(),out show))
            {
                playerGo = GameObject.Instantiate<GameObject>(playerObj);
                _playerMap[player.GetControlName()] = playerGo.GetComponent<ViewShowPlayer>();
                playerGo.GetComponent<CanvasGroup>().alpha = 1;
                playerGo.name = i.ToString();
                show = _playerMap[player.GetControlName()];
                playerGo.transform.SetParent(content);
                playerGo.transform.localPosition = Vector3.zero;
                playerGo.transform.localScale = Vector3.one;
            }
            show.Update(player);

        }
    }
}
