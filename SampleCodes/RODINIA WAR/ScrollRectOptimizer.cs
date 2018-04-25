using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectOptimizer : MonoBehaviour
{
    public List<RectTransform> ignoreList;
    ScrollRect scrollRect;
    RectTransform viewRect;
    Vector2 prevPosition;
    int childCount;

    void Awake()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        if (scrollRect != null)
        {
            viewRect = scrollRect.transform as RectTransform;
        }
    }

    // Use this for initialization

    // Update is called once per frame
    void Update()
    {
        if (scrollRect != null)
        {
            if (scrollRect.content == null)
            {
                Debug.LogError(UIUtility.GetHierarchyString(transform) + "(ScrollRect) doesn't have a content.");
                enabled = false;
                return;
            }

            if (prevPosition != scrollRect.content.anchoredPosition)
            {
                prevPosition = scrollRect.content.anchoredPosition;
                Renewal();
            }
            else if (childCount != scrollRect.content.childCount)
            {
                childCount = scrollRect.content.childCount;
                Renewal();
            }
        }
    }

    void OnEnable()
    {
        if (scrollRect != null)
        {
            if (!scrollRect.enabled)
            {
                Debug.LogError(UIUtility.GetHierarchyString(transform) + "(ScrollRect) is disabled.");
                enabled = false;
            }
            else
            {
                // NOTE : Renewal을 직접 호출하지 않고, Update에서 호출되도록 초기화한다.
                prevPosition = Vector2.zero;
                childCount = 0;
                //Renewal();
            }
        }
        else
        {
            Debug.LogError(UIUtility.GetHierarchyString(transform) + "(ScrollRect) component couldn't be found.");
            enabled = false;
        }
    }

    public void Renewal()
    {
        // NullReferenceException
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        if (scrollRect == null)
        {
            Debug.LogError(UIUtility.GetHierarchyString(transform) + "(ScrollRect) component couldn't be found.");
            enabled = false;
            return;
        }

        if (scrollRect.content == null)
        {
            Debug.LogError(UIUtility.GetHierarchyString(transform) + "(ScrollRect) has no content.");
            enabled = false;
            return;
        }

        if (viewRect == null)
        {
            viewRect = scrollRect.transform as RectTransform;
        }

        List<GameObject> gameObjects = null;

        for (int i = 0; i < scrollRect.content.childCount; i++)
        {
            RectTransform child = (RectTransform)scrollRect.content.GetChild(i);

            if (child == null)
            {
                continue;
            }

            if (ignoreList.Contains(child))
            {
                continue;
            }

            Vector3 worldPosition = child.TransformPoint(0f, child.rect.yMin, 0f);
            Vector3 localPosition = viewRect.InverseTransformPoint(worldPosition);
            localPosition.y += child.rect.height;
            bool yMin = viewRect.rect.Contains(localPosition);
            worldPosition = child.TransformPoint(0f, child.rect.yMax, 0f);
            localPosition = viewRect.InverseTransformPoint(worldPosition);
            localPosition.y -= child.rect.height;
            bool yMax = viewRect.rect.Contains(localPosition);

            //child.gameObject.SetActive(yMin || yMax);
            if (yMin || yMax)
            {
                if (gameObjects == null)
                {
                    gameObjects = new List<GameObject>();
                }

                gameObjects.Add(child.gameObject);
            }
        }

        if (gameObjects == null)
        {
            return;
        }

        if (gameObjects.Count > 0)
        {
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                RectTransform child = (RectTransform)scrollRect.content.GetChild(i);

                if (child == null)
                {
                    continue;
                }

                if (ignoreList.Contains(child))
                {
                    continue;
                }

                child.gameObject.SetActive(gameObjects.Contains(child.gameObject));
            }
        }
        else
        {
            Debug.LogError(UIUtility.GetHierarchyString(transform) + "(ScrollRect) has children(" + scrollRect.content.childCount + "), but they are not visible.");

            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                RectTransform child = (RectTransform)scrollRect.content.GetChild(i);

                if (child == null)
                {
                    continue;
                }

                if (ignoreList.Contains(child))
                {
                    continue;
                }

                if (!child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
    }
}
