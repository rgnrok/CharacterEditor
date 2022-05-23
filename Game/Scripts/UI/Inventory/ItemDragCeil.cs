using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ItemDragCeil : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform OldParent { get; private set; }
    public ItemCeil ParentCeil { get; private set; }

    private GameObject _dragCeilObject;
    private GameObject _dragPrefabObject;
    private Transform _canvas;

    private Item _item;
    private bool canDropOnGround;
    private ICommonLoader<GameObject> _gameObjectLoader;
    private IPathDataProvider _pathProvider;

    void Start()
    {
        _canvas = GameManager.Instance.Canvas;
        ParentCeil = GetComponentInParent<ItemCeil>();
        _gameObjectLoader = AllServices.Container.Single<ILoaderService>().GameObjectLoader;
        _pathProvider = AllServices.Container.Single<ILoaderService>().PathDataProvider;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OldParent = transform.parent;
        _dragCeilObject = Instantiate(gameObject, _canvas);
        _dragCeilObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        _item = ParentCeil.Item;

        if (_item != null)
        {
            _gameObjectLoader.LoadByPath(_pathProvider.GetPath(_item.Data.prefab), (path, prefab) =>
                {
                    _dragPrefabObject = Instantiate(prefab);
                    _dragPrefabObject.SetActive(false);
                });
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        canDropOnGround = false;
        if (EventSystem.current.IsPointerOverGameObject())
        {
            _dragCeilObject.transform.position = Input.mousePosition;
            _dragCeilObject.SetActive(true);
            if (_dragPrefabObject != null)_dragPrefabObject.SetActive(false);
        }
        else
        {
            if (_dragPrefabObject != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var mask = 1 << Constants.LAYER_GROUND;

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue, mask))
                {
                    _dragPrefabObject.transform.position = hit.point;
                    _dragCeilObject.SetActive(false);
                    if (_dragPrefabObject != null) _dragPrefabObject.SetActive(true);
                    canDropOnGround = true;
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        OnEndDragHandler();
    }

    public void OnEndDragHandler()
    {
        if (_dragCeilObject != null) Destroy(_dragCeilObject);
        if (_dragPrefabObject != null)
        {
            if (canDropOnGround && _item != null) DropOnGround(ParentCeil, _dragPrefabObject.transform.position);
            Destroy(_dragPrefabObject);

        }
    }

    protected abstract void DropOnGround(ItemCeil itemCeil, Vector3 position);
}
