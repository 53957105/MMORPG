﻿using GameServer.FightSystem;
using GameServer.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MMORPG.Common.Proto.Entity;
using GameServer.AiSystem;
using GameServer.Tool;
using GameServer.MapSystem;
using GameServer.EntitySystem;

namespace GameServer.MissileSystem
{
    public class Missile : Entity
    {
        public float Speed;

        private Vector3 _moveTargetPos;
        private AiBase? _ai;

        public Missile(int entityId, int unitId, Map map, Vector3 pos, Vector3 dire) : base(EntityType.Missile, entityId, unitId, map)
        {
            EntityType = EntityType.Missile;
            Map = map;
            Position = pos;
            Direction = dire;
        }

        public override void Start()
        {
            base.Update();
            switch (DataHelper.GetUnitDefine(UnitId).Ai)
            {
            }
            _ai?.Start();
        }

        public override void Update()
        {
            base.Update();
            _ai?.Update();
        }
    }
}