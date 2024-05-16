using System;
using Sirenix.OdinInspector;
using UnityEngine;

 namespace MMORPG.Game
{
    public enum HandleWeaponMode
    {
        LeftHand,
        RightHand
    }

    public class PlayerHandleWeapon : MonoBehaviour
    {
        [Title("Weapon")]
        public Weapon InitialWeapon;

        [ReadOnly]
        [ShowInInspector]
        public Weapon CurrentWeapon { get; private set; }
        [Title("Binding")]
        public Transform WeaponAttachment;
        public HandleWeaponMode HandleMode = HandleWeaponMode.RightHand;
        public Transform LeftHandTarget;
        public Transform RightHandTarget;

        [Button]
        private void UpdateWeaponAttachmentTransform()
        {
            if (WeaponAttachment)
            {
                switch (HandleMode)
                {
                    case HandleWeaponMode.LeftHand:
                        if (LeftHandTarget)
                            WeaponAttachment.SetPositionAndRotation(LeftHandTarget.position, LeftHandTarget.rotation);
                        break;
                    default:
                        if (RightHandTarget)
                            WeaponAttachment.SetPositionAndRotation(RightHandTarget.position, RightHandTarget.rotation);
                        break;
                }
            }
        }

        public delegate void WeaponChangedHandler(Weapon current, Weapon previous);
        public event WeaponChangedHandler OnWeaponChanged;

        public PlayerBrain Brain { get; private set; }

        private void Awake()
        {
            if (InitialWeapon)
            {
                ChangeWeapon(InitialWeapon);
            }
        }

        private void Update()
        {
            UpdateWeaponAttachmentTransform();
        }

        public void ChangeWeapon(Weapon newWeapon, bool combo = false)
        {
            if (CurrentWeapon)
            {
                CurrentWeapon.TurnWeaponOff();
                if (!combo)
                {
                    //TODO 销毁武器
                }
            }

            var tmp = CurrentWeapon;
            if (newWeapon != null)
            {
                InstantiateWeapon(newWeapon, combo);
            }
            else
            {
                CurrentWeapon = null;
            }
            OnWeaponChanged?.Invoke(newWeapon, tmp);
        }

        private void InstantiateWeapon(Weapon newWeapon, bool combo)
        {
            if (!combo)
            {
                CurrentWeapon = Instantiate(newWeapon,
                    WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset,
                    WeaponAttachment.transform.rotation);
            }

            CurrentWeapon.transform.parent = WeaponAttachment.transform;
            CurrentWeapon.transform.localPosition = newWeapon.WeaponAttachmentOffset;
            CurrentWeapon.Setup(Brain);
            CurrentWeapon.Initialize();
        }

        public void Setup(PlayerBrain brain)
        {
            Brain = brain;
        }
    }
}