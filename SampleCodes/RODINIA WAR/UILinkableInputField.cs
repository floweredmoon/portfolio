using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILinkableInputField : UIInputField
{
    protected List<Linker> linkers = new List<Linker>();
    protected string original = string.Empty;
    protected int lateCaretPosition;
    protected int lateCaretSelectPos;

    protected override void Awake()
    {
        base.Awake();
        onValidateInput += Listener;
    }

    // Use this for initialization

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        lateCaretPosition = caretPositionInternal;
        lateCaretSelectPos = caretSelectPositionInternal;

        bool left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.RightArrow);

        if (left || right)
        {
            Linker linker = FindLinkerContains(caretSelectPositionInternal);

            if (linker != null)
            {
                bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (left)
                {
                    if (shift)
                    {
                        if (caretSelectPositionInternal < linker.display.endIndex)
                        {
                            caretSelectPositionInternal = linker.display.startIndex;
                        }
                    }
                    else
                    {
                        if (caretPositionInternal < linker.display.endIndex)
                        {
                            caretPositionInternal = linker.display.startIndex;
                            caretSelectPositionInternal = linker.display.startIndex;
                        }
                    }
                }
                else if (right)
                {
                    if (shift)
                    {
                        if (caretSelectPositionInternal > linker.display.startIndex)
                        {
                            caretSelectPositionInternal = linker.display.endIndex;
                        }
                    }
                    else
                    {
                        if (caretPositionInternal > linker.display.startIndex)
                        {
                            caretPositionInternal = linker.display.endIndex;
                            caretSelectPositionInternal = linker.display.endIndex;
                        }
                    }
                }
            }
        }
    }

    public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        Linker linker = FindLinkerContains(caretPositionInternal);

        if (linker != null)
        {
            int front = caretPositionInternal - linker.display.startIndex;
            int back = linker.display.endIndex - caretPositionInternal;

            if (front < back)
            {
                caretPositionInternal = linker.display.startIndex;
                caretSelectPositionInternal = linker.display.startIndex;
            }
            else
            {
                caretPositionInternal = linker.display.endIndex;
                caretSelectPositionInternal = linker.display.endIndex;
            }
        }
    }

    public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnDrag(eventData);

        bool left = false, right = false;
        int front = -1, back = -1;

        if (caretPositionInternal < caretSelectPositionInternal)
        {
            right = true;
            front = caretPositionInternal;
            back = caretSelectPositionInternal;
        }
        else if (caretPositionInternal > caretSelectPositionInternal)
        {
            left = true;
            front = caretSelectPositionInternal;
            back = caretPositionInternal;
        }

        Linker linker = null;

        if (front != -1)
        {
            front++;
            linker = FindLinkerContains(front);

            if (linker != null)
            {
                int startIndex = linker.display.startIndex;

                if (left)
                {
                    caretSelectPositionInternal = startIndex;
                }
                else if (right)
                {
                    caretPositionInternal = startIndex;
                }
            }
        }

        if (back != -1)
        {
            back--;
            linker = FindLinkerContains(back);

            if (linker != null)
            {
                int endIndex = linker.display.endIndex;

                if (left)
                {
                    caretPositionInternal = endIndex;
                }
                else if (right)
                {
                    caretSelectPositionInternal = endIndex;
                }
            }
        }
    }

    protected Linker FindLinkerContains(int charIndex)
    {
        for (int i = 0; i < linkers.Count; i++)
        {
            Linker linker = linkers[i];

            if (linker.display.startIndex <= charIndex && linker.display.endIndex >= charIndex)
            {
                return linker;
            }
        }

        return null;

        //return linkers.Find(linker => linker.display.startIndex <= charIndex && linker.display.endIndex >= charIndex);
    }

    public void AppendLinker(ItemProtocolDef.ItemInfo itemInfo)
    {
        if (itemInfo != null)
        {
            string itemName = "[" + Languages.GetItemName(itemInfo.itemNo) + "]";

            if (characterLimit >= text.Length + itemName.Length)
            {
                Linker linker = Linker.New();
                linker.data = itemInfo;

                // TO DO : Linker.TryParse()를 사용하도록 수정한다.
                string itemLink = Linker.ToString(itemInfo);
                int startIndex = original.Length;
                int endIndex = original.Length + itemLink.Length;

                linker.original.value = itemLink;
                linker.original.startIndex = startIndex;
                linker.original.endIndex = endIndex;

                original += itemLink;
                startIndex = text.Length;
                endIndex = text.Length + itemName.Length;

                linker.display.value = itemName;
                linker.display.startIndex = startIndex;
                linker.display.endIndex = endIndex;

                text += itemName;

                if (Kernel.canvasManager != null)
                {
                    Kernel.canvasManager.FocusedInputField = this;
                }

                MoveTextEnd(false);

                linkers.Add(linker);
            }
        }
    }

    protected virtual char Listener(string text, int charIndex, char addedChar)
    {
        if (characterLimit >= text.Length + 1)
        {
            Linker linker = null;

            if (caretPositionInternal != caretSelectPositionInternal)
            {
                int front = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
                int back = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
                int startIndex = front;
                int endIndex = back;

                // 선택 영역보다 앞선 Linker를 찾는다.
                for (int i = front; i >= 0; i--)
                {
                    linker = FindLinkerContains(i);

                    if (linker != null)
                    {
                        break;
                    }
                }

                if (linker != null)
                {
                    // 선택 영역보다 앞선 Linker를 찾으면 startIndex, endIndex를 갱신한다.
                    if (startIndex > linker.display.endIndex) // >=
                    {
                        startIndex = linker.original.endIndex + (front - linker.display.endIndex);
                        endIndex = linker.original.endIndex + (back - linker.display.endIndex);
                    }
                }

                // 선택 영역에서 Linker를 찾는다.
                for (int i = front; i <= back; i++) // -> endIndex만 갱신하므로 (for int i = back; i >= front i--), break;
                {
                    linker = FindLinkerContains(i);

                    if (linker != null)
                    {
                        // 선택 영역에서 Linker를 찾으면 endIndex를 갱신한다.
                        if (i >= linker.display.endIndex)
                        {
                            endIndex = linker.original.endIndex + (back - linker.display.endIndex);
                            i = linker.display.endIndex;
                        }
                    }
                }

                original = original.Remove(startIndex, endIndex - startIndex);

                charIndex = front;
            }

            linker = FindLinkerContains(charIndex);

            if (linker != null)
            {
                if (charIndex == linker.display.startIndex)
                {
                    original = original.Insert(linker.original.startIndex, addedChar.ToString());
                }
                else if (charIndex >= linker.display.endIndex)
                {
                    original = original.Insert(linker.original.endIndex, addedChar.ToString());
                }
            }
            else
            {
                for (int i = charIndex; i >= 0; i--)
                {
                    linker = FindLinkerContains(i);

                    if (linker != null)
                    {
                        break;
                    }
                }

                //if (char.GetUnicodeCategory(addedChar) == System.Globalization.UnicodeCategory.OtherLetter)
                if (text.Length < charIndex)
                {
                    charIndex = text.Length;
                }

                if (linker != null)
                {
                    charIndex = linker.original.endIndex + (charIndex - linker.display.endIndex);
                }

                original = original.Insert(charIndex, addedChar.ToString());
            }

            Linker.TryParse(original, ref linkers);
        }

        return addedChar;
    }

    protected override void Listener(string value)
    {
        bool backspace = Input.GetKeyDown(KeyCode.Backspace) || Input.GetKey(KeyCode.Backspace);
        bool delete = Input.GetKeyDown(KeyCode.Delete) || Input.GetKey(KeyCode.Delete);

        if (backspace || delete)
        {
            Linker linker = null;
            int startIndex = 0;
            int length = 0;
            bool seperated = false;

            // TO DO : RightArrow, LeftArrow 통합 처리, 나눌 필요가 없음.
            #region (KeyCode.LeftShift || KeyCode.RightShift) + KeyCode.RightArrow
            if (caretPositionInternal < lateCaretSelectPos && lateCaretPosition != lateCaretSelectPos)
            {
                int front = Mathf.Min(caretPositionInternal, lateCaretSelectPos);
                int back = Mathf.Max(caretPositionInternal, lateCaretSelectPos);
                startIndex = front;
                int endIndex = back;

                for (int i = front; i >= 0; i--)
                {
                    linker = FindLinkerContains(i);

                    if (linker != null)
                    {
                        break;
                    }
                }

                if (linker != null)
                {
                    if (startIndex >= linker.display.endIndex)
                    {
                        startIndex = linker.original.endIndex + (front - linker.display.endIndex);
                        endIndex = linker.original.endIndex + (back - linker.display.endIndex);
                    }
                }

                for (int i = front; i <= back; i++)
                {
                    linker = FindLinkerContains(i);

                    if (linker != null)
                    {
                        if (i >= linker.display.endIndex)
                        {
                            endIndex = linker.original.endIndex + (back - linker.display.endIndex);
                            i = linker.display.endIndex;
                        }
                    }
                }

                length = endIndex - startIndex;
            }
            #endregion

            #region (KeyCode.LeftShift || KeyCode.RightShift) + KeyCode.LeftArrow
            else if (lateCaretPosition > caretSelectPositionInternal && lateCaretPosition != lateCaretSelectPos)
            {
                int front = Mathf.Min(caretSelectPositionInternal, lateCaretPosition);
                int back = Mathf.Max(caretSelectPositionInternal, lateCaretPosition);
                startIndex = front;
                int endIndex = back;

                for (int i = front; i >= 0; i--)
                {
                    linker = FindLinkerContains(i);

                    if (linker != null)
                    {
                        break;
                    }
                }

                if (linker != null)
                {
                    if (startIndex >= linker.display.endIndex)
                    {
                        startIndex = linker.original.endIndex + (front - linker.display.endIndex);
                        endIndex = linker.original.endIndex + (back - linker.display.endIndex);
                    }
                }

                for (int i = front; i <= back; i++)
                {
                    linker = FindLinkerContains(i);

                    if (linker != null)
                    {
                        if (i >= linker.display.endIndex)
                        {
                            endIndex = linker.original.endIndex + (back - linker.display.endIndex);
                            i = linker.display.endIndex;
                        }
                    }
                }

                length = endIndex - startIndex;
            }
            #endregion

            #region caretPosition == caretSelectPos
            else
            {
                if (backspace)
                {
                    for (int i = lateCaretPosition; i >= 0; i--)
                    {
                        linker = FindLinkerContains(i);

                        if (linker != null)
                        {
                            break;
                        }
                    }
                }
                else if (delete)
                {
                    for (int i = lateCaretPosition; i >= 0; i--)
                    {
                        linker = FindLinkerContains(i);

                        if (linker != null)
                        {
                            break;
                        }
                    }
                }

                if (linker != null)
                {
                    startIndex = caretPositionInternal;
                    length = 1;

                    if (backspace)
                    {
                        if (lateCaretPosition == linker.display.startIndex)
                        {
                            startIndex = linker.original.startIndex - 1; // WARNING!
                            length = 1;
                        }
                        else if (lateCaretPosition == linker.display.endIndex)
                        {
                            string insert = linker.display.value.Substring(0, linker.display.value.Length - 1);

                            startIndex = linker.original.startIndex;
                            length = linker.original.endIndex - linker.original.startIndex;

                            original = original.Remove(startIndex, length);
                            original = original.Insert(startIndex, insert);

                            seperated = true;
                        }
                        else
                        {
                            startIndex = linker.original.endIndex + (caretPositionInternal - linker.display.endIndex);
                            length = 1;
                        }
                    }
                    else if (delete)
                    {
                        if (caretPositionInternal == linker.display.startIndex)
                        {
                            string insert = linker.display.value.Substring(1, linker.display.value.Length - 1);

                            startIndex = linker.original.startIndex;
                            length = linker.original.endIndex - linker.original.startIndex;

                            original = original.Remove(startIndex, length);
                            original = original.Insert(startIndex, insert);

                            seperated = true;
                        }
                        else
                        {
                            startIndex = linker.original.endIndex + (caretPositionInternal - linker.display.endIndex);
                            length = 1;
                        }
                    }
                }
                else
                {
                    startIndex = caretPositionInternal;
                    length = 1;
                }
            }
            #endregion

            if (!seperated)
            {
                original = original.Remove(startIndex, length);
            }

            Linker.TryParse(original, ref linkers);
        }

        base.Listener(value);
    }

