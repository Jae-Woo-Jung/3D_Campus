using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class water_check : MonoBehaviour
{
    
    [SerializeField][Tooltip("물 속 저항력")] private float waterDrag=5.0f;
    [SerializeField] [Tooltip("물 밖 저항력")] private float originDrag=0.0f; 

    [SerializeField] private GameObject thePlayer;


    AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "player")
            GetInWater(other);  // 물에 들어감

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "player")
            GetOutWater(other);  // 물에서 나옴
    }

    private void GetInWater(Collider _player)
    {

        GameManager.isWater = true;
        _player.transform.GetComponent<Rigidbody>().drag = waterDrag;
        audioSource.Play();
    }

    private void GetOutWater(Collider _player)
    {

        if (GameManager.isWater)
        {
            GameManager.isWater = false;
            Invoke("restoreDrag", 1.0f);
            audioSource.Play();
        }
    }

    void restoreDrag()
    {
        if (!GameManager.isWater) thePlayer.transform.GetComponent<Rigidbody>().drag = originDrag;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

}
