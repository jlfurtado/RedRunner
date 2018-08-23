using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RedRunner.Characters;
using System;

namespace RedRunner.Enemies
{

    public class Fire : Enemy
    {
        [SerializeField]
        private Collider2D m_Collider2D;

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
                character.EnterFire(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Character character = other.GetComponent<Character>();
            if (character != null)
            {
                character.ExitFire(this);
            }
        }

        public override void Kill(Character target)
        {
            //target.Die();
            //Vector3 spawnPosition = target.transform.position;
            ////ParticleSystem particle = Instantiate<ParticleSystem>(target.WaterParticleSystem, spawnPosition, Quaternion.identity);
            ////Destroy(particle.gameObject, particle.main.duration);
            //AudioManager.Singleton.PlaySpikeSound(transform.position);
        }

    }

}