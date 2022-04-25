using CharacterEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayBtn : MonoBehaviour, IPointerClickHandler {
    public void OnPointerClick(PointerEventData eventData)
    {
//        if (!SaveManager.Instance.HasSave("characterData")) return;
//        SceneManager.LoadScene("Play_Character_Scene");
    }
}
