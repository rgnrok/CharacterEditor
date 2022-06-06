using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.UI;

public class SkinTexturesGroup : MonoBehaviour
{
    [SerializeField] private RawImage skinImage;
    [SerializeField] private RawImage armorImage;

    private ICharacterEquipItemService _equipItemService;

    private void Awake()
    {
        _equipItemService = AllServices.Container.Single<ICharacterEquipItemService>();
        if (_equipItemService != null)
        {
            _equipItemService.OnTexturesChanged += UpdateTextures;
        }
    }

    private void OnDestroy()
    {
        if (_equipItemService != null)
        {
            _equipItemService.OnTexturesChanged -= UpdateTextures;
        }
    }

    private void UpdateTextures()
    {
        skinImage.texture = _equipItemService.GetCurrentCharacterTexture();
        armorImage.texture = _equipItemService.GetCurrentCharacterArmorTexture();
    }
}
