using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void ServerScene() => SceneManager.LoadScene("ServerScene");
    public void ClientScene() => SceneManager.LoadScene("ClientScene");
    public void LocalScene() => SceneManager.LoadScene("BaseScene");
    public void PassiveServerScene() => SceneManager.LoadScene("PassiveServerScene");
    public void PassiveClientScene() => SceneManager.LoadScene("PassiveClientScene");
}
