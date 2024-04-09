using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorIndex;
    public FixedString64Bytes playerName;
    
    public bool Equals(PlayerData other)
    {
        return 
            clientId == other.clientId &&
            colorIndex == other.colorIndex &&
            playerName == other.playerName;
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return clientId.GetHashCode();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorIndex);
        serializer.SerializeValue(ref playerName);
    }
}
