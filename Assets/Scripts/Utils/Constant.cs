using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constant
{
    public static Point[] dir = { new Point( -1, 0 ), new Point( 1, 0 ), new Point( 0, 1 ), new Point ( 0, -1 ) };

    public static Point[,] tripleRoomCheck = { 
        { new Point(-1, 0), new Point(-1, 1) },
        { new Point(-1, 0), new Point(-1, -1), },
        { new Point(1, 0), new Point(1, 1), },
        { new Point(1, 0), new Point(1, -1), },
        { new Point(0, 1), new Point(-1, 1), },
        { new Point(0, 1), new Point(1, 1), },       
        { new Point(0, -1), new Point(-1, -1), },       
        { new Point(0, -1), new Point( 1, -1), }     
    };
}

public struct Point
{
    public int x, y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Point operator + (Point a, Point b)
    {
        return new Point(a.x+b.x, a.y+b.y);
    }

}

public enum RoomType { Start, Boss, Normal, Double, Triple, Small, Shop, Treasure };
public enum DoubleRoomDir { Left, Right, Up, Down }
public enum TripleRoomDir { LeftUp, LeftDown, RightUp, RightDown, UpLeft, UpRight, DownLeft, DownRight,  }
public enum PathDir { Left, Right, Top, Bottom, None }
