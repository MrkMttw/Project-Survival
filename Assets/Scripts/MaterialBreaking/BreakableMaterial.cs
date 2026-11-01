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

    public bool IsBroken { get; private set; }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public bool TakeDamage(int damage)
    {
        if (IsBroken)
            return false;

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
        IsBroken = true;

        Debug.Log(gameObject.name + " BROKE!");
    }
}