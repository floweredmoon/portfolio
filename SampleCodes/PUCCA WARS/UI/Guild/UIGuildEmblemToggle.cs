using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIGuildEmblemToggle : MonoBehaviour
{
    public Toggle m_Toggle;
    public Image m_Image;

    #region Properties
    public int patternIndex
    {
        get;
        private set;
    }

    public bool isOn
    {
        get
        {
            return m_Toggle ? m_Toggle.isOn : false;
        }

        set
        {
            if (m_Toggle)
            {
                if (m_Toggle.isOn)
                {
                    // Force invoke.
                    OnToggleValueChanged(true);
                }
                else
                {
                    m_Toggle.isOn = value;

                    // Force Invoke.
                    if (m_Toggle.onValueChanged.GetPersistentEventCount() <= 0)
                    {
                        OnToggleValueChanged(value);
                    }
                }
            }
        }
    }
    #endregion

    public delegate void OnEmblemPick(int patternIndex, Sprite patternSprite);
    public OnEmblemPick onEmblemPick;

    void Reset()
    {
        m_Toggle = GetComponent<Toggle>();
    }

    void Awake()
    {
        if (m_Toggle)
        {
            m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        m_Image.transform.localScale = new Vector3(.7f, .7f, 1f);
    }

    // Use this for initialization

    // Update is called once per frame

    public void SetEmblem(int patternIndex)
    {
        this.patternIndex = patternIndex;
        /*
        if (mark)
        {
            m_Image.sprite = TextureManager.GetGuildMarkSprite(index);
        }
        else
        {
            m_Image.sprite = TextureManager.GetGuildPatternSprite(index);
        }
        */

        m_Image.sprite = TextureManager.GetSprite(SpritePackingTag.Guild, string.Format("ui_guild_emblem_pattern_{0:00}", patternIndex + 1));
        m_Image.color = Color.white; //new Color32(171, 162, 153, 255);
        m_Image.SetNativeSize();
        //m_Image.rectTransform.sizeDelta = new Vector2(73.7f, 63.4f);

        m_Toggle.GetComponent<Image>().color = new Color32(107, 99, 97, 180);
        ((RectTransform)m_Toggle.transform).sizeDelta = new Vector2(90.8f, 86f); //m_Image.rectTransform.sizeDelta;
    }

    void OnToggleValueChanged(bool value)
    {
        if (!value)
        {
            return;
        }

        if (onEmblemPick != null)
        {
            onEmblemPick(patternIndex + 1, (m_Image != null) ? m_Image.sprite : null);
        }
    }
}
