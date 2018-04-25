using Common.Packet;
using Common.Util;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAchieveObject : MonoBehaviour
{
    #region Variables
    static Transform m_AchieveListTransform;
    public Image m_AchieveTypeImage;
    public GameObject m_AchieveCompleteStepContainer;
    public List<Transform> m_StarContainers;
    public List<Animator> m_StarAnimators;
    public List<AnimationEventHandler> m_StarAnimationEventHandlers;
    public Image m_AchieveTitleFrameImage;
    public Text m_AchieveTitleText;
    public Text m_AchieveDescriptionText;
    public UISliderAnimator m_AchieveProgressSliderAnimator;
    public Image m_AchieveRewardGoodsTypeImage;
    public Text m_AchieveRewardValueText;
    public Image m_AchieveRewardCompleteImage;
    public UITooltipObject m_AchieveRewardTooltipObject;
    public Button m_GetAchieveRewardButton;
    public Text m_GetAchieveRewardButtonText;
    public Image m_BadgeImage;
    int m_AchieveIndex;
    int m_AchieveGroup;
    RectTransform m_RectTransform;
    CReceivedGoods m_ReceivedGoods;
    bool m_IsLevelUp;
    Transform m_ContainerTransform;
    Transform m_DirectingTransform;
    #endregion

    #region Properties
    public RectTransform rectTransform
    {
        get
        {
            if (m_RectTransform == null)
            {
                m_RectTransform = transform as RectTransform;
            }

            return m_RectTransform;
        }
    }

    public bool isDaily
    {
        get;
        private set;
    }

    public bool isCompleted
    {
        get;
        private set;
    }

    public bool acquirable
    {
        get;
        private set;
    }

    public int achieveIndex
    {
        get
        {
            return m_AchieveIndex;
        }
    }

    public int achieveGroup
    {
        get
        {
            return m_AchieveGroup;
        }
    }

    public Transform achieveListTransform
    {
        get
        {
            return m_AchieveListTransform;
        }

        set
        {
            if (m_AchieveListTransform != value)
            {
                m_AchieveListTransform = value;
            }
        }
    }

    static bool m_Interactable = true;

    public static bool interactable
    {
        get
        {
            return m_Interactable;
        }

        set
        {
            if (m_Interactable != value)
            {
                m_Interactable = value;

                if (onInteractableChange != null)
                {
                    onInteractableChange(m_Interactable);
                }
            }
        }
    }
    #endregion

    public delegate void OnCompleteAchieveAnimationFinished();
    public OnCompleteAchieveAnimationFinished onCompleteAchieveAnimationFinished;

    public delegate void OnInteractableChange(bool interactable);
    public static OnInteractableChange onInteractableChange;

    #region MonoBehaviour
    void Awake()
    {
        onInteractableChange += Listener;
        m_GetAchieveRewardButton.onClick.AddListener(OnGetAchieveRewardButtonClick);

        for (int i = 0; i < m_StarAnimationEventHandlers.Count; i++)
        {
            AnimationEventHandler animationEventHandler = m_StarAnimationEventHandlers[i];
            if (animationEventHandler != null)
            {
                animationEventHandler.onAnimationEventCallback += OnAnimationEvent;
            }
        }
    }

    // Use this for initialization

    // Update is called once per frame
    #endregion

    #region Daily Achieve
    public void OnCompleteDailyAchieveResult(CDailyAchieve dailyAchieve, CReceivedGoods receivedGoods, bool isLevelUp)
    {
        if (dailyAchieve != null)
        {
            OnCompleteAchieveResult(receivedGoods, isLevelUp);
            SetDailyAchieve(dailyAchieve, receivedGoods, isLevelUp);
        }
    }

    public void SetDailyAchieve(CDailyAchieve dailyAchieve, CReceivedGoods receivedGoods, bool isLevelUp)
    {
        if (dailyAchieve != null)
        {
            isDaily = true;
            m_AchieveIndex = dailyAchieve.m_iAchieveIndex;

            if (receivedGoods != null)
            {
                m_ReceivedGoods = receivedGoods;
            }
            m_IsLevelUp = isLevelUp;

            DB_DailyAchieveList.Schema dailyAchieveList = DB_DailyAchieveList.Query(DB_DailyAchieveList.Field.Index, m_AchieveIndex);
            if (dailyAchieveList != null)
            {
                m_AchieveTypeImage.sprite = TextureManager.GetAchieveTypeSprite(dailyAchieveList.Achieve_Type);

                m_AchieveCompleteStepContainer.SetActive(false);

                DBStr_DailyAchieveString.Schema dailyAchieveString = DBStr_DailyAchieveString.Query(DBStr_DailyAchieveString.Field.Index, m_AchieveIndex);
                if (dailyAchieveString != null)
                {
                    SetAchieveTitleText(dailyAchieveString.TITLE_STRING);
                    m_AchieveDescriptionText.text = string.Format(dailyAchieveString.CONTENT_STRING, Languages.ToString(dailyAchieveList.Terms_Count));
                }
                int achieveAccumulate = 0;
                DailyAchieveBase dailyAchieveBase = Kernel.achieveManager.FindDailyAchieveBase(m_AchieveIndex);
                if (dailyAchieveBase != null)
                {
                    //Debug.Log(dailyAchieveString.TITLE_STRING + ", " + dailyAchieve.m_bIsComplete + ", " + dailyAchieveBase.isCompleted);
                    achieveAccumulate = dailyAchieveBase.achieveAccumulate;
                }
                else
                {
                    // 완료한 업적은 DailyAchieveBase를 생성하지 않기 때문에, CDailyAchieve의 값을 사용합니다.
                    achieveAccumulate = dailyAchieve.m_iAchieveAccumulatedAmount;
                }

                m_BadgeImage.enabled = (!dailyAchieve.m_bIsComplete && dailyAchieveBase != null && dailyAchieveBase.isCompleted);

                m_AchieveProgressSliderAnimator.isUse = !dailyAchieve.m_bIsComplete;
                m_AchieveProgressSliderAnimator.slider.maxValue = dailyAchieveList.Terms_Count;
                m_AchieveProgressSliderAnimator.slider.value = achieveAccumulate;
                SetAchieveRewardGoodsTypeSprite(dailyAchieveList.Goods_Type);
                m_AchieveRewardTooltipObject.content = Languages.ToString(dailyAchieveList.Goods_Type);
                m_AchieveRewardValueText.text = Languages.ToString(dailyAchieveList.Goods_Obtain);
                m_AchieveRewardCompleteImage.gameObject.SetActive(dailyAchieve.m_bIsComplete);

                string spriteName = string.Empty, text = string.Empty, shadowEffectColorName = string.Empty, outlineEffectColorName = string.Empty;
                if (dailyAchieve.m_bIsComplete)
                {
                    spriteName = "ui_button_disable";
                    text = Languages.ToString(TEXT_UI.FINISH);
                    shadowEffectColorName = "ui_button_05_shadow";
                    outlineEffectColorName = "ui_button_05_outline";
                }
                else
                {
                    if (dailyAchieveBase != null)
                    {
                        if (dailyAchieveBase.isCompleted)
                        {
                            spriteName = "ui_button_02";
                            text = Languages.ToString(TEXT_UI.GET);
                            shadowEffectColorName = "ui_button_02_shadow";
                            outlineEffectColorName = "ui_button_02_outline";
                        }
                        else
                        {
                            spriteName = "ui_button_disable";
                            text = Languages.ToString(TEXT_UI.ACHIEVE_ING);
                            shadowEffectColorName = "ui_button_05_shadow";
                            outlineEffectColorName = "ui_button_05_outline";
                        }
                    }
                    //else CRITICAL!
                }
                Color shadowEffectColor, outlineEffectColor;
                Kernel.colorManager.TryGetColor(shadowEffectColorName, out shadowEffectColor);
                Kernel.colorManager.TryGetColor(outlineEffectColorName, out outlineEffectColor);

                m_GetAchieveRewardButton.image.sprite = TextureManager.GetSprite(SpritePackingTag.Extras, spriteName);
                m_GetAchieveRewardButtonText.text = text;
                UIUtility.SetBaseMeshEffectColor(m_GetAchieveRewardButtonText.gameObject, true, shadowEffectColor, outlineEffectColor);

                isCompleted = dailyAchieve.m_bIsComplete;
                // 업적 완료 (보상 받은) 후 DailyAchieveBase는 제거되기 때문에, null 체크.
                acquirable = (dailyAchieveBase != null) ? dailyAchieveBase.isCompleted : false;
            }
        }
    }
    #endregion

    #region Achieve
    public void OnCompleteAchieveResult(int achieveGroup, byte achieveCompleteStep, int achieveAccumulate, CReceivedGoods receivedGoods, bool isLevelUp)
    {
        if (m_AchieveGroup != achieveGroup)
        {
            return;
        }

        if (receivedGoods != null)
        {
            m_ReceivedGoods = receivedGoods;
        }
        m_IsLevelUp = isLevelUp;

        // 목록 내의 다른 UIAchieveObject에 연출이 가려지는 문제가 있어, 해당 문제 해결을 위해 최상위로 올려줍니다.
        rectTransform.SetAsLastSibling();

        for (int i = 0; i < m_StarAnimators.Count; i++)
        {
            if ((i + 1) == achieveCompleteStep)
            {
                Animator animator = m_StarAnimators[i];
                if (animator != null)
                {
                    // ref. PUC-691
                    m_DirectingTransform = animator.transform;
                    m_ContainerTransform = m_StarContainers[i];
                    animator.transform.SetParent(m_AchieveListTransform, true);
                    StartCoroutine("Follow");

                    animator.gameObject.SetActive(true);
                    animator.SetTrigger("Star_Animation");
                }

                break;
            }
        }
    }

    public void SetAchieve(int achieveGroup, byte achieveCompleteStep, int achieveAccumulate)
    {
        isDaily = false;
        m_AchieveGroup = achieveGroup;

        DB_AchieveList.Schema achieveList = DB_AchieveList.Query(DB_AchieveList.Field.Achieve_Group, m_AchieveGroup,
                                                                 DB_AchieveList.Field.Achieve_Step, Mathf.Clamp(achieveCompleteStep + 1, 1, Kernel.entry.achieve.achieveLastStep));
        if (achieveList != null)
        {
            m_AchieveIndex = achieveList.Index;

            AchieveBase achieveBase = Kernel.achieveManager.FindAchieveBase(m_AchieveGroup); // NullRefExcpt 처리

            m_BadgeImage.enabled = (achieveBase != null && achieveBase.isCompleted);

            m_AchieveTypeImage.sprite = TextureManager.GetAchieveTypeSprite(achieveList.Achieve_Type);

            m_AchieveCompleteStepContainer.SetActive(true);
            for (int i = 0; i < m_StarAnimators.Count; i++)
            {
                Animator animator = m_StarAnimators[i];
                if (animator != null)
                {
                    // Animator has not been initialized.
                    /*
                    if (animator.isInitialized)
                    {
                        animator.SetTrigger("Normal");
                    }
                    */
                    animator.gameObject.SetActive(i < achieveCompleteStep);
                }
            }

            DBStr_AchieveString.Schema achieveString = DBStr_AchieveString.Query(DB_AchieveList.Field.Achieve_Group, m_AchieveGroup /*achieveList.Index*/);
            if (achieveString != null)
            {
                SetAchieveTitleText(achieveString.TITLE_STRING);
                m_AchieveDescriptionText.text = string.Format(achieveString.CONTENT_STRING, Languages.ToString(achieveList.Terms_COUNT));
            }

            m_AchieveProgressSliderAnimator.isUse = !(achieveCompleteStep >= Kernel.entry.achieve.achieveLastStep);
            m_AchieveProgressSliderAnimator.slider.maxValue = achieveList.Terms_COUNT;
            m_AchieveProgressSliderAnimator.slider.value = achieveAccumulate;
            SetAchieveRewardGoodsTypeSprite(achieveList.Goods_Type);
            m_AchieveRewardTooltipObject.content = Languages.ToString(achieveList.Goods_Type);
            m_AchieveRewardValueText.text = Languages.ToString(achieveList.Goods_Obtain);
            m_AchieveRewardCompleteImage.gameObject.SetActive(achieveCompleteStep >= Kernel.entry.achieve.achieveLastStep);

            string spriteName = string.Empty, text = string.Empty, shadowEffectColorName = string.Empty, outlineEffectColorName = string.Empty;
            if (achieveCompleteStep >= Kernel.entry.achieve.achieveLastStep)
            {
                spriteName = "ui_button_disable";
                text = Languages.ToString(TEXT_UI.FINISH);
                shadowEffectColorName = "ui_button_05_shadow";
                outlineEffectColorName = "ui_button_05_outline";
            }
            else if (achieveBase != null && achieveBase.isCompleted)
            {
                spriteName = "ui_button_02";
                text = Languages.ToString(TEXT_UI.GET);
                shadowEffectColorName = "ui_button_02_shadow";
                outlineEffectColorName = "ui_button_02_outline";
            }
            else
            {
                spriteName = "ui_button_disable";
                text = Languages.ToString(TEXT_UI.ACHIEVE_ING);
                shadowEffectColorName = "ui_button_05_shadow";
                outlineEffectColorName = "ui_button_05_outline";
            }
            Color shadowEffectColor, outlineEffectColor;
            Kernel.colorManager.TryGetColor(shadowEffectColorName, out shadowEffectColor);
            Kernel.colorManager.TryGetColor(outlineEffectColorName, out outlineEffectColor);

            m_GetAchieveRewardButton.image.overrideSprite = TextureManager.GetSprite(SpritePackingTag.Extras, spriteName);
            m_GetAchieveRewardButtonText.text = text;
            UIUtility.SetBaseMeshEffectColor(m_GetAchieveRewardButtonText.gameObject, true, shadowEffectColor, outlineEffectColor);

            isCompleted = (achieveCompleteStep >= Kernel.entry.achieve.achieveLastStep);
            // AchieveBase가 null이면, 완료된 업적?
            acquirable = (achieveBase != null) ? achieveBase.isCompleted : false;
        }
    }
    #endregion

    // ref. PUC-691
    IEnumerator Follow()
    {
        while (m_DirectingTransform != null && m_ContainerTransform != null)
        {
            m_DirectingTransform.position = m_ContainerTransform.position;

            yield return null;
        }

        yield break;
    }

    void OnAnimationEvent(string value)
    {
        if (!string.Equals("FX", value))
        {
            return;
        }

        // ref. PUC-691
        m_DirectingTransform.SetParent(m_ContainerTransform, true);
        m_DirectingTransform = null;
        m_ContainerTransform = null;
        //StopCoroutine("Follow");

        CAchieve achieve = Kernel.entry.achieve.FindAchieve(m_AchieveGroup);
        if (achieve != null)
        {
            SetAchieve(achieve.m_iAchieveGroup, achieve.m_byCompleteStep, achieve.m_iAchieveAccumulatedAmount);
            OnCompleteAchieveResult(m_ReceivedGoods, m_IsLevelUp);
        }

        if (onCompleteAchieveAnimationFinished != null)
        {
            onCompleteAchieveAnimationFinished();
        }

        interactable = true;
    }

    void OnCompleteAchieveResult(CReceivedGoods receivedGoods, bool isLevelUp)
    {
        if (receivedGoods != null)
        {
            switch (receivedGoods.m_eGoodsType)
            {
                case eGoodsType.SkillUpHealer:
                case eGoodsType.SkillUpHitter:
                case eGoodsType.SkillUpKeeper:
                case eGoodsType.SkillUpRanger:
                case eGoodsType.SkillUpWizard:
                    // 성급권은 우편으로 지급되므로, 메세지 처리합니다.
                    UINotificationCenter.Enqueue(Languages.ToString(TEXT_UI.MAIL_PROVIDE));
                    break;
                default:
                    DB_Goods.Schema goods = DB_Goods.Query(DB_Goods.Field.Index, receivedGoods.m_eGoodsType);
                    if (goods != null)
                    {
                        UIPopupReceive popupReceive = Kernel.uiManager.Get<UIPopupReceive>(UI.PopupReceive, true, false);
                        if (popupReceive != null)
                        {
                            popupReceive.SetData(eReceivePopupType.RT_ONE,
                                                 new List<Goods_Type>() { goods.Goods_Type },
                                                 new List<int>() { receivedGoods.m_iReceivedAmount },
                                                 isDaily ? Languages.ToString(TEXT_UI.MAIL_ACHIEVE_DAILY_REWARD) : Languages.ToString(TEXT_UI.MAIL_ACHIEVE_NORMAL_REWARD),
                                                 Languages.ToString(TEXT_UI.REWARD_RECEIVE));
                            Kernel.uiManager.Open(UI.PopupReceive);
                        }
                    }
                    break;
            }
        }

        if (isLevelUp)
        {
            Kernel.uiManager.Open(UI.LevelUp);
        }
    }

    void OnGetAchieveRewardButtonClick()
    {
        if (Kernel.entry != null)
        {
            SoundDataInfo.ChangeUISound(UISOUND.UIS_CANCEL_01, m_GetAchieveRewardButton.gameObject);

            if (isDaily)
            {
                CDailyAchieve dailyAchieve = Kernel.entry.achieve.FindDailyAchieve(m_AchieveIndex);
                if (dailyAchieve != null)
                {
                    if (dailyAchieve.m_bIsComplete)
                    {
                        NetworkEventHandler.OnNetworkException(Result_Define.eResult.ALREADY_COMPLETED_ACHIEVE);
                    }
                    else
                    {
                        DailyAchieveBase dailyAchieveBase = Kernel.achieveManager.FindDailyAchieveBase(m_AchieveIndex);
                        if (dailyAchieveBase != null)
                        {
                            if (dailyAchieveBase.isCompleted)
                            {
                                SoundDataInfo.RevertSound(m_GetAchieveRewardButton.gameObject);
                                interactable = false;
                                Kernel.entry.achieve.REQ_PACKET_CG_GAME_COMPLETE_DAILY_ACHIEVE_SYN(m_AchieveIndex);
                            }
                            else
                            {
                                NetworkEventHandler.OnNetworkException(Result_Define.eResult.NOT_ENOUGH_TERMS_COUNT);
                            }
                        }
                        else Debug.LogError(string.Format("DailyAchieveBase could not be found. (achieveIndex : {0})", m_AchieveIndex));
                    }
                }
                else Debug.LogError(string.Format("CDailyAchieve could not be found. (achieveIndex : {0})", m_AchieveIndex));
            }
            else
            {
                AchieveBase achieveBase = Kernel.achieveManager.FindAchieveBase(m_AchieveGroup);
                if (achieveBase != null)
                {
                    if (achieveBase.isCompleted)
                    {
                        SoundDataInfo.RevertSound(m_GetAchieveRewardButton.gameObject);
                        interactable = false;
                        Kernel.entry.achieve.REQ_PACKET_CG_GAME_COMPLETE_ACHIEVE_SYN(m_AchieveIndex);
                    }
                    else
                    {
                        NetworkEventHandler.OnNetworkException(Result_Define.eResult.NOT_ENOUGH_TERMS_COUNT);
                    }
                }
                else
                {
                    // 최종 achieveStep을 완료한 업적은 AchieveBase를 생성하지 않기 때문에,
                    // AchieveBase 컴포넌트가 없는 경우는 완료된 업적으로 처리합니다.
                    NetworkEventHandler.OnNetworkException(Result_Define.eResult.ALREADY_COMPLETED_ACHIEVE);
                }
            }
        }
    }

    void SetAchieveTitleText(string text)
    {
        // 목록 정렬 및 디버깅 편의를 위해 gameObject.name을 변경합니다.
        gameObject.name = m_AchieveTitleText.text = text;
        UIUtility.FitSizeToContent(m_AchieveTitleText);

        float margin = Mathf.Abs(m_AchieveTitleText.rectTransform.anchoredPosition.x);
        float width = m_AchieveTitleText.rectTransform.rect.width;
        m_AchieveTitleFrameImage.rectTransform.sizeDelta = new Vector2(width + (margin * 2f), m_AchieveTitleText.rectTransform.rect.height);
    }

    void SetAchieveRewardGoodsTypeSprite(Goods_Type goodsType)
    {
        Sprite rewardGoodsTypeSprite = null;
        Vector3 localScale = Vector3.one;

        //** 08.18. 랜덤 카드 모양을 사용하지 않고, 툴팁에 나오는 그대로 아이콘 이미지 표현으로 변경.
        rewardGoodsTypeSprite = TextureManager.GetGoodsTypeSprite(goodsType);

        switch (goodsType)
        {
            case Goods_Type.AccountExp:
                // 임시
                //rewardGoodsTypeSprite = TextureManager.GetGoodsTypeSprite(goodsType);
                localScale = new Vector3(.6f, .6f, 1f);
                break;
            case Goods_Type.SkillUpHealer:
            case Goods_Type.SkillUpHitter:
            case Goods_Type.SkillUpKeeper:
            case Goods_Type.SkillUpRanger:
            case Goods_Type.SkillUpWizard:
            case Goods_Type.EquipUpAccessory:
            case Goods_Type.EquipUpArmor:
            case Goods_Type.EquipUpWeapon:
                //rewardGoodsTypeSprite = TextureManager.GetSprite(SpritePackingTag.Extras, "item_shop_gacha_04"); // 임시
                localScale = new Vector3(.45f, .45f, 1f);
                break;
            default:
                //rewardGoodsTypeSprite = TextureManager.GetGoodsTypeSprite(goodsType);
                localScale = new Vector3(.9f, .9f, 1f);
                break;
        }

        m_AchieveRewardGoodsTypeImage.sprite = rewardGoodsTypeSprite;
        m_AchieveRewardGoodsTypeImage.SetNativeSize();
        m_AchieveRewardGoodsTypeImage.rectTransform.localScale = localScale;
    }

    void Listener(bool interactable)
    {
        if (m_GetAchieveRewardButton != null)
        {
            m_GetAchieveRewardButton.interactable = interactable;
        }
    }
}
