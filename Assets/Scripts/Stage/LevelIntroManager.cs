using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelIntroManager : MonoBehaviour
{
    GameManager gameManager;
    Vector3 levelDisplayMiddlePos = new(6.969f, 5.625f, 0);
    Vector3 levelDisplayFinalPos = new(19.22f, 5.625f, 0);
    Vector3 scenarioFinalPos = new(0.5f, 0.5f, 0);

    public Sprite[] levelDisplaySprites;
    public GameObject levelDisplay;
    public GameObject scenario;
    public Tilemap backgroundTilemap, destructiblesTilemap, indestructiblesTilemap,
        townBorderTilemap, villageBorderTilemap, castleBorderTilemap;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        SetTilemapsColor();
        ChangeLevelDisplaySprite();
        StartCoroutine(MoveLevelDisplayRight(levelDisplayMiddlePos));
    }

    public void SetTilemapsColor()
    {
        backgroundTilemap.color = Color.black;
        destructiblesTilemap.color = Color.black;
        indestructiblesTilemap.color = Color.black;

        if (townBorderTilemap.gameObject.activeSelf) townBorderTilemap.color = Color.black;
        else if (villageBorderTilemap.gameObject.activeSelf) villageBorderTilemap.color = Color.black;
        else if (castleBorderTilemap.gameObject.activeSelf) castleBorderTilemap.color = Color.black;
    }

    public void ChangeLevelDisplaySprite()
    {
        levelDisplay.GetComponent<SpriteRenderer>().sprite = levelDisplaySprites[gameManager.currentLevel - 1];
    }

    private IEnumerator MoveLevelDisplayRight(Vector3 position)
    {
        while (levelDisplay.transform.position != position)
        {
            levelDisplay.transform.position = Vector3.MoveTowards(levelDisplay.transform.localPosition, position, 20 * Time.deltaTime);
            yield return null;
        }

        if (position == levelDisplayMiddlePos)
        {
            StartCoroutine(MoveScenarioUp());
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(ChangeScenarioColor());
        }
        else if (position == levelDisplayFinalPos)
        {
            yield return new WaitForSeconds(0.25f);
            gameManager.StartNewLevel();
        }
    }

    private IEnumerator MoveScenarioUp()
    {
        while (scenario.transform.position != scenarioFinalPos)
        {
            scenario.transform.position = Vector3.MoveTowards(scenario.transform.position, scenarioFinalPos, 20 * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(1);
        StartCoroutine(MoveLevelDisplayRight(levelDisplayFinalPos));
    }

    private IEnumerator ChangeScenarioColor()
    {
        float t = Mathf.Clamp01(Time.time * Time.deltaTime);

        while (t < 1)
        {
            backgroundTilemap.color = Color.Lerp(backgroundTilemap.color, Color.white, t);
            destructiblesTilemap.color = Color.Lerp(destructiblesTilemap.color, Color.white, t);
            indestructiblesTilemap.color = Color.Lerp(indestructiblesTilemap.color, Color.white, t);

            if (townBorderTilemap.gameObject.activeSelf) townBorderTilemap.color = Color.Lerp(townBorderTilemap.color, Color.white, t);
            else if (villageBorderTilemap.gameObject.activeSelf) villageBorderTilemap.color = Color.Lerp(villageBorderTilemap.color, Color.white, t);
            else if (castleBorderTilemap.gameObject.activeSelf) castleBorderTilemap.color = Color.Lerp(castleBorderTilemap.color, Color.white, t);

            t = Mathf.Clamp01(Time.time * Time.deltaTime);
            // print(t);
            yield return null;
        }
    }

}
