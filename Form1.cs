using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketClent
{
    public partial class Form1 : Form
    {
        private Socket clientSocket = null;
        private Thread ReceiveThread = null;
        private bool ReceiveFlag = false;

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
            timerSocketSend.Interval = 60*1000;
            


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text.Equals("开始连接"))
            {
                button1.Enabled = false;
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Thread t = new Thread(new ThreadStart(ConnectServerSocket));
                t.Start();
            }
            else 
            {
                clientSocket.Close();
                button1.Text = "开始连接";
                button2.Enabled = false;
                ReceiveFlag = false;
                timerSocketSend.Stop();
                clientSocket = null;
            }
            
        }


         void ConnectServerSocket()
        {
            this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
            {
                textBox3.AppendText("开始获取ip... " + Environment.NewLine);
            }));
            IPAddress[] iPAddresses = null;
            if (checkBox2.Checked)
            {
                IPHostEntry iPHostEntry = Dns.GetHostEntry(IPtextBox.Text.Trim());
                iPAddresses = iPHostEntry.AddressList;
                foreach (IPAddress ip in iPAddresses)
                {
                    Console.WriteLine(ip);
                }
            }
            else
            {
                Console.WriteLine(IPtextBox.Text.Trim());
                if (!string.IsNullOrEmpty(IPtextBox.Text.Trim()))
                {
                    try
                    {
                        Console.WriteLine("stringIsNullorEmpty:" + IPtextBox.Text.Trim());
                        iPAddresses = new IPAddress[1];
                        iPAddresses[0] = IPAddress.Parse(IPtextBox.Text.Trim());
                        Console.WriteLine(iPAddresses[0]);
                    }
                    catch
                    {
                        MessageBox.Show("输入的IP地址格式错误！");
                        this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                        {
                            button1.Enabled = true;
                        }));
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("输入的IP地址格式错误！");
                    this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                    {
                        button1.Enabled = true;
                    }));
                    return;
                }
            }
            if ( iPAddresses != null &&  iPAddresses.Length > 0)
            {
                this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                {
                    textBox3.AppendText("IP:" + iPAddresses[0] + "  获取成功！" + Environment.NewLine);
                }));
                try
                {
                    int port = 0;
                    int.TryParse(PorttextBox.Text,out port);
                    clientSocket.Connect(new IPEndPoint(iPAddresses[0], port));
                    Console.WriteLine("连接到服务器成功！\n");
                    

                    this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                    {
                        textBox3.AppendText("连接成功！" + Environment.NewLine);
                    }));
                    this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                    {
                        button1.Text = "断开连接";
                        button1.Enabled = true;
                        button2.Enabled = true;
                    }));

                   

                    ReceiveFlag = true;
                    ReceiveThread = new Thread(new ThreadStart(receiverLoop));
                    ReceiveThread.Start();
                    if (checkBox1.Checked)
                    {
                        this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                        {
                            timerSocketSend.Start();
                        }));
                    }
                      


                }
                catch
                {
                    Console.WriteLine("连接到服务器失败！");
                    MessageBox.Show("连接失败！");
                    this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                    {
                        button1.Enabled = true;
                    }));
                    return;
                }
            }
        }


        void receiverLoop()
        {
            byte[] rbyte = new byte[1024 * 4];
            int rlength = 0;
            StringBuilder sReceive = new StringBuilder();
            Console.WriteLine("socket 超时时间：" + clientSocket.SendTimeout);
            while (ReceiveFlag)
            {
                try
                {
                    rlength = clientSocket.Receive(rbyte);
                    if (rlength > 0)
                    {
                        byte[] r = new byte[rlength];
                        Array.Copy(rbyte, r, rlength);
                        this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                        {
                            textBox3.Text = textBox3.Text +  Encoding.UTF8.GetString(r).Replace("\n", Environment.NewLine);
                            textBox3.Focus();
                      
                        }));
                        Console.WriteLine(Encoding.UTF8.GetString(r));
                    }
                    else
                    {
                        this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                        {
                            if (clientSocket != null)
                            {
                                clientSocket.Close();
                            }
                            button1.Text = "开始连接";
                            button2.Enabled = false;
                            ReceiveFlag = false;
                        }));
                        return;
                    }
                }
                catch
                {
                    this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
                    {
                        if (clientSocket != null)
                        {
                            clientSocket.Close();
                        }
                        button1.Text = "开始连接";
                        button2.Enabled = false;
                        ReceiveFlag = false;
                    }));
                    return;
                }
                
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new System.Threading.ThreadStart(delegate ()
            {
                textBox3.Text = "";
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox4.Text.Length > 0)
            {
                byte[] t = Encoding.UTF8.GetBytes(textBox4.Text);
                if (clientSocket != null)
                {
                    clientSocket.Send(t);
                }
                
            }
            Console.WriteLine(checkBox1.Checked);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
           
            if (clientSocket != null)
            {
                clientSocket.Close();
            }
            timerSocketSend.Stop();
            ReceiveFlag = false;
            clientSocket = null;
            Console.WriteLine("关闭");
        }


        private void timerSocketSend_Tick(object sender, EventArgs e)
        {

            if(clientSocket != null)
            {
                clientSocket.Send(Encoding.UTF8.GetBytes("I live new!"+ Environment.NewLine));
                Console.WriteLine("发送心跳包！");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine(checkBox1.Checked);
            
            if (clientSocket != null)
            {
                if (checkBox1.Checked)
                {
                    timerSocketSend.Start();
                }
                else
                {
                    timerSocketSend.Stop();
                }
                
            }
        }
    }
}
