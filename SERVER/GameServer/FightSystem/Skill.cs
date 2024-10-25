﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MMORPG.Common.Proto.Fight;
using GameServer.AiSystem;
using GameServer.BuffSystem;
using GameServer.Manager;
using GameServer.Tool;
using Serilog;
using GameServer.EntitySystem;
using MMORPG.Common.Proto.Entity;
using MMORPG.Common.Tool;
using System.Diagnostics;

namespace GameServer.FightSystem
{
    public class Skill
    {
        public enum Stage
        {
            Idle = 0,
            Ready,  // 就绪
            Intonate,   // 吟唱
            Active,     // 已激活
            Cooling,   // 冷却中
        }

        public Actor OwnerActor { get; }
        public SkillDefine Define { get; }

        public Vector3 AreaOffset { get; }
        public float[] HitDelay { get; }
        public int[] BuffArr { get; }

        public Stage CurrentStage;

        private float _timeCounter;
        private int _hitDelayIndex;
        private CastTarget _castTarget;
        private Random _random = new();

        public Skill(Actor ownerActor, SkillDefine define)
        {
            OwnerActor = ownerActor;
            Define = define;

            AreaOffset = DataHelper.ParseVector3(define.AreaOffset);
            HitDelay = DataHelper.ParseFloats(Define.HitDelay);
            BuffArr = DataHelper.ParseIntegers(Define.Buff);
            if (HitDelay == null || HitDelay.Length == 0)
            {
                HitDelay = new[] { 0.0f };
            }
        }

        public void Start()
        {
        }

        public void Update()
        {
            if (CurrentStage == Stage.Idle) return;

            if (CurrentStage == Stage.Ready)
            {
                _timeCounter = 0;
                CurrentStage = Stage.Intonate;
            }
            else
            {
                _timeCounter += Time.DeltaTime;
            }
            

            // 如果是吟唱阶段并且吟唱已经结束
            if (CurrentStage == Stage.Intonate && _timeCounter >= Define.IntonateTime)
            {
                OnActive();
            }

            // 如果是技能激活阶段
            if (CurrentStage == Stage.Active)
            {
                OnRun();
                if (CurrentStage == Stage.Cooling)
                {
                    // 技能释放完成
                    OnFinish();
                }
            }

            // 如果是技能冷却阶段
            if (CurrentStage == Stage.Cooling && _timeCounter >= Define.Cd)
            {
                // 技能冷却完成
                OnCoolingEnded();
            } 
        }

        public CastResult CanCast(CastTarget castTarget)
        {
            if (CurrentStage == Stage.Cooling)
            {
                return CastResult.Cooling;
            }
            if (CurrentStage != Stage.Idle)
            {
                return CastResult.Running;
            }
            if (!OwnerActor.IsValid() || OwnerActor.IsDeath())
            {
                return CastResult.EntityDead;
            }

            if (OwnerActor.Mp < Define.Cost)
            {
                return CastResult.MpLack;
            }

            if (castTarget is CastTargetEntity target)
            {
                var targetActor = target.Entity as Actor;
                if (targetActor == null || !targetActor.IsValid() || targetActor.IsDeath())
                {
                    return CastResult.TargetInvaild;
                }
            }
            var dist = Vector2.Distance(OwnerActor.Position, castTarget.Position);
            if (dist > Define.SpellRange)
            {
                return CastResult.OutOfRange;
            }
            return CastResult.Success;
        }

        public CastResult Cast(CastTarget castTarget)
        {
            CurrentStage = Stage.Ready;
            _castTarget = castTarget;
            OwnerActor.Spell.CurrentRunSkill = this;

            OwnerActor.ChangeMp(-Define.Cost);

            return CastResult.Success;
        }

        /// <summary>
        /// 技能激活
        /// </summary>
        private void OnActive()
        {
            CurrentStage = Stage.Active;

            // 技能激活
            _timeCounter -= Define.IntonateTime;

            if (Define.MissileUnitId != 0)
            {
                var missileUnitDefine = DataManager.Instance.UnitDict[Define.MissileUnitId];

                var missile = OwnerActor.Map.MissileManager.NewMissile(Define.MissileUnitId,
                    OwnerActor.Position.ToVector3(), OwnerActor.Direction, 
                    Define.Area, missileUnitDefine.Speed, _castTarget,
                    entity =>
                    {
                        Log.Information("Missile命中");
                    });
            }
            else
            {
                _hitDelayIndex = 0;
            }
        }

