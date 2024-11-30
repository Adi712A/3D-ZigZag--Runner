using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO; // Required for file writing

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Sets the speed of the player
    /// </summary>
    public float speed;
    /// <summary>
    /// Sets the direction of the player
    /// </summary>
    private Vector3 dir;
    /// <summary>
    /// ps for particle system
    /// </summary>
    public GameObject ps;
    /// <summary>
    /// variable to check if player is dead
    /// </summary>
    private bool isDead;
    /// <summary>
    /// Reference to retry button
    /// </summary>
    public GameObject retryBtn;
    /// <summary>
    /// Player score 
    /// </summary>
    private int score = 0;

    public Text scoreText;

    public Animator gameOverAnim;

    public Text newHighScore;

    public Image background;

    public Text[] scoreTexts;

    public LayerMask whatIsGround;

    private bool isPlaying = false;

    public Transform contactPoint;

    public AudioClip musicClip;

    public AudioSource musicSource;

    // Path to store the score data
    private string filePath = "PlayerScore.csv"; // Path to the CSV file

    // To prevent multiple score entries for one game over
    private bool hasLoggedScore = false;

    // [SerializeField] to make it visible in Inspector for assignment
    [SerializeField] private Button pauseButton; // Ensure this is assigned in the Inspector

    // Start is called before the first frame update
    void Start()
    {
        isDead = false;
        dir = Vector3.zero;
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGrounded() && isPlaying)
        {
            isDead = true;
            musicSource.Stop();
            GameOver();
            retryBtn.SetActive(true);
            
            if (transform.childCount > 0)
            {
                transform.GetChild(0).transform.parent = null;
            }
        }

        if (Input.GetMouseButtonDown(0) && !isDead)
        {
            isPlaying = true;
            score++;
            scoreText.text = score.ToString();
            if (dir == Vector3.forward)
            {
                dir = Vector3.left;
            }
            else
            {
                dir = Vector3.forward;
            }
        }

        // Moves the player
        float amtToMove = speed * Time.deltaTime;
        transform.Translate(dir * amtToMove);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PickUp")
        {
            other.gameObject.SetActive(false);
            Instantiate(ps, transform.position, Quaternion.identity);
            score += 3;
            scoreText.text = score.ToString();
        }
    }

    private void GameOver()
    {
        gameOverAnim.SetTrigger("GameOver");
        scoreTexts[1].text = score.ToString();
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", score);
            newHighScore.gameObject.SetActive(true);
            background.color = new Color(255, 118, 246, 255);
            foreach (Text txt in scoreTexts)
            {
                txt.color = Color.magenta;
            }
        }
        scoreTexts[3].text = PlayerPrefs.GetInt("BestScore", 0).ToString();

        // Only log the score once
        if (!hasLoggedScore)
        {
            LogScore(score); // Log the actual score when the game is over
            hasLoggedScore = true; // Prevent further logging
        }

        // Check if the pauseButton has been assigned before using it
        if (pauseButton != null)
        {
            pauseButton.interactable = false; // Disable the pause button
        }
    }

    void LogScore(int score)
    {
        // Log the score to the CSV file with "Player" only
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine("Player, " + score); // Logs "Player" and their score
        }
    }

    private bool isGrounded()
    {
        Collider[] colliders = Physics.OverlapSphere(contactPoint.position, 0.5f, whatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
