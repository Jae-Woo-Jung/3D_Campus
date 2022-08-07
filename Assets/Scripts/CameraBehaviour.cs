using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] GameObject Help_Description;

    [Tooltip("카메라로 비춰줄 대상.")]
    public Transform target;

    [SerializeField]
    [Tooltip("대상에 대한 카메라의 상대적 위치.")]
    Vector3 lookOffset = new Vector3(0, 10.0f, -1.0f);

    [Tooltip("대상과 카메라 간 거리 조절.")]
    public float currentZoom = 2.5f;

    public float minZoom = 1.0f;
    public float maxZoom = 10.0f;

    [Tooltip("회전 속도, q, e, shift키로 회전")]
    public float rotSpeed = 5.0f;

    /// <summary>
    /// 화면 터치로 카메라 회전 구현.
    /// </summary>
    public Vector2 nowPos, prePos;
    public Vector3 movePos;
    public Vector2 clickPoint;


    private void Start()
    {
        Input.simulateMouseWithTouches = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 goose_position = target.position + new Vector3(0.0f, 1.0f, 0.0f);

        if (Input.GetKey("q") && !Input.GetKey("left shift"))
        {
            //Y축 기준으로 회전
            transform.RotateAround(goose_position, Vector3.up, rotSpeed);
        }
        if (Input.GetKey("e") && !Input.GetKey("left shift"))
        {
            //Y축 기준으로 회전
            transform.RotateAround(goose_position, Vector3.up, -rotSpeed);
        }

        if (Input.GetKey("q") && Input.GetKey("left shift"))
        {
            //X축 기준으로 회전
            transform.RotateAround(goose_position, target.rotation*Vector3.right, -rotSpeed);
        }

        if (Input.GetKey("e") && Input.GetKey("left shift"))
        {
            //X축 기준으로 회전
            transform.RotateAround(goose_position, target.rotation*Vector3.right, rotSpeed);
        }


        if (!Help_Description.activeSelf)  //도움말이 켜져있을 때는 마우스 및 터치에 의한 화면 조작 방지.
        {
            //마우스로 카메라 회전
            if (Input.GetMouseButtonDown(0)) clickPoint = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                Vector3 movePos = Camera.main.ScreenToViewportPoint((Vector2)Input.mousePosition - clickPoint);

                //Swap movePos.x and movePos.y
                movePos.x = movePos.y + movePos.x;
                movePos.y = movePos.x - movePos.y;
                movePos.x = -(movePos.x - movePos.y);  //(x, y, 0)가 마우스 이동이면, 회전축은 (y, -x, 0).

                transform.RotateAround(goose_position, target.rotation * movePos, rotSpeed / 2.0f); //회전 속도가 빨라서 2.0f로 나눔.
                clickPoint = Input.mousePosition;
            }

            //화면 터치로 화면 회전
            if (Input.touchCount == 1)
            {
                Debug.Log("터치 감지 : " + Input.touchCount);

                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    prePos = touch.position - touch.deltaPosition;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    nowPos = touch.position - touch.deltaPosition;
                    movePos = (Vector3)(prePos - nowPos) * Time.deltaTime;
                    //Swap movePos.x and movePos.y
                    movePos.x = movePos.y + movePos.x;
                    movePos.y = movePos.x - movePos.y;
                    movePos.x = -(movePos.x - movePos.y);  //(x, y, 0)가 마우스 이동이면, 회전축은 (y, -x, 0).

                    transform.RotateAround(goose_position, target.rotation * movePos, rotSpeed);
                    prePos = touch.position - touch.deltaPosition;
                }
            }

            //화면 터치로 화면 줌 인/아웃
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0); //첫 번째 손가락 터치
                Touch touchOne = Input.GetTouch(1); //두 번째 손가락 터치

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                //프레임 간 두 터치한 손가락 사이의 벡터 거리 계산
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float TouchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                currentZoom += prevTouchDeltaMag - TouchDeltaMag;  //양수, 즉 두 손가락이 가까워지면 줌 아웃, 음수이면 줌 인.
            }

            // 마우스 휠로 줌 인아웃
            currentZoom -= 3 * Input.GetAxis("Mouse ScrollWheel");
            // 줌 최소 및 최대 설정에 따라 필터링.
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }

        lookOffset = transform.position - goose_position;
        lookOffset.Normalize();
        lookOffset *= 2;
    }

    void LateUpdate()
    {

        if (target != null)
        {
            Vector3 goose_position = target.position + new Vector3(0f, 1.0f, 0.0f);
            // 대상으로부터 offset 만큼 떨어져 있도록 설정
            transform.position = goose_position + lookOffset*currentZoom;
            // 대상을 보도록 설정
            transform.LookAt(target);

        }
        else  //대상이 없을 경우. 
        { 
            Debug.Log("Target is not yet assigned or destroyed.");
        }
    }
}