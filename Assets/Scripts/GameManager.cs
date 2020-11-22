using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

#pragma warning disable 0649 // Disable "Field is never assigned" warning for SerializeField

    [HideInInspector] public bool paused;
    [SerializeField] private GameObject pauseMenu;

    public static GameManager manager;

    private void Awake() {
        manager = this;

        pauseMenu.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Paused = !Paused;
        }
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
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void UnpauseGame() {
        if(!paused)
            return;

        paused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LoadScene(string scene) {
        SceneManager.LoadScene(scene);
        Paused = false;
    }
    
    public void ExitGame() {
        Application.Quit();
    }

}