using UnityEngine;
using Weapon;

namespace Bullet
{
	public class LaserAmmoPickUp : MonoBehaviour
	{
		private int bulletsAmount = 10;
		public void OnTriggerEnter2D(Collider2D other)
		{
			var ammoPickup = new AmmoPickUp<Laser>();
			ammoPickup.PickUp(gameObject, other, bulletsAmount);
		}
	}
}
