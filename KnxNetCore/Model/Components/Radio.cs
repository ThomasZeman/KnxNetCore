using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using KnxNetCore;
using KnxNetCore.Telegrams;

namespace KnxRadio.Model.Components
{
    public class Radio : IComponent
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
        private Entity _entity;
        private ConsoleTrace _trace;

        public void AddedToEntity(Entity entity)
        {
            _entity = entity;
            _trace = new ConsoleTrace();
        }

        public async Task Receive(Message message)
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
            // Wait 15mins and terminate current radio process (unless it got terminated already)            
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
        }
    }
}
