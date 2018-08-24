using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RedRunner.Characters;
using System;

namespace RedRunner.Enemies
{

    public class Freezer : Enemy
    {
        [SerializeField]
        private Collider2D m_Collider2D;

        private Character m_character;

        public override Collider2D Collider2D
        {
            get
            {
                return m_Collider2D;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Character character = other.GetComponent<Character>();
            if (character != null)
            {
                character.EnterFreezer(this);
                m_character = character;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Character character = other.GetComponent<Character>();
            if (character != null)
            {
                character.ExitFreezer(this);
                m_character = character;
            }
        }

        public override void Kill(Character target)
        {
            target.Die();
            Vector3 spawnPosition = target.transform.position;
            //ParticleSystem particle = Instantiate<ParticleSystem>(target.WaterParticleSystem, spawnPosition, Quaternion.identity);
            //Destroy(particle.gameObject, particle.main.duration);
            AudioManager.Singleton.PlaySpikeSound(transform.position);
        }

    }

}