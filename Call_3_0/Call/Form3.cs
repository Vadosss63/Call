using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Call
{
    public partial class Form3 : Form
    {
        readonly string _name;
        readonly string[] _job = new string[200];//для задание 
        readonly IPHostEntry _ipHost;
        readonly int _port;
        int _portsss;
        readonly byte[] _bagrp = new byte[14];//получения порта
        public Form3(string nn, string ip, int ports)
        {
            InitializeComponent();
            _name = nn;
            DTPvhoda.CustomFormat = "d MMMM";
            DTPvhoda.Format = DateTimePickerFormat.Custom;
            _port = ports;
#pragma warning disable 612,618
            _ipHost = Dns.Resolve(ip);
#pragma warning restore 612,618

        }
        int _kk;  
        

        private void Form3_Load(object sender, EventArgs e)
        {
            search_jobs(DTPvhoda.Text,"7");
        }

        private void button1_Click(object sender, EventArgs e)
        {
             Hide();
        }
        private void search_jobs(string pos, string team)// задание
        {
            var bytes2 = new byte[1024];//запрос
            var bytes3 = new byte[1024];//запрос
            var bytes4 = new byte[1024];//запрос
            //Соединяемся с удаленным устройством
            try
            {
                //Устанавливаем удаленную конечную точку для сокета
                Port();
                var ipAddr = _ipHost.AddressList[0];
                var ipEndPoint = new IPEndPoint(ipAddr, _portsss);
                var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Соединяем сокет с удаленной конечной точкой
                sender.Connect(ipEndPoint);
                
                byte[] msg = Encoding.Unicode.GetBytes(team);
                ////отправляем данные через сокет
                sender.Send(msg);
                sender.Receive(bytes2);

                ////отправляем данные через сокет
                sender.Send(Encoding.Unicode.GetBytes(pos));
                sender.Receive(bytes2);

                sender.Send(Encoding.Unicode.GetBytes(_name));
                dataGridView1.Rows.Clear();
                _kk = 0;
                string st = null;
                while (st != "0")
                    {
                        //Получаем ответ от удаленного устройства
                        var bytesRec3 = sender.Receive(bytes3);

                        if (Encoding.Unicode.GetString(bytes3, 0, bytesRec3) != "stop")
                        {
                            _job[_kk] = Encoding.Unicode.GetString(bytes3, 0, bytesRec3);
                            dataGridView1.Rows.Add();//добавление строк                                       
                            msg = Encoding.Unicode.GetBytes(team);
                         //отправляем данные через сокет
                            sender.Send(msg);
                            for (int m = 0; m < 10; m++)
                            {
                                int bytesRec4 = sender.Receive(bytes4);//получения массива
                                dataGridView1[m, _kk].Value = Encoding.Unicode.GetString(bytes4, 0, bytesRec4);
                                sender.Send(msg);
                            }
                            _kk++;
                        }
                        else
                        {
                            st = "0";
                        }
                    }
                

                if (_kk == 0)
                {
                    dataGridView1[0, 0].Value = "Нет звонков";
                }
                //Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch 
            {
                MessageBox.Show("Ошибка соединения с сервером!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            textBox2.Text = Convert.ToString(_kk);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            search_jobs(DTPvhoda.Text,"2");
        }
        private void Port()
        {
            var ipAddr = _ipHost.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddr, _port);
            var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Соединяем сокет с удаленной конечной точкой
            sender.Connect(ipEndPoint);

            sender.Send(Encoding.Unicode.GetBytes("0"));
            var bport = sender.Receive(_bagrp);
            _portsss = (int)Convert.ToInt64(Encoding.Unicode.GetString(_bagrp, 0, bport));
        }
    }
}
