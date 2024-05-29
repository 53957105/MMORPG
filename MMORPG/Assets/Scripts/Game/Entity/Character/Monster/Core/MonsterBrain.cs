using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MMORPG.Global;
using MMORPG.Tool;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

namespace MMORPG.Game
{
    public class MonsterBrain : MonoBehaviour
    {
        public Animator Animator;

        public enum States
        {
            None = 0,
            Idle,
            Move,
            Attack,
        }
        public FSM<States> FSM = new FSM<States>();

        public EntityTransformSyncData Data;

        public class IdleState : AbstractState<States, MonsterBrain>
        {

            public IdleState(FSM<States> fsm, MonsterBrain target) : base(fsm, target)
            {

            }

            protected override void OnEnter()
            {
                
            }

            protected override void OnUpdate()
            {
                
            }
        }

        public class MoveState : AbstractState<States, MonsterBrain>
        {
            public MoveState(FSM<States> fsm, MonsterBrain target) : base(fsm, target)
            {

            }

            protected override void OnEnter()
            {
                mTarget.Animator.SetBool("Walking", true);
            }

            protected override void OnExit()
            {
                mTarget.Animator.SetBool("Walking", false);
            }

            protected override void OnUpdate()
            {
                mTarget.CharacterController.SmoothMove(CalculateGroundPosition(mTarget.Data.Position, 6));
                mTarget.CharacterController.SmoothRotate(mTarget.Data.Rotation);
                //mTarget.Position = mTarget.Data.Position;
            }

            public Vector3 CalculateGroundPosition(Vector3 position, int layer)
            {
                int layerMask = 1 << layer;
                if (Physics.Raycast(position, Vector3.down, out var hit, Mathf.Infinity, layerMask))
                {
                    return hit.point;
                }
                return position;
            }
        }

        public class AttackState : AbstractState<States, MonsterBrain>
        {
            public AttackState(FSM<States> fsm, MonsterBrain target) : base(fsm, target)
            {

            }

            protected override void OnEnter()
            {
                mTarget.Animator.SetBool("Attack", true);
            }

            protected override void OnExit()
            {
                mTarget.Animator.SetBool("Attack", false);
            }
        }

        [Required]
        public CharacterController CharacterController;
        [Required]
        //public MonsterAnimationController AnimationController;


        private void Awake()
        {
            //AnimationController.Setup(this);
            CharacterController.Entity.OnTransformSync += OnTransformEntitySync;
        }

        private void OnTransformEntitySync(EntityTransformSyncData data)
        {
            States state = (States)data.StateId;
            if (FSM.CurrentStateId != state)
            {
                FSM.ChangeState(state);
            }

            Data = data;
            //state.Actions.ForEach(x => x.TransformEntitySync(data));
        }

        private void Start()
        {
            FSM.AddState(States.Idle, new IdleState(FSM, this));
            FSM.AddState(States.Move, new MoveState(FSM, this));
            FSM.AddState(States.Attack, new AttackState(FSM, this));
            FSM.StartState(States.Idle);
        }

        private void Update()
        {
            FSM.Update();
        }

        private void FixedUpdate()
        {
            FSM.FixedUpdate();
        }

        private void OnGUI()
        {
            FSM.OnGUI();
        }

        private void OnDestroy()
        {
            FSM.Clear();
        }

        private void OnFireStarted(InputAction.CallbackContext obj)
        {
            CharacterController.HandleWeapon.ShootStart();
        }

    }

}
