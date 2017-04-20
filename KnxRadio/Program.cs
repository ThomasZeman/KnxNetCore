using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using KnxNetCore;
using KnxNetCore.Telegrams;

namespace KnxRadio
{
    public class Program
    {
        private static readonly string[] RadioStations =
        {

            "http://webradio.hitradion1.de:8000/listen.pls",
            "http://media-ice.musicradio.com:80/CapitalYorkshireSouthWestMP3",
            "http://bbcmedia.ic.llnwd.net/stream/bbcmedia_radio2_mf_p",
            "http://bbcmedia.ic.llnwd.net/stream/bbcmedia_radio4fm_mf_p"
        };

        private static readonly string[] Announcers =
        {
            "n1.wav",
            "captialfm.wav",
            "bbc2.wav",
            "bbc4.wav",
            "radiooff.wav"
        };

        private static Process _lastProcess;
        static int _radioCounter = -1;
        private static ITrace _trace;

        public static void Main(string[] args)
        {
            // Establish tunneling connection with gateway at 10.0.0.135
            _trace = new ConsoleTrace();
            var connection = new KnxConnection(new IPEndPoint(IPAddress.Parse("10.0.0.102"), 3671), new IPEndPoint(IPAddress.Parse("10.0.0.135"), 3671), _trace);
            connection.KnxEventReceived += Connection_KnxEventReceived;
            Console.ReadKey();
            connection.Dispose();
        }

        private static void Connection_KnxEventReceived(KnxConnection arg1, KnxEvent arg2)
        {
            lock (RadioStations)
            {
                if (Equals(arg2.DestinationAddress, GroupAddress.FromGroups(0, 3, 20)))
                {
                    
                }
                // When any event with destination group 0/4/0 is received switch to next radio station
                if (Equals(arg2.DestinationAddress, GroupAddress.FromGroups(0, 4, 0)))
                {
                    if (_lastProcess != null)
                    {
                        try
                        {
                            _lastProcess.Kill();
                        }
                        catch
                        {
                            // ignored
                        }
                        _lastProcess.Dispose();
                        _lastProcess = null;
                    }
                    _radioCounter++;
                    _trace.Write(TraceSeverities.Info, "Starting: {0}", Announcers[_radioCounter]);
                    Process.Start("mpv", Announcers[_radioCounter]);
                    if (_radioCounter == RadioStations.Length)
                    {
                        _radioCounter = -1;
                        return;
                    }
                    // Start listening to radio station with "mpv" (linux) 
                    var process = Process.Start(@"mpv", RadioStations[_radioCounter]);
                    _lastProcess = process;
                    // Wait 15mins and terminate current radio process
                    Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMinutes(15));
                        try
                        {
                            _trace.Write(TraceSeverities.Debug, "Trying to kill old process");
                            process.Kill();
                            process.Dispose();
                            _radioCounter = -1;
                        }
                        catch (Exception ex)
                        {
                            _trace.Write(TraceSeverities.Error, ex.ToString());
                        }
                    });
                }
            }
        }
    }
}
