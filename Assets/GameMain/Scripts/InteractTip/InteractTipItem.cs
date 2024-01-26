using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using Entity=GameMain.Entity;
using GameEntry=GameMain.GameEntry;

public class InteractTipItem : MonoBehaviour
{
    private Entity owner;
    private Canvas parentCanvas;
    public CanvasGroup canvasGroup;
    private int ownerId;
    public Entity Owner => owner;

    public Text textComp;

    public void Init(Entity owner, Canvas parentCanvas,string msg)
    {
        if (owner == null)
        {
            Log.Error("Owner is invalid.");
            return;
        }

        this.parentCanvas = parentCanvas;

        gameObject.SetActive(true);
        

        canvasGroup.alpha = 1f;
        if (this.owner != owner || ownerId != owner.Id)
        {
            textComp.text = msg;
            this.owner = owner;
            ownerId = owner.Id;
        }

        Refresh();

     
    }

    public void Refresh()
    {
        if (owner != null && Owner.Available && Owner.Id == ownerId)
        {
            Vector3 worldPosition = owner.CachedTransform.position ;
            Vector3 screenPosition = GameEntry.Scene.MainCamera.WorldToScreenPoint(worldPosition);

            Vector2 position;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)parentCanvas.transform, screenPosition,
                    parentCanvas.worldCamera, out position))
            {
                transform.localPosition = position;
            }
        }
    }

    public void Reset()
    {
        textComp.text = "";
        canvasGroup.alpha = 0;
        owner = null;
        ownerId = 0;
        
    }
}
