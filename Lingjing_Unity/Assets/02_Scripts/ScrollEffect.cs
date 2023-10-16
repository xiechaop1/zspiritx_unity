using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 卷轴拖动效果
/// </summary>
public class ScrollEffect : MonoBehaviour
{
    public float speed=0.2f;
    public RectTransform drayScroll;
    public Animator ani;

    void Start()
    {
        
        ani.speed = 0;

    }
    public bool isDray;
    float step;
    Vector3 mouseOldPos;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseOldPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if (!isDray)
                isDray = GetFirstPickGameObject(Input.mousePosition) == drayScroll.gameObject;

            //单击卷轴可移动
            if (!isDray)
                return;

            Vector2 mousePos = Input.mousePosition - mouseOldPos;
            step += mousePos.x*speed * Time.deltaTime;

            if (mousePos.x>0)
            {
                if (step>=1)
                {
                    step = 1;
                }
            }
            else
            {
                if (step <= 0)
                {
                    step = 0;
                }
            }
            ani.Play("Test", 0, step);

            mouseOldPos = Input.mousePosition;

        }

        if (Input.GetMouseButtonUp(0))
        {
            isDray = false;
            ani.StopPlayback();
        }
    }


    #region UI射线检测
    /// <summary>
    /// 获取点击的UI类型Item
    /// </summary>
    /// <param name="position">点击屏幕坐标</param>
    /// <returns></returns>
    public GameObject GetFirstPickGameObject(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = position;
        //射线检测ui
        List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
        if (uiRaycastResultCache.Count > 0)
            return uiRaycastResultCache[0].gameObject;
        return null;
    }
    #endregion

}
