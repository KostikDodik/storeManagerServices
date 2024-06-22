namespace Model.Extentions;

public interface ISpecificInfoToDbEntity
{
    /// <summary>
    /// If expected <see cref="IModelEntity"/> inheritor passed, fills it's specific fields
    /// </summary>
    /// <param name="entity"> entity to fill fields in</param>
    void Pass(IDbEntity entity);
}