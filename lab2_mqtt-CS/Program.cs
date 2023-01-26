using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using System.Configuration;
using System.Text;

namespace lab2_mqtt_CS
{
    public class Program
    {
        private static readonly bool CONTAINER = false;

        /// <summary>
        /// MQTTnet usage: https://blog.behroozbc.ir/mqtt-client-with-mqttnet-4-and-c
        /// </summary>
        static async Task Main(string[] args)
        {
            Console.WriteLine($"{Environment.NewLine}" +
                $"MQTTnet ConsoleApp - A The Things Network V3 C# App {Environment.NewLine}");

            var TTN_APP_ID = "demo-ap-la";
            var TTN_API_KEY = "NNSXS.YRCUBFYUK35J2MB6KBRVFCT3ST4IZLDUWGA6UII.6372L5SZNTYYYDQCOPQ777KRMIYS73YO2GQR5JCSKPYLMZDP5R6Q";
            var TTN_REGION = "eu1";
            var TTN_BROKER = $"{TTN_REGION}.cloud.thethings.network";
            var TOPIC = "v3/+/devices/+/up";

            IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();

            // Create client options object with keep alive 1 sec
            var builder = new MqttClientOptionsBuilder()
                            .WithTcpServer(TTN_BROKER, 1883)
                            .WithCredentials(TTN_APP_ID, TTN_API_KEY)
                            .WithCleanSession(true)
                            .WithKeepAlivePeriod(TimeSpan.FromSeconds(1));

            // auto reconnect after 5 sec if disconnected
            var options = new ManagedMqttClientOptionsBuilder()
                   .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                   .WithClientOptions(builder.Build())
                   .Build();

            // Set up handlers
            _mqttClient.ApplicationMessageReceivedAsync += MqttOnNewMessageAsync;
            _mqttClient.ConnectedAsync += MqttOnConnectedAsync;
            _mqttClient.DisconnectedAsync += MqttOnDisconnectedAsync;
            _mqttClient.ConnectingFailedAsync += MqttConnectingFailedAsync;

            var topics = new List<MqttTopicFilter>();
            var opts = new MqttTopicFilterBuilder()
                .WithTopic(TOPIC)
                .Build();
            topics.Add(opts);
            await _mqttClient.SubscribeAsync(topics);
            await _mqttClient.StartAsync(options);

            if (CONTAINER)
            {
                // use for testing when running as container
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                Console.WriteLine("Press return to exit!");
                Console.ReadLine();
                Console.WriteLine("\nAloha, Goodbye, Vaarwel!");
                Thread.Sleep(1000);
            }
        }

        public static Task MqttOnNewMessageAsync(MqttApplicationMessageReceivedEventArgs eArg)
        {
            var obj = eArg.ApplicationMessage;
            var ttn = TtnMessage.DeserialiseMessageV3(obj);
            var data = ttn.Payload != null ? BitConverter.ToString(ttn.Payload) : string.Empty;

            Console.WriteLine($"\nTimestamp: {ttn.Timestamp} \nDevice: {ttn.DeviceID} \nTopic: {ttn.Topic} \nPayload: {data}");
            Console.WriteLine($"Payload decoded: {ConvertHexToAscii(data)}\n");

            return Task.CompletedTask;
        }

        private static Task MqttOnConnectedAsync(MqttClientConnectedEventArgs eArg)
        {
            Console.WriteLine($"MQTTnet Client -> Connected with result: {eArg.ConnectResult.ResultCode}");
            return Task.CompletedTask;
        }
        private static Task MqttOnDisconnectedAsync(MqttClientDisconnectedEventArgs eArg)
        {
            Console.WriteLine($"MQTTnet Client -> Connection lost! Reason: {eArg.Reason}");
            return Task.CompletedTask;
        }
        private static Task MqttConnectingFailedAsync(ConnectingFailedEventArgs eArg)
        {
            Console.WriteLine($"MQTTnet Client -> Connection failed! Reason: {eArg.Exception}");
            return Task.CompletedTask;
        }
        public static string ConvertHexToAscii(string hex)
        {
            string hexStr = new string((from c in hex
                                        where char.IsLetterOrDigit(c)
                                        select c).ToArray());

            if (hexStr == "" || hexStr == null)
            {
                throw new Exception();
            }

            if (hexStr.Length % 2 == 0)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hexStr.Length; i += 2)
                {
                    string hs = hexStr.Substring(i, 2);
                    sb.Append(Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }
                return sb.ToString();
            }
            else
            {
                throw new Exception();
            }

        }




    }



}
