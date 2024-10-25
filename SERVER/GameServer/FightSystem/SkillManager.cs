﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.EntitySystem;
using GameServer.Manager;

namespace GameServer.FightSystem
{
    public class SkillManager
    {
        public Actor OwnerActor;
        public Dictionary<int, Skill> SkillDict = new();

        public SkillManager(Actor ownerActor)
        {
            OwnerActor = ownerActor;
        }

        public void Start()
        {
            // 应该从数据库中读取，当前未设计，直接加载所有技能
            var list = DataManager.Instance.SkillDict.Values
                .Where(s => s.UnitID == OwnerActor.UnitDefine.ID)
                .ToList();
            foreach (var define in list)
            {
                var skill = new Skill(OwnerActor, define);
                SkillDict[skill.Define.ID] = skill;
                skill.Start();
            }
        }

        public void Update()
        {
            foreach (var skill in SkillDict.Values)
            {
                skill.Update();
            }
        }

        public Skill? GetSkill(int skillId)
        {
            if (!SkillDict.TryGetValue(skillId, out var skill))
            {
                return null;
            }
            return skill;
        }
    }
}
