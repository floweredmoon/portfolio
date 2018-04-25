using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Common.Packet;

public class UIGuildIntroduceEdit : UIObject
{
    public UIInputField m_InputField;
    public Button m_ConfirmButton;

    protected override void Awake()
    {
        base.Awake();

        m_ConfirmButton.onClick.AddListener(OnConfirmButtonClick);

        if (Kernel.entry != null)
        {
            m_InputField.characterLimit = Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Introduce_Length_Limit);
            m_InputField.lineLimit = 3;
        }
    }

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildBaseUpdate += OnGuildBaseUpdate;

            m_InputField.text = Kernel.entry.guild.guildIntroduce;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onGuildBaseUpdate -= OnGuildBaseUpdate;
        }
    }

    void OnGuildBaseUpdate(CGuildBase guildBase)
    {
        OnCloseButtonClick();
    }

    void OnConfirmButtonClick()
    {
        if (Kernel.entry != null)
        {
            string guildIntroduce = m_InputField.text;
            if (string.IsNullOrEmpty(guildIntroduce))
            {
                NetworkEventHandler.OnNetworkException(Result_Define.eResult.EMPTY_GUILD_INTRODUCE);
            }
            else
            {
                if (Kernel.entry.guild.guildIntroduce != guildIntroduce)
                {
                    Kernel.entry.guild.REQ_PACKET_CG_GUILD_UPDATE_GUILD_INTRODUCE_SYN(guildIntroduce);
                }
                else
                {
                    OnCloseButtonClick();
                }
            }
        }
    }
}
