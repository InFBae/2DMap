using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomData
{
    public RoomType roomType;
    public List<Point> points;
    public Point[] connectedPoint;
    public RoomData beforeNode;
    [NonSerialized] public List<RoomData> nextNodeList;
    public int dir;

    public RoomData(RoomType roomType, List<Point> points, Point[] connectedPoint, int dir = 0)
    {
        this.roomType = roomType;
        this.points = points;
        this.connectedPoint = connectedPoint;
        nextNodeList = new List<RoomData>();
        this.dir = dir;
    }
}
