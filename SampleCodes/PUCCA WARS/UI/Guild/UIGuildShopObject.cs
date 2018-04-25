using Common.Packet;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildShopObject : MonoBehaviour
{
    public Button m_Button;
    public Text m_NameText;
    public Image m_IconImage;
    public UIMiniCharCard m_MiniCharCard;
    public Text m_PriceText;
    public Text m_CountText;

    int m_ItemIndex;

    public int itemIndex
    {
        get
        {
            return m_ItemIndex;
        }
    }

    string m_Name;
    int m_Price;

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
        m_Button.onClick.AddListener(OnButtonClick);
    }

    // Use this for initialization

    // Update is called once per frame

    void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onBuyResult += OnBuyResult;
        }
    }

    void OnDisable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onBuyResult -= OnBuyResult;
        }
    }

    void OnBuyResult(int itemIndex, byte buyCount)
    {
        if (int.Equals(m_ItemIndex, itemIndex))
        {
            m_Button.interactable = true;
        }
    }

    public void Renewal(int itemIndex, byte buyCount = 0)
    {
        m_ItemIndex = itemIndex;

        DB_GuildShopList.Schema guildShopList = DB_GuildShopList.Query(DB_GuildShopList.Field.Index, m_ItemIndex);
        if (guildShopList != null)
        {
            bool isLevelLimited = (Kernel.entry.guild.guildLevel < guildShopList.GulidLevel_Open);
            if (buyCount.Equals(0) && Kernel.entry != null)
            {
                buyCount = Kernel.entry.guild.GetBuyCount(m_ItemIndex);
            }

            switch (guildShopList.Goods_Type)
            {
                case Goods_Type.None:
                case Goods_Type.Card:
                    DBStr_Character.Schema table = DBStr_Character.Query(DBStr_Character.Field.Char_Index, guildShopList.Card_IndexID);
                    if (table != null)
                    {
                        m_NameText.text = table.StringData;
                    }

                    m_MiniCharCard.SetCardInfo(guildShopList.Card_IndexID);
                    break;
                default:
                    m_NameText.text = Languages.ToString(guildShopList.Goods_Type);
                    m_IconImage.sprite = TextureManager.GetGoodsTypeSprite(guildShopList.Goods_Type);
                    //m_IconImage.SetNativeSize();
                    break;
            }

            m_IconImage.gameObject.SetActive(!guildShopList.Goods_Type.Equals(Goods_Type.None) && !guildShopList.Goods_Type.Equals(Goods_Type.Card));
            m_MiniCharCard.gameObject.SetActive(guildShopList.Goods_Type.Equals(Goods_Type.None) || guildShopList.Goods_Type.Equals(Goods_Type.Card));
            m_PriceText.text = Languages.ToString<int>(guildShopList.Need_GuildPoint);

            if (isLevelLimited)
            {
                m_CountText.text = string.Format("{0}{1}", Languages.ToString(TEXT_UI.LV), guildShopList.GulidLevel_Open);
            }
            else
            {
                int maxCount = guildShopList.Buylimit_Base + ((Kernel.entry.guild.guildLevel - guildShopList.GulidLevel_Open) * guildShopList.Buylimit_Add);
                m_CountText.text = Languages.ToString(TEXT_UI.SHOP_LIMIT_BUY, maxCount - buyCount);
            }

            m_Name = m_NameText.text;
            m_Price = guildShopList.Need_GuildPoint;
        }
    }

    void OnButtonClick()
    {
        if (Kernel.entry != null)
        {
            DB_GuildShopList.Schema guildShopList = DB_GuildShopList.Query(DB_GuildShopList.Field.Index, m_ItemIndex);
            if (guildShopList != null)
            {
                SoundDataInfo.ChangeUISound(UISOUND.UIS_CANCEL_01, m_Button.gameObject);

                int MaxCount = guildShopList.Buylimit_Base + ((Kernel.entry.guild.guildLevel - guildShopList.GulidLevel_Open) * guildShopList.Buylimit_Add);

                if (MaxCount <= Kernel.entry.guild.GetBuyCount(m_ItemIndex))
                {
                    NetworkEventHandler.OnNetworkException(Result_Define.eResult.NOT_ENOUGH_BUY_COUNT);
                }
                else if (guildShopList.GulidLevel_Open > Kernel.entry.guild.guildLevel)
                {
                    NetworkEventHandler.OnNetworkException(Result_Define.eResult.NOT_MATCHED_GUILD_LEVEL);
                }
                else if (guildShopList.Need_GuildPoint > Kernel.entry.account.guildPoint)
                {
                    NetworkEventHandler.OnNetworkException(Result_Define.eResult.NOT_ENOUGH_GUILD_POINT);
                }
                else
                {
                    SoundDataInfo.RevertSound(m_Button.gameObject);
                    PopupBuyInfo.OpenPopupBuyInfo(m_ItemIndex, m_Name, m_Price.ToString(), Goods_Type.GuildPoint, OnBuyResult);
                }
            }
        }
    }

    void OnBuyResult(int itemIndex)
    {
        if (m_ItemIndex != itemIndex)
        {
            return;
        }

        Kernel.entry.guild.REQ_PACKET_CG_GUILD_BUY_GUILD_SHOP_ITEM_SYN(m_ItemIndex);
        m_Button.interactable = false;
    }
}
