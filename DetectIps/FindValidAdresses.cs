using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace DetectIps
{
    public class FindValidAdresses
    {
        private readonly List<Ping> _pingers = new List<Ping>();
        private int _instances;

        private readonly object _lock = new object();

        private List<NetworkInterfaceType> _networkInterfaceTypes = new List<NetworkInterfaceType>
        {
            NetworkInterfaceType.Wireless80211,
            NetworkInterfaceType.Ethernet
        };

        public int Result { get; private set; }

        private const int TIME_OUT = 250;

        private const int TTL = 5;

        public List<NetworkInterfaceType> NetworkInterfaceTypes
        {
            get { return _networkInterfaceTypes; }
            set { _networkInterfaceTypes = value; }
        }

        public List<string> ValidAdresses { get; private set; }
        
        public List<string> GetValidAdresses()
        {
            var baseIps = GetBaseIps().ToArray();
            ValidAdresses = new List<string>();
            foreach (var baseIp in baseIps)
            {
                CreatePingers(255);

                var po = new PingOptions(TTL, true);
                var enc = new System.Text.ASCIIEncoding();
                byte[] data = enc.GetBytes("abababababababababababababababab");

                var wait = new SpinWait();
                int cnt = 1;

                Stopwatch watch = Stopwatch.StartNew();

                foreach (Ping p in _pingers)
                {
                    lock (_lock)
                    {
                        _instances += 1;
                    }

                    p.SendAsync(string.Concat(baseIp, cnt.ToString(CultureInfo.InvariantCulture)), TIME_OUT, data, po);
                    cnt += 1;
                }

                while (_instances > 0)
                {
                    wait.SpinOnce();
                }

                watch.Stop();

                DestroyPingers();

                Elapsed = watch.Elapsed;
            }

            ValidAdresses.Sort();
            return ValidAdresses;
        }

        public TimeSpan Elapsed { get; set; }

        private IEnumerable<string> GetBaseIps()
        {
            return GetIps().Select(_ => string.Join(".", _.Split(new[] {'.'}).ToList().GetRange(0, 3)) + ".");
        }

        public IEnumerable<string> GetOwnIps()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (NetworkInterfaceTypes.Contains(ni.NetworkInterfaceType))
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            yield return ip.Address.ToString();
                        }
                    }
                }
            }
            yield return null;
        }

        public IEnumerable<string> GetGatewayIps()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (NetworkInterfaceTypes.Contains(ni.NetworkInterfaceType))
                {
                    foreach (var ip in ni.GetIPProperties().GatewayAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            yield return ip.Address.ToString();
                        }
                    }
                }
            }
            yield return null;
        }

        private IEnumerable<string> GetIps()
        {
            var interfaces = GetInterfacesByType(NetworkInterfaceType.Ethernet);
            return interfaces.SelectMany(networkInterface => networkInterface.GetIPProperties().GatewayAddresses)
                .Select(ip => ip.Address.ToString());
        }

        public IEnumerable<NetworkInterface> GetInterfacesByType(NetworkInterfaceType type)
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(_ => _.NetworkInterfaceType == type);
        }

        public void Ping_completed(object s, PingCompletedEventArgs e)
        {
            lock (_lock)
            {
                _instances -= 1;

                if (e.Reply.Status == IPStatus.Success)
                {
                    ValidAdresses.Add(e.Reply.Address.ToString());
                    Result += 1;
                }
            }
        }

        private void CreatePingers(int cnt)
        {
            for (int i = 1; i <= cnt; i++)
            {
                var p = new Ping();
                p.PingCompleted += Ping_completed;
                _pingers.Add(p);
            }
        }

        private void DestroyPingers()
        {
            foreach (Ping p in _pingers)
            {
                p.PingCompleted -= Ping_completed;
                p.Dispose();
            }

            _pingers.Clear();
        }
    }
}