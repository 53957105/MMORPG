using MMORPG.Common.Tool;
using GameServer.Tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.BuffSystem;

namespace GameServer.Manager
{
    public class DataManager : Singleton<DataManager>
    {
        // 地图数据字典，用于存储所有地图的定义信息
        public Dictionary<int, MapDefine> MapDict;
        // 单位数据字典，用于存储所有单位的定义信息
        public Dictionary<int, UnitDefine> UnitDict;
        // 生物数据字典，用于存储所有生物的定义信息
        public Dictionary<int, SpawnDefine> SpawnDict;
        // 物品数据字典，用于存储所有物品的定义信息
        public Dictionary<int, ItemDefine> ItemDict;
        // 技能数据字典，用于存储所有技能的定义信息
        public Dictionary<int, SkillDefine> SkillDict;
        // NPC数据字典，用于存储所有NPC的定义信息
        public Dictionary<int, NpcDefine> NpcDict;
        // 对话数据字典，用于存储所有对话的定义信息
        public Dictionary<int, DialogueDefine> DialogueDict;
        // 任务数据字典，用于存储所有任务的定义信息
        public Dictionary<int, TaskDefine> TaskDict;
        // 奖励数据字典，用于存储所有奖励的定义信息
        public Dictionary<int, RewardDefine> RewardDict;
        // Buff数据字典，用于存储所有Buff的定义信息
        public Dictionary<int, BuffDefine> BuffDict;
    
        // 私有构造函数，防止外部实例化
        private DataManager() { }
    
        // 初始化数据管理器，加载所有类型的数据定义
        public void Start()
        {
            // 加载地图定义数据
            MapDict = Load<Dictionary<int, MapDefine>>("Data/Json/MapDefine.json");
            // 加载单位定义数据
            UnitDict = Load<Dictionary<int, UnitDefine>>("Data/Json/UnitDefine.json");
            // 加载生物定义数据
            SpawnDict = Load<Dictionary<int, SpawnDefine>>("Data/Json/SpawnDefine.json");
            // 加载物品定义数据
            ItemDict = Load<Dictionary<int, ItemDefine>>("Data/Json/ItemDefine.json");
            // 加载技能定义数据
            SkillDict = Load<Dictionary<int, SkillDefine>>("Data/Json/SkillDefine.json");
            // 加载NPC定义数据
            NpcDict = Load<Dictionary<int, NpcDefine>>("Data/Json/NpcDefine.json");
            // 加载对话定义数据
            DialogueDict = Load<Dictionary<int, DialogueDefine>>("Data/Json/DialogueDefine.json");
            // 加载任务定义数据
            TaskDict = Load<Dictionary<int, TaskDefine>>("Data/Json/TaskDefine.json");
            // 加载奖励定义数据
            RewardDict = Load<Dictionary<int, RewardDefine>>("Data/Json/RewardDefine.json");
            // 加载Buff定义数据
            BuffDict = Load<Dictionary<int, BuffDefine>>("Data/Json/BuffDefine.json");
        }
        

        public void Update() { }

        /// <summary>
        /// 使用指定的JSON路径加载并反序列化对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的对象类型。</typeparam>
        /// <param name="jsonPath">JSON文件的路径。</param>
        /// <returns>返回反序列化后的对象。</returns>
        private T Load<T>(string jsonPath)
        {
            // 加载指定路径的文件内容。
            var content = ResourceHelper.LoadFile(jsonPath);
            // 断言文件内容不为空，以确保后续操作的有效性。
            Debug.Assert(content != null);
            
            // 将文件内容反序列化为指定类型的对象。
            var obj = JsonConvert.DeserializeObject<T>(content);
            // 断言反序列化后的对象不为空，以确保对象成功创建。
            Debug.Assert(obj != null);
            
            // 返回反序列化后的对象。
            return obj;
        }
    }
}
