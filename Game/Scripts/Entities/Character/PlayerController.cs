using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Quaternion initRotation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        initRotation = transform.rotation;
    }

    void Update()
    {
        if (!animator.GetBool("jumpComplete"))
            return;

        UpdateMove();
    }


    private void UpdateMove()
    {
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("jump");
            animator.SetBool("jumpComplete", false);
        }

        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        if (Mathf.Abs(h) > 0.001f)
            v = 0;

        if (Mathf.Abs(v) < 0.001 && Mathf.Abs(h) < 0.001) return;

        var uiObject = EventSystem.current.currentSelectedGameObject;
        if (uiObject != null && uiObject.GetComponent<InputField>() != null) return;

        if (h > 0.5f)
            transform.rotation = Quaternion.Euler(initRotation.eulerAngles + new Vector3(0, -90, 0));

        if (h < -0.5f)
            transform.rotation = Quaternion.Euler(initRotation.eulerAngles + new Vector3(0, 90, 0));

        if (v > 0.5f)
            transform.rotation = Quaternion.Euler(initRotation.eulerAngles + new Vector3(0, -180, 0));

        if (v < -0.5f)
            transform.rotation = Quaternion.Euler(initRotation.eulerAngles);

        animator.SetFloat("speed", Mathf.Max(Mathf.Abs(h), Mathf.Abs(v)));
    }
}