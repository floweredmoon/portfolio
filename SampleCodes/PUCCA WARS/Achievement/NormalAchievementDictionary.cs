using System;
using System.Collections.Generic;

public static class NormalAchievementDictionary
{

    // Key : DB_AchieveList.Achieve_Group
    // Value : 
    // [IMPLEMENT]
    // int -> enum
    private static readonly Dictionary<int, Type> s_AchvCompDict = new Dictionary<int, Type>();

    static NormalAchievementDictionary()
    {
        this[1] = typeof(AchieveGoldCollect); // 골드수집
        this[2] = typeof(AchieveGoldSpend); // 골드소모
        this[3] = typeof(AchieveCardSupport); // 카드지원
        this[4] = typeof(AchieveCardRequest); // 카드요청
        this[5] = typeof(AchieveChestOpen); // 상자열기
        this[6] = typeof(AchieveChestCollect); // 상자획득
        this[7] = typeof(AchieveCompleteAchieve); // 업적달성
        this[8] = typeof(AchieveRubySpend); // 루비소모
        this[9] = typeof(AchievePvPJoin); // 매칭참여
        this[10] = typeof(AchievePvPWin); // 매칭승리
        this[11] = typeof(AchievePvPLose); // 매칭패배
        this[12] = typeof(AchievePvEStart); // 모험참여
        this[13] = typeof(AchievePvEKillBoss); // 보스처치
        this[14] = typeof(AchievePvEKillMob); // 몬스터처치
        this[15] = typeof(AchievePvEComplete); // 모험클리어
        this[16] = typeof(AchieveCardCollect); // 카드획득
        this[17] = typeof(AchieveCardCollectGradeC); // C등급 카드 획득
        this[18] = typeof(AchieveCardCollectGradeB); // B등급 카드 획득
        this[19] = typeof(AchieveCardCollectGradeA); // A등급 카드 획득
        this[20] = typeof(AchieveCardCollectGradeS); // S등급 카드 획득
        this[21] = typeof(AchieveCardFindGradeC); // C등급 카드 소지
        this[22] = typeof(AchieveCardFindGradeB); // B등급 카드 소지
        this[23] = typeof(AchieveCardFindGradeA); // A등급 카드 소지
        this[24] = typeof(AchieveCardFindGradeS); // S등급 카드 소지
        this[25] = typeof(AchieveCardLevelUp); // 캐릭터 레벨업
        this[26] = typeof(AchieveSkillLevelUp); // 캐릭터 성급업
        this[27] = typeof(AchieveEquipLevelUp); // 캐릭터 장비업
        this[28] = typeof(AchieveRevengeJoin); // 복수전 참여
        this[29] = typeof(AchieveRevengeWin); // 복수전 승리
        this[30] = typeof(AchieveRevengeLose); // 복수전 패배
        this[31] = typeof(AchievePvPPointGet); // 승점 보상 획득
        this[32] = typeof(AchievePvPSuccessiveWin); // 연승 달성
        this[33] = typeof(AchieveSecretBusiness); // 비밀거래 완료
        this[34] = typeof(AchieveCardFindGradeSS); // SS등급 캐릭터 보유
        this[35] = typeof(AchieveCardCollectGradeS); // SS등급 카드 획득 수
        this[36] = typeof(AchieveGameAttendance); // 1일1회 출석 수
        this[37] = typeof(AchieveFranchiseRoomOpen); // 층 확장 횟수
        this[38] = typeof(AchieveFranchiseRewardGet); // 가맹점 재화 회수량
        this[39] = typeof(AchieveFranchiseSmilePointGet); // 스마일 포인트 획득 량
        this[40] = typeof(AchieveFranchiseCompleteBuilding); // 가맹점 건물 완성수
        this[41] = typeof(AchieveTreasureDetectChestOpen); // 전체 섬 입장 횟수
        this[42] = typeof(AchieveTreasureDetectIslandEnterTerrapin); // 자라섬 입장 횟수
        this[43] = typeof(AchieveTreasureDetectIslandEnterCoconut); // 야자섬 입장 횟수
        this[44] = typeof(AchieveTreasureDetectIslandEnterIce); // 얼음섬 입장 횟수
        this[45] = typeof(AchieveTreasureDetectIslandEnterLake); // 호수섬 입장 횟수
        this[46] = typeof(AchieveTreasureDetectIslandEnterBlack); // 검은섬 입장 횟수
    }

    public static bool TryGetValue(int achvGroup, out Type achvComp)
    {
        return s_AchvCompDict.TryGetValue(achvGroup, out achvComp);
    }
}
