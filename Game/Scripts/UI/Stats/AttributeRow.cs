using CharacterEditor;
using StatSystem;
using UnityEngine;
using UnityEngine.UI;

public class AttributeRow : MonoBehaviour
{
    public StatType statType;
    public Text statName;
    public Text statValue;

    private Character _currentCharacter;
    private Vital _vital;

    void Start()
    {
        GameManager.Instance.OnChangeCharacter += OnChangeCharacterHandler;
    }

    private void OnChangeCharacterHandler(Character character)
    {
        if (_currentCharacter != null && character != null && _currentCharacter.Guid.Equals(character.Guid)) return;

        if (_currentCharacter != null) _currentCharacter.StatCollection.UpdateStats -= UpdateStatsHandler;
        if (_vital != null) _vital.OnCurrentValueChange -= OnCurrentValueChangeHandler;

        _currentCharacter = character;
        if (_currentCharacter == null) return;

        _currentCharacter.StatCollection.UpdateStats += UpdateStatsHandler;
        _vital = _currentCharacter.StatCollection.GetStat<Vital>(statType);
        if (_vital != null) _vital.OnCurrentValueChange += OnCurrentValueChangeHandler;

        UpdateStatsHandler();
    }

    private void UpdateStatsHandler()
    {
        var stat = _currentCharacter.StatCollection.GetStat(statType);
        if (stat != null)
        {
            statName.text = stat.StatName;
            statValue.text = _vital != null 
                ? $"{_vital.StatCurrentValue} / {_vital.StatValue}" 
                : stat.StatValue.ToString();
        } 
    }

    private void OnCurrentValueChangeHandler()
    {
        statValue.text = _vital.StatCurrentValue.ToString();
    }
}
