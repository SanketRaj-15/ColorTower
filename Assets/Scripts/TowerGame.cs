using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TowerGame : MonoBehaviour
{
    public int score = 0;
    public int highScore = 0;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI tapInstructionText;

    public GameObject breakParticlesPrefab;
    public Camera mainCamera;

    private List<GameObject> blocks = new List<GameObject>();
    private bool isBusy = false;
    private Vector3 cameraStartPosition;
    private bool hasTappedOnce = false;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        cameraStartPosition = mainCamera.transform.position;

        FindAllBlocks();

        UpdateScoreText();
        UpdateHighScoreText();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            TapBlock();
        }
    }

    void FindAllBlocks()
    {
        blocks.Clear();

        for (int i = 0; i < 100; i++)
        {
            GameObject block = GameObject.Find("Block_" + i);

            if (block != null)
            {
                blocks.Add(block);
            }
        }
    }

    void TapBlock()
    {
        if (isBusy == true)
        {
            return;
        }

        if (blocks.Count == 0)
        {
            Debug.Log("No blocks left!");
            return;
        }

        if (hasTappedOnce == false)
        {
            hasTappedOnce = true;

            if (tapInstructionText != null)
            {
                tapInstructionText.gameObject.SetActive(false);
            }
        }

        GameObject topBlock = blocks[blocks.Count - 1];

        blocks.RemoveAt(blocks.Count - 1);

        score = score + 1;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateScoreText();
        UpdateHighScoreText();

        StartCoroutine(ShrinkAndDestroy(topBlock));
        StartCoroutine(ShakeCamera());
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CloseGame()
    {
        Debug.Log("Close button pressed");

        Application.Quit();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    void UpdateHighScoreText()
    {
        highScoreText.text = "High Score: " + highScore;
    }

    IEnumerator ShrinkAndDestroy(GameObject block)
    {
        isBusy = true;

        Vector3 particlePosition = block.transform.position;

        if (breakParticlesPrefab != null)
        {
            GameObject particles = Instantiate(breakParticlesPrefab, particlePosition, Quaternion.identity);
            Destroy(particles, 1f);
        }

        Vector3 startScale = block.transform.localScale;
        Vector3 endScale = Vector3.zero;

        float time = 0f;
        float duration = 0.18f;

        while (time < duration)
        {
            time = time + Time.deltaTime;

            float percent = time / duration;

            block.transform.localScale = Vector3.Lerp(startScale, endScale, percent);

            yield return null;
        }

        Destroy(block);

        isBusy = false;
    }

    IEnumerator ShakeCamera()
    {
        float time = 0f;
        float duration = 0.12f;
        float strength = 0.08f;

        while (time < duration)
        {
            time = time + Time.deltaTime;

            float randomX = Random.Range(-strength, strength);
            float randomY = Random.Range(-strength, strength);

            mainCamera.transform.position = cameraStartPosition + new Vector3(randomX, randomY, 0);

            yield return null;
        }

        mainCamera.transform.position = cameraStartPosition;
    }
}
