using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

#pragma warning disable 0649 // Disable "Field is never assigned" warning for SerializeField

    [HideInInspector] public bool paused;
    [SerializeField] private GameObject pauseMenu;

    private void Awake() {
        pauseMenu.SetActive(false);
    }

    public bool Paused {
        get {
            return paused;
        }
        set {
            if(value)
                PauseGame();
            else
                UnpauseGame();
        }
    }

    public void PauseGame() {
        if(paused)
            return;

        paused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void UnpauseGame() {
        if(!paused)
            return;

        paused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void LoadScene(string scene) {
        SceneManager.LoadScene(scene);
    }
    
    public void ExitGame() {
        Application.Quit();
    }

}