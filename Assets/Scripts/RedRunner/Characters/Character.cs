using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RedRunner.Utilities;
using UnityEngine.Events;
using System;
using UnityStandardAssets.CrossPlatformInput;
using RedRunner.Enemies;

namespace RedRunner.Characters
{

	[RequireComponent ( typeof ( Rigidbody2D ) )]
	[RequireComponent ( typeof ( Collider2D ) )]
	[RequireComponent ( typeof ( Animator ) )]
	[RequireComponent ( typeof ( Skeleton ) )]
	public abstract class Character : MonoBehaviour
	{
        public delegate void DeadHandler();

        public virtual event DeadHandler OnDead;

        #region Fields

        [Header("Character Details")]
        [Space]
        [SerializeField]
        protected float m_MaxRunSpeed = 8f;
        [SerializeField]
        protected float m_RunSmoothTime = 5f;
        [SerializeField]
        protected float m_RunSpeed = 5f;
        [SerializeField]
        protected float m_WalkSpeed = 1.75f;
        [SerializeField]
        protected float m_JumpStrength = 10f;
        [SerializeField]
        protected float m_snowSpeed = 0.5f;
        [SerializeField]
        protected Color m_Color = Color.white;
        [SerializeField]
        protected string[] m_Actions = new string[0];
        [SerializeField]
        protected int m_CurrentActionIndex = 0;

        [Header("Character Reference")]
        [Space]
        [SerializeField]
        protected Rigidbody2D m_Rigidbody2D;
        [SerializeField]
        protected Collider2D m_Collider2D;
        [SerializeField]
        protected Animator m_Animator;
        [SerializeField]
        protected GroundCheck m_GroundCheck;
        [SerializeField]
        protected ParticleSystem m_RunParticleSystem;
        [SerializeField]
        protected ParticleSystem m_JumpParticleSystem;
        [SerializeField]
        protected ParticleSystem m_WaterParticleSystem;
        [SerializeField]
        protected ParticleSystem m_BloodParticleSystem;
        [SerializeField]
        protected ParticleSystem m_snowParticles;

        [SerializeField]
        protected Skeleton m_Skeleton;
        [SerializeField]
        protected float m_RollForce = 10f;

        [Header("Character Audio")]
        [Space]
        [SerializeField]
        protected AudioSource m_MainAudioSource;
        [SerializeField]
        protected AudioSource m_FootstepAudioSource;
        [SerializeField]
        protected AudioSource m_JumpAndGroundedAudioSource;

        [Header("Character Events")]
        [Space]
        [SerializeField]
        protected CharacterDeadEvent m_OnCharacterDead;

        [SerializeField]
        protected float m_freezeTime = 30.0f;

        [SerializeField]
        protected float m_maxSnowEmission = 200.0f;

        [SerializeField]
        protected float m_minSnowEmission = 25.0f;

        [SerializeField]
        protected float m_minSnowStartSpeed = -5.0f;

        [SerializeField]
        protected float m_maxSnowStartSpeed = -20.0f;

        #endregion

        #region Protected Variables

        protected bool m_IsDead = false;
        protected bool m_ClosingEye = false;
        protected bool m_Guard = false;
        protected bool m_Block = false;
        protected Vector2 m_Speed = Vector2.zero;
        protected float m_CurrentRunSpeed = 0f;
        protected float m_CurrentSmoothVelocity = 0f;
        protected int m_CurrentFootstepSoundIndex = 0;
        protected Vector3 m_InitialScale;
        protected Vector3 m_InitialPosition;
        protected float m_timeLeft;
        protected List<Freezer> m_freezers = new List<Freezer>();
        protected List<Fire> m_fires = new List<Fire>();

        #endregion


        #region Public Methods

        public void EnterFreezer(Freezer freezer)
        {
            m_freezers.Add(freezer);
        }

        public void ExitFreezer(Freezer freezer)
        {
            m_freezers.Remove(freezer);
        }

        public void EnterFire(Fire fire)
        {
            m_fires.Add(fire);
        }

        public void ExitFire(Fire fire)
        {
            m_fires.Remove(fire);
        }

        #endregion

        #region MonoBehaviour Messages

