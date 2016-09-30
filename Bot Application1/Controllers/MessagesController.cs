using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using VkNet;
using VkNet.Enums.Filters;
//Add MySql Library
using MySql.Data.MySqlClient;

namespace Bot_Application1
{
    class DBConnect
    {
        public MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            server = "zevsariy.myjino.ru";
            database = "zevsariy_virtualmsp";
            uid = "035848010_mspvir";
            password = "12345678";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }


        //open connection to database
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }
        //Close connection
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public string get_bill_phrase()
        {
            string[] phrases = new string[11] { "Своими успехами в бизнесе я обязан прежде всего способности сосредоточиться на перспективных целях, не поддаваясь соблазну решать сиюминутные задачи.",
                "Миру наплевать на твое самоощущение и самоуважение. Мир ожидает от тебя каких-нибудь достижений, перед тем как принять во внимание твое чувство собственного достоинства.",
                "Чудесно праздновать свой успех, но более важным является умение выносить уроки из своих провалов.",
                "На прошлой неделе я вышел пообедать. Какой-то парень остановил меня и попросил денег. Я не знал, что делать. Потом он сказал, что я должен побывать на его домашней странице, и дал мне свой URL(унифицированный указатель ресурса). Я не понял, правду он говорит или нет, но строка оказалась настолько хороша, что я дал ему пять долларов. Так что он, возможно, бездомный человек с домашней страницей в Интернете.",
                "У моих детей, конечно, будет компьютер. Но первым делом они получат книги.",
                "Мы объясняем людям, что, если никто не смеялся хотя бы над одной из их идей, они, возможно, недостаточно творчески подходят к работе.",
                "Я всегда буду искать ленивого человека для работы, ведь он найдет много легких путей для решения поставленной задачи.",
                "Когда вам в голову пришла хорошая идея, действуйте незамедлительно.",
                "Жизнь становится намного веселее, если подходить ко всем её вызовам творчески.",
                "Чем я всегда рад поделиться, так это своим энтузиазмом.",
                "Не люблю конференции во всяких экзотических местах. Такое ощущение, что чем приятнее место, тем меньше делается работы."};
            Random rnd = new Random();
            int ph_id = rnd.Next(0, 11);
            return phrases[ph_id];
        }

        public string great_cuter(string phrase)
        {
            phrase = phrase.ToLower();
            phrase = phrase.Replace("что такое ", "");
            phrase = phrase.Replace(" что такое", "");
            phrase = phrase.Replace(" что это", "");
            phrase = phrase.Replace("что это ", "");
            phrase = phrase.Replace("определение ", "");
            phrase = phrase.Replace("?", "");
            return phrase;
        }

        public string get_answer(string query)
        {
            string answer = "Пусто";
            DBConnect MyDB = new DBConnect();
            if (MyDB.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, MyDB.connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                bool flag = true;
                while (dataReader.Read() && flag == true)
                {
                    flag = false;
                    Random rnd = new Random();
                    string s = dataReader["answer"].ToString();
                    string[] split = s.Split(";".ToCharArray());
                    int num = rnd.Next(0, split.Count());
                    answer = split[num];
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                MyDB.CloseConnection();

                //return list to be displayed
                return answer;
            }
            else
            {
                return answer;
            }
        }
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                activity.Text = activity.Text.ToLower();
                activity.Text = great_cuter(activity.Text);
                activity.Text = activity.Text.Replace(" ", "%' AND question LIKE '%");
                Console.WriteLine(activity.Text);
                string texta = "Пустой Текста";
                texta = get_answer("Select answer FROM mspbot WHERE question LIKE '%" + activity.Text + "%'");
                /* switch (activity.Text)
                {
                    case "кто ты?":
                        texta = "Я, простая симуляция MSP, основанного не реальных людях)";
                        break;
                    case "помощь":
                        texta = "Вы можете использовать следующие команды: что почитать? - посоветую какие книги почитать; кто ты? - бот расскажет кто он есть; цитата - я расскажу тебе одну из цитат Билла Гейтса;";
                        break;
                    case "цитата":
                        texta = get_bill_phrase();
                        break;
                    case "что почитать?":
                        texta = "https://mva.microsoft.com ; https://azure.microsoft.com/ru-ru/get-started/;";
                        break;
                    default:
                        texta = "Для помощи в работе со мной напишите 'помощь'";
                        break;
                }
                */
                Activity reply = activity.CreateReply(texta);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}