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
    Camera cam;

    public bool quickMake;

    HashSet<Point> map;
    GameObject mapContainer;
    public bool resume = false;
    WaitUntil waitUntilResume;

    public UnityEvent<int> RoomCountChanged = new UnityEvent<int>();

    private void Awake()
    {
        cam = FindObjectOfType<Camera>();
        waitUntilResume = new WaitUntil(() => resume);
        map = new HashSet<Point>(); 
    }

    public void StartCreate()
    {
        ResetMap();
        targetData = new MapData(mapDataUI.mapData);
        map.Clear();
        if (quickMake) { QuickCreateMap(); }
        else { StartCoroutine(CreateMapRoutine()); }
    }

    private IEnumerator CreateMapRoutine()
    {
        mapContainer = new GameObject("MapContainer");
        List<RoomBase> roomList = new List<RoomBase>();
        List<RoomBase> blockedRooms = new List<RoomBase>();
        
        Queue<RoomData> roomGenQueue = new Queue<RoomData>();

        Point curPoint = new Point(targetData.createRoomCount, targetData.createRoomCount);

        RoomData startRoom = new RoomData(RoomType.Start, new List<Point>() { curPoint }, curPoint);
        startRoom.depth = 0;
        RoomBase startRoomInstance = Instantiate(Resources.Load<RoomBase>($"Map/{startRoom.roomType}Room"), mapContainer.transform);
        startRoomInstance.roomData = startRoom;
        startRoomInstance.transform.position = new Vector2(startRoom.points[0].x * 12, startRoom.points[0].y * 12);
        cam.transform.position = new Vector3(startRoom.points[0].x * 12, startRoom.points[0].y * 12, -10);

        roomList.Add(startRoomInstance);
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

                    RoomData createdRoom = CreateRoom(creatable[0], newRoomType);
                    createdRoom.beforeNode = curRoomData;
                    createdRoom.depth = curRoomData.depth + 1;
                    curRoomData.nextNodeList.Add(createdRoom);
                    foreach(Point p in createdRoom.points)
                    {
                        map.Add(p);
                    }

                    RoomBase roomInstance = Instantiate(Resources.Load<RoomBase>($"Map/{createdRoom.roomType}Room"), mapContainer.transform);
                    roomInstance.roomData = createdRoom;
                    roomInstance.SetDir(createdRoom.dir);
                    roomInstance.RoomId = roomList.Count + created.Count;
                    roomInstance.transform.position = new Vector2(createdRoom.points[0].x * 12, createdRoom.points[0].y * 12 );

                    GameObject pathInstance = Instantiate(Resources.Load<GameObject>("Map/Path"), roomInstance.transform);
                    pathInstance.transform.position = new Vector2((roomInstance.roomData.points[0].x + roomInstance.roomData.connectedPoint.x) * 6, (roomInstance.roomData.points[0].y + roomInstance.roomData.connectedPoint.y) * 6);

                    created.Add(roomInstance);

                    RoomCountChanged?.Invoke(roomList.Count + created.Count);

                    creatable = CreatableSpaceCheck(curRoomData);
                }               
            }
            yield return waitUntilResume;
            resume = false;

            int removeCount = Random.Range(0, created.Count -1);
            removeCount = created.Count - removeCount > leftRoomCount ? created.Count - leftRoomCount : removeCount;
            leftRoomCount -= (created.Count - removeCount);

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

                RoomCountChanged?.Invoke(roomList.Count + created.Count);
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
                blockedRooms[randomRoomIndex].roomData.dir %= 2;
            }
            else if (targetData.treasureRoomCount > 0)
            {
                targetData.treasureRoomCount--;
                blockedRooms[randomRoomIndex].roomData.roomType = RoomType.Treasure;
                blockedRooms[randomRoomIndex].roomData.dir %= 2;
            }
            else if (targetData.shopRoomCount > 0)
            {
                targetData.shopRoomCount--;
                blockedRooms[randomRoomIndex].roomData.roomType = RoomType.Shop;
                blockedRooms[randomRoomIndex].roomData.dir %= 2;
            }
            else
            {
                break;
            }
            RoomBase roomInstance = Instantiate(Resources.Load<RoomBase>($"Map/{blockedRooms[randomRoomIndex].roomData.roomType}Room"), mapContainer.transform);
            roomInstance.roomData = blockedRooms[randomRoomIndex].roomData;
            roomInstance.SetDir(blockedRooms[randomRoomIndex].roomData.dir);
            roomInstance.RoomId = blockedRooms[randomRoomIndex].RoomId;
            roomInstance.transform.position = new Vector2(blockedRooms[randomRoomIndex].roomData.points[0].x * 12, blockedRooms[randomRoomIndex].roomData.points[0].y * 12);

            GameObject pathInstance = Instantiate(Resources.Load<GameObject>("Map/Path"), roomInstance.transform);
            pathInstance.transform.position = new Vector2((roomInstance.roomData.points[0].x + roomInstance.roomData.connectedPoint.x) * 6, (roomInstance.roomData.points[0].y + roomInstance.roomData.connectedPoint.y) * 6);

            Destroy(blockedRooms[randomRoomIndex].gameObject);
            blockedRooms[randomRoomIndex] = roomInstance;

            blockedRooms.RemoveAt(randomRoomIndex);

            yield return waitUntilResume;
            resume = false;
        }

        Debug.Log("Create Completed!");

        playerController.gameObject.SetActive(true);
        playerController.curRoom = startRoomInstance;
        playerController.transform.position = startRoomInstance.transform.position;
        playerController.mapManager = this;
    }

    private void QuickCreateMap()
    {
        mapContainer = new GameObject("MapContainer");
        List<RoomData> roomList = new List<RoomData>();
        List<RoomData> blockedRooms = new List<RoomData>();

        Queue<RoomData> roomGenQueue = new Queue<RoomData>();

        Point curPoint = new Point(targetData.createRoomCount, targetData.createRoomCount);

        RoomData startRoom = new RoomData(RoomType.Start, new List<Point>() { curPoint }, curPoint);
        startRoom.depth = 0;

        cam.transform.position = new Vector3(startRoom.points[0].x * 12, startRoom.points[0].y * 12, -10);

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

                    RoomData createdRoom = CreateRoom(creatable[0], newRoomType);
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
                blockedRooms[randomRoomIndex].dir %= 2;
            }
            else if (targetData.treasureRoomCount > 0)
            {
                targetData.treasureRoomCount--;
                blockedRooms[randomRoomIndex].roomType = RoomType.Treasure;
                blockedRooms[randomRoomIndex].dir %= 2;
            }
            else if (targetData.shopRoomCount > 0)
            {
                targetData.shopRoomCount--;
                blockedRooms[randomRoomIndex].roomType = RoomType.Shop;
                blockedRooms[randomRoomIndex].dir %= 2;
            }
            else
            {
                break;
            }           

            blockedRooms.RemoveAt(randomRoomIndex);
        }

        RoomBase startRoomInstance = Instantiate(Resources.Load<RoomBase>($"Map/{roomList[0].roomType}Room"), mapContainer.transform);
        startRoomInstance.roomData = roomList[0];
        startRoomInstance.SetDir(roomList[0].dir);
        startRoomInstance.RoomId = 0;
        startRoomInstance.transform.position = new Vector2(roomList[0].points[0].x * 12, roomList[0].points[0].y * 12);

        for (int i = 1; i < roomList.Count; i++)
        {
            RoomBase roomInstance = Instantiate(Resources.Load<RoomBase>($"Map/{roomList[i].roomType}Room"), mapContainer.transform);
            roomInstance.roomData = roomList[i];
            roomInstance.SetDir(roomList[i].dir);
            roomInstance.RoomId = i;
            roomInstance.transform.position = new Vector2(roomList[i].points[0].x * 12, roomList[i].points[0].y * 12);
           
            GameObject pathInstance = Instantiate(Resources.Load<GameObject>("Map/Path"), roomInstance.transform);
            pathInstance.transform.position = new Vector2((roomInstance.roomData.points[0].x + roomInstance.roomData.connectedPoint.x) * 6, (roomInstance.roomData.points[0].y + roomInstance.roomData.connectedPoint.y) * 6);            
        }
        RoomCountChanged?.Invoke(roomList.Count);

        Debug.Log("Create Completed!");

        playerController.gameObject.SetActive(true);
        playerController.curRoom = startRoomInstance;
        playerController.transform.position = startRoomInstance.transform.position;
        playerController.mapManager = this;
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
    }

    private List<Point[]> CreatableSpaceCheck(RoomData curRoomData)
    {
        List<Point[]> creatable = new List<Point[]>();

        if (curRoomData.roomType == RoomType.Small)
        {
            for (int i = 0; i < curRoomData.points.Count; i++)
            {
                if (curRoomData.dir == 0)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (!map.Contains(new Point(curRoomData.points[i] + Constant.dir[j])))
                        {
                            creatable.Add(new Point[] { curRoomData.points[i] + Constant.dir[j], curRoomData.points[i] });
                        }
                    }
                }
                else
                {
                    for (int j = 2; j < 4; j++)
                    {
                        if (!map.Contains(new Point(curRoomData.points[i] + Constant.dir[j])))
                        {
                            creatable.Add(new Point[] { curRoomData.points[i] + Constant.dir[j], curRoomData.points[i] });
                        }
                    }
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

    private RoomData CreateRoom(Point[] point, RoomType roomType)
    {
        RoomData newRoomData;
        if (roomType == RoomType.Double)
        {
            List<DoubleRoomDir> randomDirList = new List<DoubleRoomDir>();
            foreach (DoubleRoomDir dir in Enum.GetValues(typeof(DoubleRoomDir)))
            {
                randomDirList.Add(dir);
            }

            while (randomDirList.Count > 0)
            {
                int randomIndex = Random.Range(0, randomDirList.Count);
                foreach (DoubleRoomDir dir in Enum.GetValues(typeof(DoubleRoomDir)))
                {
                    if (randomDirList[randomIndex] == dir)
                    {
                        if (!map.Contains(new Point(point[0] + Constant.dir[(int)dir])))
                        {
                            newRoomData = new RoomData(roomType, new List<Point>() { point[0], point[0] + Constant.dir[(int)dir] }, point[1], (int)dir);
                            return newRoomData;
                        }
                    }
                }
                randomDirList.RemoveAt(randomIndex);
            }
            newRoomData = new RoomData(RoomType.Normal, new List<Point>() { point[0] }, point[1]);
        }
        else if (roomType == RoomType.Triple)
        {
            List<TripleRoomDir> randomDirList = new List<TripleRoomDir>();
            foreach (TripleRoomDir dir in Enum.GetValues(typeof(TripleRoomDir)))
            {
                randomDirList.Add(dir);
            }
            
            while (randomDirList.Count > 0)
            {
                int randomIndex = Random.Range(0, randomDirList.Count);
                foreach (TripleRoomDir dir in Enum.GetValues(typeof(TripleRoomDir)))
                {
                    if (randomDirList[randomIndex] == dir)
                    {
                        bool canCreate = true;
                        List<Point> points = new List<Point>();
                        points.Add(point[0]);

                        for (int i = 0; i < Constant.tripleRoomCheck.GetLength(1); i++)
                        {
                            if (map.Contains(new Point(point[0] + Constant.tripleRoomCheck[(int)dir, i])))
                            {
                                canCreate = false;
                                break;
                            }
                            else
                            {
                                points.Add(point[0] + Constant.tripleRoomCheck[(int)dir, i]);
                            }
                        }
                        if (canCreate)
                        {
                            newRoomData = new RoomData(roomType, points, point[1], (int)dir);
                            return newRoomData;
                        }
                    }
                }
                randomDirList.RemoveAt(randomIndex);
            }
            newRoomData = new RoomData(RoomType.Normal, new List<Point>() { point[0] }, point[1]);
        }
        else
        {
            if (point[0].x == point[1].x)
            {
                newRoomData = new RoomData(roomType, new List<Point>() { point[0] }, point[1], 1);
            }
            else
            {
                newRoomData = new RoomData(roomType, new List<Point>() { point[0] }, point[1], 0);
            }
        }
        return newRoomData;
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

        Debug.Log(path.Count);

        return path;
    }

}
