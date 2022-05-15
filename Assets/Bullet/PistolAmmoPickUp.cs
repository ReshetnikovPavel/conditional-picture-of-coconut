using Player;
using UnityEngine;
using Weapon;

namespace Bullet
{
	public class PistolAmmoPickUp : MonoBehaviour
	{
		private int bulletsAmount = 10;
		public void OnTriggerEnter2D(Collider2D other)
		{
			var ammoPickup = new AmmoPickUp<Pistol>();
			ammoPickup.PickUp(gameObject, other, bulletsAmount);
		}
	}
}
