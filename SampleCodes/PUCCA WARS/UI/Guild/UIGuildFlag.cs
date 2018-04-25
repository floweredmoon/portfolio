using UnityEngine;
using UnityEngine.UI;

public class UIGuildFlag : MonoBehaviour
{
    [SerializeField]
    Image m_Flag;
    [SerializeField]
    Image m_Pattern;
    [SerializeField]
    Text m_Level;

    public Image flagImage
    {
        get
        {
            return m_Flag;
        }
    }

    void Reset()
    {
        if (m_Flag == null)
        {
            m_Flag = GetComponent<Image>();
        }
    }

    // Use this for initialization

    // Update is called once per frame

    public void SetGuildEmblem(string guildEmblem)
    {
        SetGuildEmblem(string.IsNullOrEmpty(guildEmblem) ? "ui_img_guild_empty" : "ui_guild_flag_big", guildEmblem);
    }

    public void SetGuildEmblem(string flagSpriteName, string guildEmblem)
    {
        m_Flag.sprite = TextureManager.GetSprite(SpritePackingTag.Guild, flagSpriteName);

        Color patternColor = Color.clear;
        int patternIndex = 0;
        if (UIUtility.TryParseGuildEmblem(guildEmblem, ref patternColor, ref patternIndex))
        {
            m_Pattern.color = patternColor;
            m_Pattern.sprite = TextureManager.GetGuildPatternSprite(patternIndex);
            m_Pattern.SetNativeSize();
            m_Pattern.enabled = true;
        }
        else
        {
            m_Pattern.enabled = false;
        }
    }

    public byte guildLevel
    {
        set
        {
            if (m_Level != null)
            {
                m_Level.text = Languages.LevelString(value);
            }
        }
    }
}
