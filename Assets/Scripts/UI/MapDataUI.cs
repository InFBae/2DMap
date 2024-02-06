using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataUI : BaseUI
{
    [SerializeField] public MapData mapData;

    protected override void Awake()
    {
        base.Awake();

        inputFields["CreateRoomInputField"].onValueChanged.AddListener((string value) => int.TryParse(value, out mapData.createRoomCount));
        inputFields["BossRoomInputField"].onValueChanged.AddListener((string value) => int.TryParse(value, out mapData.bossRoomCount));
        inputFields["TreasureRoomInputField"].onValueChanged.AddListener((string value) => int.TryParse(value, out mapData.treasureRoomCount));
        inputFields["ShopRoomInputField"].onValueChanged.AddListener((string value) => int.TryParse(value, out mapData.shopRoomCount));
        inputFields["SmallRoomRatioInputField"].onValueChanged.AddListener((string value) => int.TryParse(value, out mapData.smallRoomRatio));
        inputFields["BigRoomRatioInputField"].onValueChanged.AddListener((string value) => int.TryParse(value, out mapData.bigRoomRatio));
    }

    private void Start()
    {
        inputFields["CreateRoomInputField"].text = mapData.createRoomCount.ToString();
        inputFields["BossRoomInputField"].text = mapData.bossRoomCount.ToString();
        inputFields["TreasureRoomInputField"].text = mapData.treasureRoomCount.ToString();
        inputFields["ShopRoomInputField"].text = mapData.shopRoomCount.ToString();
        inputFields["SmallRoomRatioInputField"].text = mapData.smallRoomRatio.ToString();
        inputFields["BigRoomRatioInputField"].text = mapData.bigRoomRatio.ToString();
    }
}
