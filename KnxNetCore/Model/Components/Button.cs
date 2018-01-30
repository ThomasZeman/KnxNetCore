using System.Threading.Tasks;
using KnxNetCore.MessageBus;
using KnxNetCore.Messages;

namespace KnxNetCore.Model.Components
{
    public class Button : IComponent
    {
        private Entity _entity;
        private readonly IMessageBusAddress _targetEntityAddress;
        private bool _switchState;

        public Button(IMessageBusAddress targetEntityAddress)
        {
            _targetEntityAddress = targetEntityAddress;
        }

        public void AddedToEntity(Entity entity)
        {
            _entity = entity;
        }

        public async Task Receive(Message message)
        {
            await Task.CompletedTask;
        }

        public void Switch()
        {
            _switchState = !_switchState;
            _entity.Inlet.Send(_targetEntityAddress, new SwitchMessage(_switchState));
        }
    }
}