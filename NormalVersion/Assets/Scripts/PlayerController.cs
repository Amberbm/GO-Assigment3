using System;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
                                     // eigen variabeles << 
    public float baseSpeed = 0.4f;     // houd bij hoeveel dot zijn opgegeten binen een cycle
    public static int dotCount = 0;
    public  float speedAdjustment = 1;

    public float firstPunishment = 0.7f;  // snelheid aanpassing vanaf 1 dots
    public float secondPunishment = 0.4f; //snelheid aanpassing vanaf 5 dots
    public int cycleDuration = 50;  // houdbij hoelang de speler niets moet oppakken voor reset
    public static int cycleTracker = 0;   // houd bij hoe lang geleden een dot was opgepakt
                                          // >>
    public float speed = 0.4f;
    Vector2 _dest = Vector2.zero;
    Vector2 _dir = Vector2.zero;
    Vector2 _nextDir = Vector2.zero;

    [Serializable]
    public class PointSprites
    {
        public GameObject[] pointSprites;
    }

    public PointSprites points;

    public static int killstreak = 0;

    // script handles
    private GameGUINavigation GUINav;
    private GameManager GM;
    private ScoreManager SM;

    private bool _deadPlaying = false;

    // Use this for initialization
    void Start()
    {
        GM = GameObject.Find("Game Manager").GetComponent<GameManager>();
        SM = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
        GUINav = GameObject.Find("UI Manager").GetComponent<GameGUINavigation>();
        _dest = transform.position;
    }

    void HandleSpeedAjustments()            ///// eigen methode
    {
        cycleTracker++;
        
        if (cycleTracker >= cycleDuration)// houd bij wanneer de dotcount moet worden gereset
        {
            dotCount = 0;
            cycleTracker = 0;
        }

        if (dotCount == 0)                 //handeled de speedajustment gebasseerd op de dotcount
            speedAdjustment = 1;
        if (dotCount >= 5)
            speedAdjustment = firstPunishment;
        if (dotCount >= 10)
            speedAdjustment = secondPunishment;
        
        speed = baseSpeed * speedAdjustment;//pas de speed aan gebaseerd op de speed adjustment
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (GameManager.gameState)
        {
            case GameManager.GameState.Game:
                ReadInputAndMove();
                Animate();
                HandleSpeedAjustments();
                break;

            case GameManager.GameState.Dead: 
                                    // eigen code <<
                dotCount = 0;         // reset dotcount/ tracker als de speler doodgaat
                cycleTracker = 0;
                                       // >>
                if (!_deadPlaying)
                    StartCoroutine("PlayDeadAnimation");
                break;
        }


    }

    IEnumerator PlayDeadAnimation()
    {
        _deadPlaying = true;
        GetComponent<Animator>().SetBool("Die", true);
        yield return new WaitForSeconds(1);
        GetComponent<Animator>().SetBool("Die", false);
        _deadPlaying = false;

        if (GameManager.lives <= 0)
        {
            Debug.Log("Treshold for High Score: " + SM.LowestHigh());
            if (GameManager.score >= SM.LowestHigh())
                GUINav.getScoresMenu();
            else
                GUINav.H_ShowGameOverScreen();
        }

        else
            GM.ResetScene();
    }

    void Animate()
    {
        Vector2 dir = _dest - (Vector2)transform.position;
        GetComponent<Animator>().SetFloat("DirX", dir.x);
        GetComponent<Animator>().SetFloat("DirY", dir.y);
    }

    bool Valid(Vector2 direction)
    {
        // cast line from 'next to pacman' to pacman
        // not from directly the center of next tile but just a little further from center of next tile
        Vector2 pos = transform.position;
        direction += new Vector2(direction.x * 0.45f, direction.y * 0.45f);
        RaycastHit2D hit = Physics2D.Linecast(pos + direction, pos);
        return hit.collider.name == "pacdot" || (hit.collider == GetComponent<Collider2D>());
    }

    public void ResetDestination()
    {
        _dest = new Vector2(15f, 11f);
        GetComponent<Animator>().SetFloat("DirX", 1);
        GetComponent<Animator>().SetFloat("DirY", 0);
    }

    void ReadInputAndMove()
    {
        // move closer to destination
        Vector2 p = Vector2.MoveTowards(transform.position, _dest, speed);
        GetComponent<Rigidbody2D>().MovePosition(p);

        // get the next direction from keyboard
        if (Input.GetAxis("Horizontal") > 0) _nextDir = Vector2.right;
        if (Input.GetAxis("Horizontal") < 0) _nextDir = -Vector2.right;
        if (Input.GetAxis("Vertical") > 0) _nextDir = Vector2.up;
        if (Input.GetAxis("Vertical") < 0) _nextDir = -Vector2.up;

        // if pacman is in the center of a tile
        if (Vector2.Distance(_dest, transform.position) < 0.00001f)
        {
            if (Valid(_nextDir))
            {
                _dest = (Vector2)transform.position + _nextDir;
                _dir = _nextDir;
            }
            else   // if next direction is not valid
            {
                if (Valid(_dir))  // and the prev. direction is valid
                    _dest = (Vector2)transform.position + _dir;   // continue on that direction

                // otherwise, do nothing
            }
        }
    }

    public Vector2 getDir()
    {
        return _dir;
    }

    public void UpdateScore()
    {
        killstreak++;

        // limit killstreak at 4
        if (killstreak > 4) killstreak = 4;

        Instantiate(points.pointSprites[killstreak - 1], transform.position, Quaternion.identity);
        GameManager.score += (int)Mathf.Pow(2, killstreak) * 100;

    }
}
