using Common.Packet;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildDonationMemberObject : MonoBehaviour
{
    private TextLevelMaxEffect m_LevelMaxEffect;

    public Button m_Button;
    public Image m_RankImage;
    public Text m_RankText;
    public Image m_BackgroundImage;
    public Image m_PortraitImage;
    public Image m_FrameImage;
    public Text m_LevelText;
    public Text m_NameText;
    public Text m_DonationExpText;

    long m_AID;

    #region RectTransform
    RectTransform m_RectTransform;

    public RectTransform rectTransform
    {
        get
        {
            if (!m_RectTransform)
            {
                m_RectTransform = transform as RectTransform;
            }

            return m_RectTransform;
        }
    }
    #endregion

    void Awake()
    {
        m_Button.onClick.AddListener(OnClick);
    }

    // Use this for initialization

    // Update is called once per frame

    public void SetGuildMember(int rank, CGuildMember guildMember)
    {
        bool highRank = (rank <= 3);
        if (highRank)
        {
            m_RankImage.sprite = TextureManager.GetSprite(SpritePackingTag.Guild, string.Format("{0}st_Class", rank));
        }
        else
        {
            m_RankText.text = rank.ToString();
        }

        m_RankImage.gameObject.SetActive(highRank);
        m_RankText.gameObject.SetActive(!highRank);

        if (guildMember != null)
        {
            m_AID = guildMember.m_AID;

            DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, guildMember.m_iLeaderCardIndex);
            if (card != null)
            {
                m_BackgroundImage.sprite = TextureManager.GetGradeTypeBackgroundSprite(card.Grade_Type);
                m_PortraitImage.sprite = TextureManager.GetPortraitSprite(card.IdentificationName);
                m_FrameImage.sprite = TextureManager.GetGradeTypeFrameSprite(card.Grade_Type);
            }

            m_LevelText.text = string.Format("{0}{1}", Languages.ToString(TEXT_UI.LV), guildMember.m_byAccountLevel);
            m_NameText.text = guildMember.m_sUserName;
            m_DonationExpText.text = string.Format("+{0}", Languages.ToString<int>(guildMember.m_iDonatedGuildExp));

            if (m_LevelText != null)
                m_LevelMaxEffect = m_LevelText.GetComponent<TextLevelMaxEffect>();

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.MaxValue = Kernel.entry.data.GetValue<byte>(Const_IndexID.Const_Account_Level_Limit);

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.Value = guildMember.m_byAccountLevel;

        }
    }

    void OnClick()
    {
        Kernel.entry.ranking.REQ_PACKET_CG_READ_DETAIL_USER_INFO_SYN(m_AID);
    }
}
