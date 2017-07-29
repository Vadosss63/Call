using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Data.OleDb;
using System.IO;
using System.Threading;
using System.Data.SqlClient;
using System.Linq;


namespace Сервер_клиент
{
    public class SServer
    {
        static readonly string[] Abc2 = { "Дата", "Модель", "Год", "Цена", "Телефон", "Владелец", "Коментарий", "Дата_перезвона", "Оператор", "Статус" };
        static readonly string[] Setting = new string[3];//для настроек
        private const string FileName = "path.txt"; //путь к path файлу
        static string _path;
        private static string _connectionString;
        static readonly List<Thread> Threads = new List<Thread>();
        public static void Main()
        {
            var com = new byte[2];// для получения команды
            try
            {
                //чтение файла
                var g = 0;
                var allText = File.ReadAllLines(FileName);
                foreach (var s in allText)
                {
                    Setting[g] = s;
                    g++;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Ошибка открытия файла Path!");
                return; 
            }
            _path = new FileInfo(Setting[0]).FullName;
            _connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}", _path );
            //Устанавливаем для сокета локальную конечную точку
            var port = 9000;//отправка порта
            var ipAddr = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork);
            var ports = (int)Convert.ToInt64(Setting[1]);
            Socket handler = null;
            while (true)
            {
                try
                {
                    var ipEndPoint = new IPEndPoint(ipAddr, ports);
                    //Создаем сокет TCP\IP
                    var sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //и слушаем входящие сокеты
                    sListener.Bind(ipEndPoint);
                    sListener.Listen(10);
                    //Начинаем слущать соединения 
                    while (true)
                    {

                        Console.WriteLine("Ожидание выдачи порта {0}", ipEndPoint);
                        //программа приостанавливается,ожидая входящее соединение
                        handler = sListener.Accept();

                        // соединяемся с клиентом и выдаем port 
                        var comRec = handler.Receive(com);
                        string data = Encoding.Unicode.GetString(com, 0, comRec);
                        if (data == "0")
                        {
                            var agr = Encoding.Unicode.GetBytes(Convert.ToString(port));
                            handler.Send(agr);
                            var port1 = port;
                            var t = new Thread(() => Potok(port1));
                            Threads.Add(t);
                            t.Start();
                        }
                        if (port > 9998)
                        {
                            port = 9000;
                        }
                        else
                        {
                            port++;
                        }
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();

                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка главного потока!");
                    if (handler == null) return;
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
        }
        // ReSharper restore FunctionNeverReturns
        static private void Potok(int port)
        {
            var com = new byte[2];// для получения команды
            var bname = new byte[50];// для приема имени
            var bdite = new byte[50];// для приема даты
            var bnamber = new byte[24];// для приема номера
            var bagr = new byte[2];//команда подтверждения
            var bmod = new byte[128];// для приема марки
            var bgod = new byte[8];// для приема года
            var bpis = new byte[20];//цена                             
            var boper = new byte[30]; //оператор
            var bcoment = new byte[512]; //коментарий
            var bhoz = new byte[30]; //владелец
            var bstatus = new byte[20]; //статус            
            var bdp = new byte[40]; //дата перезвона            
            var bn = new byte[12]; //N
            var brs = new byte[2]; //ответ
            Socket handler = null;
            OleDbConnection connection = null;
            OleDbDataReader dataReader=null;
            try
            {
            //Устанавливаем для сокета локальную конечную точку
            var ipAddr = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork);

            var ipEndPoint = new IPEndPoint(ipAddr, port);
            //Создаем сокет TCP\IP
            var sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //и слушаем входящие сокеты
            sListener.Bind(ipEndPoint);
            sListener.Listen(10);
            //Начинаем слущать соединения 
            Console.WriteLine("Ожидание подключения... {0}", ipEndPoint);
            //программа приостанавливается,ожидая входящее соединение
            handler = sListener.Accept();
            //дождались клиента,пытающегося с нами соединиться  
            var comRec = handler.Receive(com);
            string data = Encoding.Unicode.GetString(com, 0, comRec);
            //выводим данные на консоль
            Console.WriteLine("Сообщение клиента : {0}", data);
            
                var time1 = DateTime.Now; //Точка начала отсчета времени
                using (connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();
                    var agr = Encoding.Unicode.GetBytes("0");//переменная ответа
                switch (data)
                {
                    case "1":// проверка имени по базе
                        string name = null;
                        // Имя для поиска.
                        handler.Send(agr);
                        var bnameRec = handler.Receive(bname);
                        string nameToFind = Encoding.Unicode.GetString(bname, 0, bnameRec);
                        int ii = 0;
                        using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "Select * From пароли Where [пароли]='" + nameToFind + "'";
                                dataReader = command.ExecuteReader();
                            }
                            if (dataReader != null)
                            {
                                while (dataReader.Read())
                                {
                                    handler.Send(Encoding.Unicode.GetBytes((string)dataReader["имя"]));
                                    ii = 1;
                                }
                                dataReader.Close();
                            }
                            
                            if (ii == 0)
                            {
                                handler.Send(Encoding.Unicode.GetBytes("00"));
                            }
                        handler.Receive(brs);
                        handler.Send(Encoding.Unicode.GetBytes("в6"));
                       break;

                    case "2": // загрузка задания
                        handler.Send(agr);
                        var bditeRec = handler.Receive(bdite);//дата
                        nameToFind = Encoding.Unicode.GetString(bdite, 0, bditeRec);
                        handler.Send(agr);
                        bnameRec = handler.Receive(bname);//имя
                        name = Encoding.Unicode.GetString(bname, 0, bnameRec);
                        using (var command = connection.CreateCommand())
                            {
                                command.CommandText = string.Format("Select * From Лист1 Where [Дата_перезвона]='{0}' and [Оператор]='{1}'", nameToFind, name);
                                dataReader = command.ExecuteReader();
                            }
                            if (dataReader != null)
                            {
                                while (dataReader.Read())
                                {
                                    //масив для редактирования
                                    handler.Send(Encoding.Unicode.GetBytes(dataReader["N"].ToString()));
                                    handler.Receive(bagr);
                                    for (var m = 0; m < 10; m++)
                                    {
                                            handler.Send(Encoding.Unicode.GetBytes((string)dataReader[Abc2[m]]));
                                            handler.Receive(bagr);
                                    }
                                   
                                }
                                handler.Send(Encoding.Unicode.GetBytes("stop"));
                                dataReader.Close();
                            }
                        
                        break;
                    case "3":
                        handler.Send(agr);
                        var bnamberRec = handler.Receive(bnamber);//номер
                        nameToFind = Encoding.Unicode.GetString(bnamber, 0, bnamberRec);
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "Select * From Лист1 Where [телефон]='" + nameToFind + "'";
                                dataReader = command.ExecuteReader();
                            }
                            if (dataReader != null)
                            {

                                while (dataReader.Read())
                                {
                                    handler.Send(Encoding.Unicode.GetBytes(dataReader["N"].ToString()));
                                    handler.Receive(bagr);
                                    for (var m = 0; m < 10; m++)
                                    {
                                        handler.Send(Encoding.Unicode.GetBytes((string)dataReader[Abc2[m]]));
                                        handler.Receive(bagr);//запрос
                                      
                                    }
                                   

                                }
                                handler.Send(Encoding.Unicode.GetBytes("stop"));

                                dataReader.Close();
                            }
                        
                        break;
                    case "4":// редоктирование
                        handler.Send(agr);
                        bnamberRec = handler.Receive(bnamber);//t1
                        var t1 = Encoding.Unicode.GetString(bnamber, 0, bnamberRec);
                        handler.Send(agr);
                        bnamberRec = handler.Receive(bnamber);//t2
                        handler.Send(agr);//?

                        var p1 = Convert.ToInt32(t1);//для редоктирования поиска
                        var t2 = Encoding.Unicode.GetString(bnamber, 0, bnamberRec);
                        var p2 = Convert.ToInt32(t2);//для редоктирования задания
                        using (var command = connection.CreateCommand())
                            {
                                for (var i = 0; i < p1; i++)
                                {
                                    var byt2 = handler.Receive(bhoz);//владелец
                                    handler.Send(agr);
                                    var byt1 = handler.Receive(bcoment);//коментарий
                                    handler.Send(agr);
                                    var byt5 = handler.Receive(bdp);//дата перезвона
                                    handler.Send(agr);
                                    var byt3 = handler.Receive(bstatus);//статус
                                    handler.Send(agr);
                                    var byt7 = handler.Receive(bn);//N
                                    handler.Send(agr);
                                    command.CommandText = "UPDATE Лист1 SET Коментарий = '" + Encoding.Unicode.GetString(bcoment, 0, byt1) + "', Владелец = '" + Encoding.Unicode.GetString(bhoz, 0, byt2) + "', Статус = '" + Encoding.Unicode.GetString(bstatus, 0, byt3) + "', Дата_перезвона = '" + Encoding.Unicode.GetString(bdp, 0, byt5) + "' WHERE [N]=" + Encoding.Unicode.GetString(bn, 0, byt7) + "";
                                    command.ExecuteNonQuery();
                                }
                                for (var i = 0; i < p2; i++)
                                {
                                    var byt2 = handler.Receive(bhoz);//владелец                                            
                                    handler.Send(agr);
                                    var byt1 = handler.Receive(bcoment);//коментарий                                           
                                    handler.Send(agr);
                                    var byt5 = handler.Receive(bdp);//дата перезвона                                            
                                    handler.Send(agr);
                                    var byt3 = handler.Receive(bstatus);//статус
                                    handler.Send(agr);
                                    var byt7 = handler.Receive(bn);//N
                                    handler.Send(agr);
                                    command.CommandText = "UPDATE Лист1 SET Коментарий = '" + Encoding.Unicode.GetString(bcoment, 0, byt1) + "', Владелец = '" + Encoding.Unicode.GetString(bhoz, 0, byt2) + "', Статус = '" + Encoding.Unicode.GetString(bstatus, 0, byt3) + "', Дата_перезвона = '" + Encoding.Unicode.GetString(bdp, 0, byt5) + "' WHERE [N]=" + Encoding.Unicode.GetString(bn, 0, byt7) + "";
                                    command.ExecuteNonQuery();
                                }

                            }
                        
                        break;
                    case "5"://создание
                        handler.Send(agr);
                        bditeRec = handler.Receive(bdite);//дата  
                        var d1 = Encoding.Unicode.GetString(bdite, 0, bditeRec);
                        handler.Send(agr);
                        var bmodRec = handler.Receive(bmod);//модель 
                        var d2 = Encoding.Unicode.GetString(bmod, 0, bmodRec);
                        handler.Send(agr);
                        var bgodRec = handler.Receive(bgod);//год  
                        var d3 = Encoding.Unicode.GetString(bgod, 0, bgodRec);
                        handler.Send(agr);
                        var bpisRec = handler.Receive(bpis);//цена 
                        var d4 = Encoding.Unicode.GetString(bpis, 0, bpisRec);
                        handler.Send(agr);
                        bnamberRec = handler.Receive(bnamber);//телефон  
                        var d6 = Encoding.Unicode.GetString(bnamber, 0, bnamberRec);
                        handler.Send(agr);
                        var byts2Rec = handler.Receive(bhoz);//владелец 
                        var d7 = Encoding.Unicode.GetString(bhoz, 0, byts2Rec);
                        handler.Send(agr);
                        var byts1Rec = handler.Receive(bcoment);//коментарий  
                        var d8 = Encoding.Unicode.GetString(bcoment, 0, byts1Rec);
                        handler.Send(agr);
                        var byts5Rec = handler.Receive(bdp);//дата перезвона  
                        var d9 = Encoding.Unicode.GetString(bdp, 0, byts5Rec);
                        handler.Send(agr);
                        var boperRec = handler.Receive(boper);//оператор
                        var d11 = Encoding.Unicode.GetString(boper, 0, boperRec);
                        handler.Send(agr);
                        var byts3Rec = handler.Receive(bstatus);//статус 
                        var d12 = Encoding.Unicode.GetString(bstatus, 0, byts3Rec);
                        handler.Send(agr);
                            using (OleDbCommand command = connection.CreateCommand())
                            {
                                command.CommandText =
                                  string.Format("INSERT INTO Лист1 (Дата, Модель, Год, Цена, Телефон, Владелец, Коментарий, Дата_перезвона, Оператор, Статус) " +
                                                "values ('{0}', '{1}', '{2}','{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}');"
                                                ,d1,d2,d3,d4,d6,d7,d8,d9,d11,d12);
                                command.ExecuteNonQuery();
                            }
                       
                        break;
                    case "6":  // проверка списка номеров
                        handler.Send(agr);
                        nameToFind = null;
                        while (nameToFind!="stop")
                        {
                            bnamberRec = handler.Receive(bnamber);//номер
                            nameToFind = Encoding.Unicode.GetString(bnamber, 0, bnamberRec);
                            using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "Select * From Лист1 Where [телефон]='" + nameToFind + "'";
                            dataReader = command.ExecuteReader();
                        }
                        if (dataReader != null)
                        {
                            var status = "Нет";
                            while (dataReader.Read())
                            {
                                status = "Есть";
                            }
                            handler.Send(Encoding.Unicode.GetBytes(status));
                            
                            dataReader.Close();
                        }
                            
                        }
                        

                        break;


                    case "7"://просмотор сделанных звонков
                        handler.Send(agr);
                        bditeRec = handler.Receive(bdite);//дата
                        nameToFind = Encoding.Unicode.GetString(bdite, 0, bditeRec);
                        handler.Send(agr);
                        bnameRec = handler.Receive(bname);//имя
                        name = Encoding.Unicode.GetString(bname, 0, bnameRec);
                        // Создаём команду выборки элементов с именем name2
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = string.Format("Select * From Лист1 Where [Дата]='{0}' and [Оператор]='{1}'", nameToFind, name);
                                dataReader = command.ExecuteReader();
                            }
                            if (dataReader != null)
                            {

                                while (dataReader.Read())
                                {
                                        ;//масив для редактирования
                                        handler.Send(Encoding.Unicode.GetBytes(dataReader["N"].ToString()));
                                        handler.Receive(bagr);

                                        for (int m = 0; m < 10; m++)
                                        {
                                            handler.Send(Encoding.Unicode.GetBytes((string)dataReader[Abc2[m]]));
                                            handler.Receive(bagr);//имя
                                        }
                                    
                                }
                                handler.Send(Encoding.Unicode.GetBytes("stop"));
                                dataReader.Close();
                            }
                       
                        break;
                    case "8"://количество сделанных звонков
                        handler.Send(agr);
                        bditeRec = handler.Receive(bdite);//дата
                        nameToFind = Encoding.Unicode.GetString(bdite, 0, bditeRec);
                        handler.Send(agr);
                        bnameRec = handler.Receive(bname);//имя
                        name = Encoding.Unicode.GetString(bname, 0, bnameRec);
                        using (var command = connection.CreateCommand())
                            {
                                command.CommandText = string.Format("SELECT count(*) From Лист1 Where [Дата]='{0}' and [Оператор]='{1}'", nameToFind, name);
                                handler.Send(Encoding.Unicode.GetBytes(Convert.ToString((int)command.ExecuteScalar())));
                            }
                        
                        
                        break;
                    default:
                        Console.WriteLine("Ошибка соединения");
                        break;
                }
                    connection.Close();
                }
                var time2 = DateTime.Now; //Точка окончания отсчета времени
                var elapsedTicks = time2.Ticks - time1.Ticks; // подсчитываем число тактов, один такт соответствует 100 наносекундам
                Console.WriteLine("\"{0}\"время = {1}",data,elapsedTicks * 1E-7); // делим на 10^7 для отображения времени в секундах
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (handler != null)
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
                if (connection != null) connection.Close();
                if (dataReader != null) dataReader.Close();
            }
        }
    }
}