        /// <summary>
        /// 技能每帧运行
        /// </summary>
        private void OnRun()
        {
            if (_hitDelayIndex < HitDelay.Length)
            {
                if (_timeCounter >= HitDelay[_hitDelayIndex])
                {
                    _timeCounter -= HitDelay[_hitDelayIndex];
                    // 命中延迟触发
                    OnHit(_castTarget);
                    ++_hitDelayIndex;
                }
            }
            else
            {
                CurrentStage = Stage.Cooling;
            }
        }

        public void OnHit(CastTarget castTarget)
        {
            if (Define.Area == 0)
            {
                if (castTarget is CastTargetEntity target)
                {
                    CauseDamage((Actor)target.Entity);
                }
            }
            else
            {
                var offset = VectorHelper.RotateVector2(AreaOffset.ToVector2(), -OwnerActor.Direction.Y);

                OwnerActor.Map.ScanEntityFollowing(OwnerActor, e =>
                {
                    switch (OwnerActor.EntityType)
                    {
                        case EntityType.Player:
                            if (e.EntityType != EntityType.Monster) return;
                            break;
                        case EntityType.Monster:
                            if (e.EntityType != EntityType.Player) return;
                            break;
                        default:
                            return;
                    }

                    float distance = Vector2.Distance(castTarget.Position + offset, e.Position);
                    if (distance > Define.Area) return;
                    var actor = e as Actor;
                    if (actor != null && actor.IsValid() && !actor.IsDeath())
                    {
                        var info = CauseDamage(actor);
                        if (!info.IsMiss)
                        {
                            foreach (var buff in BuffArr)
                            {
                                actor.BuffManager.AddBuff(buff, OwnerActor);
                            }
                        }
                    }
                });
            }
        }

        private DamageInfo CauseDamage(Actor target)
        {
            // 伤害 = 攻击 × (1 - 护甲 / (护甲 + 400 ＋ 85 × 等级))
            var a = OwnerActor.AttributeManager.Final;
            var b = target.AttributeManager.Final;
            var amount = 0f;

            var damageInfo = new DamageInfo()
            {
                AttackerInfo = new()
                {
                    AttackerId = OwnerActor.EntityId,
                    AttackerType = AttackerType.Skill,
                    SkillId = Define.ID
                },
                TargetId = target.EntityId,
                DamageType = DamageType.Physical,
            };

            var hitRate = a.HitRate - b.DodgeRate;
            var randHitRate = _random.NextSingle();
            if (hitRate >= randHitRate)
            {
                var ad = Define.Ad + a.Ad * Define.Adc;
                var ap = Define.Ap + a.Ap * Define.Apc;

                var ads = ad * (1 - b.Def / (b.Def + 400 + 85 * OwnerActor.Level));
                var aps = ap * (1 - b.Mdef / (b.Mdef + 400 + 85 * OwnerActor.Level));
                amount = ads + aps;

                float levelDifference = OwnerActor.Level - target.Level;
                amount *= (float)Math.Pow(2, levelDifference / 10);
                if (amount < 0) amount = 0;

                var randCri = _random.NextSingle();
                var cri = a.Cri;
                if (cri >= randCri)
                {
                    damageInfo.IsCrit = true;
                    amount *= a.Crd;
                }

                var offset = 0.95f + _random.NextSingle() * (1.05f - 0.95f);
                amount *= offset;
            }
            else
            {
                damageInfo.IsMiss = true;
                amount = 0;
            }
            damageInfo.Amount = (int)amount;
            target.OnHurt(damageInfo);
            return damageInfo;
        }

        /// <summary>
        /// 技能释放完成
        /// </summary>
        private void OnFinish()
        {
            OwnerActor.Spell.CurrentRunSkill = null;
        }

        /// <summary>
        /// 技能冷却完成
        /// </summary>
        private void OnCoolingEnded()
        {
            CurrentStage = Stage.Idle;
        }
    }
}
