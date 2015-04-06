using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Data.SqlClient;
namespace RemoteMontring//name of project 
{
    public partial class RemoteSystem : Form
    {
        private ArrayList alSockets;
        Queue<string> q = new Queue<string>();
        public int my_port = 8080;
        public RemoteSystem()
        {
            InitializeComponent();
            pass_txt.PasswordChar = '*';//pass_txt is name of textbox of password
            timer1.Enabled = true;
        }



        private void button1_Click(object sender, EventArgs e)//this function for cancle button 
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)//this function for login button
        {

            try {
                 
                SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\users\tahani\Documents\Data.mdf;Integrated Security=True;Connect Timeout=30");// place of database can defind from your database right click then copy conncetion string and replce here 
                //SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\users\tahani\Documents\Data.mdf;Integrated Security=True;Connect Timeout=30");// place of database can defind from your database right click then copy conncetion string and replce here 

                //C:\users\tahani\Documents\Data.mdf
                SqlCommand quiry = new SqlCommand(" Select * From doctor where Id ='" + Id_txt.Text + "'and Password ='" + this.pass_txt.Text + "'", con);//chang id_txt and pass_txt are depand of your textbox 
              

                SqlDataReader myreader;
                con.Open();
                myreader = quiry.ExecuteReader();// 
                int count = 0;
                while (myreader.Read()) {
                    count = +1;
                }
                if (count == 1) {
                    MessageBox.Show("ID and Password are correct");
                    doctor frm = new doctor(Id_txt.Text);// depand on name of scaned form your create it.
                    frm.Show();
                }
                else if (count>1){
                    MessageBox.Show("Dublicate ID Access denide");
                }
                else
                    MessageBox.Show("Somthing wrong, Retrye to enter correct username and password");
                con.Close();
               
                
            }
            catch (Exception ex){
                MessageBox.Show(ex.Message);
            }

            
                
        }

        private void RemoteSystem_Load(object sender, EventArgs e)
        {
            alSockets = new ArrayList();
            Thread thdListener = new Thread(new ThreadStart(listenerThread));
            thdListener.Start();
        }
        public void listenerThread()
        {
            TcpListener tcpListener = new TcpListener(8080);
            tcpListener.Start();
            while (true)
            {
                Socket handlerSocket = tcpListener.AcceptSocket();
                if (handlerSocket.Connected)
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        lbConnections.Items.Add(handlerSocket.RemoteEndPoint.ToString() + " connected.");// here problem
                    });
                    lock (this)
                    {
                        alSockets.Add(handlerSocket);
                    }
                    Thread thdHandler = new Thread(handlerThread);
                    thdHandler.Start();
                }
            }
        }
        public void handlerThread()
        {
            Socket handlerSocket = (Socket)alSockets[alSockets.Count - 1];
            NetworkStream networkStream = new NetworkStream(handlerSocket);
            int thisRead = 0;
            int blockSize = 1024;

            Byte[] dataByte = new Byte[blockSize];

            lock (this)
            {
                // Only one process can access the same file at any given 􀆟me
                //Stream fileStream = File.OpenWrite("E:\\magdi.txt");
                while (true)
                {
                    thisRead = networkStream.Read(dataByte, 0, blockSize);
                    string stringData = Encoding.ASCII.GetString(dataByte, 0, thisRead);
                    q.Enqueue(stringData);
                   
                    //
                    if (thisRead == 0) break;
                }
                //fileStream.Close();
            }
            lbConnections.Items.Add("File Wri􀆩en");
            handlerSocket = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string new_data;
            if (q.Count > 0)
            {
                new_data = q.Dequeue();
                lbConnections.Items.Add(new_data);
                add_new_temp(new_data);
            }
        }
        private void add_new_temp(string my_data)
        {
            string my_id = "";
            string my_temp = "";
            DateTime my_day = DateTime.Now;
            DateTime my_time = DateTime.Now.ToLocalTime();
            my_id = my_data.Substring(0, 3);
            my_temp = my_data.Substring(3, 2);
            try
            {

                SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\users\tahani\Documents\Data.mdf;Integrated Security=True;Connect Timeout=30");// place of database can defind from your database right click then copy conncetion string and replce here 
                //SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\users\tahani\Documents\Data.mdf;Integrated Security=True;Connect Timeout=30");// place of database can defind from your database right click then copy conncetion string and replce here 
                con.Open();
                string stmt = "INSERT INTO dbo.temp_p(IDPatient, temprture, date) VALUES(@ID, @temperature, @Date )";

                SqlCommand cmd = new SqlCommand(stmt, con);
                cmd.Parameters.Add("@ID", SqlDbType.Char);
                cmd.Parameters.Add("@temperature", SqlDbType.Char);
                cmd.Parameters.Add("@Date", SqlDbType.Date );
                //cmd.Parameters.Add("@Time", SqlDbType.Time);
                cmd.Parameters["@ID"].Value = my_id;
                cmd.Parameters["@temperature"].Value = my_temp;
                cmd.Parameters["@Date"].Value = my_day;
                //cmd.Parameters["@Time"].Value = my_time;
                cmd.ExecuteNonQuery();     
                
                con.Close();


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }

}
            
        

        
    

