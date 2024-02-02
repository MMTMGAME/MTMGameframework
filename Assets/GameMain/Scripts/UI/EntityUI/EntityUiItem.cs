using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using Entity=GameMain.Entity;
using GameEntry=GameMain.GameEntry;

public abstract class EntityUiItem : MonoBehaviour
{
    private Canvas parentCanvas;
    public CanvasGroup canvasGroup;
    [SerializeField]
    private int ownerId;
    public Entity Owner { get; private set; }
    
    
    private RectTransform rectTransform;
    private void Awake()
    {
        canvasGroup = transform.GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }


    public virtual void Init(Entity owner, params object[] args)
    {
        if (owner == null)
        {
            Log.Error("Owner is invalid.");
            return;
        }

       

        gameObject.SetActive(true);
        StopAllCoroutines();

        canvasGroup.alpha = 1f;
        if (Owner != owner || ownerId != owner.Id)
        {

            Owner = owner;
            ownerId = owner.Id;
        }

        Refresh();


    }

    public virtual bool Refresh()
    {
        if (canvasGroup.alpha <= 0f || Owner==null || Owner.Available==false)
        {
            return false;
        }

        if (Owner != null && Owner.Available && Owner.Id == ownerId)
        {
            Vector3 worldPosition = Owner.CachedTransform.position ;
            Vector3 screenPosition = GameEntry.Scene.MainCamera.WorldToScreenPoint(worldPosition);

            // if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)parentCanvas.transform, screenPosition,
            //         parentCanvas.worldCamera, out var position))
            // {
            //     rectTransform.localPosition = position;
            // }

            transform.position = screenPosition;
        }

        return true;
    }

    public virtual void Reset()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 1f;
        gameObject.SetActive(false);
    }
}
