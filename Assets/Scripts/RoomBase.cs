using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomBase : MonoBehaviour 
{
    public RoomData roomData;
    public int roomId;
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject[] roomObject;

    private void Update()
    {
        text.text = roomId.ToString();
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
}
