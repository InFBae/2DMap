using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUI : BaseUI
{
    MapManager mapManager;
    protected override void Awake()
    {
        base.Awake();
        mapManager = GetComponentInParent<MapManager>();

        texts["TargetCountText"].text = mapManager.mapData.createRoomCount.ToString();
        buttons["ResumeButton"].onClick.AddListener(() => mapManager.resume = true);
    }

    private void OnEnable()
    {
        mapManager.RoomCountChanged.AddListener(ChangeCurrentRoomText);
    }

    private void OnDisable()
    {
        mapManager.RoomCountChanged.RemoveListener(ChangeCurrentRoomText);
    }

    private void ChangeCurrentRoomText(int roomListCount, int tempRoomCount)
    {
        texts["CurrentCountText"].text = (roomListCount + tempRoomCount).ToString();
    }
}
