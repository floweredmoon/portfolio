using System.Collections.Generic;
using ItemProtocolDef;
using ProtoTypeDefine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIText : Text, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool parse;
    public bool nativeSize;
    Tag highlightTag = new Tag();
    int lordTagId = -1;

    private List<UnityEngine.UICharInfo> charInfos = new List<UnityEngine.UICharInfo>();
    private List<UILineInfo> lineInfos = new List<UILineInfo>();

    private TextGenerationSettings TextGenerationSettings
    {
        get
        {
            return GetGenerationSettings(rectTransform.sizeDelta);
        }
    }

    private List<Tag> tags = new List<Tag>();
    private List<Linker> linkers = new List<Linker>();

    private string value;

    // Text.text is virtual now (4.6.3f1).
    public string Value
    {
        get
        {
            return value;
        }

        set
        {
            Parse(value);

            if (nativeSize)
            {
                SetNativeSize();
            }
        }
    }
    private lordNo_t objectNo;
    public lordNo_t ObjectNo
    {
        get
        {
            return objectNo;
        }
        set
        {
            objectNo = value;
        }
    }

    private bool isPointerStayed;

    public delegate void OnPointerEntered(UIText value);
    public OnPointerEntered onPointerEntered;

    public delegate void OnPointerExited(UIText value);
    public OnPointerExited onPointerExited;

    public delegate void OnPointerStayed(UIText value);
    public OnPointerStayed onPointerStayed;

    // Use this for initialization

    // Update is called once per frame
    private void Update()
    {
        if (isPointerStayed)
        {
            if (onPointerStayed != null)
            {
                onPointerStayed(this);
            }
            LordNameLinkUpdate();
        }
    }

    void LordNameLinkUpdate()
    {
        if (tags != null && tags.Count > 0)
        {
            Vector3 localPosition = transform.InverseTransformPoint(Kernel.canvasManager.commonCanvasCamera.ScreenToWorldPoint(Input.mousePosition));
            Tag tag = FindTag(localPosition);

            if (tag != null && tag.id != lordTagId)
                return;

            if (tag != null && highlightTag == null)
            {
                Linker linker = FindLinker(tag.id);
                SetLordNameLink(linker, tag, true);
            }
            else if (highlightTag != tag)
            {
                Linker linker = FindLinker(highlightTag.id);
                SetLordNameLink(linker, tag, false);
                highlightTag = null;
            }
        }
    }

    void SetLordNameLink(Linker linker, Tag tag, bool over)
    {
        if (linker != null && linker.data != null)
        {
            if (linker.nameLink == true)
            {
                string str = text.Clone().ToString();
                string insertString = over ? string.Format("<color=#FF9600>{0}</color>", linker.original.value) : string.Format("<color=#F6D882FF>{0}</color>", linker.original.value);
                string[] splitted = str.Split(' ');
                int startIndex = splitted[0].Length + 1;
                str = str.Remove(startIndex, splitted[1].Length);
                str = str.Insert(startIndex, insertString);
                text = str;
                highlightTag = tag;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tags != null && tags.Count > 0)
        {
            Vector3 localPosition = transform.InverseTransformPoint(eventData.pressEventCamera.ScreenToWorldPoint(eventData.pressPosition));
            Tag tag = FindTag(localPosition);

            if (tag != null)
            {
                Linker linker = FindLinker(tag.id);

                if (linker != null && linker.data != null)
                {
                    if (linker.data is ItemInfo)
                    {
                        ProcessOf(linker.data as ItemInfo);
                    }
                    else if (linker.data is string)
                    {
                        if (ObjectNo != 0)
                        {
                            var lordinfo = Kernel.canvasManager.Get<UILordPopup>(UI.LordPopup, true);

                            if (lordinfo != null)
                            {
                                lordinfo.SetOtherLordInfo(linker.original.value, ObjectNo);
                                Kernel.canvasManager.Open<UILordPopup>(UI.LordPopup);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log(linker.data.GetType());
                    }
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointerEntered != null)
        {
            onPointerEntered(this);
        }

        isPointerStayed = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerStayed = false;

        if (onPointerExited != null)
        {
            onPointerExited(this);
        }

        if (highlightTag != null)
        {
            Linker linker = FindLinker(highlightTag.id);

            SetLordNameLink(linker, null, false);
            highlightTag = null;
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < tags.Count; i++)
        {
            tags[i].Dispose();
        }

        tags.Clear();

        for (int i = 0; i < linkers.Count; i++)
        {
            linkers[i].Dispose();
        }

        linkers.Clear();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeGameObject != gameObject)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(rectTransform.TransformPoint(rectTransform.rect.center), rectTransform.rect.size * (canvas.transform.localScale.x * rectTransform.localScale.x));

        foreach (Tag tag in tags)
        {
            foreach (Rect rect in tag.rectangles)
            {
                Vector3 center = rectTransform.TransformPoint(rect.center);
                float scaleFactor = canvas.transform.localScale.x;
                Vector3 size = rect.size;
                size.x = size.x * rectTransform.localScale.x;
                size.y = size.y * rectTransform.localScale.y;
                size.z = size.z * rectTransform.localScale.z;
                size = size * scaleFactor;

                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
#endif

    private Linker FindLinker(int id)
    {
        return linkers.Find(item => int.Equals(item.id, id));
    }

    private Tag FindTag(Vector2 localPosition)
    {
        return tags.Find(item => item.Contains(localPosition));
    }

    private Tag FindTag(int index)
    {
        return tags.Find(item => item.startIndex <= index && item.Length > index);
    }

    private void SetNativeSize()
    {
        if (!nativeSize)
        {
            return;
        }

        rectTransform.sizeDelta = new Vector2((TextGenerationSettings.horizontalOverflow != HorizontalWrapMode.Overflow) ? Mathf.Min(preferredWidth, rectTransform.sizeDelta.x) : preferredWidth, rectTransform.sizeDelta.y);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, (TextGenerationSettings.verticalOverflow != VerticalWrapMode.Overflow) ? Mathf.Min(preferredHeight, rectTransform.sizeDelta.y) : preferredHeight);
    }

    #region deprecated
    float GetPreferredWidth()
    {
        float preferredWidth = 0f;

        if (charInfos != null && charInfos.Count > 0)
        {
            for (int i = 0; i < charInfos.Count; i++)
            {
                UnityEngine.UICharInfo charInfo = charInfos[i];

                if (preferredWidth < charInfo.charWidth + charInfo.cursorPos.x)
                {
                    preferredWidth = charInfo.charWidth + charInfo.cursorPos.x;
                }
            }
        }

        return preferredWidth;
    }

    float GetPreferredHeight()
    {
        float preferredHeight = 0f;

        for (int i = 0; i < lineInfos.Count; i++)
        {
            preferredHeight = preferredHeight + lineInfos[i].height;
        }

        return preferredHeight;
    }
    #endregion

    void Parse(string value)
    {
        this.value = value;

        if (parse)
        {
            value = Linker.TryParse(value, ref linkers);
        }

        tags.ForEach(item => item.Dispose());
        tags.Clear();

        bool completed = true;

        if (parse && !string.IsNullOrEmpty(value))
        {
            int startIndex = 0;

            while (startIndex != -1)
            {
                startIndex = value.IndexOf(Tag.STARTER, startIndex);

                if (startIndex != -1)
                {
                    int endIndex = value.IndexOf(Tag.FINISHER, startIndex);

                    if (endIndex != -1)
                    {
                        Tag tag = Tag.New();

                        tag.startIndex = startIndex;
                        tag.original = value.Substring(startIndex, (endIndex + Tag.FINISHER.Length) - startIndex);

                        if (Tag.TryParse(tag.original, out tag.display))
                        {

                        }
                        else
                        {
                            Debug.LogError(string.Format("String does not contains <content> or </content>. ({0})", tag.original));
                            value = value.Remove(startIndex, (endIndex + Tag.FINISHER.Length) - startIndex);
                            completed = false;
                            break;
                        }

                        if (Tag.TryParse(tag.original, out tag.id))
                        {

                        }
                        else
                        {
                            Debug.LogError(string.Format("String does not contains <id> or </id>. ({0})", tag.original));
                            value = value.Remove(startIndex, (endIndex + Tag.FINISHER.Length) - startIndex);
                            completed = false;
                            break;
                        }

                        if (completed)
                        {
                            value = value.Remove(startIndex, (endIndex + Tag.FINISHER.Length) - startIndex);
                            value = value.Insert(startIndex, tag.display);

                            tags.Add(tag);
                        }
                        else
                        {
                            completed = true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //영주 이름 태그 추가//
            startIndex = 0;
            string[] splitted = value.Split(' ');
            if (splitted[0].Contains(UIChat.ToString(ChatProtocolDef.ChatType.ChatTypeLordNormal)))
            {
                startIndex = value.IndexOf(splitted[1], startIndex);

                if (startIndex != -1)
                {
                    int endIndex = startIndex + splitted[1].Length;

                    if (endIndex != -1)
                    {
                        Tag tag = Tag.New();

                        tag.startIndex = startIndex;
                        tag.original = splitted[1];
                        tag.display = splitted[1];
                        tag.id = tags.Count;
                        lordTagId = tag.id;
                        tags.Add(tag);
                    }
                }
            }
        }

        text = value;

        Renewal();

        SetLordNameLink(FindLinker(lordTagId), null, false);
    }

    private void Renewal()
    {
        cachedTextGeneratorForLayout.Populate(m_Text, TextGenerationSettings);
        cachedTextGeneratorForLayout.GetCharacters(charInfos);
        cachedTextGeneratorForLayout.GetLines(lineInfos);

        for (int i = 0; i < tags.Count; i++)
        {
            tags[i].Renewal(charInfos, lineInfos);
        }
    }

    private void ProcessOf(ItemInfo itemInfo)
    {
        if (itemInfo != null)
        {
            UILinkItemTooltip linkItemTooltip = Kernel.canvasManager.Get<UILinkItemTooltip>(UI.LinkItemTooltip, true);

            if (linkItemTooltip != null)
            {
                ItemTooltipContent itemTooltipContent = ItemTooltipContent.New();
                itemTooltipContent.itemInfo = itemInfo;
                itemTooltipContent.Display(linkItemTooltip);

                linkItemTooltip.target = rectTransform;
                Kernel.canvasManager.Open(UI.LinkItemTooltip);
            }
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(UIText))]
public class UITextInspector : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif
