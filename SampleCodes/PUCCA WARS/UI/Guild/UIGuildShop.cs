using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.Packet;
using System;

public class UIGuildShop : UIObject
{
    private TextLevelMaxEffect m_LevelMaxEffect;

    public ScrollRect m_ScrollRect;
    List<UIGuildShopObject> m_GuildShopObjectList = new List<UIGuildShopObject>();
    public UIGuildFlag m_GuildFlag;
    public Text m_GuildNameText;
    public Text m_GuildLevelText;
    public Button m_HelpButton;
    public UISlider m_GuildExpSlider;
    public Button m_DonationButton;
    public Text m_RemainTimeText;
    public GameObjectPool m_GuildShopObjectPool;

    public RectOffset m_Padding;
    public Vector2 m_Spacing;
    public Vector2 m_CellSize;
    public int m_ConstraintCount;

    // Use this for initialization

    // Update is called once per frame

    protected override void Awake()
    {
        m_HelpButton.onClick.AddListener(OnHelpButtonClick);
        m_DonationButton.onClick.AddListener(OnDonationButtonClick);
    }

    protected override void Update()
    {
        TimeSpan ts = DateTime.Today.AddDays(1) - TimeUtility.currentServerTime;
        m_RemainTimeText.text = Languages.ToString(TEXT_UI.REMAINING, string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds));
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onBuyResult += OnGuildShopBuyResult;
            Kernel.entry.guild.onBuyCountUpdate += OnBuyCountUpdate;
            Kernel.entry.guild.onGuildBaseUpdate += OnGuildBaseUpdate;
            Kernel.entry.guild.onGuildLevelUpdate += OnGuildLevelUpdate;

