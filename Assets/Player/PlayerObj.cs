using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;
using Game;
using MainGameScripts;

namespace Player
{
    public class PlayerObj : MonoBehaviour
    {
        public CameraController Camera;
        public Character character;
        public readonly List<Character> unlockedCharacters = new List<Character>();
        public PlayerController controller;
        private PlayerUI ui;
        public readonly PlayerInput input = new PlayerInput();
        public Collider2D collider;
        public event EventHandler OnCharacterChange;
        public static event EventHandler OnNoCharactersLeft;

        [SerializeField] private LayerMask dashLayerMask;

        private void Awake()
        {
            GameData.Players.Add(this);
            GameData.player = this; 
            controller = new PlayerController(dashLayerMask);
            if (!unlockedCharacters.Contains(character))
                unlockedCharacters.Add(character);
            //OnCharacterChange?.Invoke(this, EventArgs.Empty);
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
                    ChangeCharacterOnSamePosition(unlockedCharacters[0]);
            }

            if (unlockedCharacters.Count <= 0) 
                OnNoCharactersLeft?.Invoke(this, EventArgs.Empty);
        }

        private void FixedUpdate()
        {
            controller.FixedUpdate(this);
            if (input.IsChangeCharacter)
            {
                input.DropIsChangeCharacter();
                TryChangeCharacter();
            }

            if (input.IsChooseCharacter)
            {
                input.DropIsChangeCharacter();
                ChangeCharacterWithNumbers();
            }
                
                
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
        
        //private void OnTriggerEnter2D(Collider2D other)
        //{
        //    if (other.gameObject.GetComponent<Rigidbody2D>()) 
        //        other.gameObject.GetComponent<Rigidbody2D>().WakeUp();
        //}

        //private void OnTriggerStay2D(Collider2D other)
        //{
        //    if (!other.GetComponent<Character>() || other.gameObject == character.gameObject)
        //        return;
        //    if (!input.IsChangeCharacter)
        //        return;
        //    input.DropIsChangeCharacter();
        //    ChangeCharacter(other.GetComponent<Character>());
        //}

        private void ChangeCharacter(Character other)
        {
            if (other == null)
                return;
            Camera.player = other;
            var transform = character.weapon.transform;
            var weaponPosition = transform.localPosition;
            var weaponRotation = transform.localRotation;
            character.gameObject.SetActive(false);
            character = other;
            character.gameObject.SetActive(true);
            var transformWeapon = character.weapon.transform;
            transformWeapon.localPosition = weaponPosition;
            var weaponEulerAngles = weaponRotation.eulerAngles;
            transformWeapon.localRotation = Quaternion.Euler(weaponEulerAngles.x, weaponEulerAngles.y, controller.latestAimAngle);
			
            if (!unlockedCharacters.Contains(character))
                unlockedCharacters.Add(character);
            
            OnCharacterChange?.Invoke(this, EventArgs.Empty);
        }

        private void TryChangeCharacter()
        {
            var radius = 5f;
            var objects = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Character"));
            foreach (var obj in objects)
            {
                if (obj.gameObject != GameData.player.character.gameObject)
                {
                    ChangeCharacter(obj.GetComponent<Character>());
                    return;
                }
            }

        }

        private void ChangeCharacterWithNumbers()
        {
            if (input.ChosenCharacterIndex < unlockedCharacters.Count) 
                ChangeCharacterOnSamePosition(unlockedCharacters[input.ChosenCharacterIndex]);
        }

        private void ChangeCharacterOnSamePosition(Character other)
        {
            var position = character.transform.position;
            ChangeCharacter(other);
            character.transform.position = position;
        }


        private void UpdateEyeDirection()
        {
            character.sprite.flipX = (int) Mathf.Sign(-character.rb.velocity.x) == 1;
        }
    }
}
