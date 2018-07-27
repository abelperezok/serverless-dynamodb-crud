using System.Collections.Generic;
using System.Threading.Tasks;
using Generator.Domain;

namespace Generator.Domain
{
    public interface IEntityRepository
    {
        Task<List<Entity>> GetEntitiesByUserAsync(string userId);

        Task<Entity> GetItemAsync(string userId, string entityId);

        Task<Entity> PutItemAsync(Entity item);

        Task<int> DeleteItemAsync(Entity item);
    }
}
