using UnityEngine;

namespace Weapon
{
    public class SwordColliderHandler : MonoBehaviour
    {
        private HealthObj target;
        [SerializeField] private int damageAmount;
        [SerializeField] private Sword sword;
        private void OnTriggerEnter2D(Collider2D other)
        {
            target = other.GetComponentInChildren<HealthObj>();
            if (target != null && (sword.SwordState == Sword.State.AttackLeft || sword.SwordState == Sword.State.AttackRight))
                target.Damage(damageAmount);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            target = null;
        }
    }
}
