﻿using Common.Tool;
using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    public class SpaceMgr : Singleton<SpaceMgr>
    {
        public readonly int InitSpaceId = 1;

        private Dictionary<int, Space> _spaceSet = new();

        public SpaceMgr()
        {
            var noviceVillage = new Space()
            {
                SpaceId = 1,
                Name = "新手村",
                Description = "新手村",
                Music = 0,
            };
            _spaceSet[noviceVillage.SpaceId] = noviceVillage;
        }

        public Space? GetSpaceById(int spaceId)
        {
            if (!_spaceSet.ContainsKey(spaceId))
            {
                return null;
            }
            return _spaceSet[spaceId];
        }

        public Space? GetSpaceByName(string spaceName)
        {
            foreach (var space in _spaceSet)
            {
                if (space.Value.Name == spaceName)
                {
                    return space.Value;
                }
            }
            return null;
        }

    }
}