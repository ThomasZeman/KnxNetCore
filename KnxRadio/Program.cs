using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amplifier.Units;
using KnxNetCore;
using KnxNetCore.Telegrams;
using KnxRadio.Messages;
using KnxRadio.Model.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KnxRadio
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfigurationRoot Configuration { get; private set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory
                .AddConsole(LogLevel.Trace)
                .AddDebug();
            app.UseDeveloperExceptionPage();
            app.UseFileServer();
            var test = (ITest)app.ApplicationServices.GetService(typeof(ITest));
            //var test = app.ServerFeatures.Get<ITest>();
            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            var routerBuilder = new RouteBuilder(app, null);
            routerBuilder.MapGet("temp/{id}", context =>
            {
                var id = (string)context.GetRouteData().Values["id"];
                if (id == "1")
                {
                    return context.Response.WriteJson(test.Messages.Select(_ => _.Value));
                }
                else if (id == "2")
                {
                    return context.Response.WriteJson(test.Messages2.Select(_ => _.Value));
                }
                throw new ArgumentException();
              //  return context.Response.WriteAsync("Temperatures: " + string.Join(Environment.NewLine, test.Messages.Select(_ => _.ToString())));
            });
            app.UseRouter(routerBuilder.Build());
        }



    }

    public static class HttpExtensions
    {
        public static Task WriteJson<T>(this HttpResponse response, T obj)
        {
            response.ContentType = "application/json";
            return response.WriteAsync(JsonConvert.SerializeObject(obj));
        }
    }

    public interface ITest
    {
        List<Measure<Celsius>> Messages { get; }
        List<Measure<Celsius>> Messages2 { get; }
    }

    class Test : ITest
    {
        public List<Measure<Celsius>> Messages { get; }
        public List<Measure<Celsius>> Messages2 { get; }

        public Test(List<Measure<Celsius>> messages, List<Measure<Celsius>> messages2)
        {
            Messages = messages;
            Messages2 = messages2;

        }
    }

    public class Program
    {

        private static ITrace _trace;

        public static IDisposable RunAsp(ITest test)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            var builder = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ThreadCount = 1;
                })
                .UseUrls("http://10.0.0.102:5000")                
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(collection => ConfigureServices(collection, test))
                .UseConfiguration(new ConfigurationBuilder().Build())
                .UseStartup<Startup>();
            var host = builder.Build();
            host.Start();
            return host;
        }

        private static void ConfigureServices(IServiceCollection obj, ITest test)
        {
            obj.AddRouting();
            obj.AddSingleton<ITest>(test);
        }

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

            var temperatureGauge = new TemperatureGauge();
            var temperatureLivingRoom = new Entity(messageBus, new IntegerMessageBusAddress(1001), new[] { temperatureGauge });


            var temperatureGauge2 = new TemperatureGauge();
            var temperatureDressingRoom= new Entity(messageBus, new IntegerMessageBusAddress(1002), new[] { temperatureGauge2 });

            var radio = new Entity(messageBus, new IntegerMessageBusAddress(999), new[] { new Radio() });

            var binding = new KnxBinding(connection, messageBus, new IntegerMessageBusAddress(10000));
            binding.AddSwitch(GroupAddress.FromGroups(0, 0, 6), new IntegerMessageBusAddress(123), KnxAddressBindingTypes.Switch);
            binding.AddSwitch(GroupAddress.FromGroups(0, 4, 0), new IntegerMessageBusAddress(999), KnxAddressBindingTypes.Switch);
            binding.AddSwitch(GroupAddress.FromGroups(0, 3, 21), new IntegerMessageBusAddress(1001), KnxAddressBindingTypes.Temperature);
            binding.AddSwitch(GroupAddress.FromGroups(0, 3, 20), new IntegerMessageBusAddress(1002), KnxAddressBindingTypes.Temperature);

            var test = new Test(temperatureGauge.Temperatures, temperatureGauge2.Temperatures);
            using (RunAsp(test))
            {
                for (;;)
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
                Console.ReadKey();
                connection.Dispose();
            }
        }

    }
}
