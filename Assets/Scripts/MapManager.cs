using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    [SerializeField] MapDataUI mapDataUI;
    [SerializeField] PlayerController playerController;
    public MapData targetData;

    public bool quickMake;

    HashSet<Point> map;
    GameObject mapContainer;
    public bool resume = false;
    WaitUntil waitUntilResume;

    public UnityEvent<int> RoomCountChanged = new UnityEvent<int>();

    private void Awake()
    {        
        waitUntilResume = new WaitUntil(() => resume);
        map = new HashSet<Point>(); 
    }

    public void StartCreate()
    {
        ResetMap();
        targetData = new MapData(mapDataUI.mapData);      
        if (quickMake) { QuickCreateMap(); }
        else { StartCoroutine(CreateMapRoutine()); }
    }

    private IEnumerator CreateMapRoutine()
    {
        mapContainer = new GameObject("MapContainer");
        List<RoomBase> roomList = new List<RoomBase>();
        List<RoomBase> blockedRooms = new List<RoomBase>();

        Queue<RoomData> roomGenQueue = new Queue<RoomData>();

        Point curPoint = new Point(0, 0);

        RoomData startRoom = new RoomData(RoomType.Start, new List<Point>() { curPoint }, curPoint);
        startRoom.depth = 0;      

        roomList.Add(CreateRoomInstance(startRoom, 0));
        roomGenQueue.Enqueue(startRoom);

        RoomCountChanged?.Invoke(roomList.Count);

        map.Add(new Point(curPoint));
        
        int leftRoomCount = targetData.createRoomCount -1;

        yield return waitUntilResume;
        resume = false;

        while (leftRoomCount > 0 )
        {
            List<RoomBase> created = new List<RoomBase>();
            while (roomGenQueue.Count > 0)
            {
                RoomData curRoomData = roomGenQueue.Dequeue();

                List<Point[]> creatable = CreatableSpaceCheck(curRoomData);

                while (creatable.Count > 0)
                {
                    RoomType newRoomType = GetRandomRoomType();

                    RoomData createdRoom = CreateRoomData(creatable[0], newRoomType);
                    createdRoom.beforeNode = curRoomData;
                    createdRoom.depth = curRoomData.depth + 1;
                    curRoomData.nextNodeList.Add(createdRoom);
                    foreach(Point p in createdRoom.points)
                    {
                        map.Add(p);
                    }                              

                    created.Add(CreateRoomInstance(createdRoom, roomList.Count + created.Count));
                    RoomCountChanged?.Invoke(roomList.Count + created.Count);

                    creatable = CreatableSpaceCheck(curRoomData);
                }               
            }

            yield return waitUntilResume;
            resume = false;

            int removeCount = Random.Range(0, created.Count -1);
            removeCount = created.Count - removeCount > leftRoomCount ? created.Count - leftRoomCount : removeCount;
            leftRoomCount -= (created.Count - removeCount);

            // 위에서 랜덤으로 뽑은 수만큼 방 삭제
            for (int i = 0; i < removeCount; i++)
            {
                int randomIndex = Random.Range(0, created.Count);
                created[randomIndex].roomData.beforeNode.nextNodeList.Remove(created[randomIndex].roomData);
                foreach (Point p in created[randomIndex].roomData.points)
                {
                    map.Remove(p);
                }

                Destroy(created[randomIndex].gameObject);                 
                created.RemoveAt(randomIndex);
            }
              
            for(int i = 0; i < created.Count; i++)
            {
                roomList.Add(created[i]);
                created[i].RoomId = roomList.Count - 1;
                roomGenQueue.Enqueue(created[i].roomData);
            }

            yield return waitUntilResume;
            resume = false;
        }
        // 방 후처리
        for (int i = 0; i < this.targetData.createRoomCount; i++)
        {
            if (roomList[i].roomData.nextNodeList.Count == 0)
            {
                blockedRooms.Add(roomList[i]);
            }
        }

        while(blockedRooms.Count > 0)
        {
            int randomRoomIndex = Random.Range(0, blockedRooms.Count);
            if (targetData.bossRoomCount > 0)
            {
                targetData.bossRoomCount--;
                blockedRooms[randomRoomIndex].roomData.roomType = RoomType.Boss;
            }
            else if (targetData.treasureRoomCount > 0)
            {
                targetData.treasureRoomCount--;
                blockedRooms[randomRoomIndex].roomData.roomType = RoomType.Treasure;
            }
            else if (targetData.shopRoomCount > 0)
            {
                targetData.shopRoomCount--;
                blockedRooms[randomRoomIndex].roomData.roomType = RoomType.Shop;                
            }
            else
            {
                break;
            }

            blockedRooms[randomRoomIndex].roomData.dir %= 2;
            CreateRoomInstance(blockedRooms[randomRoomIndex].roomData, blockedRooms[randomRoomIndex].RoomId);
            Destroy(blockedRooms[randomRoomIndex].gameObject);
            blockedRooms.RemoveAt(randomRoomIndex);

            yield return waitUntilResume;
            resume = false;
        }

        Debug.Log("Create Completed!");

        playerController.SetCurRoom(roomList[0], this);
        playerController.gameObject.SetActive(true);
    }

    private void QuickCreateMap()
    {
        mapContainer = new GameObject("MapContainer");
        List<RoomData> roomList = new List<RoomData>();
        List<RoomData> blockedRooms = new List<RoomData>();

        Queue<RoomData> roomGenQueue = new Queue<RoomData>();

        Point curPoint = new Point(0, 0);

        RoomData startRoom = new RoomData(RoomType.Start, new List<Point>() { curPoint }, curPoint);
        startRoom.depth = 0;

        roomList.Add(startRoom);
        roomGenQueue.Enqueue(startRoom);

        map.Add(new Point(curPoint));
        int leftRoomCount = targetData.createRoomCount - 1;

        while (leftRoomCount > 0)
        {
            List<RoomData> created = new List<RoomData>();
            while (roomGenQueue.Count > 0)
            {
                RoomData curRoomData = roomGenQueue.Dequeue();

                List<Point[]> creatable = CreatableSpaceCheck(curRoomData);

                while (creatable.Count > 0)
                {
                    RoomType newRoomType = GetRandomRoomType();

                    RoomData createdRoom = CreateRoomData(creatable[0], newRoomType);
                    createdRoom.beforeNode = curRoomData;
                    createdRoom.depth = curRoomData.depth + 1;
                    curRoomData.nextNodeList.Add(createdRoom);
                    foreach (Point p in createdRoom.points)
                    {
                        map.Add(p);
                    }

                    created.Add(createdRoom);

                    creatable = CreatableSpaceCheck(curRoomData);
                }
            }

            int removeCount = Random.Range(0, created.Count - 1);
            removeCount = created.Count - removeCount > leftRoomCount ? created.Count - leftRoomCount : removeCount;
            leftRoomCount -= (created.Count - removeCount);

            for (int i = 0; i < removeCount; i++)
            {
                int randomIndex = Random.Range(0, created.Count);
                created[randomIndex].beforeNode.nextNodeList.Remove(created[randomIndex]);
                foreach (Point p in created[randomIndex].points)
                {
                    map.Remove(p);
                }

                created.RemoveAt(randomIndex);

                RoomCountChanged?.Invoke(roomList.Count + created.Count);
            }

            for (int i = 0; i < created.Count; i++)
            {
                roomList.Add(created[i]);
                roomGenQueue.Enqueue(created[i]);
            }
        }
        // 방 후처리
        for (int i = 0; i < targetData.createRoomCount; i++)
        {
            if (roomList[i].nextNodeList.Count == 0)
            {
                blockedRooms.Add(roomList[i]);
            }
        }

        while (blockedRooms.Count > 0)
        {
            int randomRoomIndex = Random.Range(0, blockedRooms.Count);
            if (targetData.bossRoomCount > 0)
            {
                targetData.bossRoomCount--;
                blockedRooms[randomRoomIndex].roomType = RoomType.Boss;              
            }
            else if (targetData.treasureRoomCount > 0)
            {
                targetData.treasureRoomCount--;
                blockedRooms[randomRoomIndex].roomType = RoomType.Treasure;
            }
            else if (targetData.shopRoomCount > 0)
            {
                targetData.shopRoomCount--;
                blockedRooms[randomRoomIndex].roomType = RoomType.Shop;
            }
            else
            {
                break;
            }
            blockedRooms[randomRoomIndex].dir %= 2;
            blockedRooms.RemoveAt(randomRoomIndex);
        }

        RoomBase startRoomInstance = CreateRoomInstance(roomList[0], 0);

        for (int i = 1; i < roomList.Count; i++)
        {
            CreateRoomInstance(roomList[i], i);
        }
        RoomCountChanged?.Invoke(roomList.Count);

        Debug.Log("Create Completed!");

        playerController.SetCurRoom(startRoomInstance, this);
        playerController.gameObject.SetActive(true);       
    }

    public void ResetMap()
    {
        playerController.gameObject.SetActive(false);
        if (mapContainer != null)
        {
            Destroy(mapContainer);
            RoomCountChanged?.Invoke(0);
        }        
        StopAllCoroutines();
        resume = false;
        map.Clear();
    }

    private RoomType GetRandomRoomType()
    {
        RoomType newRoomType;

        int randomSize = Random.Range(0, 10000);
        if (randomSize < targetData.smallRoomRatio * 100) { newRoomType = RoomType.Small; }
        else if (randomSize > (100 - targetData.bigRoomRatio) * 100)
        {
            int randomBig = Random.Range(0, 100);
            if (randomBig > 70) { newRoomType = RoomType.Triple; }
            else { newRoomType = RoomType.Double; }
        }
        else { newRoomType = RoomType.Normal; }

        return newRoomType;
    }

    private List<Point[]> CreatableSpaceCheck(RoomData curRoomData)
    {
        List<Point[]> creatable = new List<Point[]>();

        // 작은 방의 경우 앏은 쪽 벽으로는 방을 생성할 수 없음
        if (curRoomData.roomType == RoomType.Small)
        {
            for (int j = curRoomData.dir * 2; j < (curRoomData.dir * 2) + 2; j++)
            {
                if (!map.Contains(new Point(curRoomData.points[0] + Constant.dir[j])))
                {
                    creatable.Add(new Point[] { curRoomData.points[0] + Constant.dir[j], curRoomData.points[0] });
                }
            }           
        }
        else
        {
            for (int i = 0; i < curRoomData.points.Count; i++)
            {
                for (int j = 0; j < Constant.dir.Length; j++)
                {
                    if (!map.Contains(new Point(curRoomData.points[i] + Constant.dir[j])))
                    {
                        creatable.Add(new Point[] { curRoomData.points[i] + Constant.dir[j], curRoomData.points[i] });
                    }
                }
            }
        }                
        return creatable;
    }

    private RoomData CreateRoomData(Point[] point, RoomType roomType)
    {
        RoomData newRoomData;
        if (roomType == RoomType.Double)
        {
            List<int> randomDirList = new List<int>();
            // 모든 방향을 확인하도록 리스트에 저장
            foreach (DoubleRoomDir dir in Enum.GetValues(typeof(DoubleRoomDir)))
            {
                randomDirList.Add((int)dir);
            }

            while (randomDirList.Count > 0)
            {
                int randomIndex = Random.Range(0, randomDirList.Count);
                // 랜덤으로 뽑은 방향이 생성 가능하다면 바로 return                        
                if (!map.Contains(new Point(point[0] + Constant.dir[randomDirList[randomIndex]])))
                    return new RoomData(roomType, new List<Point>() { point[0], point[0] + Constant.dir[randomDirList[randomIndex]] }, point[1], randomDirList[randomIndex]);
                // 불가능하다면 방향리스트에서 해당 방향 제거
                randomDirList.RemoveAt(randomIndex);
            }
            // 모든 방향에서 Double 방 생성이 불가능하면 일반 방 생성
            newRoomData = new RoomData(RoomType.Normal, new List<Point>() { point[0] }, point[1]);
        }
        else if (roomType == RoomType.Triple)
        {
            List<int> randomDirList = new List<int>();
            foreach (TripleRoomDir dir in Enum.GetValues(typeof(TripleRoomDir)))
            {
                randomDirList.Add((int)dir);
            }
            
            while (randomDirList.Count > 0)
            {
                int randomIndex = Random.Range(0, randomDirList.Count);

                // 실제로 막힌 부분을 확인할 때까지 생성이 가능하다고 가정
                bool canCreate = true;
                List<Point> points = new List<Point>();
                points.Add(point[0]);

                for (int i = 0; i < Constant.tripleRoomCheck.GetLength(1); i++)
                {
                    if (map.Contains(new Point(point[0] + Constant.tripleRoomCheck[randomDirList[randomIndex], i])))
                    {
                        canCreate = false;
                        break;
                    }
                    points.Add(point[0] + Constant.tripleRoomCheck[randomDirList[randomIndex], i]);                       
                }
                if (canCreate)
                    return new RoomData(roomType, points, point[1], randomDirList[randomIndex]);
                
                randomDirList.RemoveAt(randomIndex);
            }
            newRoomData = new RoomData(RoomType.Normal, new List<Point>() { point[0] }, point[1]);
        }
        else
        {
            // 이전 방과 연결방향에 맞게 방향 설정
            newRoomData = point[0].x == point[1].x ? new RoomData(roomType, new List<Point>() { point[0] }, point[1], 1) : new RoomData(roomType, new List<Point>() { point[0] }, point[1], 0);
        }
        return newRoomData;
    }

    private RoomBase CreateRoomInstance(RoomData roomData, int roomID)
    {
        RoomBase roomInstance = Instantiate(Resources.Load<RoomBase>($"Map/{roomData.roomType}Room"), mapContainer.transform);
        roomInstance.roomData = roomData;       
        roomInstance.SetDir(roomData.dir);
        roomInstance.RoomId = roomID;
        roomInstance.transform.position = new Vector2(roomData.points[0].x * 12, roomData.points[0].y * 12);

        GameObject pathInstance = Instantiate(Resources.Load<GameObject>("Map/Path"), roomInstance.transform);
        pathInstance.transform.position = new Vector2((roomInstance.roomData.points[0].x + roomInstance.roomData.connectedPoint.x) * 6, (roomInstance.roomData.points[0].y + roomInstance.roomData.connectedPoint.y) * 6);
        return roomInstance;
    }

    public List<RoomData> FindPath(RoomData start, RoomData dest)
    {
        List<RoomData> path = new List<RoomData>();
        Stack<RoomData> pathStack = new Stack<RoomData>();
        RoomData fromStart = start;
        RoomData fromDest = dest;

        // 깊이를 기준으로 부모 검사
        while (!fromStart.Equals(fromDest))
        {
            if(fromStart.depth > fromDest.depth)
            {
                path.Add(fromStart);
                fromStart = fromStart.beforeNode;               
            }
            else if (fromStart.depth < fromDest.depth)
            {
                pathStack.Push(fromDest);
                fromDest = fromDest.beforeNode;                
            }
            else
            {
                path.Add(fromStart);
                fromStart = fromStart.beforeNode;
                pathStack.Push(fromDest);
                fromDest = fromDest.beforeNode;               
            }
        }
        path.Add(fromStart);
        while(pathStack.Count > 0)
        {
            path.Add(pathStack.Pop());
        }

        return path;
    }

}
