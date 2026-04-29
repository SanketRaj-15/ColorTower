using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BlockTower : MonoBehaviour
{
    public int score = 0;
    public int highScore = 0;
    public int perfectStreak = 0;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI tapInstructionText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI perfectText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalHighScoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelUpText;

    public GameObject gameOverPanel;

    public Camera mainCamera;

    public GameObject blockPrefab;
    public Transform baseBlock;
    public Material[] blockMaterials;

    public GameObject perfectParticlesPrefab;

    public AudioSource sfxAudioSource;
    public AudioClip placeSound;
    public AudioClip perfectSound;

    public float moveSpeed = 4f;
    public float speedIncreasePerBlock = 0.08f;
    public float maxMoveSpeed = 8f;

    public float blockHeight = 0.5f;
    public float perfectLimit = 0.12f;
    public float cameraFollowSpeed = 3f;

    public int levelScoreGap = 25;

    private int blockCount = 0;
    private int currentLevel = 1;
    private GameObject currentBlock;
    private Transform previousBlock;
    private int moveDirection = 1;
    private bool gameOver = false;
    private bool hasTappedOnce = false;
    private Vector3 cameraStartPosition;
    private Vector3 cameraTargetPosition;
    private Coroutine perfectTextRoutine;
    private Coroutine levelUpTextRoutine;

    void Start()
    {
        Debug.Log("Game started");

        highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        cameraStartPosition = mainCamera.transform.position;
        cameraTargetPosition = cameraStartPosition;

        previousBlock = baseBlock;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (perfectText != null)
        {
            perfectText.gameObject.SetActive(false);
        }

        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
        }

        if (levelUpText != null)
        {
            levelUpText.gameObject.SetActive(false);
        }

        UpdateScoreText();
        UpdateHighScoreText();
        UpdateLevelText();

        SpawnNextBlock();
    }

    void Update()
    {
        MoveCameraUp();

        if (gameOver == true)
        {
            return;
        }

        MoveCurrentBlock();

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            PlaceCurrentBlock();
        }
    }

    void MoveCameraUp()
    {
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            cameraTargetPosition,
            cameraFollowSpeed * Time.deltaTime
        );
    }

    void MoveCurrentBlock()
    {
        if (currentBlock == null)
        {
            return;
        }

        float currentSpeed = moveSpeed + score * speedIncreasePerBlock;

        if (currentSpeed > maxMoveSpeed)
        {
            currentSpeed = maxMoveSpeed;
        }

        Vector3 position = currentBlock.transform.position;

        position.x = position.x + currentSpeed * moveDirection * Time.deltaTime;

        if (position.x > 4f)
        {
            moveDirection = -1;
        }

        if (position.x < -4f)
        {
            moveDirection = 1;
        }

        currentBlock.transform.position = position;
    }

    void SpawnNextBlock()
    {
        Vector3 newScale = previousBlock.localScale;
        Vector3 newPosition = previousBlock.position;

        newPosition.y = previousBlock.position.y + blockHeight;
        newPosition.z = previousBlock.position.z;

        bool startFromLeft = Random.value > 0.5f;

        if (startFromLeft == true)
        {
            newPosition.x = -4f;
            moveDirection = 1;
        }
        else
        {
            newPosition.x = 4f;
            moveDirection = -1;
        }

        currentBlock = Instantiate(blockPrefab, newPosition, Quaternion.identity);
        currentBlock.transform.localScale = newScale;
        currentBlock.name = "MovingBlock_" + blockCount;

        ApplyRandomMaterial(currentBlock);
    }

    void ApplyRandomMaterial(GameObject block)
    {
        if (blockMaterials == null || blockMaterials.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, blockMaterials.Length);

        Renderer blockRenderer = block.GetComponent<Renderer>();

        if (blockRenderer != null)
        {
            blockRenderer.material = blockMaterials[randomIndex];
        }
    }

    void PlaceCurrentBlock()
    {
        if (currentBlock == null)
        {
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

        float distance = currentBlock.transform.position.x - previousBlock.position.x;
        float overlap = previousBlock.localScale.x - Mathf.Abs(distance);
        bool isPerfect = false;

        if (overlap <= 0)
        {
            EndGame();
            return;
        }

        if (Mathf.Abs(distance) <= perfectLimit)
        {
            isPerfect = true;

            currentBlock.transform.position = new Vector3(
                previousBlock.position.x,
                currentBlock.transform.position.y,
                currentBlock.transform.position.z
            );

            overlap = previousBlock.localScale.x;
            distance = 0f;
        }

        float extra = currentBlock.transform.localScale.x - overlap;
        float newSize = overlap;
        float newPositionX = previousBlock.position.x + distance / 2f;
        float fallingPieceX = currentBlock.transform.position.x + Mathf.Sign(distance) * (overlap / 2f + extra / 2f);

        currentBlock.transform.localScale = new Vector3(
            newSize,
            currentBlock.transform.localScale.y,
            currentBlock.transform.localScale.z
        );

        currentBlock.transform.position = new Vector3(
            newPositionX,
            currentBlock.transform.position.y,
            currentBlock.transform.position.z
        );

        if (extra > 0.01f && isPerfect == false)
        {
            CreateFallingPiece(
                fallingPieceX,
                currentBlock.transform.position.y,
                currentBlock.transform.position.z,
                extra,
                currentBlock.transform.localScale.y,
                currentBlock.transform.localScale.z
            );
        }

        previousBlock = currentBlock.transform;
        currentBlock = null;

        blockCount = blockCount + 1;

        if (isPerfect == true)
        {
            perfectStreak = perfectStreak + 1;

            int perfectPoints = GetPerfectScorePoints();
            score = score + perfectPoints;

            ShowPerfectText(perfectPoints);
            ShowComboText(perfectPoints);
            PlaySound(perfectSound);
            StartCoroutine(PerfectBlockAnimation(previousBlock.gameObject));
            SpawnPerfectParticles(previousBlock.position);

            Debug.Log("Perfect block placed. Streak: " + perfectStreak + ". Points added: " + perfectPoints + ". Score: " + score);
        }
        else
        {
            perfectStreak = 0;
            score = score + 1;
            HideComboText();
            PlaySound(placeSound);

            Debug.Log("Normal block placed. Points added: 1. Score: " + score);
        }

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();

            Debug.Log("New high score: " + highScore);
        }

        UpdateLevel();

        cameraTargetPosition = cameraStartPosition + new Vector3(0, blockCount * blockHeight, 0);

        UpdateScoreText();
        UpdateHighScoreText();
        UpdateLevelText();

        StartCoroutine(ShakeCamera());

        SpawnNextBlock();
    }

    int GetPerfectScorePoints()
    {
        if (perfectStreak <= 2)
        {
            return 2;
        }

        return perfectStreak * 2;
    }

    void UpdateLevel()
    {
        int newLevel = (score / levelScoreGap) + 1;

        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;

            Debug.Log("Level changed to Level " + currentLevel);

            ShowLevelUpText();
        }
    }

    void UpdateLevelText()
    {
        if (levelText == null)
        {
            return;
        }

        levelText.text = "LEVEL " + currentLevel;
    }

    void ShowLevelUpText()
    {
        if (levelUpText == null)
        {
            return;
        }

        if (levelUpTextRoutine != null)
        {
            StopCoroutine(levelUpTextRoutine);
        }

        levelUpText.text = "LEVEL " + currentLevel + "!";
        levelUpTextRoutine = StartCoroutine(LevelUpTextAnimation());
    }

    IEnumerator LevelUpTextAnimation()
    {
        levelUpText.gameObject.SetActive(true);

        Vector3 smallScale = Vector3.one * 0.8f;
        Vector3 bigScale = Vector3.one * 1.25f;
        Vector3 normalScale = Vector3.one;

        float time = 0f;
        float duration = 0.45f;

        while (time < duration)
        {
            time = time + Time.deltaTime;

            float percent = time / duration;

            levelUpText.transform.localScale = Vector3.Lerp(smallScale, bigScale, percent);

            yield return null;
        }

        time = 0f;
        duration = 0.35f;

        while (time < duration)
        {
            time = time + Time.deltaTime;

            float percent = time / duration;

            levelUpText.transform.localScale = Vector3.Lerp(bigScale, normalScale, percent);

            yield return null;
        }

        yield return new WaitForSeconds(0.35f);

        levelUpText.gameObject.SetActive(false);
        levelUpText.transform.localScale = Vector3.one;
    }

    void SpawnPerfectParticles(Vector3 position)
    {
        if (perfectParticlesPrefab == null)
        {
            return;
        }

        Vector3 particlePosition = position + new Vector3(0, blockHeight * 0.6f, 0);

        GameObject particles = Instantiate(perfectParticlesPrefab, particlePosition, Quaternion.identity);

        Destroy(particles, 1f);
    }

    void ShowPerfectText(int points)
    {
        if (perfectText == null)
        {
            return;
        }

        if (perfectTextRoutine != null)
        {
            StopCoroutine(perfectTextRoutine);
        }

        perfectText.text = "PERFECT! +" + points;

        if (perfectStreak >= 2)
        {
            perfectText.text = "PERFECT x" + perfectStreak + " +" + points;
        }

        perfectTextRoutine = StartCoroutine(PerfectTextAnimation());
    }

    void ShowComboText(int points)
    {
        if (comboText == null)
        {
            return;
        }

        if (perfectStreak < 2)
        {
            comboText.gameObject.SetActive(false);
            return;
        }

        comboText.text = "COMBO x" + perfectStreak + "  +" + points;
        comboText.gameObject.SetActive(true);
    }

    void HideComboText()
    {
        if (comboText == null)
        {
            return;
        }

        comboText.gameObject.SetActive(false);
    }

    IEnumerator PerfectBlockAnimation(GameObject block)
    {
        Renderer blockRenderer = block.GetComponent<Renderer>();

        if (blockRenderer == null)
        {
            yield break;
        }

        Material blockMaterial = blockRenderer.material;

        Color originalColor = blockMaterial.color;
        Color flashColor = Color.white;

        Vector3 originalScale = block.transform.localScale;
        Vector3 biggerScale = originalScale * 1.06f;

        float time = 0f;
        float duration = 0.12f;

        while (time < duration)
        {
            time = time + Time.deltaTime;

            float percent = time / duration;

            blockMaterial.color = Color.Lerp(originalColor, flashColor, percent);
            block.transform.localScale = Vector3.Lerp(originalScale, biggerScale, percent);

            yield return null;
        }

        time = 0f;

        while (time < duration)
        {
            time = time + Time.deltaTime;

            float percent = time / duration;

            blockMaterial.color = Color.Lerp(flashColor, originalColor, percent);
            block.transform.localScale = Vector3.Lerp(biggerScale, originalScale, percent);

            yield return null;
        }

        blockMaterial.color = originalColor;
        block.transform.localScale = originalScale;
    }

    IEnumerator PerfectTextAnimation()
    {
        perfectText.gameObject.SetActive(true);

        Vector3 startScale = Vector3.one;
        Vector3 bigScale = Vector3.one * 1.25f;

        float time = 0f;
        float duration = 0.35f;

        while (time < duration)
        {
            time = time + Time.deltaTime;

            float percent = time / duration;

            perfectText.transform.localScale = Vector3.Lerp(bigScale, startScale, percent);

            yield return null;
        }

        perfectText.gameObject.SetActive(false);
        perfectText.transform.localScale = Vector3.one;
    }

    void CreateFallingPiece(float x, float y, float z, float sizeX, float sizeY, float sizeZ)
    {
        GameObject fallingPiece = Instantiate(blockPrefab, new Vector3(x, y, z), Quaternion.identity);
        fallingPiece.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
        fallingPiece.name = "FallingPiece";

        ApplyRandomMaterial(fallingPiece);

        Rigidbody body = fallingPiece.AddComponent<Rigidbody>();
        body.mass = 1f;

        Destroy(fallingPiece, 2f);
    }

    void EndGame()
    {
        gameOver = true;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Score: " + score;
        }

        if (finalHighScoreText != null)
        {
            finalHighScoreText.text = "Best: " + highScore;
        }

        if (currentBlock != null)
        {
            Rigidbody body = currentBlock.AddComponent<Rigidbody>();
            body.mass = 1f;
            Destroy(currentBlock, 2f);
        }

        Debug.Log("Game Over. Final Score: " + score + ". High Score: " + highScore);
    }

    void PlaySound(AudioClip clip)
    {
        if (sfxAudioSource == null)
        {
            return;
        }

        if (clip == null)
        {
            return;
        }

        sfxAudioSource.PlayOneShot(clip);
    }

    public void RestartGame()
    {
        Debug.Log("Restart button pressed");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CloseGame()
    {
        Debug.Log("Close button pressed");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    void UpdateHighScoreText()
    {
        highScoreText.text = "High Score: " + highScore;
    }

    IEnumerator ShakeCamera()
    {
        Vector3 shakeBasePosition = cameraTargetPosition;

        float time = 0f;
        float duration = 0.08f;
        float strength = 0.05f;

        while (time < duration)
        {
            time = time + Time.deltaTime;

            float randomX = Random.Range(-strength, strength);
            float randomY = Random.Range(-strength, strength);

            mainCamera.transform.position = shakeBasePosition + new Vector3(randomX, randomY, 0);

            yield return null;
        }

        mainCamera.transform.position = shakeBasePosition;
    }
}
