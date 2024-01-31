using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData
{
    public Constant.RoomType roomType;
    public Constant.Point pos;
    public Constant.PathDir connectedDir;
    public RoomData beforeNode;
    public List<RoomData> nextNodeList;

    public RoomData(Constant.RoomType roomType, int x, int y, Constant.PathDir connectedDir)
    {
        this.roomType = roomType;
        this.pos = new Constant.Point(x, y);
        this.connectedDir = connectedDir;
        nextNodeList = new List<RoomData>();
    }
}
