using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KnxNetCore;
using KnxNetCore.Datapoints;
using KnxNetCore.Telegrams;
using KnxRadio.Model.Components;

namespace KnxRadio
{
    public class Program
    {

        private static ITrace _trace;

        public static void Main(string[] args)
        {
            // Establish tunneling connection with gateway at 10.0.0.135
            _trace = new ConsoleTrace();
            var connection = new KnxConnection(new IPEndPoint(IPAddress.Parse("10.0.0.102"), 50000), new IPEndPoint(IPAddress.Parse("10.0.0.135"), 3671), _trace);


            var messageBus = new MessageBus();

            var dressingLight = new Entity(messageBus,
                new IntegerMessageBusAddress(123),
                new[] { new Switch() });

            var button = new Button(new IntegerMessageBusAddress(123));
            var dressingLightButton = new Entity(messageBus, new IntegerMessageBusAddress(456), new[] { button });

            var temperatureLivingRoom = new Entity(messageBus, new IntegerMessageBusAddress(1001), new[] { new TemperatureGauge() });

            var radio = new Entity(messageBus, new IntegerMessageBusAddress(999), new[] { new Radio() });

            var binding = new KnxBinding(connection, messageBus, new IntegerMessageBusAddress(10000));
            binding.AddSwitch(GroupAddress.FromGroups(0, 0, 6), new IntegerMessageBusAddress(123), KnxAddressBindingTypes.Switch);
            binding.AddSwitch(GroupAddress.FromGroups(0, 4, 0), new IntegerMessageBusAddress(999), KnxAddressBindingTypes.Switch);
            binding.AddSwitch(GroupAddress.FromGroups(0, 3, 20), new IntegerMessageBusAddress(1001), KnxAddressBindingTypes.Temperature);
            binding.AddSwitch(GroupAddress.FromGroups(0, 3, 21), new IntegerMessageBusAddress(1001), KnxAddressBindingTypes.Temperature);
            for (;;)
            {
                if (Console.KeyAvailable)
                {
                    var ck = Console.ReadKey();
                    CemiFrame cemiFrame = null;
                    if (ck.Key == ConsoleKey.R)
                    {
                        cemiFrame = new CemiFrame(CemiFrame.MessageCodes.DataRequest,
                            CemiFrame.Control1Flags.DoNotRepeat | CemiFrame.Control1Flags.PriorityLow |
                            CemiFrame.Control1Flags.StandardFrame, CemiFrame.Control2Flags.GroupAddress, IndividualAddress.FromAddressLineDevice(1, 1, 60),
                            GroupAddress.FromGroups(0, 0, 6), 1, (ushort)(CemiFrame.Commands.ValueRead) << 6);
                    }
                    else if (ck.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                    if (cemiFrame != null)
                    {
                        var task = connection.SendTunnelingRequest(cemiFrame);
                        task.Wait();
                        Console.WriteLine("Seq sent: " + task.Result);
                    }
                }
            }
            Console.ReadKey();
            connection.Dispose();
        }

    }
}
