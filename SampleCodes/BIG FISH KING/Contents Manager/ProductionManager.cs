using Delta.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;
using System.Linq;

public class ProductionManager : kSingletonPersistent<ProductionManager>
{
    #region Properties

    // Key is slotIndex.
    public Dictionary<int, ProductionInfoRes.Slot> slotInfoDict
    {
        get; // ReadOnly
        private set;
    }

    #endregion

    #region Delegates

    public delegate void OnDisassemble(long itemId, uint itemIndex, int diffNormalPowder, int diffGoldPowder);
    public OnDisassemble onDisassemble;

    public delegate void OnStartProduction(int slotIndex, uint itemIndex, bool immediate);
    public OnStartProduction onStartProduction;

    public delegate void OnCompleteProduction(int slotIndex, uint itemIndex, bool immediate);
    public OnCompleteProduction onCompleteProduction;

    public delegate void OnCancelProduction(int slotIndex);
    public OnCancelProduction onCancelProduction;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        slotInfoDict = new Dictionary<int, ProductionInfoRes.Slot>();
    }

    // Use this for initialization
    private void Start()
    {
        StartCoroutine(UpdateBadgeByCoroutine());
    }

    // Update is called once per frame

    #endregion

    #region Static Methods

    public static bool CanCompleteImmediateByItemIndex(uint itemIndex)
    {
        uint requireCash;
        if (TryGetRequireCashForCompleteImmediate(itemIndex, out requireCash))
        {
            return requireCash <= Global.account.userInfo.Cash;
        }

        return false;
    }

    public static bool CanCompleteImmediateBySlotIndex(int slotIndex)
    {
        bool isCan = false;
        var slotInfo = ProductionManager.Instance.FindSlotInfo(slotIndex);
        if (slotInfo != null)
        {
            DateTime endTime = slotInfo.FinishUTime.FromUnixTimeToDateTime();
            TimeSpan remainTimeSpan = endTime - DateTime.UtcNow;
            uint requireCash;
            if (TryGetRequireCashForCompleteImmediate((uint)slotInfo.tiProductItem, (int)remainTimeSpan.TotalSeconds, out requireCash))
            {
                isCan = requireCash <= Global.account.userInfo.Cash;
            }
        }

        return isCan;
    }

    public static bool CanStartProduction(uint itemIndex)
    {
        bool isCan = false;
        GoodsChunk requireGoods;
        if (TryGetProductionRequireGoods(itemIndex, out requireGoods))
        {
            isCan = true;
            foreach (var requireGood in requireGoods.goods)
            {
                switch (requireGood.type)
                {
                    case eMailType.BattleCoin:
                        isCan = isCan && requireGood.amount <= Global.account.userInfo.BattleCoin;
                        break;
                    case eMailType.EcoPoint:
                        isCan = isCan && requireGood.amount <= Global.account.userInfo.Eco;
                        break;
                    case eMailType.Gold:
                        isCan = isCan && requireGood.amount <= Global.account.userInfo.Gold;
                        break;
                    case eMailType.GoldPowder:
                        isCan = isCan && requireGood.amount <= Global.account.userInfo.GoldPowder;
                        break;
                    case eMailType.NormalPowder:
                        isCan = isCan && requireGood.amount <= Global.account.userInfo.NormalPowder;
                        break;
                }
            }
        }

        return isCan;
    }

    public static bool TryGetProductionRequireTime(uint itemIndex, out TimeSpan productionTimeSpan)
    {
        productionTimeSpan = default(TimeSpan);

        var itemProductionData = TableManager.Instance.itemProduction.GetData(itemIndex);
        var fishingRodData = TableManager.Instance.fishingRod.GetData(itemIndex);
        var formulaData = TableManager.Instance.formulaValue.GetData(1);
        if (itemProductionData != null && fishingRodData != null && formulaData != null)
        {
            // 제작시간
            // (((낚싯대Lv*제작시간기본값)*Lv보너스)*(낚싯대칸수*제작시간기본값))*낚싯대등급
            float productionTime = (((fishingRodData.RodLevel * formulaData.PTime) * formulaData.LvBonusValue) * (fishingRodData.RodLength * formulaData.PTime)) * fishingRodData.RodType;
            productionTimeSpan = TimeSpan.FromSeconds(Mathf.CeilToInt(productionTime));
        }

        return productionTimeSpan != default(TimeSpan);
    }

    public static bool TryGetProductionRequireGoods(uint itemIndex, out GoodsChunk requireGoods)
    {
        requireGoods = default(GoodsChunk);

        var itemProductionData = TableManager.Instance.itemProduction.GetData(itemIndex);
        var fishingRodData = TableManager.Instance.fishingRod.GetData(itemIndex);
        var formulaData = TableManager.Instance.formulaValue.GetData(1);
        if (itemProductionData != null && fishingRodData != null && formulaData != null)
        {
            requireGoods = new GoodsChunk();
            // 필요일반카본가루
            // (((낚싯대Lv*Lv값)^Lv보너스)*((낚싯대칸수*칸수보조값)*(낚싯대등급*카본가루값))
            float requireNormalPowder = Mathf.Pow(fishingRodData.RodLevel * formulaData.RodLevelValue, formulaData.LvBonusValue) * ((fishingRodData.RodLength * formulaData.FRJoAsi) * (fishingRodData.RodType * formulaData.P_NormalCarbonPowder));
            requireGoods.AddItem(eMailType.NormalPowder, (uint)Mathf.CeilToInt(requireNormalPowder));
            // 필요황금카본가루
            // (((낚싯대Lv*Lv값)*Lv보너스)((낚싯대칸수*칸수보조값)*(낚싯대등급*황금카본가루값))
            float requireGoldPowder = ((fishingRodData.RodLevel * formulaData.RodLevelValue) * formulaData.LvBonusValue) * ((fishingRodData.RodLength * formulaData.FRJoAsi) * (fishingRodData.RodType * formulaData.PGoldCarbonPowder));
            requireGoods.AddItem(eMailType.GoldPowder, (uint)Mathf.CeilToInt(requireGoldPowder));
            // 필요배틀코인
            // ((황금카본가루*제작배틀코인값)
            float requireBattleCoin = requireGoldPowder * formulaData.PBattleCoin;
            requireGoods.AddItem(eMailType.BattleCoin, (uint)Mathf.CeilToInt(requireBattleCoin));
            // 제작에코포인트
            // ((일반카본가루*제작에코값)
            float requireEcoPoint = requireNormalPowder * formulaData.PEcoPoint;
            requireGoods.AddItem(eMailType.EcoPoint, (uint)Mathf.CeilToInt(requireEcoPoint));
            // 제작골드
            // ((일반카본가루*제작골드값)
            float requireGold = requireNormalPowder * formulaData.PGold;
            requireGoods.AddItem(eMailType.Gold, (uint)Mathf.CeilToInt(requireGold));
        }

        return (requireGoods != default(GoodsChunk));
    }

    public static bool TryGetRequireCashForCompleteImmediate(uint itemIndex, int remainTime, out uint requireCash)
    {
        requireCash = default(int);

        TimeSpan productionTimeSpan;
        if (TryGetRequireCashForCompleteImmediate(itemIndex, out requireCash) && TryGetProductionRequireTime(itemIndex, out productionTimeSpan))
        {
            // 제작중즉시완료캐시
            // (즉시완료캐시/완료시간)*남은시간
            requireCash = (uint)Mathf.CeilToInt(((float)requireCash / (float)productionTimeSpan.TotalSeconds) * remainTime);
            return true;
        }

        return false;
    }

    public static bool TryGetRequireCashForCompleteImmediate(uint itemIndex, out uint requireCash)
    {
        requireCash = default(uint);

        var itemProductionData = TableManager.Instance.itemProduction.GetData(itemIndex);
        var fishingRodData = TableManager.Instance.fishingRod.GetData(itemIndex);
        var formulaData = TableManager.Instance.formulaValue.GetData(1);
        if (itemProductionData != null && fishingRodData != null && formulaData != null)
        {
            // 제작즉시완료보석값
            // (((낚싯대Lv*제작시간기본값)*Lv보너스)*(낚싯대칸수*낚싯대등급))*제작즉시완료값
            requireCash = (uint)Mathf.CeilToInt((((fishingRodData.RodLevel * formulaData.PTime) * formulaData.LvBonusValue) * (fishingRodData.RodLength * fishingRodData.RodType)) * formulaData.PCompletedGem);
            return true;
        }

        return false;
    }

    public static bool TryGetDisassembleReturn(uint itemIndex, int tuneGrade, out GoodsChunk returnGoods)
    {
        returnGoods = default(GoodsChunk);

        var fishingRodData = TableManager.Instance.fishingRod.GetData(itemIndex);
        if (fishingRodData != null)
        {
            returnGoods = new GoodsChunk();
            uint normalPowder = fishingRodData.NormalCarbonPowder;
            uint goldPowder = 0;
            returnGoods.AddItem(eMailType.NormalPowder, normalPowder);

            if (tuneGrade > 0)
            {
                var formulaData = TableManager.Instance.formulaValue.GetData(1);
                if (formulaData != null)
                {
                    // 튜닝에따른황금카본추출
                    // (보상비율1+보상비율2*일반카본가루)*튜닝단계
                    goldPowder = (uint)Mathf.CeilToInt((formulaData.TuningGoldCarbon1 + formulaData.TuningGoldCarbon2 * normalPowder) * tuneGrade);
                }
            }

            returnGoods.AddItem(eMailType.GoldPowder, goldPowder);
        }

        return returnGoods != default(GoodsChunk);
    }

    public static bool TryGetCancelReturn(uint itemIndex, int remainTime, out GoodsChunk returnGoods)
    {
        returnGoods = default(GoodsChunk);

        GoodsChunk requireGoods;
        TimeSpan requireTimeSpan;
        if (TryGetProductionRequireGoods(itemIndex, out requireGoods) && TryGetProductionRequireTime(itemIndex, out requireTimeSpan))
        {
            var itemProductionData = TableManager.Instance.itemProduction.GetData(itemIndex);
            var fishingRodData = TableManager.Instance.fishingRod.GetData(itemIndex);
            var formulaData = TableManager.Instance.formulaValue.GetData(1);
            if (itemProductionData != null && fishingRodData != null && formulaData != null)
            {
                returnGoods = new GoodsChunk();
                // 제작취소시 반환골드
                // (제작골드/제작시간)*남은시간
                var requireGold = requireGoods.goods.Find(match => match.type == eMailType.Gold);
                float returnGold = ((float)requireGold.amount / (float)requireTimeSpan.TotalSeconds) * remainTime;
                returnGoods.AddItem(eMailType.Gold, (uint)Mathf.CeilToInt(returnGold));
                // 제작취소시 반환일반카본가루
                // (제작일반카본가루/제작시간)*남은시간
                var requireNormalPowder = requireGoods.goods.Find(match => match.type == eMailType.NormalPowder);
                float returnNormalPowder = ((float)requireNormalPowder.amount / (float)requireTimeSpan.TotalSeconds) * remainTime;
                returnGoods.AddItem(eMailType.NormalPowder, (uint)Mathf.CeilToInt(returnNormalPowder));
                // 제작취소시 반환황금카본가루
                // (제작황금카본가루/제작시간)*남은시간
                var requireGoldPowder = requireGoods.goods.Find(match => match.type == eMailType.GoldPowder);
                float returnGoldPowder = ((float)requireGoldPowder.amount / (float)requireTimeSpan.TotalSeconds) * remainTime;
                returnGoods.AddItem(eMailType.GoldPowder, (uint)Mathf.CeilToInt(returnGoldPowder));
                // 제작취소시 반환배틀코인
                // (제작배틀코인/제작시간)*남은시간
                var requireBattleCoin = requireGoods.goods.Find(match => match.type == eMailType.BattleCoin);
                float returnBattleCoin = ((float)requireBattleCoin.amount / (float)requireTimeSpan.TotalSeconds) * remainTime;
                returnGoods.AddItem(eMailType.BattleCoin, (uint)Mathf.CeilToInt(returnBattleCoin));
                // 제작취소시 반환에코포인트
                // (제작에코포인트/제작시간)*남은시간
                var requireEcoPoint = requireGoods.goods.Find(match => match.type == eMailType.EcoPoint);
                float returnEcoPoint = ((float)requireEcoPoint.amount / (float)requireTimeSpan.TotalSeconds) * remainTime;
                returnGoods.AddItem(eMailType.EcoPoint, (uint)Mathf.CeilToInt(returnEcoPoint));
            }
        }

        return returnGoods != default(GoodsChunk);
    }

    #endregion

    #region Local Push Notifications

    public void ScheduleLocalPushNotifications()
    {
        StartCoroutine(ScheduleLocalPushNotificationsByCoroutine());
    }

    private IEnumerator ScheduleLocalPushNotificationsByCoroutine()
    {
        foreach (var slotInfo in slotInfoDict.Values)
        {
            if (slotInfo != null)
            {
                CancelLocalPushNotifications(slotInfo.Index);
                ScheduleLocalPushNotifications(slotInfo);
            }

            yield return null;
        }
    }

    private void ScheduleLocalPushNotifications(ProductionInfoRes.Slot slotInfo)
    {
        if (slotInfo != null)
        {
            var endTime = slotInfo.FinishUTime.FromUnixTimeToDateTime();
            if (endTime > DateTime.UtcNow)
            {
                var remainTimeSpan = endTime - DateTime.UtcNow;
                // 24608 : 제작 완료
                string title = TableManager.Instance.GetLanguageString(24608);
                string fishingRodName = string.Empty;
                var fishingRodData = TableManager.Instance.fishingRod.GetData((uint)slotInfo.tiProductItem);
                if (fishingRodData != null)
                {
                    fishingRodName = TableManager.Instance.GetLanguageString(fishingRodData.FishingRodNameTableId);
                }
                // 24615 : {0} 제작이 완료되었습니다!
                string message = TableManager.Instance.GetLanguageString(24615, fishingRodName);
                string localPushNotifications = DevicePushManager.Instance.LocalNotification((long)remainTimeSpan.TotalSeconds, DevicePushManager.eTagType.Type_Production, title, message);
                string prefsKey = string.Empty;
                if (!string.IsNullOrEmpty(localPushNotifications))
                {
                    prefsKey = "production" + slotInfo.Index;
                    PlayerPrefs.SetString(prefsKey, localPushNotifications);
                }
                Debug.LogFormat("[ProductionManager] ScheduleLocalPushNotifications slotIndex : {0}, prefsKey : {1}, localPushNotifications : {2}, title : {3}, message : {4}", slotInfo.Index, prefsKey, localPushNotifications, title, message);
            }
        }
    }

    private void CancelLocalPushNotifications(int slotIndex)
    {
        string prefsKey = "production" + slotIndex;
        string localPushNotifications = PlayerPrefs.GetString(prefsKey, string.Empty);
        Debug.LogFormat("[ProductionManager] CancelLocalPushNotifications slotIndex : {0}, prefsKey : {1}, localPushNotifications : {2}", slotIndex, prefsKey, localPushNotifications);
        if (!string.IsNullOrEmpty(localPushNotifications))
        {
            DevicePushManager.Instance.CancelLocalNotification(localPushNotifications);
            PlayerPrefs.SetString(prefsKey, string.Empty);
        }
    }

    #endregion

    private IEnumerator UpdateBadgeByCoroutine()
    {
        while (true)
        {
            bool isEnabled = false;
            if (slotInfoDict != null && slotInfoDict.Count > 0)
            {
                foreach (var slotInfo in slotInfoDict.Values)
                {
                    if (slotInfo != null && slotInfo.HasComplete())
                    {
                        isEnabled = true;
                        break;
                    }

                    yield return null;
                }
            }

            if (isEnabled)
            {
                LobbyNewActiveDataManager.Instance.AddLobbyNewActiveData(LobbyNewActiveDataManager.eLobbyIconType.Lobby_Production);
            }
            else
            {
                LobbyNewActiveDataManager.Instance.RemoveLobbyNewActiveData(LobbyNewActiveDataManager.eLobbyIconType.Lobby_Production);

                if (Lobby.singletone != null)
                {
                    Lobby.singletone.SetUiLobbyNewIconActiveToType(LobbyNewActiveDataManager.eLobbyIconType.Lobby_Production, false);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public ProductionInfoRes.Slot FindSlotInfo(int slotIndex)
    {
        ProductionInfoRes.Slot slotInfo = null;
        if (slotInfoDict.ContainsKey(slotIndex))
        {
            slotInfo = slotInfoDict[slotIndex];
        }

        return slotInfo;
    }

    private bool RemoveSlotInfo(int slotIndex, out uint itemIndex)
    {
        itemIndex = default(uint);

        ProductionInfoRes.Slot slotInfo;
        if (slotInfoDict.TryGetValue(slotIndex, out slotInfo))
        {
            itemIndex = (uint)slotInfo.tiProductItem;

        }
        bool isRemoved = slotInfoDict.Remove(slotIndex);
        Debug.LogFormat("[ProductionManager] RemoveSlotInfo(slotIndex : {0}, out itemIndex : {1}) isRemoved : {2}", slotIndex, itemIndex, isRemoved);
        return isRemoved;
    }

    private bool RemoveSlotInfo(int slotIndex)
    {
        bool isRemoved = slotInfoDict.Remove(slotIndex);
        Debug.LogFormat("[ProductionManager] RemoveSlotInfo(slotIndex : {0}) isRemoved : {1}", slotIndex, isRemoved);
        return isRemoved;
    }

    private ProductionInfoRes.Slot UpdateSlotInfo(int slotIndex, int itemIndex, int startTime, int endTime)
    {
        Debug.LogFormat("[ProductionManager] UpdateSlotInfo (slotIndex : {0}, itemIndex : {1}, startTime : {2}, endTime : {3}", slotIndex, itemIndex, startTime, endTime);
        ProductionInfoRes.Slot slotInfo;
        if (!slotInfoDict.TryGetValue(slotIndex, out slotInfo))
        {
            slotInfoDict.Add(slotIndex, slotInfo = new ProductionInfoRes.Slot());
        }

        // Deep Copy
        slotInfo.Index = slotIndex;
        slotInfo.tiProductItem = itemIndex;
        slotInfo.StartUTime = startTime;
        slotInfo.FinishUTime = endTime;

        return slotInfo;
    }

    public void CompleteProductionImmediate(int slotIndex)
    {
        StartCoroutine(CompleteProductionImmediateByCoroutine(slotIndex));
    }

    private IEnumerator CompleteProductionImmediateByCoroutine(int slotIndex)
    {
        var protocol = new ServerRequest<ProductionCompletedImmediatelyReq, ProductionCompletedImmediatelyRes>();
        protocol.showLoadingCircle = true;
        protocol.openErrorMsgBoxWhenReqFailed = true;
        protocol.req.SlotIndex = slotIndex;

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            uint itemIndex;
            RemoveSlotInfo(protocol.res.SlotIndex, out itemIndex);
            CancelLocalPushNotifications(protocol.res.SlotIndex);

            int diffCash = Global.account.UpdateCash(protocol.res.Cash);
            UI_AccountTopInfo.sUpdateMoveToCash(true);

            if (protocol.res.Items != null && protocol.res.Items.Length > 0)
            {
                Global.myInventory.AddItem(protocol.res.Items);
            }

            if (onCompleteProduction != null)
            {
                onCompleteProduction(slotIndex, itemIndex, true);
            }
        }
    }

    public void CompleteProduction(int slotIndex)
    {
        StartCoroutine(CompleteProductionByCoroutine(slotIndex));
    }

    private IEnumerator CompleteProductionByCoroutine(int slotIndex)
    {
        var protocol = new ServerRequest<ProductionCompletedReq, ProductionCompletedRes>();
        protocol.showLoadingCircle = true;
        protocol.openErrorMsgBoxWhenReqFailed = true;
        protocol.req.SlotIndex = slotIndex;

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            uint itemIndex;
            RemoveSlotInfo(protocol.res.SlotIndex, out itemIndex);
            CancelLocalPushNotifications(protocol.res.SlotIndex);

            if (protocol.res.Items != null && protocol.res.Items.Length > 0)
            {
                Global.myInventory.AddItem(protocol.res.Items);
            }

            if (onCompleteProduction != null)
            {
                onCompleteProduction(slotIndex, itemIndex, false);
            }
        }
    }

    public void CancelProduction(int slotIndex)
    {
        StartCoroutine(CancelProductionByCoroutine(slotIndex));
    }

    private IEnumerator CancelProductionByCoroutine(int slotIndex)
    {
        var protocol = new ServerRequest<ProductionCancelReq, ProductionCancelRes>();
        protocol.showLoadingCircle = true;
        protocol.openErrorMsgBoxWhenReqFailed = true;
        protocol.req.SlotIndex = slotIndex;

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            RemoveSlotInfo(protocol.res.SlotIndex);
            CancelLocalPushNotifications(protocol.res.SlotIndex);

            if (protocol.res.BattleCoin.HasValue)
            {
                Global.account.UpdateBattleCoin(protocol.res.BattleCoin.Value);
            }

            if (protocol.res.Eco.HasValue)
            {
                Global.account.UpdateEco(protocol.res.Eco.Value);
                UI_AccountTopInfo.sUpdateMoveToEcoPoint();
            }

            if (protocol.res.Gold.HasValue)
            {
                Global.account.UpdateGold(protocol.res.Gold.Value);
                UI_AccountTopInfo.sUpdateMoveToGold(true);
            }

            if (protocol.res.GoldPowder.HasValue)
            {
                Global.account.UpdateGoldPowder(protocol.res.GoldPowder.Value);
            }

            if (protocol.res.NormalPowder.HasValue)
            {
                Global.account.UpdateNormalPowder(protocol.res.NormalPowder.Value);
            }

            if (onCancelProduction != null)
            {
                onCancelProduction(slotIndex);
            }
        }
    }

    public void StartProduction(uint itemIndex, bool immediate)
    {
        StartCoroutine(StartProductionByCoroutine(itemIndex, immediate));
    }

    private IEnumerator StartProductionByCoroutine(uint itemIndex, bool immediate)
    {
        var protocol = new ServerRequest<ProductionStartReq, ProductionStartRes>();
        protocol.showLoadingCircle = true;
        protocol.openErrorMsgBoxWhenReqFailed = true;
        protocol.req.tiItemProduction = (int)itemIndex;
        protocol.req.CompletedImmediately = immediate ? 1 : 0;

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            ValuePotionManager.TrackEvent("FishingRodProduct_Count", "Start", "itemIndex" + itemIndex.ToString(), 1);

            bool isCompleted = protocol.res.isCompleted.HasValue ? protocol.res.isCompleted.Value == 1 : false;
            int slotIndex = protocol.res.SlotIndex ?? 0;

            if (isCompleted)
            {
                RemoveSlotInfo(slotIndex);
            }
            else
            {
                int startTime = protocol.res.StartUTime ?? 0;
                int endTime = protocol.res.FinishUTime ?? 0;

                var slotInfo = UpdateSlotInfo(slotIndex, (int)itemIndex, startTime, endTime);
                if (slotInfo != null)
                {
                    ScheduleLocalPushNotifications(slotInfo);
                }
            }

            if (protocol.res.Cash.HasValue)
            {
                int diffCash = Global.account.UpdateCash(protocol.res.Cash.Value);
                UI_AccountTopInfo.sUpdateMoveToCash(true);

                if (diffCash > 0)
                {
                    ValuePotionManager.TrackEvent("Gem_Spend", "RodProduct", "Complete", -diffCash);
                    ValuePotionManager.TrackEvent("RodProduct_Gem", -diffCash);
                }
            }

            if (protocol.res.Gold.HasValue)
            {
                int diffGold = Global.account.UpdateGold(protocol.res.Gold.Value);
                UI_AccountTopInfo.sUpdateMoveToGold(true);

                if (diffGold > 0)
                {
                    ValuePotionManager.TrackEvent("Gold_Spend", "RodProduct", string.Empty, -diffGold);
                    ValuePotionManager.TrackEvent("RodProduct_Gold", -diffGold);
                }
            }

            if (protocol.res.Eco.HasValue)
            {
                int diffEco = Global.account.UpdateEco(protocol.res.Eco.Value);
                UI_AccountTopInfo.sUpdateMoveToEcoPoint(true);

                if (diffEco > 0)
                {
                    ValuePotionManager.TrackEvent("EcoPoint_Spend", "RodProduct", string.Empty, -diffEco);
                    ValuePotionManager.TrackEvent("RodProduct_Gold", -diffEco);
                }
            }

            if (protocol.res.BattleCoin.HasValue)
            {
                int diffBattleCoin = Global.account.UpdateBattleCoin(protocol.res.BattleCoin.Value);
                if (diffBattleCoin > 0)
                {
                    ValuePotionManager.TrackEvent("BattleCoin_Spend", "RodProduct", string.Empty, -diffBattleCoin);
                    ValuePotionManager.TrackEvent("RodProduct_Gold", -diffBattleCoin);
                }
            }

            if (protocol.res.NormalPowder.HasValue)
            {
                int diffNormalPowder = Global.account.UpdateNormalPowder(protocol.res.NormalPowder.Value);
                if (diffNormalPowder > 0)
                {
                    ValuePotionManager.TrackEvent("NormalPowder_Spend", "RodProduct", string.Empty, -diffNormalPowder);
                    ValuePotionManager.TrackEvent("RodProduct_NormalPowder", -diffNormalPowder);
                }
            }

            if (protocol.res.GoldPowder.HasValue)
            {
                int diffGoldPowder = Global.account.UpdateGoldPowder(protocol.res.GoldPowder.Value);
                if (diffGoldPowder > 0)
                {
                    ValuePotionManager.TrackEvent("GoldPowder_Spend", "RodProduct", string.Empty, -diffGoldPowder);
                    ValuePotionManager.TrackEvent("RodProduct_GoldPowder", -diffGoldPowder);
                }
            }

            if (protocol.res.Items != null && protocol.res.Items.Length > 0)
            {
                Global.myInventory.AddItem(protocol.res.Items);

                foreach (var itemInfo in protocol.res.Items)
                {
                    if (itemInfo != null)
                    {
                        ValuePotionManager.TrackEvent("FishingRodProduct_Count", "Complete", "itemIndex" + itemInfo.tidItem.ToString(), 1);
                    }

                    yield return null;
                }
            }

            if (onStartProduction != null)
            {
                onStartProduction(slotIndex, itemIndex, isCompleted);
            }
        }
    }

    public void GetSlotInfoList(Action<List<ProductionInfoRes.Slot>> callback)
    {
        StartCoroutine(GetSlotInfoListByCoroutine(callback));
    }

    private IEnumerator GetSlotInfoListByCoroutine(Action<List<ProductionInfoRes.Slot>> callback)
    {
        if (slotInfoDict.Count > 0)
        {
            slotInfoDict.Clear();
        }

        var protocol = new ServerRequest<ProductionInfoReq, ProductionInfoRes>();

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            if (protocol.res.Slots != null && protocol.res.Slots.Length > 0)
            {
                foreach (var slotInfo in protocol.res.Slots)
                {
                    if (slotInfo != null)
                    {
                        if (!slotInfoDict.ContainsKey(slotInfo.Index))
                        {
                            UpdateSlotInfo(slotInfo.Index, slotInfo.tiProductItem, slotInfo.StartUTime, slotInfo.FinishUTime);
                        }
                    }
                }
            }
        }

        callback.InvokeWithNullCheck(slotInfoDict.Values.ToList());
    }

    public void Disassemble(long itemId, uint itemIndex)
    {
        StartCoroutine(DisassembleByCoroutine(itemId, itemIndex));
    }

    private IEnumerator DisassembleByCoroutine(long itemId, uint itemIndex)
    {
        var protocol = new ServerRequest<ItemDisassembleReq, ItemDisassembleRes>();
        protocol.showLoadingCircle = true;
        protocol.openErrorMsgBoxWhenReqFailed = true;
        protocol.req.idItem = itemId;

        yield return protocol.Send();

        if (protocol.IsSuccess())
        {
            int diffNormalPowder = 0;
            if (protocol.res.UserNormalPowder.HasValue)
            {
                diffNormalPowder = Global.account.UpdateNormalPowder(protocol.res.UserNormalPowder.Value);
            }
            if (diffNormalPowder > 0)
            {
                ValuePotionManager.TrackEvent("NormalPowder_Supply", "Disassemble", string.Empty, -diffNormalPowder);
            }

            int diffGoldPowder = 0;
            if (protocol.res.UserGoldPowder.HasValue)
            {
                diffGoldPowder = Global.account.UpdateGoldPowder(protocol.res.UserGoldPowder.Value);
            }
            if (diffGoldPowder > 0)
            {
                ValuePotionManager.TrackEvent("GoldPowder_Supply", "Disassemble", string.Empty, -diffGoldPowder);
            }

            // Remove
            // UI_BagPopup.ItemDeleteResetPosition에서 처리

            if (onDisassemble != null)
            {
                onDisassemble(itemId, itemIndex, diffNormalPowder, diffGoldPowder);
            }
        }
    }
}
