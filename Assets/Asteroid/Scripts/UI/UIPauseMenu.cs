using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPauseMenu : MonoBehaviour
{
    [SerializeField] GameObject menuCanvas;
    [SerializeField] GameManager gameManager;
    [SerializeField] Button resumeButton, newGameButton, quitGameButton, controlsButton;

    private TextMeshProUGUI controlButtonText;

    private void Start()
    {
        quitGameButton.onClick.AddListener(() => Application.Quit());
        resumeButton.onClick.AddListener(() => ResumeButton());
        newGameButton.onClick.AddListener(() => NewGameButton());
        controlsButton.onClick.AddListener(()=> ChangeControlsButton());

        controlButtonText = controlsButton.GetComponentInChildren<TextMeshProUGUI>();

        //update text at start as well
        controlButtonText.text = gameManager.UseMouseInput ? "”правление: клавиатура + мышь" : "”правление: клавиатура";
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuCanvas.activeSelf && gameManager.GameStarted)
                ResumeButton();
            else if (!menuCanvas.activeSelf)
                Show();
        }
    }

    public void Show()
    {
        gameManager.PauseGame();

        //update resume button
        resumeButton.interactable = gameManager.GameStarted;

        menuCanvas.SetActive(true);
    }
    private void ChangeControlsButton()
    {
        gameManager.ChangeInputType();

        controlButtonText.text = gameManager.UseMouseInput ? "”правление: клавиатура + мышь" : "”правление: клавиатура";
    }
    private void NewGameButton()
    {
        menuCanvas.SetActive(false);

        gameManager.StopGame();
        gameManager.StartGame();
        gameManager.ResumeGame();
    }
    private void ResumeButton()
    {
        menuCanvas.SetActive(false);

        gameManager.ResumeGame();
    }
}
