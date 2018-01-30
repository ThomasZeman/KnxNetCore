using System.Threading.Tasks;
using KnxNetCore.MessageBus;

namespace KnxNetCore.Model
{
    public interface IComponent
    {
        void AddedToEntity(Entity entity);

        Task Receive(Message message);
    }
}