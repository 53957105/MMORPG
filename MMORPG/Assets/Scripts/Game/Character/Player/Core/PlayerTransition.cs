using System;
using System.Linq;
using System.Reflection;
using Malee.List;
using QFramework;
using UnityEngine;

[Serializable]
public class PlayerTransition
{
    public string ConditionMethodName;
    public UnityEngine.Object ConditionMethodObject;
    public UnityEngine.Object AttachedObject;
    public string TrueStateName;
    public string FalseStateName;
#if UNITY_EDITOR
    public bool IsExpanding;
#endif

    public PlayerState TrueState { get; private set; }
    public PlayerState FalseState { get; private set; }

    public bool IsInitialized { get; private set; }
    public Func<bool> ConditionFunc { get; private set; }
    public PlayerBrain Owner { get; private set; }

    public void Initialize(PlayerBrain owner)
    {
        if (IsInitialized) return;
        Owner = owner;
        if (TrueStateName != string.Empty)
        {
            TrueState = owner.States.Find(x => x.Name == TrueStateName);
            Debug.Assert(TrueState != null);
        }

        if (FalseStateName != string.Empty)
        {
            FalseState = owner.States.Find(x => x.Name == FalseStateName);
            Debug.Assert(FalseState != null);
        }

        if (ConditionMethodName != string.Empty)
        {
            var methods = (
                from method in ConditionMethodObject.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                where method.HasAttribute<StateConditionAttribute>()
                select method).ToArray();
            var cond = methods.FirstOrDefault(x => x.Name == ConditionMethodName);
            Debug.Assert(cond != null);
            Debug.Assert(cond.ReturnType != typeof(bool), "Condition的返回值必须是bool!");
            ConditionFunc = () => (bool)cond.Invoke(ConditionMethodObject, null);
        }
    }


    public bool Evaluate()
    {
        if (ConditionFunc == null)
            return false;
        if (ConditionFunc())
        {
            if (TrueState != null)
            {
                Owner.ChangeState(TrueState);
                return true;
            }
        }
        else
        {
            if (FalseState != null)
            {
                Owner.ChangeState(FalseState);
                return true;
            }
        }
        return false;
    }
}

[Serializable]
public class PlayerTransitionArray : ReorderableArray<PlayerTransition> { }
