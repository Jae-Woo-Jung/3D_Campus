using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Rigidbody 컴포넌트가 반드시 추가된다
[RequireComponent(typeof(Rigidbody))]
public class PlayerBehavior : MonoBehaviour
{
    [Tooltip("걸음 수, 헤엄 수, 날갯짓 수")]
    public Text score;

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

    /// <summary>
    /// 플레이어 음악 설정
    /// </summary>
    AudioSource audioSource;

    public AudioClip swimClip;
    public AudioClip flyClip;
    public AudioClip walkClip;
    public AudioClip runClip;
    public AudioClip idleClip;

    /// <summary>
    /// 거위의 울음소리 간격 랜덤 설정
    /// </summary>
    float next_honk_time;
    float time_since_last_honk;

    [Tooltip("마우스 위치")]
    Ray ray;                        

    [SerializeField]
    [Tooltip("바닥과 플레이어 간 거리")]
    float heightAboveGround;

    [SerializeField] [Tooltip("현재 속도")] float moveSpeed;

    /// <summary>
    /// 비행 중 상승할지 하강할지.
    /// </summary>
    int elevation = 0;

    animationTime AnimationTime;
    /// <summary>
    /// 걸음 수, 헤엄 수, 날갯짓 수를 세기 위함. 
    /// </summary>
    private struct animationTime
    {
        public float swim;
        public float walk;
        public float run;
        public float fly;
    }

    private string CurrentPlayingAnimation
    {
        get
        {
            int w = animator.GetCurrentAnimatorClipInfo(0).Length;
            string[] clipName = new string[w];
            for (int i = 0; i < w; i += 1)
            {
                clipName[i] = animator.GetCurrentAnimatorClipInfo(0)[i].clip.name;
            }
            return string.Join(", ", clipName);
        }
    }


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

    public void MoveForward()
    {
        //이동 속도가 연속적으로 변함. 
        moveSpeed += 1 * Time.deltaTime;
        moveSpeed = Mathf.Clamp(moveSpeed, 0.0f, MaxMoveSpeed);
        animator.SetFloat("Speed", moveSpeed);
    }

    public void TurnRight()
    {
        Vector3 rotateInput = new Vector3(0, 1, 0);
        Vector3 rotateVelocity = rotateInput.normalized * rotationSpeed;
        transform.Rotate(rotateVelocity);
    }

