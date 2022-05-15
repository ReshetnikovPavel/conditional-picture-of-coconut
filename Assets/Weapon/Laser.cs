using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Weapon
{
	public class Laser : Weapon
	{
		public LineRenderer lineRenderer;
		private float maxDistance = 100;
		public Character character;
		private int damageAmount = 1;
		private RaycastHit2D hit;

		protected override void SetMaxBulletAmount()
		{
			maxAmmoAmount = 10;
		}

		protected override void SetReloadingTime()
		{
			reloadingTime = 0.2f;
		}

		public void Update()
		{
			if (character.State != PlayerState.Normal)
				TurnLaserOff();
		}
		
		public override void Fire(bool isButtonPressed)
		{
			if (!isButtonPressed)
				return;
			
			switch (state)
			{
				case WeaponState.Ready:
					switch (ammoState)
					{
						case AmmoState.Empty:
							TurnLaserOff();
							break;
						case AmmoState.Full:
						case AmmoState.Normal:
							ammoState = AmmoState.Normal;
							CastRay();
							Damage();
							currentAmmoAmount--;

							if (currentAmmoAmount <= 0)
								ammoState = AmmoState.Empty;
							
							reloadStart = Time.time;
							state = WeaponState.Reloading;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				case WeaponState.Reloading:
					CastRay();
					timeDifference = Time.time - reloadStart;
					if (Time.time - reloadStart >= reloadingTime)
						state = WeaponState.Ready;
					break;
			}
		}

		private void CastRay()
		{
			lineRenderer.enabled = true;
			var position = firePoint.position;
			if (Physics2D.Raycast(position, firePoint.up))
			{
				hit = Physics2D.Raycast(position, transform.up);
				DrawRay(position, hit.point);
			}
			else
			{
				DrawRay(position, firePoint.transform.up * maxDistance);
			}
		}

		private void DrawRay(Vector2 startPosition, Vector2 endPosition)
		{
			lineRenderer.widthMultiplier = 0.1f;
			lineRenderer.SetPosition(0, startPosition);
			lineRenderer.SetPosition(1, endPosition);
		}

		public override void FireReleased(bool isButtonReleased)
		{
			if (isButtonReleased)
				TurnLaserOff();
		}
		
		private void TurnLaserOff()
		{
			lineRenderer.enabled = false;
		}

		private void Damage()
		{
			if (hit.collider == null)
				return;
			if (hit.collider.GetComponentInChildren<HealthObj>())
				hit.collider.GetComponentInChildren<HealthObj>().Damage(damageAmount);
		}
	}
}

