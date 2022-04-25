using System.Collections;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;

public class ContainerCeil : ItemCeil
{
    protected override void OnClickHandler()
    {
        GameManager.Instance.ContainerPopup.AddToInventory(this);


    }

}
