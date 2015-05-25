using System.Threading;

namespace DetectIps
{
    public class AccessPoint
    {
        public void Enable()
        {
            new ExecuteAsAdministrator(string.Format("netsh wlan set hostednetwork mode=allow ssid={0} key={1}",
                                                     "PigDetAP", "silicon123")).Run();

            Thread.Sleep(2000);

            new ExecuteAsAdministrator("netsh wlan start hostednetwork").Run();
        }

    }
}