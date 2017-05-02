using System.Threading.Tasks;

namespace KnxRadio
{
    public class Button : IComponent
    {
        private Entity _entity;
        private readonly IEntityAddress _targetEntityAddress;
        private bool _switchState;

        public Button(IEntityAddress targetEntityAddress)
        {
            _targetEntityAddress = targetEntityAddress;
        }

        public void AddedToEntity(Entity entity)
        {
            _entity = entity;
        }

        public async Task Receive(Message message)
        {
        }

        public void Switch()
        {
            _switchState = !_switchState;
            _entity.Inlet.Send(_targetEntityAddress, new SwitchMessage(_switchState));
        }
    }
}