#if UNITY_EDITOR

    [UnityEditor.MenuItem("User Interface/InputField to UILinkableInputField")]
    static public void Convert()
    {
        GameObject[] gameObjects = UnityEditor.Selection.gameObjects;

        for (int i = 0; i < gameObjects.Length; i++)
        {
            InputField deprecated = gameObjects[i].GetComponent<InputField>();

            if (deprecated != null)
            {
                bool interactable = deprecated.interactable;
                Graphic targetGraphic = deprecated.targetGraphic;
                Transition transition = deprecated.transition;
                SpriteState spriteState = new SpriteState();
                ColorBlock colors = new ColorBlock();
                AnimationTriggers animationTriggers = null;
                Navigation navigation = deprecated.navigation;
                Text textComponent = deprecated.textComponent;
                string text = deprecated.text;
                int characterLimit = deprecated.characterLimit;
                ContentType contentType = deprecated.contentType;
                LineType lineType = deprecated.lineType;
                Graphic placeholder = deprecated.placeholder;
                float caretBlinkRate = deprecated.caretBlinkRate;
                Color selectionColor = deprecated.selectionColor;
                bool shouldHideMobileInput = deprecated.shouldHideMobileInput;
                TouchScreenKeyboardType keyboardType = deprecated.keyboardType;
                CharacterValidation characterValidation = deprecated.characterValidation;

                switch (transition)
                {
                    case Transition.Animation:
                        animationTriggers = deprecated.animationTriggers;
                        break;
                    case Transition.ColorTint:
                        colors = deprecated.colors;
                        break;
                    case Transition.SpriteSwap:
                        spriteState = deprecated.spriteState;
                        break;
                }

                DestroyImmediate(deprecated);

                UILinkableInputField linkableInputField = gameObjects[i].AddComponent<UILinkableInputField>();

                if (linkableInputField != null)
                {
                    linkableInputField.interactable = interactable;
                    linkableInputField.targetGraphic = targetGraphic;
                    linkableInputField.transition = transition;

                    switch (transition)
                    {
                        case Transition.Animation:
                            linkableInputField.animationTriggers = animationTriggers;
                            break;
                        case Transition.ColorTint:
                            linkableInputField.colors = colors;
                            break;
                        case Transition.SpriteSwap:
                            linkableInputField.spriteState = spriteState;
                            break;
                    }

                    linkableInputField.navigation = navigation;
                    linkableInputField.textComponent = textComponent;
                    linkableInputField.text = linkableInputField.text;
                    linkableInputField.characterLimit = characterLimit;
                    linkableInputField.contentType = contentType;
                    linkableInputField.lineType = lineType;
                    linkableInputField.placeholder = placeholder;
                    linkableInputField.caretBlinkRate = caretBlinkRate;
                    linkableInputField.selectionColor = selectionColor;
                    linkableInputField.shouldHideMobileInput = shouldHideMobileInput;

                    Debug.Log(gameObjects[i].name + " has been successfully converted to UIInputField.");
                }
                else Debug.LogError("Failed to add InputField component to " + gameObjects[i].name + ".");
            }
            else Debug.LogError(gameObjects[i].name + " is not contains InputField component.");
        }
    }
#endif
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(UILinkableInputField))]
public class UILinkableInputFieldInspector : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

#endif
