using StatSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterBattlePortraitUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image portrait;
    [SerializeField] private Image selectedBorder;

    private IBattleEntity _battleEntity;
    private Vital _health;

    public void Init(IBattleEntity battleEntity)
    {
        _battleEntity = battleEntity;
        portrait.sprite = _battleEntity.Portrait;

        _health = _battleEntity.StatCollection.GetStat<Vital>(StatType.Health);
        _health.OnCurrentValueChange += OnHealthChanged;
        OnHealthChanged();
    }

    public void Clean()
    {
        if (_health != null) _health.OnCurrentValueChange -= OnHealthChanged;
        Destroy(gameObject);
    }

    private void OnHealthChanged()
    {
        hpSlider.value = (float)_health.StatCurrentValue / _health.StatValue;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      
    }

    public void SetSelected(bool isVisible)
    {
        selectedBorder.gameObject.SetActive(isVisible);
    }
}
