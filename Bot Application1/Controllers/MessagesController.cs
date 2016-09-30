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
using System.Collections.Generic;

namespace Bot_Application1
{
    //Описываю класс для работы с базой данных
    class DBConnect
    {
        public MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Конструктор класса
        public DBConnect()
        {
            Initialize();
        }

        //Иницилизация подключения к БД
        private void Initialize()
        {
            //Данные для подключения к БД
            server = "zevsariy.myjino.ru";
            database = "zevsariy_virtualmsp";
            uid = "035848010_mspvir";
            password = "12345678";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }


        //Открытие подключения к БД
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
        //Закрытие подключения к БД
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
    //Контроллер сообщений из чата
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        //Великий обрезатель простых фраз
        public string great_cuter(string phrase)
        {
            phrase = phrase.ToLower();
            phrase = phrase.Replace("что такое ", "");
            phrase = phrase.Replace(" что такое", "");
            phrase = phrase.Replace(" что это", "");
            phrase = phrase.Replace("что это ", "");
            phrase = phrase.Replace("определение ", "");
            phrase = phrase.Replace("?", "");
            phrase = phrase.Replace("кто такой ", "");
            phrase = phrase.Replace(" кто такой", "");
            phrase = phrase.Replace("кто это ", "");
            phrase = phrase.Replace(" кто это", "");
            phrase = phrase.Replace("кто такая ", "");
            phrase = phrase.Replace(" кто такая", "");
            return phrase;
        }

        //Функция для получения ответов на вопросы
        public string get_answer(string query)
        {
            string answer = "Для получения справки по общению со мной скажите \"помощь\")";
            DBConnect MyDB = new DBConnect();
            if (MyDB.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, MyDB.connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                //Флаг нужен чтобы вывести только первый ответ из БД, а не несколько
                bool flag = true;
                while (dataReader.Read() && flag == true)
                {
                    flag = false;
                    //Рандомный вариант ответа из базы данных за счет сплита ответа по спец знаку
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

        //Получить ответ-реакцию на холивар
        public string get_holywar_answer(string query)
        {
            //-1 значит нет холивара
            string answer = "-1";
            DBConnect MyDB = new DBConnect();
            if (MyDB.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, MyDB.connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                //Флаг опять таки для одного оборота цикла
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

        //Функция получения ивентов
        public string get_events_answer(string query)
        {
            string temp = "";
            DBConnect MyDB = new DBConnect();
            if (MyDB.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, MyDB.connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                bool flag = true;
                int iter = 1;
                while (dataReader.Read() && flag == true)
                {
                    temp += iter + ") " + dataReader["name"] + " Дата: " + dataReader["date"] + " Ссылка: " + dataReader["link"] + "  ";
                    iter++;
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                MyDB.CloseConnection();

                //return list to be displayed
                if (temp == "")
                {
                    return "-1";
                }
                else
                {
                    return temp;
                }
            }
            else
            {
                return "-1";
            }
        }

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                string message_got = activity.Text;
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                string holywar_text = activity.Text;
                holywar_text = holywar_text.ToLower();
                holywar_text = great_cuter(holywar_text);
                holywar_text = holywar_text.Replace(" ", "%' OR question LIKE '%");
                string holywar_rez = get_holywar_answer("SELECT answer FROM holywars WHERE question LIKE '%" + holywar_text + "%'");

                string event_rez = "";
                if (activity.Text == "мероприятия" 
                    || activity.Text == "мероприятие"
                    || activity.Text == "ближайшее мероприятие"
                    || activity.Text == "ближайшие мероприятия"
                    || activity.Text == "куда сходить"
                    || activity.Text == "ивенты"
                    || activity.Text == "events")
                {
                    event_rez = get_events_answer("SELECT name, date, link FROM events WHERE date > Now()");
                }
                else
                {
                    event_rez = "-1";
                }

                activity.Text = activity.Text.ToLower();
                activity.Text = great_cuter(activity.Text);
                activity.Text = activity.Text.Replace(" ", "%' AND question LIKE '%");
                
                string texta = "Пустой Текста";

                if (holywar_rez != "-1")
                {
                    texta = holywar_rez;
                }
                else if (event_rez != "-1")
                {
                    texta = event_rez;
                }
                else
                {
                    texta = get_answer("Select answer FROM mspbot WHERE question LIKE '%" + activity.Text + "%'");
                }
                if(message_got != "Разработчики")
                {
                    Activity reply = activity.CreateReply(texta);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    Activity replyToConversation = activity.CreateReply("");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://virtual-msp.2tsy.ru/team.jpg"));
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction MSP_But = new CardAction()
                    {
                    Value = "https://vk.com/rumsp",
                    Type = "openUrl",
                    Title = "MSP ВКонтакте"
                    };
                    CardAction MSP_School = new CardAction()
                    {
                        Value = "http://msp.2tsy.ru/",
                        Type = "openUrl",
                        Title = "Школа MSP"
                    };
                    cardButtons.Add(MSP_But);
                    cardButtons.Add(MSP_School);
                    HeroCard plCard = new HeroCard()
                    {
                               Title = "Разработчики",
                               Subtitle = "Никонорова Анастасия <br> Филиппов Дмитрий <br/> Ткаченко Сергей",
                               Text = "Мы студенты партнеры Microsoft. Наша цель - развивать и продвигать технологии в массы. Люди должны знать больше.©",
                               Images = cardImages,
                               Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);
                }
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