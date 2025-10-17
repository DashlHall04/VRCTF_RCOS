using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))] // remove this line if you want to use it on UI elements
public class InteractiveButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Tooltip("If left empty, this GameObject's InterativeObjectTemplate will be used.")]
    public InterativeObjectTemplate target;

    void Awake()
    {
        if (target == null) target = GetComponent<InterativeObjectTemplate>();
        if (target == null)
            Debug.LogWarning("InteractiveButton: No InterativeObjectTemplate found. Assign 'target' in the Inspector.");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        target?.OnPointerEnter();   // calls your base template method (no args)
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target?.OnPointerExit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        target?.OnPointerClick();
    }
}