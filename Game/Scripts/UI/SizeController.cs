using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeController : MonoBehaviour
{
    enum SizeType
    {
        Content,
        Constant
    }

    [SerializeField]
    private SizeType WidthSizeType;
    [SerializeField]
    private SizeType HeightSizeType;
    [SerializeField]
    private int AdditionalHeight;

    private RectTransform _content;
    private RectTransform _rectTransform;


    protected virtual RectTransform GetContent()
    {
        if (_content == null)
        {
            var content = transform.Find("Content");
            if (content != null) _content = content.GetComponent<RectTransform>();
        }

        return _content;
    }

    public void Recalculate()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

        switch (HeightSizeType)
        {
            case SizeType.Constant:
                    break;
            case SizeType.Content:
                var content = GetContent();
                if (content != null)
                {
                    float height = AdditionalHeight;
                    for (int i = 0; i < content.childCount; i++)
                    {
                        height += content.GetChild(i).GetComponent<RectTransform>().rect.height;
                    }
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                }
                break;
        }

        switch (WidthSizeType)
        {
            case SizeType.Constant:
                break;
            case SizeType.Content:
                break;
        }
    }
}
