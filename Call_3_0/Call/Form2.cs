using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
namespace Call
{
    public partial class Form2 : Form
    {

        int _p1;//для редоктирования поиска
        int _p2;//для редоктирования задания
        readonly string[] _job = new string[200];//для задание 
        readonly string[] _search = new string[200];// для поиска
        readonly string _name;
        readonly byte[] _bagr = new byte[2];//команда подтверждения
        readonly byte[] _jobnab = new byte[12];//команда масива
        readonly byte[] _bdataGrid = new byte[512];// для заполнения таблицы
        readonly byte[] _bagrp = new byte[14];//получения порта
        readonly IPHostEntry _ipHost;
        private readonly string _ips;
        readonly int _port;
        int _portsss;
        int _eroor;
        DateTime _d;
        string _date;
        public Form2(string nn, string ip, int ports)
        {
            _port = ports;
#pragma warning disable 612,618
            _ipHost = Dns.Resolve(ip);
#pragma warning restore 612,618
            InitializeComponent();
            _name = nn;
            textBox11.Text = _name;
            _ips = ip;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            _eroor = 0;
            Dlina();                               //допустимое число символов в строке
            Nom();// корекция номера
            if (_eroor != 0) return;
            if (textBox6.Text == "")
            {
                MessageBox.Show("Заполнените номер", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                number_search(textBox6.Text.Trim(' '));
                if ((string) dataGridPos[0, 0].Value != "нет номеров")
                {
                    if (MessageBox.Show("Данный номер в базе существует, он отобразился в графе поиска!",
                                        "Вы хотите его создать???", MessageBoxButtons.YesNo) !=
                        DialogResult.Yes) return;
                    Soz();
                }
                else
                {
                    Soz();
                }
            }
        }

        private void Dlina()// ограничение по символам
        {
            if (textBox2.Text.Length <= 64)
            {
            }
            else
            {
                MessageBox.Show("Превышено количество смволов поля #Марка#", "", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                _eroor++;
            }
            if (textBox4.Text.Length <= 10)
            {
            }
            else
            {
                MessageBox.Show("Превышено количество смволов поля #Цена#", "", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                _eroor++;
            }
            if (textBox6.Text.Length <= 12)
            {
            }
            else
            {
                MessageBox.Show("Превышено количество смволов поля #Номер тел#", "", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                _eroor++;
            }
            if (textBox7.Text.Length <= 15)
            {
            }
            else
            {
                MessageBox.Show("Превышено количество смволов поля #Владелец#", "", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                _eroor++;
            }
            if (textBox8.Text.Length <= 256) return;
            MessageBox.Show("Превышено количество смволов поля #Комментарий#", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _eroor++;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            search_jobs(dateTimePicker1.Text);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox3.SelectedIndex = 0;
            dateTimePicker1.CustomFormat = "d MMMM";
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker9.CustomFormat = "d MMMM";
            dateTimePicker9.Format = DateTimePickerFormat.Custom;
            search_jobs(dateTimePicker1.Text);
        }



        private void button4_Click(object sender, EventArgs e)//поиск
        {
            Nom();// корекция номера
            if (textBox6.Text != "")
            {
                number_search(textBox6.Text);
            }
            else
            {
                MessageBox.Show("Заполнените номер", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            Editres();
        }

        private void number_search(string pos)//поиск 
        {
            //Соединяемся с удаленным устройством
            try
            {
                Port();
                //Устанавливаем удаленную конечную точку для сокета
                var ipAddr = _ipHost.AddressList[0];
                var ipEndPoint = new IPEndPoint(ipAddr, _portsss);
                var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Соединяем сокет с удаленной конечной точкой
                sender.Connect(ipEndPoint);
                //отправляем данные через сокет
                sender.Send(Encoding.Unicode.GetBytes("3"));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(pos));
                dataGridPos.Rows.Clear();
                var kk = 0;
                string st = null;
                while (st != "0")
                {

                    //Получаем ответ от удаленного устройства
                    int bjobnabRec = sender.Receive(_jobnab);

                    if (Encoding.Unicode.GetString(_jobnab, 0, bjobnabRec) != "stop")
                    {
                        _search[kk] = Encoding.Unicode.GetString(_jobnab, 0, bjobnabRec);
                        dataGridPos.Rows.Add();//добавление строк                                       
                        //отправляем данные через сокет
                        sender.Send(Encoding.Unicode.GetBytes("0"));

                        for (var m = 0; m < 10; m++)
                        {
                            var bdataGridRec = sender.Receive(_bdataGrid);//получения массива
                            dataGridPos[m, kk].Value = Encoding.Unicode.GetString(_bdataGrid, 0, bdataGridRec);
                            sender.Send(Encoding.Unicode.GetBytes("0"));


                        }
                        if (_name != (string)dataGridPos[8, kk].Value)
                        {
                            dataGridPos.Rows[kk].ReadOnly = true;
                        }
                        kk++;
                    }
                    else
                    {
                        st = "0";
                    }
                }

                if (kk == 0)
                {
                    dataGridPos[0, 0].Value = "нет номеров";
                }
                _p2 = kk;
                //Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch
            {
                MessageBox.Show("Ошибка соединения с сервером!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void search_jobs(string pos)// задание
        {
            //Соединяемся с удаленным устройством
            try
            {
                Port();
                //Устанавливаем удаленную конечную точку для сокета
                var ipAddr = _ipHost.AddressList[0];
                var ipEndPoint = new IPEndPoint(ipAddr, _portsss);
                var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Соединяем сокет с удаленной конечной точкой
                sender.Connect(ipEndPoint);

                //отправляем данные через сокет
                sender.Send(Encoding.Unicode.GetBytes("2"));
                sender.Receive(_bagr);

                //отправляем данные через сокет
                sender.Send(Encoding.Unicode.GetBytes(pos));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(_name));
                dataGridjobs.Rows.Clear();
                var kk = 0;
                string st = null;
                while (st != "0")
                {
                    //Получаем ответ от удаленного устройства
                    int bjobnabRec = sender.Receive(_jobnab);
                    if (Encoding.Unicode.GetString(_jobnab, 0, bjobnabRec) != "stop")
                    {
                        _job[kk] = Encoding.Unicode.GetString(_jobnab, 0, bjobnabRec);
                        dataGridjobs.Rows.Add();//добавление строк                                       
                        //отправляем данные через сокет
                        sender.Send(Encoding.Unicode.GetBytes("0"));
                        for (var m = 0; m < 10; m++)
                        {
                            var bbdataGridRec = sender.Receive(_bdataGrid);//получения массива
                            dataGridjobs[m, kk].Value = Encoding.Unicode.GetString(_bdataGrid, 0, bbdataGridRec);
                            sender.Send(Encoding.Unicode.GetBytes("0"));
                        }
                        kk++;
                    }

                    else
                    {
                        st = "0";
                    }
                }



                if (kk == 0)
                {
                    dataGridjobs[0, 0].Value = "нет заданий";
                }
                _p1 = kk;
                //Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch
            {
                MessageBox.Show("Ошибка соединения с сервером!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void Editres()
        {
            Port();
            //Устанавливаем удаленную конечную точку для сокета
            var ipAddr = _ipHost.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddr, _portsss);
            var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Соединяем сокет с удаленной конечной точкой
            sender.Connect(ipEndPoint);
            try
            {

                sender.Send(Encoding.Unicode.GetBytes("4"));
                sender.Receive(_bagr);

                var t1 = Convert.ToString(_p1);
                sender.Send(Encoding.Unicode.GetBytes(t1));
                var t2 = Convert.ToString(_p2);
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(t2));
                sender.Receive(_bagr);
                //Получаем ответ от удаленного устройства
                for (var i = 0; i < _p1; i++)
                {
                    sender.Send(Encoding.Unicode.GetBytes(dataGridjobs.Rows[i].Cells[5].Value.ToString()));// владелец
                    sender.Receive(_bagr);

                    sender.Send(Encoding.Unicode.GetBytes(dataGridjobs.Rows[i].Cells[6].Value.ToString()));// комментарий
                    sender.Receive(_bagr);

                    _d = Convert.ToDateTime(dataGridjobs.Rows[i].Cells[7].Value.ToString());// дата перезвона
                    _date = _d.ToString("d MMMM"); //Можно остановиться на string
                    sender.Send(Encoding.Unicode.GetBytes(_date));
                    sender.Receive(_bagr);

                    sender.Send(Encoding.Unicode.GetBytes(dataGridjobs.Rows[i].Cells[9].Value.ToString())); // статус
                    sender.Receive(_bagr);

                    sender.Send(Encoding.Unicode.GetBytes(_job[i]));
                    sender.Receive(_bagr);

                }
                for (var i = 0; i < _p2; i++)
                {
                    sender.Send(Encoding.Unicode.GetBytes(dataGridPos.Rows[i].Cells[5].Value.ToString()));// владелец
                    sender.Receive(_bagr);

                    sender.Send(Encoding.Unicode.GetBytes(dataGridPos.Rows[i].Cells[6].Value.ToString()));// комментарий
                    sender.Receive(_bagr);

                    if (dataGridPos.Rows[i].Cells[7].Value.ToString() != "---") // дата перезвона
                    {
                        _d = Convert.ToDateTime(dataGridPos.Rows[i].Cells[7].Value.ToString());
                        _date = _d.ToString("d MMMM"); //Можно остановиться на string
                        sender.Send(Encoding.Unicode.GetBytes(_date));
                        sender.Receive(_bagr);
                    }
                    else
                    {
                        sender.Send(Encoding.Unicode.GetBytes("---"));
                        sender.Receive(_bagr);

                    }

                    sender.Send(Encoding.Unicode.GetBytes(dataGridPos.Rows[i].Cells[9].Value.ToString())); // статус
                    sender.Receive(_bagr);

                    sender.Send(Encoding.Unicode.GetBytes(_search[i]));
                    sender.Receive(_bagr);
                }

                //Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch
            {
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                MessageBox.Show("Заполнены не все поля", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }
        private void Editres2(string d9)
        {
            //Соединяемся с удаленным устройством
            try
            {
                Port();
                //Устанавливаем удаленную конечную точку для сокета
                var ipAddr = _ipHost.AddressList[0];
                var ipEndPoint = new IPEndPoint(ipAddr, _portsss);
                var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Соединяем сокет с удаленной конечной точкой
                sender.Connect(ipEndPoint);
                //отправляем данные через сокет
                sender.Send(Encoding.Unicode.GetBytes("5"));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(dateTimePicker1.Text));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(textBox2.Text));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(comboBox3.Text));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(textBox4.Text));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(textBox6.Text));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(textBox7.Text));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(textBox8.Text));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(d9));//D9
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(_name));
                sender.Receive(_bagr);

                sender.Send(Encoding.Unicode.GetBytes(comboBox12.Text));
                sender.Receive(_bagr);

                //Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch
            {
                MessageBox.Show("Ошибка соединения с сервером!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var dlg = new Form3(_name, _ips, _port);
            dlg.Show();
        }
        private void Nom()//редактор номера
        {
            string s = Convert.ToString(textBox6.Text);
            s = s.Replace("+7", "8");
            s = s.Replace("(", "");
            s = s.Replace(")", "");
            s = s.Replace("-", "");
            s = s.Replace(" ", "");
            textBox6.Text = s;
        }
        private void Soz()
        {
            textBox2.Text = textBox2.Text.Trim(' ');
            comboBox3.Text = comboBox3.Text.Trim(' ');
            textBox4.Text = textBox4.Text.Trim(' ');
            textBox6.Text = textBox6.Text.Trim(' ');
            textBox7.Text = textBox7.Text.Trim(' ');
            textBox8.Text = textBox8.Text.Trim(' ');
            if (textBox2.Text != "" && comboBox3.Text != "" && textBox4.Text != "" && textBox6.Text != "" && textBox7.Text != "" && textBox8.Text != "" && textBox11.Text != "" && comboBox12.Text != "")
            {
                var d9 = dateTimePicker9.Checked ? dateTimePicker9.Text : "---";

                Editres2(d9);
                dateTimePicker9.Checked = false;
                textBox2.Text = "";
                comboBox3.SelectedIndex = 0;
                textBox4.Text = "---";
                textBox6.Text = "";
                textBox7.Text = "---";
                textBox8.Text = "---";
                comboBox12.SelectedIndex = -1;
                


            }
            else
            {
                MessageBox.Show("Заполнены не все поля!!!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            var bytes = new byte[12];//команда
            //Соединяемся с удаленным устройством
            try
            {
                //Устанавливаем удаленную конечную точку для сокета
                Port();
                var ipAddr = _ipHost.AddressList[0];
                var ipEndPoint = new IPEndPoint(ipAddr, _portsss);
                var senders = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Соединяем сокет с удаленной конечной точкой
                senders.Connect(ipEndPoint);
                //отправляем данные через сокет
                senders.Send(Encoding.Unicode.GetBytes("8"));
                senders.Receive(_bagr);
                //отправляем данные через сокет
                senders.Send(Encoding.Unicode.GetBytes(dateTimePicker1.Text));
                senders.Receive(_bagr);
                senders.Send(Encoding.Unicode.GetBytes(_name));
                var b = senders.Receive(bytes);
                label4.Text = Encoding.Unicode.GetString(bytes, 0, b);
                //Освобождаем сокет
                senders.Shutdown(SocketShutdown.Both);
                senders.Close();

            }
            catch
            {
                MessageBox.Show("Ошибка соединения с сервером!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void button7_Click(object sender, EventArgs e)
        {
            var DTJ = new DownloadTheJob(_ips);
            DTJ.Show();
        }

    }


}

