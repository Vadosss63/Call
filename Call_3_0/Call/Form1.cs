using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;



namespace Call
{

    public partial class Form1 : Form
    {

        string _name;
        readonly byte[] _bname = new byte[50];//байт имени
        readonly byte[] _bagr = new byte[2];//команда подтверждения
        readonly byte[] _vrs = new byte[8];//команда версии
        readonly byte[] bagrp = new byte[14];//получения порта
        int _l;
        readonly string[] _setting = new string[3];//для настроек
        readonly IPHostEntry _ipHost;
        readonly int _port; // порт для соединения Call
        int portsss;
        public Form1()
        {
            InitializeComponent();
            const string fileName = "path.txt"; //путь к path файлу
            try
            {
                //чтение файла
                var g = 0;
                var allText = File.ReadAllLines(fileName);
                foreach (var s in allText)
                {
                    _setting[g] = s;
                    g++;
                }
            }
            catch (Exception)
            {

            }
            _port = (int)Convert.ToInt64(_setting[1]);
#pragma warning disable 612,618
            _ipHost = Dns.Resolve(_setting[0]);
#pragma warning restore 612,618
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sign_in();
            if (_name != "00")
            {
                var dlg = new Form2(_name, _setting[0], _port);
                dlg.Show();
                Hide();
            }
            else
            {
                if (_l == 0)
                {
                    MessageBox.Show("Неверный логин или пароль", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Clear();
                    textBox2.Clear();
                }
            }


        }


        //вход в систему
        private void sign_in()
        {
            //Соединяемся с удаленным устройством
            try
            {
                string theMessage2 = (textBox1.Text + "," + textBox2.Text);
                if (textBox1.Text != "" && textBox2.Text != "")
                {
                    //Устанавливаем удаленную конечную точку для сокета

                    Port();
                    var ipAddr = _ipHost.AddressList[0];
                    var ipEndPoint = new IPEndPoint(ipAddr, portsss);
                    var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //Соединяем сокет с удаленной конечной точкой
                    sender.Connect(ipEndPoint);

                    sender.Send(Encoding.Unicode.GetBytes("1"));
                    sender.Receive(_bagr);

                    //Получаем ответ от удаленного устройства
                    ////отправляем данные через сокет
                    sender.Send(Encoding.Unicode.GetBytes(theMessage2));
                    //Получаем ответ от удаленного устройства
                    var bnameRec = sender.Receive(_bname);

                    _name = Encoding.Unicode.GetString(_bname, 0, bnameRec);
                    sender.Send(Encoding.Unicode.GetBytes("0"));
                    //Освобождаем сокет
                    var vr = sender.Receive(_vrs);
                    var ver = Encoding.Unicode.GetString(_vrs, 0, vr);
                    if (ver != "в6")
                    {
                        MessageBox.Show("Обновите программу до следующий версии ", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _l++;
                        _name = "00";
                    }
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                else
                {
                    _name = "00";
                }
            }
            catch
            {
                _name = "00";
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void Port()
        {
            var ipAddr = _ipHost.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddr, _port);
            var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Соединяем сокет с удаленной конечной точкой
            sender.Connect(ipEndPoint);

            sender.Send(Encoding.Unicode.GetBytes("0"));
            var bport = sender.Receive(bagrp);

            portsss = (int)Convert.ToInt64(Encoding.Unicode.GetString(bagrp, 0, bport));
        }

    }
}
