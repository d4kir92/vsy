﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using NetzwerkLib;
using System.Threading;

namespace ClientForm
{
    public partial class ClientF : Form
    {
        Client client;
        bool running = false;

        void Con(string str)
        {
            Console.WriteLine("[CLIENT-FORM] " + str);
        }

        public ClientF()
        {
            InitializeComponent();
        }

        private void Client_Load(object sender, EventArgs e)
        {
            btn_action.Text = "Verbinden";
            btn_action.BackColor = Color.Green;
            label_status.Text = "Bereit";
            label_status.BackColor = Color.Green;
        }

        private void GetMsg( byte[] Data )
        {
            string text = ASCIIEncoding.ASCII.GetString(Data);
            string cl_name = tb_name.Text;
            text = text.Replace(cl_name+":", "Du:");
            text = text.Replace(cl_name + " ist", "Du bist");
            Con(text);
            tb_chat.AppendText(text + Environment.NewLine);
        }

        private void btn_action_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                running = !running;
                try
                {
                    btn_action.Text = "...";
                    btn_action.BackColor = Color.Yellow;
                    label_status.Text = "Am verbinden";
                    label_status.BackColor = Color.Yellow;

                    client = new Client();
                    IPAddress ip = IPAddress.Parse(tb_ip.Text);
                    int port = Int32.Parse(tb_port.Text);

                    IPEndPoint ipe = new IPEndPoint(ip, port);
                    client.Connect(ipe);

                    Action<byte[]> Get = new Action<byte[]>(GetMsg);
                    client.RegisterForCommand(Commands.SendMsg, Get);

                    Packet antwort = client.Receive();
                    if (antwort.Command == Commands.GetName)
                    {
                        string name = tb_name.Text;

                        Packet nP = new Packet();
                        nP.SetCommand(Commands.SetName);
                        nP.Setdata(name);

                        client.Send(nP);
                    }

                    btn_action.Text = "Trennen";
                    btn_action.BackColor = Color.Red;
                    label_status.Text = "Verbunden";
                    label_status.BackColor = Color.Green;

                    timer1.Enabled = true;
                }
                catch(Exception ex)
                {
                    btn_action.Text = "Verbinden";
                    btn_action.BackColor = Color.Green;
                    label_status.Text = "Server offline, retry!";
                    label_status.BackColor = Color.Red;
                    Con("EXCEPTION: " + ex.ToString());
                    running = false;
                }
            }
            else
            {
                running = !running;
                btn_action.Text = "Verbinden";
                btn_action.BackColor = Color.Green;
                label_status.Text = "Bereit";
                label_status.BackColor = Color.Green;

                client.Disconnect(false);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Packet p = client.Receive();
            client.ParsePacket(p);
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            string text = tb_write.Text;

            Packet p = new Packet();
            p.SetCommand(Commands.SendMsg);
            p.Setdata(tb_name.Text + ": " + text);

            client.Send(p);
            tb_write.Text = "";
        }

        private void tb_write_TextChanged(object sender, EventArgs e)
        {

        }

        private void tb_write_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_send_Click(this, null);
            }
        }
    }
}
