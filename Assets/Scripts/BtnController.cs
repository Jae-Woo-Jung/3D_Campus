using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnController : MonoBehaviour
{
    // Start is called before the first frame update
    static bool IsLButtonDown;
    static bool IsRButtonDown;
    static bool IsGoButtonDown;
    static bool IsUpButtonDown;
    static bool IsDownButtonDown;

    GameObject player;
    PlayerBehavior script;

    public static bool CheckLButton(){ return IsLButtonDown;}
    public static bool CheckRButton() { return IsRButtonDown; }
    public static bool CheckGoButton() { return IsGoButtonDown; }
    public static bool CheckUpButton() { return IsUpButtonDown; }
    public static bool CheckDownButton() { return IsDownButtonDown; }

    public void setLButton(bool check) { IsLButtonDown = check; }
    public void setRButton(bool check) { IsRButtonDown = check; }
    public void setGoButton(bool check) { IsGoButtonDown = check; }
    public void setUpButton(bool check) { IsUpButtonDown = check; }
    public void setDownButton(bool check) { IsDownButtonDown = check; }

    void Start()
    {
        IsLButtonDown=false;
        IsRButtonDown=false;
        IsGoButtonDown=false;
        IsUpButtonDown=false;
        IsDownButtonDown=false;
        player = GameObject.Find("Player").transform.Find("player_goose").gameObject;
        script = player.GetComponent<PlayerBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsLButtonDown) { script.TurnLeft(); }
        if (IsRButtonDown) { script.TurnRight(); }
        if (IsGoButtonDown) { script.MoveForward(); }
        if (IsUpButtonDown) { script.FlyUp(); }
        if (IsDownButtonDown) { script.FlyDown(); }
    }
}