        protected virtual void Awake()
        {
            m_InitialPosition = transform.position;
            m_InitialScale = transform.localScale;
            m_GroundCheck.OnGrounded += GroundCheck_OnGrounded;
            m_Skeleton.OnActiveChanged += Skeleton_OnActiveChanged;
            m_IsDead = false;
            m_ClosingEye = false;
            m_Guard = false;
            m_Block = false;
            m_CurrentFootstepSoundIndex = 0;
            GameManager.OnReset += GameManager_OnReset;
            Array.ForEach(gameObject.GetComponentsInChildren<SpriteRenderer>(), sprite => sprite.color = m_Color);
            m_timeLeft = m_freezeTime;
        }

        protected virtual void Update()
        {
            if (!GameManager.Singleton.gameStarted || !GameManager.Singleton.gameRunning)
            {
                return;
            }

            if (transform.position.y < 0f)
            {
                Die();
            }

            UpdateFreezeStatus();
            UpdateSnowParticles();

            // Speed
            m_Speed = new Vector2(Mathf.Abs(m_Rigidbody2D.velocity.x), Mathf.Abs(m_Rigidbody2D.velocity.y));

            // Speed Calculations
            m_CurrentRunSpeed = m_RunSpeed;
            if (m_Speed.x >= m_RunSpeed)
            {
                m_CurrentRunSpeed = Mathf.SmoothDamp(m_Speed.x, m_MaxRunSpeed, ref m_CurrentSmoothVelocity, m_RunSmoothTime);
            }

            // Input Processing
            Move(CrossPlatformInputManager.GetAxis("Horizontal"));
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                Jump();
            }
            if (m_IsDead && !m_ClosingEye)
            {
                StartCoroutine(CloseEye());
            }
            if (CrossPlatformInputManager.GetButtonDown("Guard"))
            {
                m_Guard = !m_Guard;
            }
            if (m_Guard)
            {
                if (CrossPlatformInputManager.GetButtonDown("Fire"))
                {
                    m_Animator.SetTrigger(m_Actions[m_CurrentActionIndex]);
                    if (m_CurrentActionIndex < m_Actions.Length - 1)
                    {
                        m_CurrentActionIndex++;
                    }
                    else
                    {
                        m_CurrentActionIndex = 0;
                    }
                }
            }

            if (Input.GetButtonDown("Roll"))
            {
                Vector2 force = new Vector2(0f, 0f);
                if (transform.localScale.z > 0f)
                {
                    force.x = m_RollForce;
                }
                else if (transform.localScale.z < 0f)
                {
                    force.x = -m_RollForce;
                }
                m_Rigidbody2D.AddForce(force);
            }
        }

