using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace multicastgroup1
{
    public class MultiCastGroupClass(string ip, int localport, string name = "Guest")
    {
        private static IPAddress ValidateIP(string ip)
        {
            if (!IPAddress.TryParse(ip, out var address))
                throw new Exception("Неправильный айпи");
            return address;
        }

        private UdpClient reciever = new UdpClient(localport);
        private IPAddress _broadAddress = ValidateIP(ip);
        private int _port = localport;

        public string Name { get; set; } = name;
        public bool Working { get; set; } = false;
        public bool Connected { get; set; } = false;


        public event Action<string>? NewMsg;
        private void ExecuteAll(string msg)
        {
            NewMsg?.Invoke(msg);
        }


        
        public bool JoinGroup(bool MulticastLoopback = true)
        {
            if (Connected) return Connected;

            try
            {
                using var receiver = new UdpClient(_port);
                receiver.JoinMulticastGroup(_broadAddress);
                receiver.MulticastLoopback = MulticastLoopback;
                Connected = true;
            }
            catch (Exception ex)
            {
                ExecuteAll(ex.Message);
            }
            return Connected;
        }

        public async Task СheckMsgAsync()
        {
            Working = true;
            if (!Connected) throw new Exception("Не подключён");
            try
            {
                while (Working)
                {
                    var result = await reciever.ReceiveAsync();
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    ExecuteAll(message);
                }
            }
            catch (Exception ex)
            {
                ExecuteAll(ex.Message);
                Working = false;
            }

            return;
        }

        public async Task SendMsgAsync(string msg)
        {
            using var sender = new UdpClient();
            var message = $"{Name}: {msg}";
            var data = Encoding.UTF8.GetBytes(msg);
            var endPoint = new IPEndPoint(_broadAddress, _port);
            await sender.SendAsync(data, endPoint);
        }
    }
}
