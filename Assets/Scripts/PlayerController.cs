using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public RoomBase curRoom;
    public MapManager mapManager;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, transform.forward);
            if (hit)
            {
                if (hit.collider.GetComponentInParent<RoomBase>() != null) { MoveRoom(hit.collider.GetComponentInParent<RoomBase>()); }
            }
        }
    }

    public void SetCurRoom(RoomBase curRoom, MapManager mapManager)
    {
        this.curRoom = curRoom;
        transform.position = curRoom.transform.position;
        this.mapManager = mapManager;
    }

    public void MoveRoom(RoomBase dest)
    {
        StartCoroutine(MoveRoomRoutine(dest));
    }

    IEnumerator MoveRoomRoutine(RoomBase dest)
    {
        List<RoomData> paths = mapManager.FindPath(curRoom.roomData, dest.roomData);

        foreach (RoomData path in paths)
        {
            transform.position = new Vector3(path.points[0].x * 12 , path.points[0].y * 12, 0);
            yield return new WaitForSeconds(0.5f);
        }

        curRoom = dest;
    }
}
