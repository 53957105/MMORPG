﻿using MMORPG.Common.Tool;
using GameServer.Network;
using GameServer.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.UserSystem
{
    /// <summary>
    /// 用户管理器
    /// 负责管理所有已登录用户
    /// </summary>
    public class UserManager : Singleton<UserManager>
    {
        private Dictionary<string, User> _userDict = new();

        UserManager() { }

        public void Start() { }

        public void Update()
        {
        }

        public User NewUser(NetChannel channel, string username, long userId)
        {
            var user = new User(channel, username, userId);
            _userDict.Add(username, user);
            
            user.Start();
            return user;
        }

        public User? GetUserByName(string username)
        {
            _userDict.TryGetValue(username, out var user);
            return user;
        }

        public void RemoveUser(User user)
        {
            _userDict.Remove(user.Username);
        }
    }
}
