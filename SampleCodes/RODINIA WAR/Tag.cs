using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class Tag : ObjectPool<Tag>
{
    public const string STARTER = "{#", FINISHER = "#}";

    public int id;
    public string original;
    public string display;
    public int startIndex;
    //public int endIndex;
    public List<Rect> rectangles = new List<Rect>();

    public int Length
    {
        get
        {
            return display.Length;
        }
    }

    protected override void ReInit()
    {
        original = string.Empty;
        display = string.Empty;
        startIndex = 0;
        rectangles.Clear();
    }

    static public string Size(string value, int size)
    {
        return string.Format("<size={0}>{1}</size>", size, value);
    }

    static public string Uncolored(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            int startIndex = 0;
            int lastIndex = 0;
            StringBuilder result = null;

            while (startIndex != -1)
            {
                startIndex = value.IndexOf("<color=", startIndex);

                if (startIndex != -1)
                {
                    int endIndex = value.IndexOf("</color>", startIndex);

                    if (endIndex != -1)
                    {
                        if (result == null)
                        {
                            result = new StringBuilder(); // initialize capacity.
                        }

                        result.Append(value.Substring(lastIndex, startIndex - lastIndex));
                        result.Append(value.Substring(startIndex + 17, endIndex - (startIndex + 17))); // 17 : <color=#????????>

                        lastIndex = startIndex = endIndex + 8; // 8 : </color>
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (result != null)
                    {
                        result.Append(value.Substring(lastIndex, value.Length - lastIndex));
                    }
                }
            }

            return result != null ? result.ToString() : value;
        }

        return string.Empty;
    }

    static public string Color(string value, Color32 color)
    {
        return Color(value, color.r, color.g, color.b, color.a);
    }

    static public string Color(string value, byte r, byte g, byte b, byte a)
    {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", r, g, b, a, value);
    }

    static public bool TryParse(string value, out int id)
    {
        if (!string.IsNullOrEmpty(value))
        {
            int startIndex = value.IndexOf("<id>");

            if (startIndex != -1)
            {
                int endIndex = value.IndexOf("</id>", startIndex);

                if (endIndex != -1)
                {
                    startIndex = startIndex + 4; // 4 : <id>
                    value = value.Substring(startIndex, endIndex - startIndex);

                    if (int.TryParse(value, out id))
                    {
                        return true;
                    }
                }
            }
        }

        id = 0;

        return false;
    }

    static public bool TryParse(string value, out string result)
    {
        if (!string.IsNullOrEmpty(value))
        {
            int startIndex = value.IndexOf("<content>");

            if (startIndex != -1)
            {
                int endIndex = value.IndexOf("</content>", startIndex);

                if (endIndex != -1)
                {
                    startIndex = startIndex + 9; // 9 : <content>
                    result = value.Substring(startIndex, endIndex - startIndex);

                    return true;
                }
            }
        }

        result = string.Empty;

        return false;
    }

    static public string ToString(string value, int id)
    {
        return string.Format("{0}<content>{1}</content><id>{2}</id>{3}", STARTER, value, id, FINISHER);
    }

    private bool GetRectangle(List<UnityEngine.UICharInfo> charInfos, List<UILineInfo> lineInfos, int charIndex, out Rect rectangle)
    {
        if (charInfos != null && charInfos.Count > 0 && lineInfos != null && lineInfos.Count > 0)
        {
            UnityEngine.UICharInfo charInfo = charInfos[charIndex];
            UILineInfo lineInfo;

            if (GetLineInfo(lineInfos, charIndex, out lineInfo))
            {
                float left = charInfo.cursorPos.x;
                float top = charInfo.cursorPos.y - lineInfo.height;
                float width = charInfo.charWidth;
                float height = lineInfo.height;

                // Text.rectTransform.sizeDelta -> Text.rectTransform.rect
                //if (sizeDelta.x >= width && sizeDelta.y >= height)
                {
                    rectangle = new Rect(left, top, width, height);

                    return true;
                }
            }
        }

        rectangle = new Rect();

        return false;
    }

    private bool GetLineInfo(List<UILineInfo> lineInfos, int charIndex, out UILineInfo lineInfo)
    {
        if (lineInfos != null && lineInfos.Count > 0)
        {
            for (int i = 0; i < lineInfos.Count; i++)
            {
                lineInfo = lineInfos[i];

                if (i < lineInfos.Count - 1)
                {
                    if (lineInfos[i].startCharIdx <= charIndex && lineInfos[i + 1].startCharIdx > charIndex)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        lineInfo = new UILineInfo();

        return false;
    }

    public bool Contains(Vector2 localPosition)
    {
        for (int i = 0; i < rectangles.Count; i++)
        {
            Rect rectangle = rectangles[i];

            if (rectangle.Contains(localPosition))
            {
                return true;
            }
        }

        return false;
        //return rectangles.Find(item => item.Contains(localPosition)) != null;
    }

    public void Renewal(List<UnityEngine.UICharInfo> charInfos, List<UILineInfo> lineInfos)
    {
        rectangles.Clear();

        for (int i = startIndex; i < startIndex + Length; i++)
        {
            Rect rectangle;

            if (GetRectangle(charInfos, lineInfos, i, out rectangle))
            {
                rectangles.Add(rectangle);
            }
        }
    }
}
