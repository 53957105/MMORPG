using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MMORPG.Global;
using MMORPG.Tool;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MMORPG.Game
{
    public class PlayerBrain : MonoBehaviour
    {
        [Required]
        public CharacterController CharacterController;
        [Required]
        public PlayerAnimationController AnimationController;

#if UNITY_EDITOR
        [SerializeField]
        [ReadOnly]
        [LabelText("CurrentState")]
        private string _currentStateName = "NONE";
#endif

        // public string StartStateName = string.Empty;

        [Information("状态机中有报错还没处理!", InfoMessageType.Error, "CheckStatesHasError")]
        [Information("空状态机是没有意义的!", InfoMessageType.Warning, "IsEmptyStates")]
        [Information("不能有相同名称的状态!", InfoMessageType.Error, "HasRepeatStateName")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Name")]
        public PlayerState[] States;

        public PlayerState CurrentState { get; private set; }

        [Title("Binding")]
        public GameObject[] AdditionalAbilityNodes;

        public GameInputControls InputControls { get; private set; }

        public LocalPlayerAbility[] GetAttachLocalAbilities() => GetAttachAbilities<LocalPlayerAbility>();

        public RemotePlayerAbility[] GetAttachRemoteAbilities() => GetAttachAbilities<RemotePlayerAbility>();

        public Vector2 GetMoveInput() => InputControls.Player.Move.ReadValue<Vector2>();
        public bool IsPressingRun() => InputControls.Player.Run.inProgress;

        public bool IsMine => CharacterController.Entity.IsMine;

        private TAbility[] GetAttachAbilities<TAbility>() where TAbility : PlayerAbility
        {
            var total = new List<TAbility>();
            total.AddRange(GetComponents<TAbility>());
            foreach (var node in AdditionalAbilityNodes)
            {
                total.AddRange(node.GetComponents<TAbility>());
            }
            return total.ToArray();
        }

        public bool ChangeState(PlayerState state)
        {
            Debug.Assert(state != null);
            Debug.Assert(States.Contains(state));
            CurrentState?.Exit();
            CurrentState = state;
#if UNITY_EDITOR
            _currentStateName = CurrentState.Name;
#endif
            CurrentState.Enter();
            return true;
        }

        public PlayerState GetState(string stateName)
        {
            return Array.Find(States, x => x.Name == stateName);
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (CheckStatesHasError())
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", "Player的PlayerBrain中的状态机有错误还未处理!", "确定");
                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
#endif
            AnimationController.Setup(this);
            CurrentState = null;
            if (States.Length == 0) return;
            CharacterController.Entity.OnTransformSync += OnTransformEntitySync;
        }

        private void OnTransformEntitySync(EntityTransformSyncData data)
        {
            Debug.Assert(!CharacterController.Entity.IsMine);
            var state = States[data.StateId];
            Debug.Assert(state != null);
            if (state != CurrentState)
            {
                ChangeState(state);
            }

            state.Actions.ForEach(x => x.TransformEntitySync(data));
        }

        private void Start()
        {
            if (IsMine)
            {
                InputControls = new();
                InputControls.Enable();

                InputControls.Player.Fire.started += OnFireStarted;
            }
            else
            {
                Destroy(CharacterController.Rigidbody);
                Destroy(CharacterController.Collider);
            }
            if (States.IsNullOrEmpty()) return;
            InitStates();
            ChangeState(States[0]);
            StartCoroutine(NetworkFixedUpdate());
        }

        private void OnFireStarted(InputAction.CallbackContext obj)
        {
            CharacterController.HandleWeapon?.ShootStart();
        }

        private void Update()
        {
            if (States.IsNullOrEmpty()) return;
            CurrentState?.Update();
        }

        private void FixedUpdate()
        {
            if (States.IsNullOrEmpty()) return;
            CurrentState?.FixedUpdate();
        }


        private IEnumerator NetworkFixedUpdate()
        {
            while (true)
            {
                CurrentState?.NetworkFixedUpdate();
                yield return new WaitForSeconds(Config.NetworkUpdateDeltaTime);
            }
        }

        private void OnEnable()
        {
            InputControls?.Enable();
        }

        private void OnDisable()
        {
            InputControls?.Disable();
        }

        private void InitStates()
        {
            States.ForEach((x, i) =>
            {
                x.Setup(this, i);
                x.Initialize();
                x.OnTransitionEvaluated += (transition, condition) =>
                {
                    ChangeState(condition ? transition.TrueState : transition.FalseState);
                };
            });
        }


#if UNITY_EDITOR
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            States?.ForEach(x => x.Brain = this);
        }

        private bool HasRepeatStateName => States.GroupBy(x => x.Name).Any(g => g.Count() > 1);

        private bool IsEmptyStates => States.Length == 0;

        private bool CheckStatesHasError()
        {
            return States.Any(x => x.HasError());
        }
#endif
    }

}
