using Common.Packet;
using Common.Util;

public partial class Guild
{
    #region Properties
    public byte donationCount
    {
        get
        {
            if (ownGuildMember != null)
            {
                return ownGuildMember.m_byDonationCount;
            }

            return 0;
        }

        set
        {
            if (ownGuildMember != null)
            {
                ownGuildMember.m_byDonationCount = value;
            }
        }
    }
    #endregion

    #region Delegates
    public delegate void OnDonationResult(byte guildLevel, long guildExp);
    public OnDonationResult onDonationResult;
    #endregion

    public bool DonationAvailable(eGuildDonation guildDonation, out Result_Define.eResult result)
    {
        result = Result_Define.eResult.SUCCESS;

        if (donationCount >= entry.data.GetValue<int>(Const_IndexID.Const_Guild_Donate_Limit))
        {
            result = Result_Define.eResult.MAX_DONATION_COUNT;
            return false;
        }
        else
        {
            DB_GuildDonation.Schema schema = DB_GuildDonation.Query(DB_GuildDonation.Field.Index, (int)guildDonation);
            if (schema != null)
            {
                switch (guildDonation)
                {
                    case eGuildDonation.Gold:
                        if (Kernel.entry.account.gold < schema.DonationPrice)
                        {
                            result = Result_Define.eResult.NOT_ENOUGH_GOLD;
                            return false;
                        }
                        break;
                    case eGuildDonation.Ruby100:
                    case eGuildDonation.Ruby50:
                        if (Kernel.entry.account.ruby < schema.DonationPrice)
                        {
                            result = Result_Define.eResult.NOT_ENOUGH_RUBY;
                            return false;
                        }
                        break;
                }
            }
        }

        return true;
    }

    #region REQ
    public void REQ_PACKET_CG_GUILD_DONATIONS_SYN(eGuildDonation guildDonation)
    {
        Kernel.networkManager.WebRequest(new PACKET_CG_GUILD_DONATIONS_SYN()
        {
            m_eDonationType = guildDonation,
        });
    }
    #endregion

    #region RCV
    void RCV_PACKET_CG_GUILD_DONATIONS_ACK(PACKET_CG_GUILD_DONATIONS_ACK packet)
    {
        if (packet.m_bIsGuildLevelUp)
        {
            guildLevel++;
        }

        guildExp = packet.m_GuildExp;
        entry.account.gold = packet.m_iRemainGold;
        entry.account.ruby = packet.m_iRemainRuby;
        entry.account.guildPoint = packet.m_iTotalGuildPoint;
        donationCount++;

        DB_GuildDonation.Schema guildDonation = DB_GuildDonation.Query(DB_GuildDonation.Field.Index, packet.m_GuildDonationType);
        if (guildDonation != null)
        {
            CGuildMember guildMember = ownGuildMember;
            if (guildMember != null)
            {
                guildMember.m_iDonatedGuildExp = guildMember.m_iDonatedGuildExp + guildDonation.GulidExp_Obtain;
            }
        }

        if (onDonationResult != null)
        {
            onDonationResult(guildLevel, guildExp);
        }
    }
    #endregion
}
