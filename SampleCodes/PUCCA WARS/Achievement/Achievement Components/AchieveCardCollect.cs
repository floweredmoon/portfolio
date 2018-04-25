
/// <summary>
/// 카드획득
/// </summary>
public class AchieveCardCollect : AchievementComponent
{
    public virtual Grade_Type gradeType
    {
        get
        {
            return Grade_Type.None;
        }
    }

    protected override void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.character.onCardInfoUpdate += Listener;
            Kernel.entry.character.onSoulInfoUpdate += Listener;
        }
    }

    protected override void OnDisable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.character.onCardInfoUpdate -= Listener;
            Kernel.entry.character.onSoulInfoUpdate -= Listener;
        }
    }

    bool Listenable(int cardIndex)
    {
        if (gradeType != Grade_Type.None)
        {
            DB_Card.Schema card = DB_Card.Query(DB_Card.Field.Index, cardIndex);
            if (card == null || card.Grade_Type != gradeType)
            {
                return false;
            }
        }

        return true;
    }

    void Listener(long cid, int cardIndex, bool isNew)
    {
        if (isNew && Listenable(cardIndex))
        {
            currentScore++;
        }
    }

    void Listener(long sequence, int soulIndex, int soulCount, int updateCount)
    {
        if (updateCount > 0 && Listenable(soulIndex))
        {
            currentScore = currentScore + updateCount;
        }
    }
}
