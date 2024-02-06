using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoomBase : MonoBehaviour 
{
    public RoomData roomData;
    private int roomId;

    [SerializeField] TMP_Text text;
    [SerializeField] GameObject[] roomObject;

    public int RoomId { get { return roomId; } set { roomId = value; roomIdChanged?.Invoke(roomId); } }

    UnityEvent<int> roomIdChanged = new UnityEvent<int>();

    private void OnEnable()
    {
        roomIdChanged.AddListener(ChangeText);
    }

    private void OnDisable()
    {
        roomIdChanged.RemoveListener(ChangeText);
    }

    public void SetDir(int n)
    {
        for(int i = 0; i < roomObject.Length; i++)
        {
            if (i == n)
            {
                roomObject[i].gameObject.SetActive(true);
            }
            else
            {
                roomObject[i].gameObject.SetActive(false);
            }
        }
    }

    private void ChangeText(int roomId)
    {
        text.text = roomId.ToString();
    }

    public void ChangeColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }
}
