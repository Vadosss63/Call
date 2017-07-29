using System;
using System.Data.OleDb;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Call
{
    public partial class DownloadTheJob : Form
    {
        static readonly string[] Abc = { "Дата", "Модель", "Год", "Цена", "Телефон", "Владелец", "Коментарий", "Дата_перезвона", "Оператор", "Статус" };
        readonly byte[] _bagrp = new byte[14];//получения порта
        readonly IPHostEntry _ipHost;
        readonly int _port;
        int _portsss;
        readonly byte[] _bagr = new byte[2];//команда подтверждения
        
        public DownloadTheJob(string ip)
        {
            _ipHost = Dns.Resolve(ip);
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileOpenDialog = new OpenFileDialog { Filter = "XLSX(*.xlsx)|*.xlsx", InitialDirectory = "." };
            fileOpenDialog.ShowDialog();
            textBox1.Text = fileOpenDialog.FileName;

        }

        
        private void Port()
        {
            var ipAddr = _ipHost.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddr, 11000);
            var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Соединяем сокет с удаленной конечной точкой
            sender.Connect(ipEndPoint);

            sender.Send(Encoding.Unicode.GetBytes("0"));
            var bport = sender.Receive(_bagrp);

            _portsss = (int)Convert.ToInt64(Encoding.Unicode.GetString(_bagrp, 0, bport));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var paths = textBox1.Text;
            string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + paths + @"; Extended Properties=""Excel 12.0 Xml;HDR=YES"";";
            Port();
            var ipAddr = _ipHost.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddr, _portsss);
            var senders = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Соединяем сокет с удаленной конечной точкой
            senders.Connect(ipEndPoint);
            //отправляем данные через сокет
            senders.Send(Encoding.Unicode.GetBytes("6"));
            senders.Receive(_bagr);

            using (var connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbDataReader dataReader;
                // Создаём команду выборки элементов с именем name2
                using (OleDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "Select * From [Лист1$]";
                    dataReader = command.ExecuteReader();
                }
                if (dataReader != null)
                {
                    int kk = 0;
                    while (dataReader.Read())
                    {
                        dataGridjobs.Rows.Add();//добавление строк                                       
                        for (var m = 0; m < 10; m++)
                        {
                            dataGridjobs[m + 1, kk].Value = dataReader[Abc[m]];
                            if (m == 4)
                            {//отправляем данные через сокет
                                senders.Send(Encoding.Unicode.GetBytes((string)dataReader[Abc[m]]));
                                var bnamber = new byte[50];
                                var status = senders.Receive(bnamber);//номер
                                dataGridjobs[11, kk].Value = Encoding.Unicode.GetString(bnamber, 0, status);
                                
                            }

                        }
                        kk++;
                    }
                    senders.Send(Encoding.Unicode.GetBytes("stop"));
                    dataReader.Close();
                    label3.Text = kk.ToString();
                }
                connection.Close();
            }
            senders.Shutdown(SocketShutdown.Both);
            senders.Close();
        }
    }
}
