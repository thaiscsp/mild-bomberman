using System.Collections;
using UnityEngine;

public class LevelIntroManager : MonoBehaviour
{
    GameManager gameManager;
    TilemapController tilemapController;
    Vector3 levelDisplayMiddlePos = new(6.969f, 5.625f, 0);
    Vector3 levelDisplayFinalPos = new(19.22f, 5.625f, 0);
    Vector3 scenarioFinalPos = new(0.5f, 0.5f, 0);

    public Sprite[] levelDisplaySprites;
    public GameObject levelDisplay;
    public GameObject scenario;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        tilemapController = FindFirstObjectByType<TilemapController>(); ;

        SetTilemapsColor();
        ChangeLevelDisplaySprite();
        StartCoroutine(MoveLevelDisplayRight(levelDisplayMiddlePos));
    }

    public void SetTilemapsColor()
    {
        tilemapController.backgroundTilemap.color = Color.black;
        tilemapController.destructiblesTilemap.color = Color.black;
        tilemapController.indestructiblesTilemap.color = Color.black;

        if (tilemapController.townBorderTilemap.gameObject.activeSelf) tilemapController.townBorderTilemap.color = Color.black;
        else if (tilemapController.villageBorderTilemap.gameObject.activeSelf) tilemapController.villageBorderTilemap.color = Color.black;
        else if (tilemapController.castleBorderTilemap.gameObject.activeSelf) tilemapController.castleBorderTilemap.color = Color.black;
    }

    public void ChangeLevelDisplaySprite()
    {
        levelDisplay.GetComponent<SpriteRenderer>().sprite = levelDisplaySprites[gameManager.level - 1];
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
            tilemapController.backgroundTilemap.color = Color.Lerp(tilemapController.backgroundTilemap.color, Color.white, t);
            tilemapController.destructiblesTilemap.color = Color.Lerp(tilemapController.destructiblesTilemap.color, Color.white, t);
            tilemapController.indestructiblesTilemap.color = Color.Lerp(tilemapController.indestructiblesTilemap.color, Color.white, t);

            if (tilemapController.townBorderTilemap.gameObject.activeSelf) tilemapController.townBorderTilemap.color = Color.Lerp(tilemapController.townBorderTilemap.color, Color.white, t);
            else if (tilemapController.villageBorderTilemap.gameObject.activeSelf) tilemapController.villageBorderTilemap.color = Color.Lerp(tilemapController.villageBorderTilemap.color, Color.white, t);
            else if (tilemapController.castleBorderTilemap.gameObject.activeSelf) tilemapController.castleBorderTilemap.color = Color.Lerp(tilemapController.castleBorderTilemap.color, Color.white, t);

            t = Mathf.Clamp01(Time.time * Time.deltaTime);
            // print(t);
            yield return null;
        }
    }

}
