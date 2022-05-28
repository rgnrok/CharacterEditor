using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ItemDragCeil : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform OldParent { get; private set; }
    public ItemCell ParentCell { get; private set; }

    private GameObject _dragCellObject;
    private GameObject _dragPrefabObject;
    private Transform _canvas;

    private Item _item;
    private bool canDropOnGround;
    private ICommonLoader<GameObject> _gameObjectLoader;
    private IPathDataProvider _pathProvider;

    void Start()
    {
        _canvas = GameManager.Instance.Canvas;
        ParentCell = GetComponentInParent<ItemCell>();
        _gameObjectLoader = AllServices.Container.Single<ILoaderService>().GameObjectLoader;
        _pathProvider = AllServices.Container.Single<ILoaderService>().PathDataProvider;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OldParent = transform.parent;
        _dragCellObject = Instantiate(gameObject, _canvas);
        _dragCellObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        _item = ParentCell.Item;

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
            _dragCellObject.transform.position = Input.mousePosition;
            _dragCellObject.SetActive(true);
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
                    _dragCellObject.SetActive(false);
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
        if (_dragCellObject != null) Destroy(_dragCellObject);
        if (_dragPrefabObject != null)
        {
            if (canDropOnGround && _item != null) DropOnGround(ParentCell, _dragPrefabObject.transform.position);
            Destroy(_dragPrefabObject);

        }
    }

    protected abstract void DropOnGround(ItemCell itemCell, Vector3 position);
}
