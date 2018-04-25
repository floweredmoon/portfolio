using UnityEngine.UI;

public class UIGuildWaitingObject : UIGuildObject
{
    public Button m_CancelButton;

    void Awake()
    {
        m_CancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    // Use this for initialization

    // Update is called once per frame

    void OnCancelButtonClick()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.guild.REQ_PACKET_CG_GUILD_CANCEL_JOIN_REQUEST_SYN(m_GID);
        }
    }
}
