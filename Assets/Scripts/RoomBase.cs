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
    [SerializeField] GameObject roomObject;

    private void Update()
    {
        text.text = roomId.ToString();
    }

    public void Rotate()
    {
        roomObject.transform.Rotate(0, 0, 90);
    }
}
