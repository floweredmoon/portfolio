using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScrollRect : ScrollRect
{
    #region Variables
    [SerializeField]
    bool m_UseActivityIndicator;
    [SerializeField]
    RectTransform m_ActivityIndicator;
    bool m_ActivityIndicatorActive;
    bool m_Available;
    float m_Spacing;
    Vector2 m_AnchoredPosition;
    bool m_IsDragEnd;
    #endregion

    #region Properties
    public bool useActivityIndicator
    {
        get
        {
            return m_UseActivityIndicator;
        }

        set
        {
            if (m_UseActivityIndicator != value)
            {
                m_UseActivityIndicator = value;
            }
        }
    }
    #endregion

    #region Delegates
    public delegate void OnInitializePotentialDragged(PointerEventData eventData);
    public OnInitializePotentialDragged onInitializePotentialDragged;

    public delegate void OnActivityIndicatorActiveChanged(bool activityIndicatorActive);
    public OnActivityIndicatorActiveChanged onActivityIndicatorActiveChanged;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        if (m_ActivityIndicator != null)
        {
            m_ActivityIndicator.gameObject.SetActive(false);
            m_Spacing = (m_ActivityIndicator.rect.size.y * .3f);

            UIScrollRectContentActivator comp = GetComponent<UIScrollRectContentActivator>();
            if (comp != null)
            {
                comp.Add(m_ActivityIndicator);
            }
        }
    }

    // Use this for initialization

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!m_UseActivityIndicator)
        {
            return;
        }

        if (viewport.rect.size.y < content.rect.size.y)
        {
            if (!m_Available && !m_ActivityIndicatorActive && verticalNormalizedPosition <= 0f && velocity.y < 1f)
            {
                m_Available = true;
                m_AnchoredPosition = content.anchoredPosition;
            }
            else if (m_Available && m_AnchoredPosition.y + m_Spacing <= content.anchoredPosition.y)
            {
                if (!m_ActivityIndicatorActive)
                {
                    m_ActivityIndicatorActive = true;

                    if (m_ActivityIndicator)
                    {
                        if (!m_ActivityIndicator.gameObject.activeSelf)
                        {
                            m_ActivityIndicator.gameObject.SetActive(true);
                        }

                        content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y + m_ActivityIndicator.rect.size.y);
                    }
                }

                if (m_IsDragEnd)
                {
                    m_Available = false;
                    m_IsDragEnd = false;

                    if (onActivityIndicatorActiveChanged != null)
                    {
                        onActivityIndicatorActiveChanged(true);
                    }
                }
            }
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        if (m_UseActivityIndicator && m_Available && m_ActivityIndicatorActive)
        {
            m_IsDragEnd = true;
        }
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);

        if (onInitializePotentialDragged != null)
        {
            onInitializePotentialDragged(eventData);
        }
    }

    public void SetActivity(bool activity)
    {
        if (activity)
        {

        }
        else
        {
            m_ActivityIndicatorActive = false;
            m_ActivityIndicator.gameObject.SetActive(false);
            content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y - m_ActivityIndicator.rect.size.y);
        }
    }
}
