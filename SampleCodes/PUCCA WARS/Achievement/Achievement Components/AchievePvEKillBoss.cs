using Common.Packet;

/// <summary>
/// 보스처치
/// </summary>
public class AchievePvEKillBoss : AchievementComponent
{

    protected override void OnEnable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.adventure.onPveResultDelegate += Listener;
        }
    }

    protected override void OnDisable()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.adventure.onPveResultDelegate -= Listener;
        }
    }

    void Listener(PACKET_CG_GAME_PVE_RESULT_ACK packet)
    {
        if (packet.m_bIsClear)
        {
            DB_StagePVE.Schema stagePvE = DB_StagePVE.Query(DB_StagePVE.Field.Index, packet.m_iClearStage);
            if (stagePvE != null)
            {
                if (stagePvE.STAGE_TYPE == STAGE_TYPE.BOSS)
                {
                    currentScore++;
                }
            }
        }
    }
}
