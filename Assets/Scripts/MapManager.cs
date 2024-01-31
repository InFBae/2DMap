using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] MapData mapData;
    int[,] map;

    private void Start()
    {
        map = new int[mapData.createRoomCount * 2, mapData.createRoomCount * 2];
        CreateRoom();
    }

    private void CreateRoom()
    {
        GameObject mapContainer = new GameObject("MapContainer");
        List<RoomData> roomList = new List<RoomData>();
        List<RoomData> blockedRooms = new List<RoomData>();

        Constant.Point curPoint = new Constant.Point(mapData.createRoomCount, mapData.createRoomCount);
        roomList.Add(new RoomData(Constant.RoomType.Start, curPoint.x, curPoint.y, Constant.PathDir.None));
        map[curPoint.x, curPoint.y] = 1;

        int curIndex = 0;

        // 방 공간 생성
        while (roomList.Count <= mapData.createRoomCount)
        {
            RoomData curRoomData = roomList[curIndex];
            curPoint = curRoomData.pos;
            Constant.RoomType newRoomType;

            int randomSize = Random.Range(0, 10000);
            if (randomSize < mapData.smallRoomRatio * 100)
            {
                newRoomType = Constant.RoomType.Small;
            }
            else if (randomSize > (100 - mapData.bigRoomRatio) * 100)
            {
                newRoomType = Constant.RoomType.Big;
            }
            else
            {
                newRoomType = Constant.RoomType.Normal;
            }

            List<int> creatable = CreatableCheck(curRoomData, newRoomType, curPoint);  

            if (creatable.Count == 0)
            {
                // TODO : 백트래킹? 
                curIndex--;
                continue;
            }
            else
            {                
                int createCount;
                
                if (roomList.Count == curIndex + 1 && (mapData.createRoomCount - roomList.Count) > 0)
                {
                    createCount = Random.Range(1, creatable.Count);
                }
                else
                {
                    createCount = Random.Range(0, creatable.Count);
                }
                
                if (createCount > 0)
                {                    
                    for (int i = 0; i < createCount; i++)
                    {
                        int randomDir = Random.Range(0, creatable.Count);
                        RoomData newRoomData = new RoomData(newRoomType, curPoint.x + Constant.dx[creatable[randomDir]], curPoint.y + Constant.dy[creatable[randomDir]], (Constant.PathDir)creatable[randomDir]);
                        roomList.Add(newRoomData);
                        curRoomData.nextNodeList.Add(newRoomData);
                        newRoomData.beforeNode = curRoomData;
                        map[curPoint.x + Constant.dx[creatable[randomDir]], curPoint.y + Constant.dy[creatable[randomDir]]] = 1;
                        creatable.RemoveAt(randomDir);
                    }
                }
                if (curIndex < roomList.Count - 1)
                {
                    curIndex++;
                }             
            }                                    
        }
        // 방 후처리
        for (int i = 0; i < mapData.createRoomCount; i++)
        {
            if (roomList[i].nextNodeList.Count == 0 && roomList[i].roomType == Constant.RoomType.Normal)
            {
                blockedRooms.Add(roomList[i]);
            }
        }

        MapData targetData = new MapData(mapData);
        while(blockedRooms.Count > 0)
        {
            int randomRoomIndex = Random.Range(0, blockedRooms.Count);
            if (targetData.bossRoomCount > 0)
            {
                targetData.bossRoomCount--;
                blockedRooms[randomRoomIndex].roomType = Constant.RoomType.Boss;
            }
            else if (targetData.treasureRoomCount > 0)
            {
                targetData.treasureRoomCount--;
                blockedRooms[randomRoomIndex].roomType = Constant.RoomType.Treasure;
            }
            else if (targetData.shopRoomCount > 0)
            {
                targetData.shopRoomCount--;
                blockedRooms[randomRoomIndex].roomType = Constant.RoomType.Shop;
            }
            else
            {
                break;
            }
            blockedRooms.RemoveAt(randomRoomIndex);
        }       

        // 최종 생성
        for(int i = 0; i < mapData.createRoomCount; i++)
        {
            RoomData curRoomData = roomList[i];

            RoomBase roomInstance = Instantiate(Resources.Load<RoomBase>($"Map/{curRoomData.roomType}Room"), mapContainer.transform);
            roomInstance.roomData = curRoomData;
            roomInstance.roomId = i;
            roomInstance.transform.position = new Vector2(curRoomData.pos.x * 12, curRoomData.pos.y * 12);
            if (curRoomData.connectedDir == Constant.PathDir.Top || curRoomData.connectedDir == Constant.PathDir.Bottom)
            {
                roomInstance.Rotate();
            }
            if (roomInstance.roomData.connectedDir != Constant.PathDir.None)
            {
                Instantiate(Resources.Load<GameObject>("Map/Path"), roomInstance.transform, true).transform.position = new Vector2(roomInstance.transform.position.x + (Constant.dx[(int)roomInstance.roomData.connectedDir] * -6), roomInstance.transform.position.y + (Constant.dy[(int)roomInstance.roomData.connectedDir] * -6));
            }
        }        
    }

    private List<int> CreatableCheck(RoomData curRoomData, Constant.RoomType newRoomType, Constant.Point curPoint)
    {
        List<int> creatable = new List<int>();

        if (curRoomData.roomType == Constant.RoomType.Small)
        {
            if (curRoomData.connectedDir == Constant.PathDir.Top || curRoomData.connectedDir == Constant.PathDir.Bottom)
            {
                for (int i = 2; i < Constant.dx.Length; i++)
                {
                    if (map[curPoint.x + Constant.dx[i], curPoint.y + Constant.dy[i]] == 0)
                    {
                        creatable.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    if (map[curPoint.x + Constant.dx[i], curPoint.y + Constant.dy[i]] == 0)
                    {
                        creatable.Add(i);
                    }
                }
            }
        }
        else if (curRoomData.roomType == Constant.RoomType.Big)
        {
            
        }
        else
        {
            for (int i = 0; i < Constant.dx.Length; i++)
            {
                if (map[curPoint.x + Constant.dx[i], curPoint.y + Constant.dy[i]] == 0)
                {
                    creatable.Add(i);
                }
            }
        }       
        return creatable;
    }
}
