// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Entity/ActorState.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace MMORPG.Common.Proto.Entity {

  /// <summary>Holder for reflection information generated from Entity/ActorState.proto</summary>
  public static partial class ActorStateReflection {

    #region Descriptor
    /// <summary>File descriptor for Entity/ActorState.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ActorStateReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChdFbnRpdHkvQWN0b3JTdGF0ZS5wcm90bxIaTU1PUlBHLkNvbW1vbi5Qcm90",
            "by5FbnRpdHkqRAoOQW5pbWF0aW9uU3RhdGUSCAoESURMRRAAEggKBE1PVkUQ",
            "ARIJCgVTS0lMTBACEggKBEhVUlQQAxIJCgVERUFUSBAEKlYKCkZsYWdTdGF0",
            "ZXMSCAoEWkVSTxAAEggKBFNUVU4QARIICgRST09UEAISCwoHU0lMRU5DRRAE",
            "Eg4KCklOVklOQ0lCTEUQCBINCglJTlZJU0lCTEUQEGIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::MMORPG.Common.Proto.Entity.AnimationState), typeof(global::MMORPG.Common.Proto.Entity.FlagStates), }, null, null));
    }
    #endregion

  }
  #region Enums
  public enum AnimationState {
    [pbr::OriginalName("IDLE")] Idle = 0,
    [pbr::OriginalName("MOVE")] Move = 1,
    [pbr::OriginalName("SKILL")] Skill = 2,
    [pbr::OriginalName("HURT")] Hurt = 3,
    [pbr::OriginalName("DEATH")] Death = 4,
  }

  public enum FlagStates {
    [pbr::OriginalName("ZERO")] Zero = 0,
    [pbr::OriginalName("STUN")] Stun = 1,
    [pbr::OriginalName("ROOT")] Root = 2,
    [pbr::OriginalName("SILENCE")] Silence = 4,
    [pbr::OriginalName("INVINCIBLE")] Invincible = 8,
    [pbr::OriginalName("INVISIBLE")] Invisible = 16,
  }

  #endregion

}

#endregion Designer generated code
