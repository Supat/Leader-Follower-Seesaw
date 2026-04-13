using UnityEngine;
using TMPro;

public class ScoreTMPBehavior : MonoBehaviour
{
    public GameObject gameManagerObject;

    private GameManager gameManager;
    private TMP_Text scoreText;

    void Start()
    {
        gameManager = gameManagerObject.GetComponent<GameManager>();
        scoreText = GetComponentInParent<TMP_Text>();
    }

    void Update()
    {
        scoreText.text = gameManager.PlayScore.ToString();
    }
}
