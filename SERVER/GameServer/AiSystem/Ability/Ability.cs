﻿using GameServer.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AiSystem.Ability
{
    public abstract class Ability
    {
        public virtual void Start() { }

        public virtual void Update() { }

    }
}
