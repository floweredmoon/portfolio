using System.Collections.Generic;
using System.Text;
using ItemProtocolDef;
using UnityEngine;

[System.Serializable]
public class Linker : ObjectPool<Linker>
{
    [System.Serializable]
    public struct String
    {
        public string value;
        public int startIndex;
        public int endIndex;

        public void ReInit()
        {
            value = string.Empty;
            startIndex = 0;
            endIndex = 0;
        }
    }

    public int id;
    public object data;
    public String original;
    public String display;
    public bool nameLink;

    protected override void ReInit()
    {
        original.ReInit();
        display.ReInit();
        nameLink = false;
    }

    static public string TryParse(string value, ref List<Linker> linkers, int indexer = 0)
    {
        if (linkers != null && linkers.Count > 0)
        {
            foreach (Linker linker in linkers)
            {
                linker.Dispose();
            }

            linkers.Clear();
        }

        if (!string.IsNullOrEmpty(value))
        {
            int startIndex = 0;
            int lastIndex = 0;
            StringBuilder result = null;            

            while (startIndex != -1)
            {
                startIndex = value.IndexOf("<iteminfo>", startIndex);

                if (startIndex != -1)
                {
                    int endIndex = value.IndexOf("</iteminfo>", startIndex);

                    if (endIndex != -1)
                    {
                        if (result == null)
                        {
                            result = new StringBuilder(); // initialize capacity.
                        }

                        Linker linker = Linker.New();
                        string original = value.Substring(startIndex, (endIndex + 11) - startIndex); // 11 : </iteminfo>
                        linker.original.value = original;
                        linker.original.startIndex = startIndex;
                        linker.original.endIndex = startIndex + original.Length;

                        #region ItemProtocolDef.ItemInfo

                        original = original.Remove(0, 10); // 10 : <iteminfo>
                        original = original.Remove(original.Length - 11, 11); // 11 : </iteminfo>

                        string[] splitted = original.Split(',');
                        ItemInfo itemInfo = new ItemInfo();

                        for (int i = 0; i < splitted.Length; i++)
                        {
                            if (splitted[i].Contains("bindRange = "))
                            {
                                ITEM_BIND_RANGE itemBindRange = (ITEM_BIND_RANGE)System.Enum.Parse(typeof(ITEM_BIND_RANGE), splitted[i].Substring(12), true); // 12 : bindRange = 

                                itemInfo.bindRange = itemBindRange;
                            }
                            else if (splitted[i].Contains("durability = "))
                            {
                                float durability;

                                if (float.TryParse(splitted[i].Substring(13), out durability)) // 13 : durability = 
                                {
                                    itemInfo.durability = durability;
                                }
                            }
                            else if (splitted[i].Contains("itemNo = "))
                            {
                                uint itemNo;

                                if (uint.TryParse(splitted[i].Substring(9), out itemNo)) // 9 : itemNo = 
                                {
                                    itemInfo.itemNo = itemNo;
                                }
                            }
                            else if (splitted[i].Contains("maxDurability = "))
                            {
                                float maxDurability;

                                if (float.TryParse(splitted[i].Substring(16), out maxDurability)) // 16 : maxDurability = 
                                {
                                    itemInfo.maxDurability = maxDurability;
                                }
                            }
                            else if (splitted[i].Contains("strengthenLevel = "))
                            {
                                int strengthenLevel;

                                if (int.TryParse(splitted[i].Substring(18), out strengthenLevel)) // 18 : strengthenLevel = 
                                {
                                    itemInfo.strengthenLevel = strengthenLevel;
                                }
                            }
                            else if (splitted[i].Contains("optionNo = "))
                            {
                                int optionNo;

                                if (int.TryParse(splitted[i].Substring(11), out optionNo)) // 11 : optionNo = 
                                {
                                    int temporary;

                                    if (int.TryParse(splitted[i + 1].Substring(8), out temporary)) // 8 : value = 
                                    {
                                        ItemProtocolDef.ItemOption itemOption = new ItemProtocolDef.ItemOption();

                                        itemOption.optionNo = optionNo;
                                        itemOption.value = temporary;

                                        itemInfo.randomOptionList.Add(itemOption);
                                    }
                                }
                            }
                        }

                        #endregion

                        string display = "[" + Languages.GetItemName(itemInfo.itemNo) + "]";
                        linker.display.value = display;

                        result.Append(value.Substring(lastIndex, startIndex - lastIndex));
                        linker.display.startIndex = result.Length;
                        result.Append(display);
                        linker.display.endIndex = linker.display.startIndex + display.Length;

                        linker.data = itemInfo;
                        linker.id = indexer++;

                        linkers.Add(linker);

                        lastIndex = startIndex = endIndex + 11; // 11 : </iteminfo>
                    }
                    else
                    {
                        Debug.LogError(string.Format("String does not contains </iteminfo>. ({0})", value));
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

            //영주 이름 링크//
            {
                string[] splitted = value.Split(' ');
                if (splitted[0].Contains(UIChat.ToString(ChatProtocolDef.ChatType.ChatTypeLordNormal)))
                {
                    Linker linker = Linker.New();
                    linker.original.value = splitted[1];
                    linker.original.startIndex = value.IndexOf(splitted[1], 0);
                    linker.original.endIndex = linker.original.startIndex + splitted[1].Length;

                    linker.data = splitted[1];
                    linker.id = indexer++;
                    linker.nameLink = true;
                    linkers.Add(linker);
                }
            }

            return result != null ? result.ToString() : value;
        }

        return string.Empty;
    }

    static public ItemProtocolDef.ItemInfo ToItemInfo(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            int start = text.IndexOf("<iteminfo>");

            if (start != -1)
            {
                int end = text.IndexOf("</iteminfo>");

                if (end != -1)
                {
                    start = start + 10; // 10 : <iteminfo>
                    text = text.Substring(start, end - start);

                    string[] splitted = text.Split(',');
                    ItemProtocolDef.ItemInfo itemInfo = new ItemProtocolDef.ItemInfo();

                    for (int i = 0; i < splitted.Length; i++)
                    {
                        if (splitted[i].Contains(" bindRange = "))
                        {
                            int bindRange;

                            if (int.TryParse(splitted[i].Substring(12), out bindRange)) // 12 : bindRange = 
                            {
                                itemInfo.bindRange = (ItemProtocolDef.ITEM_BIND_RANGE)bindRange;
                            }
                        }
                        else if (splitted[i].Contains(" durability = "))
                        {
                            float durability;

                            if (float.TryParse(splitted[i].Substring(13), out durability)) // 13 : durability = 
                            {
                                itemInfo.durability = durability;
                            }
                        }
                        else if (splitted[i].Contains(" itemNo = "))
                        {
                            uint itemNo;

                            if (uint.TryParse(splitted[i].Substring(9), out itemNo)) // 9 : itemNo = 
                            {
                                itemInfo.itemNo = itemNo;
                            }
                        }
                        else if (splitted[i].Contains(" maxDurability = "))
                        {
                            float maxDurability;

                            if (float.TryParse(splitted[i].Substring(16), out maxDurability)) // 16 : maxDurability = 
                            {
                                itemInfo.maxDurability = maxDurability;
                            }
                        }
                        else if (splitted[i].Contains(" strengthenLevel = "))
                        {
                            int strengthenLevel;

                            if (int.TryParse(splitted[i].Substring(18), out strengthenLevel)) // 18 : strengthenLevel = 
                            {
                                itemInfo.strengthenLevel = strengthenLevel;
                            }
                        }
                        else if (splitted[i].Contains(" optionNo = "))
                        {
                            int optionNo;

                            if (int.TryParse(splitted[i].Substring(11), out optionNo)) // 11 : optionNo = 
                            {
                                int value;

                                if (int.TryParse(splitted[i + 1].Substring(8), out value)) // 8 : value = 
                                {
                                    ItemProtocolDef.ItemOption itemOption = new ItemProtocolDef.ItemOption();

                                    itemOption.optionNo = optionNo;
                                    itemOption.value = value;

                                    itemInfo.randomOptionList.Add(itemOption);
                                }
                            }
                        }
                    }

                    return itemInfo;
                }
            }
        }

        return null;
    }

    static public string ToString(ItemProtocolDef.ItemInfo itemInfo)
    {
        StringBuilder result = new StringBuilder(128);

        result.Append("<iteminfo>");
        result.AppendFormat("bindRange = {0}, durability = {1}, itemNo = {2}, maxDurability = {3}, strengthenLevel = {4}", itemInfo.bindRange, itemInfo.durability, itemInfo.itemNo, itemInfo.maxDurability, itemInfo.strengthenLevel);

        for (int i = 0; i < itemInfo.randomOptionList.Count; i++)
        {
            ItemOption itemOption = itemInfo.randomOptionList[i];

            result.AppendFormat(", optionNo = {0}, value = {1}", itemOption.optionNo, itemOption.value);
        }

        result.Append("</iteminfo>");

        return result.ToString();
    }
}
