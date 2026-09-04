using UnityEngine;

public class BreakableMaterial : MonoBehaviour
{
    [Header("Material")]
    public string materialType;

    [Header("Drops")]
    public DropItemData[] drops;

    [Header("Health")]
    [Min(1)]
    public int maxHealth = 3;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public bool TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            Debug.LogWarning(
                gameObject.name +
                " received invalid damage: " +
                damage
            );

            return false;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            gameObject.name +
            " HP: " +
            currentHealth +
            "/" +
            maxHealth
        );

        if (currentHealth <= 0)
        {
            Break();
            return true;
        }

        return false;
    }

    private void Break()
    {
        Debug.Log(gameObject.name + " BROKE!");

        Destroy(gameObject);
    }
}