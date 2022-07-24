using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Rigidbody 컴포넌트가 반드시 추가된다
[RequireComponent(typeof(Rigidbody))]
public class PlayerBehavior : MonoBehaviour
{

    [Tooltip("이동 속도")]
    public float moveSpeed = 5.0f;
    [Tooltip("회전 속도")]
    public float rotationSpeed = 5.0f;
    [Tooltip("점프력")]
    public float jumpPower = 5.0f;

    /// <summary>
    /// 플레이어가 움직일 방향
    /// </summary>
    Vector3 velocity;
    /// <summary>
    /// 플레이어가 쳐다볼 방향 표시
    /// </summary>
    Vector3 LookPoint;
    /// <summary>
    /// 플레이어의 움직임을 위한 컴포넌트
    /// </summary>
    Rigidbody myRigidbody;

    [Tooltip("마우스 위치")]
    Ray ray;                        

    [Tooltip("지형")]
    public Terrain Ground; 

    [SerializeField]
    [Tooltip("바닥과 플레이어 간 거리")]
    float heightAboveGround;

    /// <summary>
    /// 플레이어의 방향 설정
    /// </summary>
    /// <param name="RayPoint">플레이어가 바라볼 위치 벡터</param>
    public void LookAt(Vector3 RayPoint)
    {
        LookPoint = RayPoint;
        LookPoint.y = transform.position.y;
        transform.LookAt(LookPoint);
    }

    /// <summary>
    /// 플레이어가 점프함.
    /// </summary>
    public void Jump()
    {
       if (Mathf.Abs(myRigidbody.velocity.y)<0.1f && heightAboveGround<0.2f) //점프 중이지 않을 때
       {
           myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse); //위쪽으로 힘을 준다.
       }
       else return; //점프 중일 때는 실행하지 않고 바로 return.
    }


    // Start is called before the first frame update
    void Start()
    {
        //myRigidbody 설정.
        myRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //플레이어 움직임과 회전 구현(화살표)
        int horizontal = 0;
        int vertical = 0;

        if (Input.GetKey(KeyCode.RightArrow)) { horizontal = 1; }
        if (Input.GetKey(KeyCode.LeftArrow)) { horizontal = -1; }
        if (Input.GetKey(KeyCode.UpArrow)) { vertical = 1; }
        if (Input.GetKey(KeyCode.DownArrow)) { vertical = -1; }
        if (Input.GetKey(KeyCode.Space)) { Jump(); }

        Vector3 moveInput = new Vector3(0, 0, vertical);
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        Vector3 rotateInput = new Vector3(0, horizontal, 0);
        Vector3 rotateVelocity = rotateInput.normalized * rotationSpeed;

        velocity = moveVelocity;
        transform.Rotate(rotateVelocity);

        //바닥과의 거리 구하기
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position+0.5f*Vector3.up, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                heightAboveGround = hit.distance-0.5f;
            }
        }
    }

    //플레이어의 움직임을 구현
    public void FixedUpdate()
    {
        myRigidbody.MovePosition(myRigidbody.position + transform.rotation*velocity * Time.fixedDeltaTime);
    }
}
