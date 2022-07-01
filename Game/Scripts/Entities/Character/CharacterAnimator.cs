using UnityEngine;

namespace CharacterEditor
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour, ICharacterAnimator
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int HitHash = Animator.StringToHash("Hit");

        private static readonly int AttackHash = Animator.StringToHash("Attack1");
        private static readonly int StartBattleHash = Animator.StringToHash("StartBattle");
        private static readonly int EndBattleHash = Animator.StringToHash("EndBattle");

        private static readonly int UseShieldHash = Animator.StringToHash("UseShield");
        private static readonly int Use2HandWeaponHash = Animator.StringToHash("Use2HandWeapon");
        private static readonly int Use2WeaponsHash = Animator.StringToHash("Use2Weapons");
        private static readonly int UseRightHandHash = Animator.StringToHash("UseRightHand");

        private Animator _animator;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
        }

        public void SetSpeed(float speed) => 
            _animator.SetFloat(SpeedHash, speed);

        public void Attack() => 
            _animator.SetTrigger(AttackHash);

        public void StartBattle() => 
            _animator.SetTrigger(StartBattleHash);

        public void EndBattle() => 
            _animator.SetTrigger(EndBattleHash);

        public void Die() => 
            _animator.SetTrigger(DieHash);

        public void Hit() => 
            _animator.SetTrigger(HitHash);

        public void UseShield(bool use) => 
            _animator.SetBool(UseShieldHash, use);

        public void Use2Weapons(bool use) => 
            _animator.SetBool(Use2WeaponsHash, use);

        public void Use2HandWeapon(bool use) => 
            _animator.SetBool(Use2HandWeaponHash, use);

        public void UseRightHand(bool use) => 
            _animator.SetBool(UseRightHandHash, use);


    }

    public interface ICharacterAnimator
    {
        void SetSpeed(float speed);
        void Attack();
        void StartBattle();
        void EndBattle();
        void Die();
        void Hit();
        void UseShield(bool use);
        void Use2Weapons(bool use);
        void Use2HandWeapon(bool use);
        void UseRightHand(bool use);
    }
}