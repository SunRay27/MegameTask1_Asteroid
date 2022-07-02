using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //public static GameManager Instance { get; private set; }
    [Header("Camera settings")]
    [SerializeField] Camera gameCamera;
    [Header("Prefabs")]
    [SerializeField] Player playerPrefab;
    [SerializeField] UFO ufoPrefab;
    [Header("In-game UI")]
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] UIPauseMenu pauseMenu;

    [Header("Score settings")]
    [SerializeField] int largeAsteroidScore = 20;
    [SerializeField] int mediumAsteroidScore = 50;
    [SerializeField] int smallAsteroidScore = 100;
    [SerializeField] int ufoScore = 200;

    [Header("Balance settings")]
    [SerializeField] int asteroidsToSpawn = 2;
    [SerializeField] float asteroidSpawnDelay = 2f;

    [SerializeField] float ufoMinSpawnDelay = 20f;
    [SerializeField] float ufoMaxSpawnDelay = 40f;

    public bool GameStarted { get; private set; } = false;
    public bool UseMouseInput { get; private set; } = false;

    //private vars
    
    private int score = 0;
    private UFO ufoInstance;
    private Player playerInstance;


    //init
    private void Start()
    {
        //init screen utility
        GameScreen.Init(gameCamera);
        AsteroidPool.Instance.SetOnAsteroidDestoyedAction(OnAsteroidDestroyed);

        //init player and init actions
        playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerInstance.SetOnPlayerDeadAction(() => OnPlayerDead());
        playerInstance.SetOnPlayerLostLifeAction(() => OnPlayerLostLife());
        playerInstance.UseMouseInput = UseMouseInput;
        playerInstance.gameObject.SetActive(false);

        //spawn UFO and set its target
        ufoInstance = Instantiate(ufoPrefab);
        ufoInstance.SetOnUFODestoyedAction(() => OnUFODestroyed());
        ufoInstance.ShootTarget = playerInstance.transform;
        ufoInstance.gameObject.SetActive(false);

        //show UI
        pauseMenu.Show();
    }


    //game control methods
    public void StartGame()
    {
        GameStarted = true;

        //reset game counters
        score = 0;
        asteroidsToSpawn = 2;

        //return all game objects to pools
        AsteroidPool.Instance.ReturnAll();
        BulletPool.Instance.ReturnAll();

        //disable UFO
        ufoInstance.gameObject.SetActive(false);

        //reset player hp, movement and position
        playerInstance.transform.localPosition = GameScreen.GetRandomPointOnScreen();
        playerInstance.enabled = true;
        playerInstance.Revive();

        //spawn first asteroids and start delayed UFO spawn
        SpawnAsteroids();
        StartCoroutine(DelayedUFOSpawn());

        //update UI
        UpdateLivesText();
        UpdateScoreText();
    }
    public void StopGame()
    {
        GameStarted = false;
        
        //stop all spawn coroutines
        StopAllCoroutines();
    }
    public void PauseGame()
    {
        //disable player input and logic
        Time.timeScale = 0.00001f;
        playerInstance.enabled = false;
    }
    public void ResumeGame()
    {
        //enable player input and logic
        playerInstance.enabled = true;
        Time.timeScale = 1f;
    }


    //spawn logic
    void SpawnAsteroids()
    {
        for (int i = 0; i < asteroidsToSpawn; i++)
        {
            Asteroid newAsteroid = AsteroidPool.Instance.GetItemToPosition(GameScreen.GetRandomPointOnScreenEdge());
            newAsteroid.StartLargeAsteroid();
        }
    }
    void SpawnUFO()
    {
        ufoInstance.Revive();
    }
    IEnumerator DelayedAsteroidSpawn()
    {
        yield return new WaitForSeconds(asteroidSpawnDelay);
        SpawnAsteroids();
    }
    IEnumerator DelayedUFOSpawn()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(ufoMinSpawnDelay, ufoMaxSpawnDelay));
        SpawnUFO();
    }


    //events
    void OnPlayerDead()
    {
        UpdateLivesText();
        StopGame();
        pauseMenu.Show();
    }
    void OnPlayerLostLife()
    {
        UpdateLivesText();
    }
    void OnUFODestroyed()
    {
        score += ufoScore;
        StartCoroutine(DelayedUFOSpawn());
        UpdateScoreText();
    }
    void OnAsteroidDestroyed(Asteroid asteroid)
    {
        //if this asteroid was last on map...
        if (AsteroidPool.Instance.InUse.Count == 0)
        {
            asteroidsToSpawn++;
            StartCoroutine(DelayedAsteroidSpawn());
        }

        //add score
        switch (asteroid.State)
        {
            case Asteroid.AsteroidState.Large:
                score += largeAsteroidScore;
                break;
            case Asteroid.AsteroidState.Medium:
                score += mediumAsteroidScore;
                break;
            case Asteroid.AsteroidState.Small:
                score += smallAsteroidScore;
                break;
        }
        UpdateScoreText();
    }


    //UI updates
    void UpdateLivesText()
    {
        livesText.text = $"∆изней осталось: {playerInstance.HP}";
    }
    void UpdateScoreText()
    {
        scoreText.text = $"—чЄт: {score}";
    }


    //change control type 
    public void ChangeInputType()
    {
        UseMouseInput = !UseMouseInput;
        playerInstance.UseMouseInput = UseMouseInput;
    }
}
