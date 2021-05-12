using System;
using System.Windows.Forms;

namespace CricleMainServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //CricleMainServer.TestConsole.TestReceiveAndProcesses.tMain();
        }

        private void openServer_Click(object sender, EventArgs e)
        {
            Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:Form1] -开启服务器:");
            ServerManager serverManager = new ServerManager();
            serverManager.OpenListenServer();
            openServer.Visible = false;
        }
    }
}
