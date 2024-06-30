using MMORPG.Common.Proto.Entity;
using QFramework;
using UnityEngine;
using AnimationState = MMORPG.Common.Proto.Entity.AnimationState;
using NotImplementedException = System.NotImplementedException;

namespace MMORPG.Game
{
    public class MonsterSkillState : AbstractState<AnimationState, MonsterBrain>, IController
    {
        public MonsterSkillState(FSM<AnimationState> fsm, MonsterBrain target) : base(fsm, target)
        {
        }

        protected override void OnEnter()
        {
            var skillSyncData = MonsterSkillSyncData.Parser.ParseFrom(mTarget.Data.Data);
            var skill = mTarget.ActorController.SkillManager.GetSkill(skillSyncData.SkillId);

            mTarget.ActorController.Animator.SetTrigger(skill.Define.Anim2);
            mTarget.AttackFeedback?.Play();
        }

        protected override void OnExit()
        {
        }

        public IArchitecture GetArchitecture()
        {
            return GameApp.Interface;
        }
    }
}
