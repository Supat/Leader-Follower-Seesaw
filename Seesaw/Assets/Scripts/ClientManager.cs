using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : GameManager
{
    // ClientManager doesn't run the game loop — it receives state via ManagerInlet.
    // Override Start/Update to avoid running base class game logic.

    protected override void Start()
    {
        CacheComponents();
        messageCanvas.enabled = false;
    }

    protected override void Update()
    {
        HideTargets();
        CheckAndShowTarget();

        if (Input.GetKey(KeyCode.Escape))
            SceneManager.LoadScene("MainMenuScene");
    }
}
