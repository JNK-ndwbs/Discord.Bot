using Codeplex.Data;
using Discord;
using Discord.WebSocket;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        static string token = "";
        static ulong channelid = 0;

        public static DiscordSocketClient client;

        static bool canRun = false;

        static string last_report = "";

        static void Main(string[] args) {
            Console.WriteLine($"Application[{get_time().Result}] >  Loaded Core System");
            Run();
        }

        static async void Run()
        {
            Console.WriteLine($"Application[{get_time().Result}] >  Loading Config");
            if (!System.IO.File.Exists("channel.conf")) { return; }
                string[] configs = System.IO.File.ReadAllLines("channel.conf", Encoding.UTF8);
            try
            {
                if (configs[0] == "" || configs[1] == "") { Console.WriteLine("No Config"); return; }
            }
            catch { Console.WriteLine("No Config"); return; }

            token = configs[0];
            channelid = Convert.ToUInt64(configs[1]);

            canRun = true;

            Console.WriteLine($"Application[{get_time().Result}] >  Loading Main System");

            MainAsync();

            Console.WriteLine($"Application[{get_time().Result}] >  Signined to Discord");

            Console.WriteLine($"Application[{get_time().Result}] >  Starting Session");
            while (true)
            {
                osp_Async();
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static async Task MainAsync()
        {
            Console.WriteLine($"Application[{get_time().Result}] >  Init...");
            client = new DiscordSocketClient();

            Console.WriteLine($"Application[{get_time().Result}] >  Auth...");
            client.LoginAsync(TokenType.Bot, token);

            Console.WriteLine($"Application[{get_time().Result}] >  Connecting...");
            client.StartAsync();

            Console.WriteLine($"Application[{get_time().Result}] >  Inited");
        }

        static async Task<string> get_time() {
            string rawtime = (await new HttpClient().GetStringAsync("http://ntp-a1.nict.go.jp/cgi-bin/time")).Substring(11, 8);
            DateTime nowtime = DateTime.Parse(rawtime);
            return nowtime.ToString("yyyyMMddHHmmss");
        }

        private static async void osp_Async()
        {
            if (canRun == false) { return; }

            string flag = "";

            System.Net.WebClient get_eqi = new System.Net.WebClient();
            get_eqi.Encoding = Encoding.UTF8;
            string res_eqi = get_eqi.DownloadString($"http://www.kmoni.bosai.go.jp/new/webservice/hypo/eew/{get_time().Result}.json");
            //string res_eqi = get_eqi.DownloadString("http://www.kmoni.bosai.go.jp/new/webservice/hypo/eew/20180424175339.json");flag="テスト";
            //string res_eqi = get_eqi.DownloadString("http://www.kmoni.bosai.go.jp/new/webservice/hypo/eew/20180105110359.json");flag="テスト";
            var eqi = DynamicJson.Parse(res_eqi);

            var chatchannnel = client.GetChannel(channelid) as SocketTextChannel;

            if (res_eqi.Contains("ありません")) { return; }

            string region_name = eqi.region_name;
            string magunitude = eqi.magunitude;
            string calcintensity = eqi.calcintensity;
            if (last_report == eqi.report_time) return;

            try
            {
                if (eqi.is_final == true)
                {
                    string text = ($"{flag} 最終情報 緊急地震速報 { region_name}で地震  マグニチュード : {magunitude} 震度 : {calcintensity}");
                    await chatchannnel.SendMessageAsync(text);
                    Console.WriteLine("Message > " + text);
                }
                else
                {
                    string text1 = ($"{flag} 緊急地震速報 { region_name}で地震  マグニチュード : {magunitude} 予測震度 : {calcintensity}");
                    await chatchannnel.SendMessageAsync(text1);
                    Console.WriteLine("Message > " + text1); ;
                }
                last_report = eqi.report_time;
            }
            catch { }
        }
    }
}
