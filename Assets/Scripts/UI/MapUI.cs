using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : BaseUI
{
    MapManager mapManager;

    protected override void Awake()
    {
        base.Awake();
        mapManager = GetComponentInParent<MapManager>();

        buttons["ResetButton"].onClick.AddListener(() => mapManager.ResetMap());
        buttons["StartButton"].onClick.AddListener(() => mapManager.StartCreate());
        buttons["ResumeButton"].onClick.AddListener(() => mapManager.resume = true);       
    }

    private void OnEnable()
    {
        mapManager.RoomCountChanged.AddListener(ChangeCurrentRoomText);
        GetComponentInChildren<Toggle>().onValueChanged.AddListener(ChangeCreateMode);
    }

    private void OnDisable()
    {
        mapManager.RoomCountChanged.RemoveListener(ChangeCurrentRoomText);
        GetComponentInChildren<Toggle>().onValueChanged.RemoveListener(ChangeCreateMode);
    }

    private void ChangeCurrentRoomText(int roomListCount, int tempRoomCount)
    {
        texts["TargetCountText"].text = mapManager.targetData.createRoomCount.ToString();
        texts["CurrentCountText"].text = (roomListCount + tempRoomCount).ToString();
    }

    private void ChangeCreateMode(bool quickMode)
    {
        mapManager.quickMake = quickMode;
    }
}
