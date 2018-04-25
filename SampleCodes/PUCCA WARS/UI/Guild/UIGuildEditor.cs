using Common.Packet;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildEditor : UIObject
{
    public Image m_PatternImage;
    public UIGuildColorEditor m_ColorEditor;
    public UIGuildEmblemEditor m_EmblemEditor;
    public InputField m_NameInputField;
    public UIInputField m_GreetingInputField;
    public Image m_HelpImage;
    public Toggle m_Toggle;
    public Text m_ToggleText;
    public Button m_ConfirmButton;

    int m_PickedPatternIndex;
    Color m_PickedColor;

    protected override void Awake()
    {
        base.Awake();

        m_PatternImage.transform.localScale = new Vector3(.8f, .8f, 1f);
        m_ColorEditor.Initialize();
        m_ColorEditor.onColorPick += OnColorPick;
        // Force invoke for initialize.
        m_EmblemEditor.OnToggleValueChange(true);
        m_EmblemEditor.onRandomButtonClicked += OnRandomButtonClicked;
        m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        m_ConfirmButton.onClick.AddListener(OnConfirmButtonClick);

        for (int i = 0; i < m_EmblemEditor.m_EmblemToggles.Count; i++)
        {
            m_EmblemEditor.m_EmblemToggles[i].onEmblemPick += OnEmblemPick;
        }

        OnToggleValueChanged(m_Toggle.isOn);

        if (Kernel.entry != null)
        {
            m_NameInputField.characterLimit = Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Name_Length_Limit);
            m_GreetingInputField.characterLimit = Kernel.entry.data.GetValue<int>(Const_IndexID.Const_Guild_Introduce_Length_Limit);
            m_GreetingInputField.lineLimit = 3;
        }
    }

    // Use this for initialization

    // Update is called once per frame

    protected override void OnEnable()
    {
        base.OnEnable();

        m_ColorEditor.Pick(0);
        m_EmblemEditor.Pick(0);
    }

    void OnRandomButtonClicked()
    {
        int index = Random.Range(0, m_EmblemEditor.count - 1);
        m_EmblemEditor.Pick(index);

        index = Random.Range(0, m_ColorEditor.count - 1);
        m_ColorEditor.Pick(index);
    }

    void OnEmblemPick(int patternIndex, Sprite patternSprite)
    {
        Debug.LogFormat("patternIndex : {0}, patternSprite : {1}", patternIndex, (patternSprite != null) ? patternSprite.name : "null");
        m_PickedPatternIndex = patternIndex;
        m_PatternImage.sprite = patternSprite;
        m_PatternImage.SetNativeSize();
    }

    void OnColorPick(Color patternColor)
    {
        Debug.LogFormat("patternColor : {0}", patternColor);
        m_PickedColor = patternColor;
        m_PatternImage.CrossFadeColor(patternColor, 0f, true, false);
    }

    void OnConfirmButtonClick()
    {
        if (Kernel.entry == null)
        {
            return;
        }

        string guildName = m_NameInputField.text; // Filtering
        if (string.IsNullOrEmpty(guildName))
        {
            NetworkEventHandler.OnNetworkException(Result_Define.eResult.EMPTY_CREATE_GUILD_NAME);
        }
        else
        {
            string guildIntroduce = m_GreetingInputField.text; // Filtering
            if (string.IsNullOrEmpty(guildIntroduce))
            {
                NetworkEventHandler.OnNetworkException(Result_Define.eResult.EMPTY_GUILD_INTRODUCE);
            }
            else
            {
                string guildEmblem = UIUtility.GuildEmblemToString(m_PickedColor, m_PickedPatternIndex);
                bool isFreeJoin = m_Toggle.isOn;

                Kernel.entry.guild.REQ_PACKET_CG_GUILD_CREATE_GUILD_SYN(guildName,
                                                                        guildIntroduce,
                                                                        guildEmblem,
                                                                        isFreeJoin);
            }
        }
    }

    void OnToggleValueChanged(bool value)
    {
        m_ToggleText.text = Languages.ToString(value ? TEXT_UI.FREE : TEXT_UI.APPROVAL);
    }
}
