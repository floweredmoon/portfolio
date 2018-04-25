using Common.Packet;
using Common.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

public class PacketBroadcaster : Node
{

    class PacketListenerInfo
    {
        public Type packetType
        {
            get;
            set;
        }

        public object target
        {
            get;
            set;
        }

        public MethodInfo method
        {
            get;
            set;
        }

        public object Execute(object obj)
        {
            if (method != null && target != null)
            {
                return method.Invoke(target, new object[] { obj });
            }

            return null;
        }
    }

    Dictionary<int, PacketListenerInfo> m_PacketListenerInfos = new Dictionary<int, PacketListenerInfo>();

    public delegate void PacketListener<T>(T packet) where T : PACKET_BASE, new();

    public delegate void OnPacketReceive(PACKET_BASE packetBase);
    public OnPacketReceive onPacketReceive;

    public bool Broadcast(PACKET_BASE packetBase, ref Result_Define.eResult result)
    {
        if (packetBase != null)
        {
            if (!ResultHandler(packetBase, ref result))
            {
                PacketListenerInfo packetListenerInfo = FindPacketListenerInfo((byte)packetBase.m_Category, packetBase.m_PacketIndex);
                if (packetListenerInfo != null)
                {
                    packetListenerInfo.Execute(packetBase);

                    return true;
                }
                else
                {
                    LogError("{0} ({1}, {2})", packetBase.GetType(), packetBase.m_Category, packetBase.m_PacketIndex);
                }
            }
        }

        return false;
    }

    bool ResultHandler(PACKET_BASE packetBase, ref Result_Define.eResult result)
    {
        if (packetBase != null)
        {
            PropertyInfo propertyInfo = packetBase.GetType().GetProperty("Result");
            if (propertyInfo != null)
            {
                return ((result = (Result_Define.eResult)propertyInfo.GetValue(packetBase, null)) != Result_Define.eResult.SUCCESS);
            }
        }

        return false;
    }

    PacketListenerInfo FindPacketListenerInfo(byte packetCategory, byte packetIndex)
    {
        int packetKey = GeneratePacketKey(packetCategory, packetIndex);
        if (m_PacketListenerInfos.ContainsKey(packetKey))
        {
            return m_PacketListenerInfos[packetKey];
        }

        return null;
    }

    public bool AddPacketListener<T>(PacketListener<T> packetListener) where T : PACKET_BASE, new()
    {
        T instance = new T();
        FieldInfo[] fieldInfos = typeof(T).GetFields();
        byte packetCateogry = 0, packetIndex = 0;
        for (int i = 0; i < fieldInfos.Length; i++)
        {
            FieldInfo fieldInfo = fieldInfos[i];
            if (Equals("m_Category", fieldInfo.Name))
            {
                packetCateogry = (byte)fieldInfo.GetValue(instance);
            }
            else if (Equals("m_PacketIndex", fieldInfo.Name))
            {
                packetIndex = (byte)fieldInfo.GetValue(instance);
            }
        }

        if (packetCateogry != 0 && packetIndex != 0)
        {
            int packetKey = GeneratePacketKey(packetCateogry, packetIndex);
            if (!m_PacketListenerInfos.ContainsKey(packetKey))
            {
                m_PacketListenerInfos.Add(packetKey,
                    new PacketListenerInfo()
                    {
                        packetType = typeof(T),
                        target = packetListener.Target,
                        method = packetListener.Method,
                    });

                return true;
            }
            else
            {
                LogError("{0}, {1}, {2}, {3}", typeof(T), (ePACKET_CATEGORY)packetCateogry, packetIndex, packetKey);
            }
        }
        else
        {
            LogError("{0}", typeof(T));
        }

        return false;
    }

    int GeneratePacketKey(byte packetCategory, byte packetIndex)
    {
        return (int)((packetCategory << 24) | packetIndex);
    }
}
