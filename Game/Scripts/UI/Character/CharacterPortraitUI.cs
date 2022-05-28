using StatSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class CharacterPortraitUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Image portrait;
        [SerializeField] private Image selectedBorder;

        private Character _character;
        private Vital _health;

        public void Init(Character character)
        {
            _character = character;
            portrait.sprite = _character.Portrait;
            GameManager.Instance.OnChangeCharacter += OnChangeCharacterHandler;

            _health = _character.StatCollection.GetStat<Vital>(StatType.Health);
            _health.OnCurrentValueChange += OnHealthChanged;
        }

        public void Clean()
        {
            if (_health != null) _health.OnCurrentValueChange -= OnHealthChanged;
            GameManager.Instance.OnChangeCharacter -= OnChangeCharacterHandler;

            Destroy(gameObject);
        }

        private void OnHealthChanged()
        {
            hpSlider.value = (float) _health.StatCurrentValue / _health.StatValue;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var isDoubleClick = eventData.clickCount > 1;
            GameManager.Instance.SetCharacter(_character, isDoubleClick);
        }

        private void OnChangeCharacterHandler(Character ch)
        {
            var isVisible = ch != null && _character != null && ch.Guid == _character.Guid;
            selectedBorder.gameObject.SetActive(isVisible);
        }
    }
}
