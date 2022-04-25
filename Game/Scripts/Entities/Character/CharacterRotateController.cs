using CharacterEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotateController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float mouseRotateSpeed = 0.3f;
    public bool isPreview;

    private bool _startMouseRotate;
    private Vector3 _prevMousePosition;
    private Transform _character;
    private ConfigManager _configManager;

    private Transform Character
    {
        get
        {
            return _configManager != null
                ? _configManager.ConfigData.CharacterObject.transform
                : GameManager.Instance.CurrentCharacter.GameObjectData.CharacterObject.transform;
        }
    }

    private Transform Preview
    {
        get { return GameManager.Instance.CurrentCharacter.GameObjectData.PreviewCharacterObject.transform.Find("Model"); }
    }

    void Awake()
    {
        _configManager = AllServices.Container.Single<ConfigManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _startMouseRotate = true;
        _prevMousePosition = Input.mousePosition;
        Cursor.visible = false;
        _character = isPreview ? Preview : Character;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _startMouseRotate = false;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (_startMouseRotate && Input.GetMouseButton(0) && _character != null)
        {
            _character.Rotate(new Vector3(0, (Input.mousePosition.x - _prevMousePosition.x) * mouseRotateSpeed, 0));
            _prevMousePosition = Input.mousePosition;
        }
    }
}
