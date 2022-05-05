using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;
using Game;

namespace Player
{
    public class PlayerObj : MonoBehaviour
    {
        public Character character;
        public readonly List<Character> unlockedCharacters = new List<Character>();
        private PlayerController controller;
        public PlayerUI ui;
        public readonly PlayerInput input = new PlayerInput();
        public Collider2D collider;

        [SerializeField] private LayerMask dashLayerMask;

        private void Awake()
        {
            GameData.Players.Add(this);
            GameData.player = this; //Временно
            controller = new PlayerController(dashLayerMask);
            if (!unlockedCharacters.Contains(character))
                unlockedCharacters.Add(character);
        }

        private void Update()
        {
            UpdateEyeDirection();
            transform.position = character.transform.position;
            input.Update();
            controller.Update(this);
            character.enabled = true;

            if (character.State == PlayerState.Dead)
            {
                unlockedCharacters.Remove(character);
                if (unlockedCharacters.Count > 0)
                    ChangeCharacter(unlockedCharacters[0]);
            }
        }

        private void FixedUpdate()
        {
            controller.FixedUpdate(this);
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.GetComponent<Rigidbody2D>()) 
                other.gameObject.GetComponent<Rigidbody2D>().WakeUp();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.GetComponent<Character>() || other == collider)
                return;
            if (!input.IsChangeCharacter)
                return;
            ChangeCharacter(other.GetComponent<Character>());
        }

        private void ChangeCharacter(Character other)
        {
            var transform = character.weapon.transform;
            var weaponPosition = transform.localPosition;
            var weaponRotation = transform.localRotation;
            character = other;
            var transformWeapon = character.weapon.transform;
            transformWeapon.localPosition = weaponPosition;
            transformWeapon.localRotation = weaponRotation;
			
            if (!unlockedCharacters.Contains(character))
                unlockedCharacters.Add(character);
            
			
            ui.UpdateAmmoText(character.weapon.CurrentAmmoAmount, character.weapon.MaxAmmoAmount);
        }


        private void UpdateEyeDirection()
        {
            character.sprite.flipX = (int) Mathf.Sign(-character.rb.velocity.x) == 1;
        }
    }
}
