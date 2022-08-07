using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class Arduino_Reading : MonoBehaviour
{
    SerialPort arduino = new SerialPort("COM7", 9600);
    
    public static int data_x;
    public static int data_y;
    public static int data_z;

    private PlayerBehavior action;
    private bool isReading;


    public static int GetX() { return data_x; }
    public static int GetY() { return data_y; }
    public static int GetZ() { return data_z; }

    // Start is called before the first frame update
    void Start()
    {
        arduino.Open();

        data_x = data_y = 512;
        data_z = 1;

        action = GameObject.Find("Player").transform.Find("player_goose").gameObject.GetComponent<PlayerBehavior>() ;

        Thread thread = new Thread(Run);
        thread.Start();

    }

    // Run is called asyncronously about Update
    void Run()
    {
        isReading = true;

        string data = arduino.ReadLine();
        int indexOfColon = data.IndexOf(":");
        if (data.Contains("x")) { data_x = int.Parse(data.Substring(indexOfColon + 1)); }   // 기본값 x=512. 왼쪽이 x=0
        if (data.Contains("y")) { data_y = int.Parse(data.Substring(indexOfColon + 1)); }  //기본값 y=512, 위쪽이 y=0.
        if (data.Contains("z")) { data_z = int.Parse(data.Substring(indexOfColon + 1)); }  //눌리면 z=0, 안 눌리면 z=1

        //Debug.Log(data);
        isReading = false;
    }

    private void Update()
    {
        if (!isReading)
        {
            Thread thread = new Thread(Run);
            thread.Start();
        }

        if (data_x > 612) { action.TurnRight(); }
        if (data_x < 412) { action.TurnLeft(); }
        if (data_y < 412) { action.MoveForward(); }

        if (data_z == 0 && !GameManager.isFlying && data_y<612) { action.Fly(); }

        if (data_y > 612 && GameManager.isFlying) { action.FlyDown(); }
        if (data_z==0 && data_y<412 && GameManager.isFlying) { action.FlyUp(); }
    }
}
