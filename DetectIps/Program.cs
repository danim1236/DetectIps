using System;
using System.Linq;

namespace DetectIps
{
    internal class Program
    {
        private static void Main()
        {
            var addressFinder = new FindValidAdresses();
            var validAdresses = addressFinder.GetValidAdresses().Except(addressFinder.GetOwnIps()).Except(addressFinder.GetGatewayIps());
            foreach (var validAdress in validAdresses)
            {
                Console.WriteLine(string.Concat("Active IP: ", validAdress));
            }
            Console.WriteLine("Finished in {0}. Found {1} active IP-addresses.", addressFinder.Elapsed,
                              addressFinder.Result);
            Console.ReadKey();
        }
    }
}


