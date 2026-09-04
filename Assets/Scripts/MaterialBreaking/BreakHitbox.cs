using UnityEngine;

public class BreakHitbox : MonoBehaviour
{
    public BreakSystem breakSystem;

    private void OnTriggerStay2D(Collider2D other)
    {
        BreakableMaterial material =
            other.GetComponentInParent<BreakableMaterial>();

        if (material != null)
        {
            breakSystem.SetTarget(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BreakableMaterial material =
            other.GetComponentInParent<BreakableMaterial>();

        if (material != null)
        {
            breakSystem.RemoveTarget(other);
        }
    }
}