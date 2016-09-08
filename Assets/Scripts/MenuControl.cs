using UnityEngine;
using System.Collections;

public class MenuControl : MonoBehaviour {
    public GameObject pnlMenu;
    private bool paused = false;
    private bool keyPressed = false;
    // Use this for initialization
    void Start() {
        pnlMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetAxisRaw("Pause") > 0) {
            if (!keyPressed) {
                keyPressed = true;
                Pause();
            }
        } else {
            keyPressed = false;
        }
    }

    public void Pause() {
        paused = !paused;
        if (paused) {
            Time.timeScale = 0;
            pnlMenu.SetActive(true);
        }
        else {
            Time.timeScale = 1;
            pnlMenu.SetActive(false);
        }
    }

    public void Quit() {
        Application.Quit();
    }
}
