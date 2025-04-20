using UnityEngine;

public class ItemController : MonoBehaviour
{
    public enum ItemName
    {
        BombUp,
        FireUp,
        SpeedUp,
        Kick,
        Vest,
        BlockPass,
        RemoteControl,
        Time,
        OneUp
    };
    public ItemName itemName;
    public int points;

    SFXManager sfxManager;

    private void Start()
    {
        sfxManager = FindFirstObjectByType<SFXManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sfxManager.PlayClip(sfxManager.itemGet);

            PlayerOneController playerOneController = collision.GetComponent<PlayerOneController>();

            switch(itemName)
            {
                case ItemName.BombUp:
                    playerOneController.TotalBombs++;
                    playerOneController.BombsRemaining++;
                    break;
                case ItemName.FireUp:
                    playerOneController.explosionRadius++;
                    break;

                case ItemName.BlockPass:
                    playerOneController.GetComponent<Collider2D>().excludeLayers |= LayerMask.GetMask("Destructible");
                    break;

                case ItemName.OneUp:
                    if (playerOneController.Lives < 9) playerOneController.Lives++;
                    break;
            }

            playerOneController.Score += points;

            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            Destroy(gameObject);
        }
    }

}
