using Netick;



namespace CockleBurs.GameFramework.Utility
{
[Networked,System.Serializable]
public struct PlayerRef
{
    /// <summary>
    /// Raw Player Id
    /// </summary>
    [Networked]public ulong Id { get; set; }

    /// <summary>
    /// only more than 0 is considered as Valid
    /// </summary>
    public bool IsValid => Id > 0;

    /// <summary>
    /// Default Value
    /// </summary>
    public static PlayerRef None => default;

    /// <summary>
    /// Only Create on OnClientConnected OR By getting other PlayerRef.Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static PlayerRef Create(ulong id) { return new PlayerRef() { Id = id }; }

    public static implicit operator string(PlayerRef playerId)
    {
        return $"[PlayerRef: {playerId.Id}]";
    }
    public static explicit operator PlayerRef(ulong playerId)
    {
        return Create(playerId);
    }
    public static implicit operator ulong(PlayerRef playerId)
    {
        return playerId.Id;
    }
    public static bool operator ==(PlayerRef ref1, PlayerRef ref2)
    {
        return ref1.Id == ref2.Id;
    }
    public static bool operator !=(PlayerRef ref1, PlayerRef ref2)
    {
        return ref1.Id != ref2.Id;
    }
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}


[Networked]
public struct EmptyRef
{

}
}