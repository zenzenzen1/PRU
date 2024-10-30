using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float turningSpeed = 90f;
    private ParticleSystem dustParticles;
    private Rigidbody2D rb;
    private GameObject scoreTextGO;
    public float torque = 4;
    public float jumpForce = 10f;
    public float speedMultiplier = 2f;
    public float slowMultiplier = 0.15f;
    private bool isGrounded = false;
    public GameObject GameManagerGO;
    public int health;
    private int level;
    public TextMeshProUGUI healthText;
    
    private int coinScore;
    // Start is called before the first frame update
    void Start()
    {
        level = PlayerPrefs.GetInt(Setting.level);
        health = level switch {
            1 => 3,
            2 => 2,
            3 => 1,
            _ => 1
        };
        coinScore = level switch {
            1 => 10,
            2 => 20,
            3 => 30,
            _ => 10
        };
        rb = GetComponent<Rigidbody2D>();
        dustParticles = transform.Find("Dust Particles").GetComponent<ParticleSystem>();
        scoreTextGO = GameObject.FindGameObjectWithTag("ScoreTextTag");
    }

    // Update is called once per frame
    void Update()
    {
        healthText.text = level switch {
            1 => "Easy",
            2 => "Medium",
            3 => "Hard",
            _ => "Easy"
        } + "\n" +
        "Health: " + health + "\nSpeed: " + rb.velocity.magnitude + "\n" + Setting.developer;
        if (Input.GetKey(KeyCode.A))
        {
            // Rotate object to the left
            rb.AddTorque(torque);
        }
        if (Input.GetKey(KeyCode.D))
        {
            // Rotate object to the right
            rb.AddTorque(-torque);

        }
        if (Input.GetKey(KeyCode.W) && isGrounded)
        {
            rb.velocity *= speedMultiplier;
        }
        // Slow down
        if (Input.GetKey(KeyCode.S) && isGrounded)
        {
            rb.velocity *= slowMultiplier;
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log("Jumping");
            rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground") || collision.collider.CompareTag("Rock"))
        {
            isGrounded = true;
            dustParticles.Play();
        }
        
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = false;
            dustParticles.Stop();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BonusItem"))
        {
            scoreTextGO.GetComponent<GameScore>().Score += coinScore;
            Destroy(collision.gameObject);
        }
        if(collision.CompareTag("Rock")){
            health -= 1;
            if(health <= 1){
                GameManagerGO.GetComponent<GameManager>().SetGameManagerState(GameManager.GameManagerState.GameOver);
            }
            System.Console.WriteLine("Rock hit");
        }
        if (collision.CompareTag("Finish"))
        {
            GameManagerGO.GetComponent<GameManager>().SetGameManagerState(GameManager.GameManagerState.Victory);
            gameObject.SetActive(false);
        }
    }
    
    
}
