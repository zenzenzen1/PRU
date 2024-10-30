using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DetectPlayerCollision : MonoBehaviour
{
    public GameObject GameManagerGO;
    public GameObject playerGO;
    public AudioClip looseSound;
    public GameObject crashEffect;
    private PlayerControl playerControl;
    // Start is called before the first frame update
    void Start()
    {
        playerControl = playerGO.GetComponent<PlayerControl>();
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerControl.health <= 1)
        {
            Debug.Log("Player hit");
            var player = collision.gameObject.transform;
            var crash = Instantiate(crashEffect, player.position, player.rotation);
            crash.GetComponent<ParticleSystem>().Play();
            playerGO.SetActive(false);

            AudioSource.PlayClipAtPoint(looseSound, transform.position);
            //Change game mangaer state to game over
            
            GameManagerGO.GetComponent<GameManager>().SetGameManagerState(GameManager.GameManagerState.GameOver);
        }
        else{
            playerControl.health -= 1;
            
        }
       
    }
}