    public void TurnLeft()
    {
        Vector3 rotateInput = new Vector3(0, -1, 0);
        Vector3 rotateVelocity = rotateInput.normalized * rotationSpeed;
        transform.Rotate(rotateVelocity);
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

    public void Fly()
    {

        if ( !(GameManager.isWater||GameManager.isFlying||flyStart) ) //물 위에 있지도 않고 비행 중도 아닐 때
        {
            flyStart = true;
            GameManager.isFlying = true;
            animator.SetBool("isFlying", true);
            if (heightAboveGround<2.0f)
            myRigidbody.AddForce(Vector3.up * 60.0f, ForceMode.Impulse); //위쪽으로 힘을 준다.
            constForce.force=new Vector3(0.0f, 9.81f*myRigidbody.mass-1.0f, 0.0f);

            Invoke("uncheck_flyStart", 1.0f);
        }
        else return; //물에 있거나 비행 중이면 실행하지 않고 바로 return.

    }


    public void FlyUp()
    {
        elevation = 10;
    }

    public void FlyDown()
    {
        elevation = -5;
    }

    void uncheck_flyStart() 
    { 
        flyStart = false;
        GameObject.Find("ScoreUI").transform.Find("FlyButton").gameObject.SetActive(false);
        GameObject.Find("ScoreUI").transform.Find("UpButton").gameObject.SetActive(true);
        GameObject.Find("ScoreUI").transform.Find("DownButton").gameObject.SetActive(true);
    }

    void FlyEnd()
    {
        GameManager.isFlying = false;
        animator.SetBool("isFlying", false);
        GameObject.Find("ScoreUI").transform.Find("UpButton").gameObject.SetActive(false);
        GameObject.Find("ScoreUI").transform.Find("DownButton").gameObject.SetActive(false);
        GameObject.Find("ScoreUI").transform.Find("FlyButton").gameObject.SetActive(true);
    }
    


    // Start is called before the first frame update
    void Start()
    {
        //myRigidbody 설정.
        myRigidbody = GetComponent<Rigidbody>();
        //animator 설정.
        animator = GetComponent<Animator>();
        constForce = GetComponent<ConstantForce>();
        AnimationTime.run = AnimationTime.fly = AnimationTime.swim = AnimationTime.walk = 0.0f;
        //오디오 소스 설정
        audioSource = GetComponent<AudioSource>();

        //게임 시작 후 몇 초 후에 울지 설정.
        next_honk_time = Random.Range(5.0f, 10.0f);
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isWater", GameManager.isWater);

        //플레이어 움직임과 회전 구현(화살표)
        int horizontal = 0;
        int vertical = 0;

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
        if (!BtnController.CheckGoButton()) moveSpeed += (vertical==0? -3 : 1)*Time.deltaTime;
        moveSpeed = Mathf.Clamp(moveSpeed, 0.0f, MaxMoveSpeed);
        animator.SetFloat("Speed", moveSpeed);
        
        velocity = moveInput * moveSpeed* (GameManager.isFlying? 3:1)+new Vector3(0, elevation, 0);  //비행 중이면 3배

        elevation = 0;  //다음 프레임에 영향이 가지 않도록 초기화

        Vector3 rotateInput = new Vector3(0, horizontal, 0);
        Vector3 rotateVelocity = rotateInput.normalized * rotationSpeed;

        if (horizontal!=0) transform.Rotate(rotateVelocity);

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

        //Fly할 때 pitch 바꿈.
        audioSource.pitch = 1.0f;

        if (CurrentPlayingAnimation.Contains("run"))
        {
            AnimationTime.run += Time.deltaTime;
            if (!audioSource.clip.name.Contains("run"))
            { 
                audioSource.clip = runClip;
                audioSource.Play();
            }
        }
        if (CurrentPlayingAnimation.Contains("Walk"))
        {
            AnimationTime.walk += Time.deltaTime;
            if (!audioSource.clip.name.Contains("walk"))
            {
                audioSource.clip = walkClip;
                audioSource.Play();
            }
        }
        if (CurrentPlayingAnimation.Contains("swim"))
        {
            AnimationTime.swim += Time.deltaTime;
            if (!audioSource.clip.name.Contains("swim"))
            {
                audioSource.clip = swimClip;
                audioSource.Play();
            }
        }
        if (CurrentPlayingAnimation.Contains("fly"))
        {
            AnimationTime.fly += Time.deltaTime;
            if (!audioSource.clip.name.Contains("wings"))
            {
                audioSource.pitch = 0.7f;
                audioSource.clip = flyClip;
                audioSource.Play();
            }
        }

        if (CurrentPlayingAnimation.Contains("idle"))
        {

            time_since_last_honk += Time.deltaTime;
            if (!audioSource.clip.name.Contains("울음"))
            {
                //몇 초 후에 울지 설정.
                next_honk_time = Random.Range(5.0f, 10.0f);
                audioSource.clip = idleClip;
            }

            if (time_since_last_honk>next_honk_time) 
            { 
                audioSource.Play();
                time_since_last_honk = 0.0f;
                next_honk_time = Random.Range(5.0f, 10.0f);
            }
        }


        //UI 수정
        score.text = "걸음 수 : " + (int)(AnimationTime.run * 4.0f + AnimationTime.walk * 2.0f) + "\n수영 수 : " + (int) (AnimationTime.swim * (1.0f / 3.0f)) + "\n날갯짓 수 : " + (int) AnimationTime.fly/2 + "\n";

    }

    //플레이어의 움직임을 구현
    public void FixedUpdate()
    {
        myRigidbody.MovePosition(myRigidbody.position + transform.rotation*velocity * Time.fixedDeltaTime);
    }
}
