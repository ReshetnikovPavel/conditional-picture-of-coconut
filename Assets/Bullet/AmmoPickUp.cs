using System;
using Game;
using Player;
using UnityEngine;
using Weapon;

namespace Bullet
{
	public class AmmoPickUp<T>
	{
		public void PickUp(GameObject gameObject, Collider2D other, int bulletsAmount)
		{
			if (!other.GetComponent<Character>())
				return;
			var character = GameData.player.character;
			if (!(character.weapon is T))
				return;

			var weapon = character.weapon;
			if (weapon.ammoState == AmmoState.Full)
				return;
			
			weapon.AddBullets(bulletsAmount);
			
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}