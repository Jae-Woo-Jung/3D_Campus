using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Rigidbody 컴포넌트가 반드시 추가된다
[RequireComponent(typeof(Rigidbody))]
public class PlayerBehavior : MonoBehaviour
{

    [Tooltip("최대 이동 속도")]
    public float MaxMoveSpeed = 5.0f;
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
    /// <summary>
    /// 애니메이션 설정을 위한 컴포넌트
    /// </summary>
    Animator animator;
    /// <summary>
    /// 중력을 조절하기 위한 컴포넌트
    /// </summary>
    ConstantForce constForce;
    /// <summary>
    /// 비행 준비 중에 비행 종료가 안 되도록 함. 
    /// </summary>
    bool flyStart;

    [Tooltip("마우스 위치")]
    Ray ray;                        

    [Tooltip("지형")]
    public Terrain Ground; 

    [SerializeField]
    [Tooltip("바닥과 플레이어 간 거리")]
    float heightAboveGround;

    [SerializeField] [Tooltip("현재 속도")] float moveSpeed;


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
    void Jump()
    {
       if (Mathf.Abs(myRigidbody.velocity.y)<0.1f && heightAboveGround<0.2f) //점프 중이지 않을 때
       {
           myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse); //위쪽으로 힘을 준다.
       }
       else return; //점프 중일 때는 실행하지 않고 바로 return.
    }

    void Fly()
    {

        if ( !(GameManager.isWater||GameManager.isFlying) ) //물 위에 있지도 않고 비행 중도 아닐 때
        {
            flyStart = true;
            GameManager.isFlying = true;
            animator.SetBool("isFlying", true);
            if (heightAboveGround<2.0f)
            myRigidbody.AddForce(Vector3.up * 60.0f, ForceMode.Impulse); //위쪽으로 힘을 준다.
            constForce.force=new Vector3(0.0f, 9.81f*myRigidbody.mass-10.0f, 0.0f);

            Invoke("uncheck_flyStart", 1.0f);
        }
        else return; //물에 있거나 비행 중이면 실행하지 않고 바로 return.

    }

    void uncheck_flyStart() { flyStart = false; }

    void FlyEnd()
    {
        GameManager.isFlying = false;
        animator.SetBool("isFlying", false);
    }


    // Start is called before the first frame update
    void Start()
    {
        //myRigidbody 설정.
        myRigidbody = GetComponent<Rigidbody>();
        //animator 설정.
        animator = GetComponent<Animator>();
        constForce = GetComponent<ConstantForce>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isWater", GameManager.isWater);

        //플레이어 움직임과 회전 구현(화살표)
        int horizontal = 0;
        int vertical = 0;
        float elevation = 0;

        //z키를 누른 경우 비행 시작.
        if (!GameManager.isFlying && Input.GetKey(KeyCode.Z) && !Input.GetKey("left shift")) { Fly(); }

        if (Input.GetKey(KeyCode.RightArrow)) { horizontal = 1;}
        if (Input.GetKey(KeyCode.LeftArrow)) { horizontal = -1; }
        if (Input.GetKey(KeyCode.UpArrow)) { vertical = 1; }
        if (Input.GetKey(KeyCode.Space)) { Jump(); }


        if (GameManager.isFlying&&Input.GetKey(KeyCode.Z) && !Input.GetKey("left shift")) { elevation = 10; }
        if (GameManager.isFlying && Input.GetKey(KeyCode.Z) && Input.GetKey("left shift")) { elevation = -5; }

        Vector3 moveInput = new Vector3(0, 0, 1.0f);
        //이동 속도가 연속적으로 변함. 
        moveSpeed += (vertical==0? -5 : 1)*Time.deltaTime;
        moveSpeed = Mathf.Clamp(moveSpeed, 0.0f, MaxMoveSpeed);
        animator.SetFloat("Speed", moveSpeed);
        
        Vector3 moveVelocity = moveInput * moveSpeed* (GameManager.isFlying? 3:1)+new Vector3(0, elevation, 0);  //비행 중이면 3배

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

        if (!flyStart && GameManager.isFlying && heightAboveGround < 2.0f)
        {
            FlyEnd();
            //1초 동안 중력 서서히 복구.
            constForce.force = new Vector3(0.0f, Mathf.Clamp(constForce.force.y-myRigidbody.mass*9.81f*Time.deltaTime, 0, 9.81f), 0.0f);
        }
    }

    //플레이어의 움직임을 구현
    public void FixedUpdate()
    {
        myRigidbody.MovePosition(myRigidbody.position + transform.rotation*velocity * Time.fixedDeltaTime);
    }
}
