using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData 
{
    public int createRoomCount;
    public int bossRoomCount;
    public int treasureRoomCount;
    public int shopRoomCount;
    public int smallRoomRatio;
    public int bigRoomRatio;

    public MapData(MapData mapData)
    {
        createRoomCount = mapData.createRoomCount;
        bossRoomCount = mapData.bossRoomCount;
        treasureRoomCount = mapData.treasureRoomCount;
        shopRoomCount = mapData.shopRoomCount;
        smallRoomRatio = mapData.smallRoomRatio;
        bigRoomRatio = mapData.bigRoomRatio;
    }
}
