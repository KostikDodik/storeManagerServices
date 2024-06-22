namespace Model.Extentions;

public interface ISpecificInfoFromDbEntity
{
    bool KeepOriginalValues { get; }
    /// <summary>
    /// If expected <see cref="IDbEntity"/> inheritor passed, fills it's specific fields
    /// </summary>
    /// <param name="dbEntity"> entity to fill fields in</param>
    void Fill(IDbEntity dbEntity);
}