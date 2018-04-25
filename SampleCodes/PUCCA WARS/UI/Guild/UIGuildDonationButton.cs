using Common.Packet;
using Common.Util;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

[RequireComponent(typeof(Button))]
public class UIGuildDonationButton : MonoBehaviour
{
    public Text m_GuildExperienceText;
    public Text m_GuildPointText;
    public Button m_Button;
    public Text m_PriceText;
    public Image m_GrayscaleImage;
    public eGuildDonation m_GuildDonation;

    void Reset()
    {
        m_Button = GetComponent<Button>();
    }

    void Awake()
    {
        m_Button.onClick.AddListener(OnButtonClick);
    }

    void Start()
    {
        DB_GuildDonation.Schema guildDonation = DB_GuildDonation.Query(DB_GuildDonation.Field.Index, (int)m_GuildDonation);
        if (guildDonation != null)
        {
            m_GuildExperienceText.text = string.Format("+{0}p", guildDonation.GulidExp_Obtain);
            m_GuildPointText.text = string.Format("+{0}p", guildDonation.GulidPoint_Obtain);
            m_PriceText.text = Languages.ToString<int>(guildDonation.DonationPrice);
        }
    }

    // Use this for initialization

    // Update is called once per frame

    void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onDonationResult += OnDonationResult;

            Renewal();
        }

        if (Kernel.networkManager)
        {
            Kernel.networkManager.onException += OnException;
        }
    }

    void OnDisable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.onDonationResult -= OnDonationResult;
        }

        if (Kernel.networkManager)
        {
            Kernel.networkManager.onException -= OnException;
        }
    }

    bool interactable
    {
        set
        {
            //m_Button.interactable = value;
            m_GrayscaleImage.gameObject.SetActive(!value);
        }
    }

    void Renewal()
    {
        if (Kernel.entry != null)
        {
            Result_Define.eResult result;
            bool available = Kernel.entry.guild.DonationAvailable(m_GuildDonation, out result);

            interactable = available;
        }
    }

    void OnException(Result_Define.eResult result, string error, ePACKET_CATEGORY category, byte index)
    {
        if (Equals(ePACKET_CATEGORY.CG_GUILD, category) && Equals(eCG_GUILD.DONATIONS_ACK, index))
        {
            Renewal();
        }
    }

    void OnDonationResult(byte guildLevel, long guildExp)
    {
        Renewal();
    }

    void OnButtonClick()
    {
        if (Kernel.entry != null)
        {
            Result_Define.eResult result;
            if (Kernel.entry.guild.DonationAvailable(m_GuildDonation, out result))
            {
                SoundDataInfo.RevertSound(m_Button.gameObject);
                // ref. https://mseedgames.atlassian.net/browse/PUC-814
                DB_GuildDonation.Schema guildDonation = DB_GuildDonation.Query(DB_GuildDonation.Field.Index, (int)m_GuildDonation);
                if (guildDonation != null)
                {
                    UIAlerter.Alert(string.Format("{0}\n\n{1}\n{2}",
                                                  Languages.ToString(TEXT_UI.DONATION_MSG,
                                                                     guildDonation.DonationPrice,
                                                                     Languages.ToString(guildDonation.Goods_Type)),
                                                  Languages.ToString(TEXT_UI.GUILD_EXP_PLUS, guildDonation.GulidExp_Obtain),
                                                  Languages.ToString(TEXT_UI.GUILD_POINT_PLUS, guildDonation.GulidPoint_Obtain)),
                                    UIAlerter.Composition.Confirm_Cancel,
                                    OnResponse,
                                    Languages.ToString(TEXT_UI.DONATION));
                }
            }
            else
            {
                SoundDataInfo.ChangeUISound(UISOUND.UIS_CANCEL_01, m_Button.gameObject);
                NetworkEventHandler.OnNetworkException(result);
            }
        }
    }

    void OnResponse(UIAlerter.Response response, params object[] args)
    {
        if (response == UIAlerter.Response.Confirm)
        {
            Kernel.entry.guild.REQ_PACKET_CG_GUILD_DONATIONS_SYN(m_GuildDonation);
        }
    }
}
