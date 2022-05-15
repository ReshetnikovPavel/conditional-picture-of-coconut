using System;
using MainGameScripts;
using Unity.Mathematics;
using UnityEngine;

namespace Player
{
	public class PlayerController
    {
        private readonly LayerMask dashLayerMask;
        private float moveSpeed = 10f;
		private float rollSpeed = 20f;

		private Vector2 cursorPosition;
		private Vector2 moveDirection;
		private Vector2 latestMoveDirection;
		public float latestAimAngle;

		public PlayerController(LayerMask dashLayerMask)
		{
			this.dashLayerMask = dashLayerMask;
        }
    
		public void Update(PlayerObj player)
		{
			moveDirection = player.input.MovementInput.normalized;
			cursorPosition = Camera.main.ScreenToWorldPoint(player.input.AimingInput);
			if (moveDirection != Vector2.zero)
				latestMoveDirection = moveDirection.normalized;
			switch (player.character.State)
			{
				case PlayerState.Normal:
					Aim(player);
					Fire(player);
					FireHeld(player);
					FireReleased(player);
					Dash(player);
					Roll(player);
					break;
				case PlayerState.Rolling:
					Aim(player);
					break;
				case PlayerState.Dead:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void FixedUpdate(PlayerObj player)
        {

            switch (player.character.State)
			{
				case PlayerState.Normal:
					Move(player);
					break;
				case PlayerState.Rolling:
					SetVelocityForRoll(player);
					break;
				case PlayerState.Dead:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void Move(PlayerObj player)
		{
			player.character.rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
		}

		private void Dash(PlayerObj player)
		{
			if (!player.input.IsDash)
				return;
		
			var dashAmount = 3f;
			var position = player.transform.position;
			var dashPosition = position + (Vector3) latestMoveDirection.normalized * dashAmount;
			var raycastHit = Physics2D.Raycast(position, latestMoveDirection.normalized, dashAmount, dashLayerMask);
			if (raycastHit.collider != null)
				dashPosition = raycastHit.point;
			player.character.rb.MovePosition(dashPosition);
		}

		private void SetVelocityForRoll(PlayerObj player)
		{
			var rollSpeedDropMultiplier = 2f;
			rollSpeed -= rollSpeed * rollSpeedDropMultiplier * Time.deltaTime;
		
			var minimumRollSpeed = 4f;
			if (rollSpeed < minimumRollSpeed) 
				player.character.State = PlayerState.Normal;

			player.character.rb.velocity = latestMoveDirection * rollSpeed;
		}

		private void Roll(PlayerObj player)
		{
			if (!player.input.IsRoll)
				return;
			player.character.State = PlayerState.Rolling;
		

			rollSpeed = 20f;

		}
	
		private void Aim(PlayerObj player)
		{
			var aimDirection = cursorPosition - player.character.rb.position;
			player.character.sprite.flipX = (int)Mathf.Sign(-aimDirection.x) == 1;
			//weapon.weaponPrefab.transform.eulerAngles = aimDirection;
			var aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
			player.character.weapon.weaponPrefab.transform.RotateAround(player.character.transform.position, Vector3.forward, aimAngle - latestAimAngle);
			latestAimAngle = aimAngle;
		}
		private static void Fire(PlayerObj player)
		{
			player.character.weapon.Fire(player.input.IsFireInput);
			player.ui.UpdateAmmoText(player.character.weapon.CurrentAmmoAmount, player.character.weapon.MaxAmmoAmount);
		}
	
		private static void FireHeld(PlayerObj player)
		{
			player.character.weapon.FireHeld(player.input.IsFireInputHeld);
			player.ui.UpdateAmmoText(player.character.weapon.CurrentAmmoAmount, player.character.weapon.MaxAmmoAmount);
		}

		private static void FireReleased(PlayerObj player)
		{
			player.character.weapon.FireReleased(player.input.IsFireInputReleased);
			player.ui.UpdateAmmoText(player.character.weapon.CurrentAmmoAmount, player.character.weapon.MaxAmmoAmount);
		}
    }
}