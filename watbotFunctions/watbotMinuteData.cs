using System;
using System.Net.Sockets;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Company.Function
{
    public static class watbotMinuteData
    {
        [FunctionName("watbotMinuteData")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            
            var soc =  new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
            soc.Connect(Environment.GetEnvironmentVariable("watbot_ip"),Int32.Parse(Environment.GetEnvironmentVariable("watbot_port")));

            var message = Encoding.ASCII.GetBytes("{\"function\":\"minute_data\"}");
            int bytesSent = soc.Send(message);

            byte[] bytes = new byte[100000];  
            int bytesRec = soc.Receive(bytes);  
            Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes,0,bytesRec));
            var data = JsonConvert.DeserializeObject<List<MinuteData>>(Encoding.ASCII.GetString(bytes));

            var cntstr = new SqlConnectionStringBuilder()
            {
                InitialCatalog = "watbot",
                DataSource = "watbot.database.windows.net",
                UserID = Environment.GetEnvironmentVariable("SqlUserId`"),
                Password = Environment.GetEnvironmentVariable("SqlUserPassword"),
                MultipleActiveResultSets = true
            };
            var cnt = new SqlConnection(cntstr.ConnectionString);
            cnt.Open();

            foreach(var minData in data)
            {
                var command = new SqlCommand("dbo.InsertMinuteData",cnt)
                {
                    CommandType=CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@Game",minData.game));
                command.Parameters.Add(new SqlParameter("@DiscordId",Int64.Parse(minData.discord_id)));
                command.Parameters.Add(new SqlParameter("@DiscordName",minData.discord_name));
                command.Parameters.Add(new SqlParameter("@DiscordDiscriminator",Int32.Parse(minData.discriminator)));
                command.Parameters.Add(new SqlParameter("@Status",minData.status));
                command.Parameters.Add(new SqlParameter("@InsertDateTime",DateTime.UtcNow));
                command.ExecuteReader();
            }    




        }
    }
}
