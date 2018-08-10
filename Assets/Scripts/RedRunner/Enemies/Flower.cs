﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RedRunner.Characters;

namespace RedRunner.Enemies
{

	public class Flower : Enemy
	{

		[SerializeField]
		private Collider2D m_Collider2D;

		public override Collider2D Collider2D {
			get {
				return m_Collider2D;
			}
		}

		void OnTriggerEnter2D (Collider2D other)
		{
			Character character = other.GetComponent<Character> ();
			if (character != null) {
				Kill (character);
			}
		}

		public override void Kill (Character target)
		{
			target.Die (true);

			// TODO: explosion SFX and particles
            //ParticleSystem particle = Instantiate<ParticleSystem> (target.WaterParticleSystem, spawnPosition, Quaternion.identity);
			//Destroy (particle.gameObject, particle.main.duration);
			//AudioManager.Singleton.PlayWaterSplashSound (transform.position);
		}

	}

}