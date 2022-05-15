using System;
using Player;
using UnityEngine;

namespace Bullet
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField]protected int damageAmount = 1;
        private void Update()
        {
            Destroy(gameObject, 5);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.GetComponent<Bullet>())
            {
                Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
                return;
            }
            Damage(collision);
            Destroy(gameObject);
        }

        protected virtual void Damage(Collision2D collision)
        {
            if (collision.collider.GetComponentInChildren<HealthObj>())
                collision.collider.GetComponentInChildren<HealthObj>().Damage(damageAmount);
        }
    }
}
