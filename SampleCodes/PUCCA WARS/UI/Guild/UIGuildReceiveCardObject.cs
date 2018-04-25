using Common.Packet;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildReceiveCardObject : MonoBehaviour
{
    public Image m_BackgroundImage;
    public Text m_Text;
    public Sprite m_OddNumberSprite;
    public Sprite m_EvenNumberSprite;

    // Use this for initialization

    // Update is called once per frame

    public void SetReceivedCard(int index, CReceicedCard receivedCard)
    {
        if (receivedCard != null)
        {
            if (index % 2 > 0)
            {
                m_BackgroundImage.sprite = m_OddNumberSprite;
                m_BackgroundImage.color = Color.white;
            }
            else
            {
                m_BackgroundImage.sprite = m_EvenNumberSprite;
                m_BackgroundImage.color = new Color32(171, 171, 171, 255);
            }
            m_Text.text = string.Format("{0} x{1}", receivedCard.m_sSenderName, receivedCard.m_iReceivedCardCount);
        }
    }
}
