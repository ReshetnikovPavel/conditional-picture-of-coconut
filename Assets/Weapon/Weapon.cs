using System;
using System.ComponentModel.Design;
using Bullet;
using UnityEngine;

namespace Weapon
{
	public class Weapon : MonoBehaviour
	{
		[SerializeField]protected GameObject bulletPrefab;
		public GameObject weaponPrefab;
		[SerializeField]protected Transform firePoint;
		[SerializeField]protected float fireForce = 20f;
		[SerializeField]protected WeaponState state;
		public AmmoState ammoState;
		[SerializeField]protected float reloadingTime;
		[SerializeField]protected float reloadStart;
		[SerializeField]protected float timeDifference;
		[SerializeField] protected int maxAmmoAmount;
		[SerializeField] protected int currentAmmoAmount;
		public int MaxAmmoAmount => maxAmmoAmount;
		public int CurrentAmmoAmount => currentAmmoAmount;
		

		public void Awake()
		{
			ammoState = AmmoState.Full;
			currentAmmoAmount = maxAmmoAmount;
		}

		protected virtual void SetMaxBulletAmount()
		{
			maxAmmoAmount = int.MaxValue;
		}

		protected virtual void SetReloadingTime()
		{
            reloadingTime = 0.5f;
        }
		

        public virtual void Fire(bool isButtonPressed)
		{
			switch (state)
			{
				case WeaponState.Ready when isButtonPressed:
					switch (ammoState)
					{
						case AmmoState.Empty:
							break;
						case AmmoState.Full:
						case AmmoState.Normal:
							CreateBullets();
							ammoState = AmmoState.Normal;
							reloadStart = Time.time;
							state = WeaponState.Reloading;
							if (currentAmmoAmount <= 0)
								ammoState = AmmoState.Empty;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				case WeaponState.Reloading:
					timeDifference = Time.time - reloadStart;
					if (timeDifference >= reloadingTime)
						state = WeaponState.Ready;
                    break;
            }
		}
		
		public virtual void FireHeld(bool isButtonPushed)
		{
			Fire(isButtonPushed);
		}

		public virtual void FireReleased(bool isButtonReleased)
		{
			
		}

		protected virtual void CreateBullets()
		{
			if (currentAmmoAmount == 0)
				return;
			var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
			bullet.GetComponent<Rigidbody2D>().AddForce(firePoint.up * fireForce, ForceMode2D.Impulse);
			currentAmmoAmount--;
		}

		public void AddBullets(int amount)
		{
			var temporary = currentAmmoAmount;
			temporary += amount;
			if (temporary > maxAmmoAmount)
			{
				currentAmmoAmount = maxAmmoAmount;
				ammoState = AmmoState.Full;
			}
			else
			{
				currentAmmoAmount = temporary;
				ammoState = AmmoState.Normal;
			}
		}
	}
}