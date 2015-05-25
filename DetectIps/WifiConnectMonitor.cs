using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DetectIps
{
    public class WifiConnectMonitor
    {
        private readonly FindValidAdresses _finder;

        private readonly Timer _timer;
        private List<string> _validIps;
        private bool _on;

        public event ChangedIpListHandler ChangedIpList;

        public bool On
        {
            get { return _on; }
            set
            {
                if (_on != value)
                {
                    _on = value;
                    _timer.Enabled = _on;
                }
            }
        }

        protected virtual void OnChangedIpList(ChangedIpListHandlerArgs args)
        {
            var handler = ChangedIpList;
            if (handler != null) handler(this, args);
        }

        public WifiConnectMonitor()
        {
            _finder = new FindValidAdresses
            {
                NetworkInterfaceTypes = new List<NetworkInterfaceType> {NetworkInterfaceType.Wireless80211}
            };
            _timer = new Timer(1000) {AutoReset = false};
            _timer.Elapsed += MonitorThread;
            _timer.Start();
        }

        private void MonitorThread(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var changedIpListHandlerArgs = new ChangedIpListHandlerArgs();
            var lastIps = _validIps;
            _validIps = _finder.GetValidAdresses();

            var newIps = _validIps.Except(lastIps).ToList();
            if (newIps.Any())
            {
                changedIpListHandlerArgs.NewIps = newIps;
            }

            var oldIps = lastIps.Except(_validIps).ToList();
            if (oldIps.Any())
            {
                changedIpListHandlerArgs.OldIps = oldIps;
            }

            if (oldIps.Any() || newIps.Any())
                OnChangedIpList(changedIpListHandlerArgs);

            if (On)
            {
                _timer.Start();
            }
        }
    }

    public delegate void ChangedIpListHandler(object sender, ChangedIpListHandlerArgs args);

    public class ChangedIpListHandlerArgs
    {
        public List<string> NewIps { get; set; }
        public List<string> OldIps { get; set; }
    }
}