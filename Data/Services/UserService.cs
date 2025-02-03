/*using Microsoft.EntityFrameworkCore;
using Model.Database;
using Model.Extentions;

namespace Data.Services;

public interface IUserService
{
    List<User> GetAll();
    User Get(Guid id);
    void Add(User user);
    void Update(User user);
    void Delete(User user);
    void Delete(Guid userId);
    void AssignRole(Guid userId, Guid roleId);
    void RemoveRole(Guid userId, Guid roleId);
    void ChangePassword(Guid userId, byte[] newHash);
}

internal class UserService(DataDbContext dataBase) : IUserService
{
    void IUserService.Add(User user)
    {
        dataBase.Users.Add(user);
        dataBase.SaveChanges();
    }

    void IUserService.Update(User user)
    {
        var existing = dataBase.Users.FirstOrDefault(c => c.Id == user.Id);
        if (existing == null)
        {
            return;
        }

        existing.CopyPossibleProperties(user);
        dataBase.Users.Update(existing);
        dataBase.SaveChanges();
    }
    
    public void Delete(User user)
    {
        dataBase.UserRoles.RemoveRange(dataBase.UserRoles.Where(s => s.UserId == user.Id));
        dataBase.Users.Remove(user);
        dataBase.SaveChanges();
    }

    void IUserService.Delete(Guid id)
    {
        var user = dataBase.Users.FirstOrDefault(p => p.Id == id);
        if (user != null)
        {
            Delete(user);
        }
    }

    List<User> IUserService.GetAll() => dataBase.Users.ToList();
    User IUserService.Get(Guid id) => dataBase.Users.Include(u => u.Roles).ThenInclude(ur => ur.Role)
        .FirstOrDefault(s => s.Id == id);

    public void AssignRole(Guid userId, Guid roleId)
    {
        var user = dataBase.Users.Include(u => u.Roles).FirstOrDefault(p => p.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (user.Roles.Any(r => r.RoleId == roleId))
        {
            return;
        }
        var role = dataBase.Roles.FirstOrDefault(r => r.Id == roleId);
        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        dataBase.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });
        user.DateModified = DateTime.UtcNow;
        dataBase.Users.Update(user);
        dataBase.SaveChanges();
    }

    public void RemoveRole(Guid userId, Guid roleId)
    {
        var user = dataBase.Users.Include(u => u.Roles).FirstOrDefault(p => p.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var role = user.Roles.FirstOrDefault(r => r.RoleId == roleId);
        if (role == null)
        {
            return;
        }

        dataBase.UserRoles.Remove(role);
        user.DateModified = DateTime.UtcNow;
        dataBase.Users.Update(user);
        dataBase.SaveChanges();
    }

    public void ChangePassword(Guid userId, byte[] newHash)
    {
        var user = dataBase.Users.FirstOrDefault(p => p.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        user.Hash = newHash;
        user.DateModified = DateTime.UtcNow;
        dataBase.Users.Update(user);
        dataBase.SaveChanges();
    }
}*/