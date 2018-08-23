using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UnityStandardAssets.CrossPlatformInput;

using RedRunner.Utilities;
using System;

namespace RedRunner.Characters
{

	public class RedCharacter : Character
	{

		public override event DeadHandler OnDead;

		#region Public Methods

		public virtual void PlayFootstepSound ()
		{
			if ( m_GroundCheck.IsGrounded )
			{
				AudioManager.Singleton.PlayFootstepSound ( m_FootstepAudioSource, ref m_CurrentFootstepSoundIndex );
			}
		}

		public override void Move ( float horizontalAxis )
		{
			if ( !m_IsDead )
			{
				float speed = m_CurrentRunSpeed * (m_GroundCheck.IsSnow ? m_snowSpeed : 1.0f);
//				if ( CrossPlatformInputManager.GetButton ( "Walk" ) )
//				{
//					speed = m_WalkSpeed;
				//				}
				Vector2 velocity = m_Rigidbody2D.velocity;
				velocity.x = speed * horizontalAxis;
				m_Rigidbody2D.velocity = velocity;
				if ( horizontalAxis > 0f )
				{
					Vector3 scale = transform.localScale;
					scale.x = Mathf.Sign ( horizontalAxis );
					transform.localScale = scale;
				}
				else if ( horizontalAxis < 0f )
				{
					Vector3 scale = transform.localScale;
					scale.x = Mathf.Sign ( horizontalAxis );
					transform.localScale = scale;
				}
			}
		}

		public override void Jump ()
		{
			if ( !m_IsDead )
			{
				if ( m_GroundCheck.IsGrounded )
				{
					Vector2 velocity = m_Rigidbody2D.velocity;
					velocity.y = m_JumpStrength;
					m_Rigidbody2D.velocity = velocity;
					m_Animator.ResetTrigger ( "Jump" );
					m_Animator.SetTrigger ( "Jump" );
					m_JumpParticleSystem.Play ();
					AudioManager.Singleton.PlayJumpSound ( m_JumpAndGroundedAudioSource );
				}
			}
		}

		public override void Die ()
		{
			Die ( false );
		}

		public override void Die ( bool blood )
		{
			if ( !m_IsDead )
			{
				if ( OnDead != null )
				{
					OnDead ();
				}
				m_IsDead = true;
				m_Skeleton.SetActive ( true, m_Rigidbody2D.velocity );
				if ( blood )
				{
					ParticleSystem particle = Instantiate<ParticleSystem> ( 
						                          m_BloodParticleSystem,
						                          transform.position,
						                          Quaternion.identity );
                    var main = particle.main;
                    main.startColor = m_Color;
                    particle.GetComponent<Rigidbody2D>().velocity = m_Rigidbody2D.velocity * 0.5f;
                    Destroy ( particle.gameObject, particle.main.duration );
				}
				m_OnCharacterDead.Invoke ();
				CameraController.Singleton.fastMove = true;
			}
		}

		public override void EmitRunParticle ()
		{
			if ( !m_IsDead )
			{
				m_RunParticleSystem.Emit ( 1 );
			}
		}

		public override void Reset ()
		{
			m_IsDead = false;
			m_ClosingEye = false;
			m_Guard = false;
			m_Block = false;
			m_CurrentFootstepSoundIndex = 0;
			transform.localScale = m_InitialScale;
			m_Rigidbody2D.velocity = Vector2.zero;
			m_Skeleton.SetActive ( false, m_Rigidbody2D.velocity );
            m_Skeleton.Reset();
            m_timeLeft = m_freezeTime;
        }

        #endregion

    }

}