using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UIScrollRectContentActivator : MonoBehaviour
{
    public ScrollRect m_ScrollRect;
    public bool m_Recursively;
    Dictionary<int, RectTransform> m_Ignores = new Dictionary<int, RectTransform>();

    Vector2 m_Min;
    Vector2 m_Max;
    float m_VerticalNormalizedPosition;
    int m_ChildCount;

    void Reset()
    {
        m_ScrollRect = GetComponent<ScrollRect>();
    }

    // Use this for initialization

    // Update is called once per frame
    void Update()
    {
        if ((m_VerticalNormalizedPosition != m_ScrollRect.verticalNormalizedPosition) ||
            (m_ChildCount != m_ScrollRect.content.childCount))
        {
            m_VerticalNormalizedPosition = m_ScrollRect.verticalNormalizedPosition;
            m_ChildCount = m_ScrollRect.content.childCount;

            for (int i = 0; i < m_ScrollRect.content.childCount; i++)
            {
                UpdateActive((RectTransform)m_ScrollRect.content.GetChild(i));
            }
        }
    }

    public void AddRange(List<RectTransform> rectTransformList)
    {
        if (rectTransformList != null && rectTransformList.Count > 0)
        {
            for (int i = 0; i < rectTransformList.Count; i++)
            {
                Add(rectTransformList[i]);
            }
        }
    }

    public void Add(RectTransform rectTransform)
    {
        if (rectTransform != null)
        {
            int hashCode = rectTransform.GetHashCode();
            if (!m_Ignores.ContainsKey(hashCode))
            {
                m_Ignores.Add(hashCode, rectTransform);
            }
        }
    }

    public void RemoveRange(List<RectTransform> rectTransformList)
    {
        if (rectTransformList != null && rectTransformList.Count > 0)
        {
            for (int i = 0; i < rectTransformList.Count; i++)
            {
                Remove(rectTransformList[i]);
            }
        }
    }

    public bool Remove(RectTransform rectTransform)
    {
        if (rectTransform != null)
        {
            return m_Ignores.Remove(rectTransform.GetHashCode());
        }

        return false;
    }

    void UpdateActive(RectTransform rectTransform)
    {
        if (m_Ignores.ContainsKey(rectTransform.GetHashCode()))
        {
            return;
        }

        bool contains = false, overlaps = false;
        Calculate(rectTransform, ref contains, ref overlaps);
        if ((contains || overlaps) && m_Recursively)
        {
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                UpdateActive((RectTransform)rectTransform.GetChild(i));
            }
        }

        rectTransform.gameObject.SetActive(contains || overlaps);
    }

    void Calculate(RectTransform rectTransform, ref bool contains, ref bool overlaps)
    {
        m_Min = m_ScrollRect.viewport.InverseTransformPoint(rectTransform.TransformPoint(rectTransform.rect.min));
        m_Max = m_ScrollRect.viewport.InverseTransformPoint(rectTransform.TransformPoint(rectTransform.rect.max));

        if (m_ScrollRect.vertical)
        {
            bool yMin = m_ScrollRect.viewport.rect.Contains(new Vector2(0f, m_Min.y));
            bool yMax = m_ScrollRect.viewport.rect.Contains(new Vector2(0f, m_Max.y));

            contains = (yMin || yMax);
            overlaps = (m_ScrollRect.viewport.rect.yMin > m_Min.y && m_ScrollRect.viewport.rect.yMax < m_Max.y);
        }
        else if (m_ScrollRect.horizontal)
        {
            bool xMin = m_ScrollRect.viewport.rect.Contains(new Vector2(m_Min.x, 0f));
            bool xMax = m_ScrollRect.viewport.rect.Contains(new Vector2(m_Max.x, 0f));

            contains = (xMin || xMax);
            overlaps = (m_ScrollRect.viewport.rect.xMin > m_Min.x && m_ScrollRect.viewport.rect.xMax < m_Max.y);
        }
    }
}
