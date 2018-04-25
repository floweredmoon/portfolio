using Common.Packet;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIGuildReceiveCardInfo : UIObject
{
    public UIMiniCharCard m_MiniCharCard;
    public ScrollRect m_ScrollRect;
    public GameObjectPool m_GameObjectPool;

    List<UIGuildReceiveCardObject> m_ReceiveCardObjectList = new List<UIGuildReceiveCardObject>();

    // Use this for initialization

    // Update is called once per frame

    public void SetGuildReceiveCardResult(int cardIndex, List<CReceicedCard> receivedCardList)
    {
        m_MiniCharCard.SetCardInfo(cardIndex);

        for (int i = 0; i < m_ReceiveCardObjectList.Count; i++)
        {
            UIGuildReceiveCardObject receiveCardObject = m_ReceiveCardObjectList[i];
            if (receiveCardObject)
            {
                receiveCardObject.gameObject.SetActive(false);
                UIUtility.SetParent(receiveCardObject.transform, transform);
                m_GameObjectPool.Push(m_ReceiveCardObjectList[i].gameObject);
            }
        }
        m_ReceiveCardObjectList.Clear();

        if (receivedCardList != null && receivedCardList.Count > 0)
        {
            for (int i = 0; i < receivedCardList.Count; i++)
            {
                UIGuildReceiveCardObject receiveCardObject = m_GameObjectPool.Pop<UIGuildReceiveCardObject>();
                if (receiveCardObject)
                {
                    receiveCardObject.gameObject.SetActive(true);
                    UIUtility.SetParent(receiveCardObject.transform, m_ScrollRect.content);
                    receiveCardObject.SetReceivedCard(i, receivedCardList[i]);
                    m_ReceiveCardObjectList.Add(receiveCardObject);
                }
            }
        }
    }
}
