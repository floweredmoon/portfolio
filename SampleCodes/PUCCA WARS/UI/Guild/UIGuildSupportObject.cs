using Common.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildSupportObject : MonoBehaviour
{
    public Text m_Title;
    public UIMiniCharCard m_MiniCharCard;
    public UISlider m_Slider;
    public Text m_Text;
    public Button m_Button;
    public Sprite m_ButtonAvailableSprite;
    public Sprite m_ButtonUnavailableSprite;

    Shadow[] m_Shadows;

    public long sequence
    {
        get;
        set;
    }

    int m_RequestCardIndex;

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

    void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.character.onSoulInfoUpdate += OnSoulInfoUpdate;
        }
    }

    void OnDisable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.character.onSoulInfoUpdate -= OnSoulInfoUpdate;
        }
    }

    void OnSoulInfoUpdate(long sequence, int soulIndex, int soulCount, int updateCount)
    {
        if (m_RequestCardIndex != soulIndex)
        {
            return;
        }

        int maxSoulCount = 0;
        CCardInfo cardInfo = Kernel.entry.character.FindCardInfo(soulIndex);
        if (cardInfo != null)
        {
            DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, soulIndex);
            if (card != null)
            {
                DB_CardLevelUp.Schema cardLevelUp = DB_CardLevelUp.Query(DB_CardLevelUp.Field.Grade_Type, card.Grade_Type,
                                                                         DB_CardLevelUp.Field.CardLevel, cardInfo.m_byLevel);
                if (cardLevelUp != null)
                {
                    maxSoulCount = cardLevelUp.Count;
                }
            }
        }

        m_Text.text = string.Format("{0}/{1}", soulCount, maxSoulCount);
    }

    public CGuildRequestCard guildRequestCard
    {
        set
        {
            if (value != null)
            {
                sequence = value.m_Sequence;
                m_RequestCardIndex = value.m_iRequestCardIndex;

                int soulCount = 0;
                CSoulInfo soulInfo = Kernel.entry.character.FindSoulInfo(value.m_iRequestCardIndex);
                if (soulInfo != null)
                {
                    soulCount = soulInfo.m_iSoulCount;
                }
                Result_Define.eResult result;
                bool isSupportableRequest = Kernel.entry.guild.IsSupportableRequest(value, out result);

                m_Title.text = Languages.ToString(TEXT_UI.CHAT_INFO, value.m_sRequesterName);
                m_MiniCharCard.SetCardInfo(value.m_iRequestCardIndex);
                m_Slider.maxValue = value.m_iMaxCardCount;
                m_Slider.value = value.m_iReceivedCardCount;
                // OnSoulInfoUpdate 함수 내부에서 sequence 값은 사용하지 않기 때문에, sequence 값은 0으로 호출합니다.
                OnSoulInfoUpdate(0, value.m_iRequestCardIndex, soulCount, 0);
                interactable = isSupportableRequest;
            }
        }
    }

    bool interactable
    {
        set
        {
            //m_Button.interactable = value;
            m_Button.image.sprite = value ? m_ButtonAvailableSprite : m_ButtonUnavailableSprite;

            Color outlineColor;
            Kernel.colorManager.TryGetColor(value ? "ui_button_02_outline" : "ui_button_04_outline", out outlineColor);
            Color shadowColor;
            Kernel.colorManager.TryGetColor(value ? "ui_button_02_shadow" : "ui_button_04_shadow", out shadowColor);
            if (m_Shadows == null || m_Shadows.Length == 0)
            {
                m_Shadows = m_Button.GetComponentsInChildren<Shadow>(true);
            }

            for (int i = 0; i < m_Shadows.Length; i++)
            {
                Shadow item = m_Shadows[i];
                if (item is Outline)
                {
                    item.effectColor = outlineColor;
                }
                else if (item is Shadow)
                {
                    item.effectColor = shadowColor;
                }
            }
        }
    }

    void OnClick()
    {
        if (Kernel.entry != null)
        {
            CGuildRequestCard guildRequestCard = Kernel.entry.guild.FindGuildRequestCard(sequence);
            if (guildRequestCard != null)
            {
                SoundDataInfo.ChangeUISound(UISOUND.UIS_CANCEL_01, m_Button.gameObject);

                Result_Define.eResult result;
                bool isSupportableRequest = Kernel.entry.guild.IsSupportableRequest(guildRequestCard, out result);

                if (isSupportableRequest)
                {
                    SoundDataInfo.RevertSound(m_Button.gameObject);
                    Kernel.entry.guild.onSupportResultForAnim += OnRevSupportResultForAnim;
                    Kernel.entry.guild.REQ_PACKET_CG_GUILD_SUPPORT_CARD_SYN(sequence);
                }
                else
                {
                    NetworkEventHandler.OnNetworkException(result);
                }
            }
        }
    }

    //** 결과 연출
    void OnRevSupportResultForAnim(List<Goods_Type> goodsList)
    {
        List<GoodsRewardAnimationData> newAnimlist = new List<GoodsRewardAnimationData>();

        for (int i = 0; i < goodsList.Count; i++)
        {
            GoodsRewardAnimationData newGoodsReward = new GoodsRewardAnimationData();
            newGoodsReward.m_eRewardGoodsType = goodsList[i];
            newGoodsReward.m_bUseBaseEndPosition = true;
            newAnimlist.Add(newGoodsReward);
        }

        Vector3 buttonWorldPos = m_Button.transform.position;

        // 보상 애니 시작
        UIHUD hud = Kernel.uiManager.Get<UIHUD>(UI.HUD, true, false);
        if (hud != null)
            hud.UseGoodsRewardAnimation(buttonWorldPos, newAnimlist); //작업

        if(Kernel.entry.guild.onSupportResultForAnim != null)
            Kernel.entry.guild.onSupportResultForAnim -= OnRevSupportResultForAnim;
    }

}
