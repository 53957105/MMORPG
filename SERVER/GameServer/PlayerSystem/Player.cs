﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MMORPG.Common.Proto.Entity;
using GameServer.Db;
using Google.Protobuf;
using Serilog;
using GameServer.MapSystem;
using GameServer.UserSystem;
using GameServer.EntitySystem;
using GameServer.NpcSystem;
using GameServer.TaskSystem;

namespace GameServer.PlayerSystem
{
    public class Player : Actor
    {
        //public static readonly float DefaultViewRange = 100;

        public User User;
        public long CharacterId;
        public int Exp;
        public long Gold;
        public InventorySystem.Inventory Knapsack;

        public Npc? InteractingNpc;
        public int CurrentDialogueId;
        public DialogueManager DialogueManager;
        public TaskManager TaskManager;

        private DbCharacter _dbCharacter;

        public Player(int entityId, DbCharacter dbCharacter, UnitDefine unitDefine,
            Map map, Vector3 pos, Vector3 dire, User user, int level)
            : base(EntityType.Player, entityId, unitDefine, map, pos, dire, dbCharacter.Name, level)
        {
            User = user;
            CharacterId = dbCharacter.Id;
            Knapsack = new(this);
            DialogueManager = new(this);
            TaskManager = new(this);

            _dbCharacter = dbCharacter;
        }

        public override void Start()
        {
            base.Start();
            Hp = _dbCharacter.Hp;
            Mp = _dbCharacter.Mp;
            Level = _dbCharacter.Level;
            Exp = _dbCharacter.Exp;
            Gold = _dbCharacter.Gold;
            Knapsack.LoadInventoryInfo(_dbCharacter.Knapsack);
            DialogueManager.LoadDialogueInfo(_dbCharacter.DialogueInfo);
            TaskManager.LoadTaskInfo(_dbCharacter.TaskInfo);
        }

        public override void Update()
        {
            base.Update();
        }

        public DbCharacter ToDbCharacter()
        {
            return new DbCharacter()
            {
                Id = CharacterId,
                Name = Name,
                UserId = User.UserId,
                UnitId = UnitDefine.ID,
                MapId = Map.MapId,
                Level = Level,
                Exp = Exp,
                Gold = Gold,
                Hp = (int)Hp,
                Mp = (int)Mp,
                X = (int)Position.X,
                Z = (int)Position.Y,
                Knapsack = Knapsack.GetInventoryInfo().ToByteArray(),
                DialogueInfo = DialogueManager.GetDialogueInfo().ToByteArray(),
                TaskInfo = TaskManager.GetTaskInfo().ToByteArray(),
            };
        }
    }
}