            OnGuildBaseUpdate(Kernel.entry.guild.guildBase);
            ItemListUpdate();
        }

        if (UIHUD.instance)
        {
            UIHUD.instance.onBackButtonClicked = OnBackButtonClicked;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onBuyResult -= OnGuildShopBuyResult;
            Kernel.entry.guild.onBuyCountUpdate -= OnBuyCountUpdate;
            Kernel.entry.guild.onGuildBaseUpdate -= OnGuildBaseUpdate;
            Kernel.entry.guild.onGuildLevelUpdate -= OnGuildLevelUpdate;
        }
    }

    void OnGuildShopBuyResult(int itemIndex, byte buyCount)
    {
        UIStrangeShopDirector strangeShopDirector = Kernel.uiManager.Get<UIStrangeShopDirector>(UI.StrangeShopDirector, true, false);
        if (strangeShopDirector != null)
        {
            strangeShopDirector.SetGuildShopItem(itemIndex, 1);
            Kernel.uiManager.Open(UI.StrangeShopDirector);
        }
    }

    void OnGuildLevelUpdate(byte guildLevel)
    {
        ItemListUpdate();
    }

    void OnGuildBaseUpdate(CGuildBase guildBase)
    {
        if (guildBase != null)
        {
            m_GuildFlag.SetGuildEmblem(guildBase.m_sGuildEmblem);
            m_GuildNameText.text = guildBase.m_sGuildName;
            m_GuildLevelText.text = string.Format("{0}{1}", Languages.ToString(TEXT_UI.LV), guildBase.m_byGuildLevel);

            int maxExp = 0;
            DB_GuildLevel.Schema guildLevel = DB_GuildLevel.Query(DB_GuildLevel.Field.GulidLevel, guildBase.m_byGuildLevel);
            if (guildLevel != null)
            {
                maxExp = guildLevel.Max_Exp;
            }

            m_GuildExpSlider.maxValue = maxExp;
            m_GuildExpSlider.value = guildBase.m_GuildExp;

            if (m_GuildLevelText != null)
                m_LevelMaxEffect = m_GuildLevelText.GetComponent<TextLevelMaxEffect>();

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.MaxValue = Kernel.entry.guild.MaxLevel;

            if (m_LevelMaxEffect != null)
                m_LevelMaxEffect.Value = guildBase.m_byGuildLevel;

        }
    }

    void OnBuyCountUpdate(int itemIndex, byte buyCount)
    {
        UIGuildShopObject guildShopObject = m_GuildShopObjectList.Find(item => int.Equals(item.itemIndex, itemIndex));
        if (guildShopObject)
        {
            guildShopObject.Renewal(itemIndex, buyCount);
        }
        else
        {
            Debug.LogError(itemIndex);
        }
    }

    void BuildLayout()
    {
        float x = m_Padding.left;
        float y = -m_Padding.top;
        for (int i = 0; i < m_GuildShopObjectList.Count; i++)
        {
            if (i > 0 && int.Equals(i % m_ConstraintCount, 0))
            {
                x = m_Padding.left;
                y = y - m_CellSize.y - m_Spacing.y;
            }

            UIGuildShopObject guildShopObject = m_GuildShopObjectList[i];
            if (guildShopObject)
            {
                guildShopObject.rectTransform.anchoredPosition = new Vector2(x, y);
            }

            x = x + m_CellSize.x + m_Spacing.x;
        }

        float rowCount = Mathf.Ceil((float)m_GuildShopObjectList.Count / (float)m_ConstraintCount);
        x = (m_ConstraintCount * m_CellSize.x) + ((m_ConstraintCount - 1f) * m_Spacing.x) + m_Padding.left + m_Padding.right;
        y = (rowCount * m_CellSize.y) + ((rowCount - 1f) * m_Spacing.y) + m_Padding.top + m_Padding.bottom;

        m_ScrollRect.content.sizeDelta = new Vector2(x, y);
    }

    void ItemListUpdate()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        //OnGuildBaseUpdate(Kernel.entry.guild.guildBase);

        #region
        for (int i = 0; i < m_GuildShopObjectList.Count; i++)
        {
            UIGuildShopObject guildShopObject = m_GuildShopObjectList[i];
            if (guildShopObject != null)
            {
                guildShopObject.gameObject.SetActive(false);
                m_GuildShopObjectPool.Push(guildShopObject.gameObject);
                UIUtility.SetParent(guildShopObject.transform, transform);
            }
        }
        m_GuildShopObjectList.Clear();
        #endregion

        #region
        List<DB_GuildShopList.Schema> table = DB_GuildShopList.instance.schemaList;
        if (table != null && table.Count > 0)
        {
            List<Goods_Type> goodsTypeList = new List<Goods_Type>();
            List<int> cardIndexList = new List<int>();
            for (int i = 0; i < table.Count; i++)
            {
                DB_GuildShopList.Schema schema = table[i];
                if (schema != null)
                {
                    if (/*schema.GulidLevel_Open > guildLevel ||*/ goodsTypeList.Contains(schema.Goods_Type) || cardIndexList.Contains(schema.Card_IndexID))
                    {
                        continue;
                    }

                    UIGuildShopObject guildShopObject = m_GuildShopObjectPool.Pop<UIGuildShopObject>(); //Instantiate<UIGuildShopObject>(m_GuildShopObject);
                    if (guildShopObject)
                    {
                        guildShopObject.gameObject.SetActive(true);
                        UIUtility.SetParent(guildShopObject.transform, m_ScrollRect.content);
                        guildShopObject.Renewal(schema.Index);

                        m_GuildShopObjectList.Add(guildShopObject);

                        if (schema.Goods_Type != Goods_Type.None && schema.Goods_Type != Goods_Type.Card)
                        {
                            goodsTypeList.Add(schema.Goods_Type);
                        }
                        else
                        {
                            cardIndexList.Add(schema.Card_IndexID);
                        }
                    }
                }
            }
        }
        #endregion

        BuildLayout();
    }

    bool OnBackButtonClicked()
    {
        OnCloseButtonClick();

        return true;
    }

    void OnHelpButtonClick()
    {
        if (Kernel.uiManager)
        {
            Kernel.uiManager.Get(UI.GuildShopHelp, true, false);
            Kernel.uiManager.Open(UI.GuildShopHelp);
        }
    }

    void OnDonationButtonClick()
    {
        if (Kernel.uiManager)
        {
            Kernel.uiManager.Get(UI.GuildDonation, true, false);
            Kernel.uiManager.Open(UI.GuildDonation);
        }
    }
}
