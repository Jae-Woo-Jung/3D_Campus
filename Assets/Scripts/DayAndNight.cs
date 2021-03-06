using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayAndNight : MonoBehaviour
{

    [Tooltip("낮 하늘")]
    public Material DaySkybox;
    [Tooltip("밤하늘")]
    public Material NightSkybox;

    [SerializeField] GameObject camera;

    [SerializeField][Tooltip("게임 세계에서의 100초 = 현실 세계의 1초")]
    float secondPerRealTimeSecond=100.0f;

    [SerializeField] [Tooltip("밤 상태의 fog 밀도")]
    float nightFogDensity= 0.05f;
    [SerializeField][Tooltip("낮 상태의 Fog 밀도")]
    float dayFogDensity = 0.0f;
    [SerializeField] [Tooltip("증감량 비율")] float fogDensityCalc = 0.1f;
    
    float currentFogDensity;

    // Start is called before the first frame update
    void Start()
    {
        dayFogDensity = RenderSettings.fogDensity;
    }

    // Update is called once per frame
    void Update()
    {
        // 계속 태양을 X 축 중심으로 회전. 현실시간 1초에  0.1f * secondPerRealTimeSecond 각도만큼 회전
        transform.Rotate(Vector3.right, 0.1f * secondPerRealTimeSecond * Time.deltaTime);

        if ((transform.eulerAngles.x+360)%360 >= 170) // x 축 회전값 170 이상이면 밤
            GameManager.isNight = true;
        else if (10<= (transform.eulerAngles.x + 360) % 360 && (transform.eulerAngles.x + 360) % 360 <= 170)  // x 축 회전값 10 이하면 낮
            GameManager.isNight = false;


        if (GameManager.isNight)
        {
            if (!RenderSettings.skybox.name.Contains("Night")) camera.GetComponent<Skybox>().material = NightSkybox;
            if (currentFogDensity <= nightFogDensity)
            {
                currentFogDensity += 0.01f * fogDensityCalc * Time.deltaTime;
                RenderSettings.fogDensity = currentFogDensity;
            }
        }
        else
        {
            if (!RenderSettings.skybox.name.Contains("Day")) camera.GetComponent<Skybox>().material = DaySkybox;
            if (currentFogDensity >= nightFogDensity)
            {
                currentFogDensity -= 0.01f * fogDensityCalc * Time.deltaTime;
                RenderSettings.fogDensity = currentFogDensity;
            }

        }
    }
}
