using System.Threading.Tasks;

namespace KnxRadio
{
    public interface IComponent
    {
        void AddedToEntity(Entity entity);

        Task Receive(Message message);
    }
}