        protected virtual void LateUpdate()
        {
            m_Animator.SetFloat("Speed", m_Speed.x);
            m_Animator.SetFloat("VelocityX", Mathf.Abs(m_Rigidbody2D.velocity.x));
            m_Animator.SetFloat("VelocityY", m_Rigidbody2D.velocity.y);
            m_Animator.SetBool("IsGrounded", m_GroundCheck.IsGrounded);
            m_Animator.SetBool("IsDead", m_IsDead);
            m_Animator.SetBool("Block", m_Block);
            m_Animator.SetBool("Guard", m_Guard);
            if (Input.GetButtonDown("Roll"))
            {
                m_Animator.SetTrigger("Roll");
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator CloseEye()
        {
            m_ClosingEye = true;
            yield return new WaitForSeconds(0.6f);
            while (m_Skeleton.RightEye.localScale.y > 0f)
            {
                if (m_Skeleton.RightEye.localScale.y > 0f)
                {
                    Vector3 scale = m_Skeleton.RightEye.localScale;
                    scale.y -= 0.1f;
                    m_Skeleton.RightEye.localScale = scale;
                }
                if (m_Skeleton.LeftEye.localScale.y > 0f)
                {
                    Vector3 scale = m_Skeleton.LeftEye.localScale;
                    scale.y -= 0.1f;
                    m_Skeleton.LeftEye.localScale = scale;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }

        protected virtual void UpdateFreezeStatus()
        {
            m_timeLeft = Mathf.Clamp(m_timeLeft + ((IsFreezing ? -1.0f : 1.0f)*Time.deltaTime), 0.0f, m_freezeTime);
            if (m_timeLeft <= 0.0f)
            {
                m_freezers[m_freezers.Count - 1].Kill(this);
            }
            Debug.Log(m_timeLeft);
        }

        private void UpdateSnowParticles()
        {
            var emission = m_snowParticles.emission;
            var main = m_snowParticles.main;

            if (IsInSnow)
            {
                emission.rateOverTime = Mathf.Lerp(m_minSnowEmission, m_maxSnowEmission, PercentFrozen);
                main.startSpeed = Mathf.Lerp(m_minSnowStartSpeed, m_maxSnowStartSpeed, PercentFrozen);
            }
            else
            {
                emission.rateOverTime = 0.0f;
                main.startSpeed = m_minSnowStartSpeed;
            }
        }

        #endregion

        #region Properties

        public virtual float MaxRunSpeed
        {
            get
            {
                return m_MaxRunSpeed;
            }
        }

        public virtual float RunSmoothTime
        {
            get
            {
                return m_RunSmoothTime;
            }
        }

        public virtual float RunSpeed
        {
            get
            {
                return m_RunSpeed;
            }
        }

        public virtual float WalkSpeed
        {
            get
            {
                return m_WalkSpeed;
            }
        }

        public virtual float JumpStrength
        {
            get
            {
                return m_JumpStrength;
            }
        }

        public virtual Vector2 Speed
        {
            get
            {
                return m_Speed;
            }
        }

        public virtual Color Color
        {
            get
            {
                return m_Color;
            }
        }

        public virtual string[] Actions
        {
            get
            {
                return m_Actions;
            }
        }

        public virtual string CurrentAction
        {
            get
            {
                return m_Actions[m_CurrentActionIndex];
            }
        }

        public virtual int CurrentActionIndex
        {
            get
            {
                return m_CurrentActionIndex;
            }
        }

        public virtual GroundCheck GroundCheck
        {
            get
            {
                return m_GroundCheck;
            }
        }

        public virtual Rigidbody2D Rigidbody2D
        {
            get
            {
                return m_Rigidbody2D;
            }
        }

        public virtual Collider2D Collider2D
        {
            get
            {
                return m_Collider2D;
            }
        }

        public virtual Animator Animator
        {
            get
            {
                return m_Animator;
            }
        }

        public virtual ParticleSystem RunParticleSystem
        {
            get
            {
                return m_RunParticleSystem;
            }
        }

        public virtual ParticleSystem JumpParticleSystem
        {
            get
            {
                return m_JumpParticleSystem;
            }
        }

        public virtual ParticleSystem WaterParticleSystem
        {
            get
            {
                return m_WaterParticleSystem;
            }
        }

        public virtual ParticleSystem BloodParticleSystem
        {
            get
            {
                return m_BloodParticleSystem;
            }
        }

        public virtual Skeleton Skeleton
        {
            get
            {
                return m_Skeleton;
            }
        }

        public virtual bool IsDead
        {
            get
            {
                return m_IsDead;
            }
        }

        public virtual bool ClosingEye
        {
            get
            {
                return m_ClosingEye;
            }
        }

        public virtual bool Guard
        {
            get
            {
                return m_Guard;
            }
        }

        public virtual bool Block
        {
            get
            {
                return m_Block;
            }
        }

        public virtual AudioSource Audio
        {
            get
            {
                return m_MainAudioSource;
            }
        }

        public bool IsFreezing
        {
            get
            {
                return m_freezers.Count > 0 && m_fires.Count <= 0;
            }
        }

        public bool IsInSnow
        {
            get
            {
                return m_freezers.Count > 0;
            }
        }

        public float PercentFrozen
        {
            get
            {
                return 1.0f - (m_timeLeft / m_freezeTime);
            }

        }
        #endregion

        #region Abstract Methods

        public abstract void Move ( float horizontalAxis );

		public abstract void Jump ();

		public abstract void Die ();

		public abstract void Die ( bool blood );

		public abstract void EmitRunParticle ();

		public abstract void Reset ();

        #endregion

        #region Events

        protected virtual void GameManager_OnReset()
        {
            transform.position = m_InitialPosition;
            Reset();
        }

        protected virtual void Skeleton_OnActiveChanged(bool active)
        {
            m_Animator.enabled = !active;
            m_Collider2D.enabled = !active;
            m_Rigidbody2D.simulated = !active;
        }

        protected virtual void GroundCheck_OnGrounded()
        {
            if (!m_IsDead)
            {
                m_JumpParticleSystem.Play();
                AudioManager.Singleton.PlayGroundedSound(m_JumpAndGroundedAudioSource);
            }
        }

        #endregion

        [System.Serializable]
        public class CharacterDeadEvent : UnityEvent
        {

        }

    }

}