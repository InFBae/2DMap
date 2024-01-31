using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constant
{
    public struct Point
    {
        public int x, y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public enum RoomType { Start, Boss, Normal, Big, Small, Shop, Treasure };
    public enum BigRoomType { };
    public enum PathDir { Left, Right, Top, Bottom, None }

    public static int[] dx = { -1, 1, 0, 0 };
    public static int[] dy = { 0, 0, 1, -1 };

}

