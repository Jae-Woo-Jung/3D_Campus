using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBehaviour : MonoBehaviour
{
    [Tooltip("카메라로 비춰줄 대상.")]
    public Transform target;

    [SerializeField]
    [Tooltip("대상에 대한 카메라의 상대적 위치.")]
    Vector3 lookOffset = new Vector3(0, 10.0f, -1.0f);

    [Tooltip("대상과 카메라 간 거리 조절.")]
    public float currentZoom = 10.0f;

    public float minZoom = 1.0f;
    public float maxZoom = 10.0f;

    [Tooltip("회전 속도, q, e, shift키로 회전")]
    public float rotSpeed = 5.0f;

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

        lookOffset = transform.position - goose_position;
        lookOffset.Normalize();
        lookOffset *= 2;

        // 마우스 휠로 줌 인아웃
        currentZoom -= 3*Input.GetAxis("Mouse ScrollWheel");
        // 줌 최소 및 최대 설정에 따라 필터링.
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
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