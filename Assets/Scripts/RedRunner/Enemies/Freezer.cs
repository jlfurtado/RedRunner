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
        [SerializeField]
        private float m_maxEmission = 200.0f;
        [SerializeField]
        private float m_minEmission = 25.0f;

        private ParticleSystem m_particles;
        private Character m_character;

        [SerializeField]
        private float m_minStartSpeed = -5.0f;
        [SerializeField]
        private float m_maxStartSpeed = -20.0f;

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

        private void Awake()
        {
            m_particles = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            var emission = m_particles.emission;
            var main = m_particles.main;

            if (m_character != null)
            {
                emission.rateOverTime = Mathf.Lerp(m_minEmission, m_maxEmission, m_character.PercentFrozen);
                main.startSpeed = Mathf.Lerp(m_minStartSpeed, m_maxStartSpeed, m_character.PercentFrozen);
            }
            else
            {
                emission.rateOverTime = m_minEmission;
                main.startSpeed = m_minStartSpeed;
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