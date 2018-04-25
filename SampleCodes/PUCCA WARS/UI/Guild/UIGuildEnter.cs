using Common.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildEnter : UIObject
{
    public Button m_CreateButton;
    public Text m_CreateButtonText;
    public Text m_GoldText;
    public List<Graphic> m_GrayscaleGraphics;

    bool goldAvailable
    {
        get;
        set;
    }

    protected override void Awake()
    {
        base.Awake();

        m_CreateButton.onClick.AddListener(OnCreateButtonClick);
    }

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildCreateResult += OnGuildCreateResult;
            Kernel.entry.guild.onJoinResult += OnJoinResult;
            Kernel.entry.account.onGoodsUpdate += OnGoodsUpdate;

            RefreshCreateButton();
        }


        //튜토리얼.
        if(Kernel.entry.tutorial.TutorialActive && Kernel.entry.tutorial.WaitSeq == 1000)
        {
            Kernel.entry.tutorial.onSetNextTutorial();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildCreateResult -= OnGuildCreateResult;
            Kernel.entry.guild.onJoinResult -= OnJoinResult;
            Kernel.entry.account.onGoodsUpdate -= OnGoodsUpdate;
        }
    }

    void OnJoinResult(long gid, string guildName, bool isJoin)
    {
        if (isJoin)
        {
            Kernel.sceneManager.LoadScene(Scene.Guild);
        }
    }

    void OnGuildCreateResult()
    {
        Kernel.sceneManager.LoadScene(Scene.Guild);
    }

    void OnGoodsUpdate(int friendship, int gold, int heart, int ranking, int ruby, int star, int guildPoint, int revengePoint, int smilePoint)
    {
        RefreshCreateButton();
    }

    void RefreshCreateButton()
    {
        goldAvailable = (Kernel.entry.account.gold >= Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Making_Cost_Gold));

        Color outlineColor;
        Color shadowColor;
        if (goldAvailable)
        {
            Kernel.colorManager.TryGetColor("button06_Outline", out outlineColor);
            Kernel.colorManager.TryGetColor("button06_shadow", out shadowColor);

            SetColor(m_GoldText, Color.white, outlineColor, shadowColor);
            SetColor(m_CreateButtonText, Color.white, outlineColor, shadowColor);
        }
        else
        {
            Kernel.colorManager.TryGetColor("ui_button_04_outline", out outlineColor);
            Kernel.colorManager.TryGetColor("ui_button_04_shadow", out shadowColor);

            SetColor(m_GoldText, Color.red, Color.black, Color.black);
            SetColor(m_CreateButtonText, Color.white, outlineColor, shadowColor);
        }

        for (int i = 0; i < m_GrayscaleGraphics.Count; i++)
        {
            m_GrayscaleGraphics[i].material = goldAvailable ? null : UIUtility.grayscaleMaterial;
        }

        m_GoldText.text = Languages.ToString<int>(Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Making_Cost_Gold));
    }

    void SetColor(Text text, Color textColor, Color outlineColor, Color shadowColor)
    {
        text.color = textColor;

        Shadow[] items = text.GetComponentsInChildren<Shadow>(true);
        for (int i = 0; i < items.Length; i++)
        {
            Shadow item = items[i];
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

    void OnCreateButtonClick()
    {
        SoundDataInfo.ChangeUISound(UISOUND.UIS_CANCEL_01, m_CreateButton.gameObject);

        if (!goldAvailable)
        {
            NetworkEventHandler.OnNetworkException(Result_Define.eResult.NOT_ENOUGH_GOLD);
        }
        else
        {
            SoundDataInfo.RevertSound(m_CreateButton.gameObject);
            Kernel.uiManager.Open(UI.GuildEditor);
        }
    }
}
