using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Runtime.InteropServices;
using TreadingTimer = System.Threading.Timer;

using dynamixel_sdk;


using Alturos;
using Alturos.Yolo;
using Alturos.Yolo.Model;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;

using PerspectiveTransformSetting;
using WebSocketSharp;
// CHINF 的類別庫
//using URControler2;
using CIRLABURControl;



namespace multimodal
{


    
    public partial class Form1 : Form
    {

        URServerAction uRServerAction_left;
        URServerAction uRServerAction_right;
        RobotMakeDrinkControl robotMakeDrinkControl;

        private delegate void SafeCallDelegate(Dictionary<TextBox, string> obj);

        public Form1()
        {
            InitializeComponent();
            int port_r = 1100;
            string addr = "0.0.0.0";
            ListenerBaseTcpListener listener_r = new ListenerBaseTcpListener(port_r, addr);
            Thread t_r = new Thread(new ThreadStart(listener_r.RunServer));
            t_r.Start();
            var stream_r = listener_r.GetNetworkStream();

            if (stream_r == null)
            {
                Console.Write("Waiting for a connection... ");
            }
            while (stream_r == null)
            {
                stream_r = listener_r.GetNetworkStream();
            }
            Console.WriteLine("Connected!");
            uRServerAction_right = new URServerAction(stream_r);



            ///////////////////////////////////////////////////////////////////////////////////////////

            int port_l = 1101;
            string addr_l = "0.0.0.0";
            ListenerBaseTcpListener listener_l = new ListenerBaseTcpListener(port_l, addr_l);
            Thread t_l = new Thread(new ThreadStart(listener_l.RunServer));
            t_l.Start();
            var stream_l = listener_l.GetNetworkStream();

            if (stream_l == null)
            {
                Console.Write("Waiting for a connection... ");
            }
            while (stream_l == null)
            {
                stream_l = listener_l.GetNetworkStream();
            }
            Console.WriteLine("Connected!");
            uRServerAction_left = new URServerAction(stream_l);


            ///////////////////////////////////////////////////////////////////////////////////////////


            robotMakeDrinkControl = new RobotMakeDrinkControl(uRServerAction_left, uRServerAction_right);
        }


        SocketTool mipy_socket;
        MxMotor motor1 = new MxMotor();
        int test;
        int initial_pos = 2020;
        int obj_pos = 1385;

        
        YoloWrapper gestureWrapper;
        YoloWrapper objectWrapper_side;
        YoloWrapper objectWrapper_top;

        IEnumerable<YoloItem> items1;
        IEnumerable<YoloItem> items2;

        private VideoCapture cap = null;
        private VideoCapture cap2 = null;
        Bitmap cap_img;
        Mat img1;

        static int OpenCamIndex_right = 0;
        static int OpenCamIndex_right_top = 2;
        static int OpenCamIndex_left = 1;
        static int OpenCamIndex_left_top = 3;


        const int MAXCAM = 2;
        Mat[] t_matrix;
        Form_PtSetting cam_set_right = new Form_PtSetting();
        Form_PtSetting cam_set_left = new Form_PtSetting();



        int cx1, cy1, cx2, cy2, cx3, cy3; //杯子x,y
        int bx1, by1, bx2, by2, bx3, by3; //瓶子x,y

        int cw1, cw2, cw3, ch1, ch2, ch3; //杯子width, height
        int bw1, bw2, bw3, bh1, bh2, bh3;  //瓶子width, height
        int brx, bry, bcx, bcy, blx, bly; //瓶子右、中、左 x ,y
        int brw, brh, bcw, bch, blw, blh; //瓶子右、中、左 width, height

        int line = 160; //放置區及瓶子區分隔線

        bool mode_1 = true; //選取飲料(true為未選)
        bool mode_1_first_answer = true;
        bool mode_2 = true; //選取糖度(true為未選)
        bool mode_3 = true; //偵測杯子(true為未執行)
        bool mode_4 = true; //飲料倒完為false
        bool cup_detect = false;

        bool move_1 = false;        
        bool move_2 = false;
        bool move_3 = false;


        bool mipy = false;


        bool cup_1, cup_2, cup_3 = false; // 杯子個數
        bool bottle_1, bottle_2, bottle_3 = false; //瓶子個數
        bool drink_1, drink_2, drink_3, drink_4 = false; //飲料種類(所選取的飲料類別為true)
        bool gradual_check = false;

        int count_t = 0; //計數
        int count, count1, count2, count3, count4 = 0; //計數

        string user_name;

        System.Windows.Forms.Timer timer;

        float[] bottle_right_r = new float[] { 0, 0 };
        float[] bottle_right_l = new float[] { 0, 0 };
        //float[] bottle_right = new float[] { 0, 0 };
        float[] bottle_center_r = new float[] { 0, 0 };
        float[] bottle_center_l = new float[] { 0, 0 };
        float[] bottle_left_r = new float[] { 0, 0 };
        float[] bottle_left_l = new float[] { 0, 0 };
        float[] cup_one_r = new float[] { 0, 0 };
        float[] cup_one_l = new float[] { 0, 0 };
        float[] cup_two = new float[] { 0, 0 };
        float[] cup_three = new float[] { 0, 0 };

        //float[] bottle_right_f;
        //float[] bottle_left_f;
        float[] cup_one_rf;
        float[] cup_two_rf;
        float[] cup_three_rf;

        float[] cup_one_lf;
        float[] cup_two_lf;
        float[] cup_three_lf;


        private void Form1_Load(object sender, EventArgs e)
        {
            var ws = new WebSocket("wss://test.chinf.me/listener/ur/");
            ws.OnMessage += (_, ee) =>
            {
                switch (ee.Data)
                {
                    case "A":
                        //uRServerAction.Move(new float[] { -0.394f, 0.059f, -0.093f, 0.28f, -3.2f, -0.15f });
                        Console.WriteLine($"Run A");
                        uRServerAction_left.GripperOpen();
                        uRServerAction_right.GripperOpen();
                        uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
                        uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);

                        clear_all();

                        mode_1 = false;
                        mode_2 = false;
                        //mode_3 = false;
                        drink_1 = true;

                        cap = new Emgu.CV.VideoCapture(1, VideoCapture.API.DShow);

                        objectWrapper_side = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");
                        while (mode_3)
                        {
                            Show_capture(sender, e);
                        }
                        while (mode_4)
                        {
                            Show_capture_b(sender, e);
                        }
                        break;
                    case "B":
                        //uRServerAction.Move(new float[] { -0.252f, -0.303f, -0.090f, 1.77f, -2.7f, 0.19f });
                        Console.WriteLine($"Run B");
                        break;
                    default:
                        Console.WriteLine($"{ee.Data} isn't know");
                        break;
                }
            };
            ws.Connect();
            ws.Send("BALUS");
        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (Mipy.Checked)
            {
                mipy = true;
            }
            else
            {
                mipy = false;
            }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            

            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            uRServerAction_left.TurnJoint(4, -15, 2);
            Thread.Sleep(2000);

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);

            uRServerAction_left.GripperCloseMAX();

            RobotInitial.robot_initial_pos_lc[1] -= 0.020F;

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);

            Thread.Sleep(2000);

            RobotInitial.robot_initial_pos_lc[1] += 0.020F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            uRServerAction_left.GripperOpen();

            Thread.Sleep(2000);

            RobotInitial.robot_initial_pos_lc[1] -= 0.01F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);

            uRServerAction_left.TurnJoint(-4, 25, 2);

            Thread.Sleep(5000);
            RobotInitial.robot_initial_pos_rc[1] += 0.02F;
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_rc);
            //RobotInitial.robot_initial_pos_lc[1] -= 0.02F;
            //uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            Thread.Sleep(2000);

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);


            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);

            clear_all();

            mode_1 = false;
            mode_2 = false;
            //mode_3 = false;
            drink_1 = true;

            cap = new Emgu.CV.VideoCapture(1, VideoCapture.API.DShow);

            objectWrapper_side = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");

            System.Windows.Forms.Application.Idle += new EventHandler(Show_capture);

        }

        private void button_settingFrom_Click(object sender, EventArgs e)
        {
            //motor1 = new MxMotor();

            test = motor1.ReadMxPosition();

            motor1.MxMotorSetPosition(initial_pos);
            System.Threading.Thread.Sleep(1000);
            motor1.MxMotorSetPosition(obj_pos);

            Form_PtSetting form_Pt = new Form_PtSetting();
            form_Pt.Show();

        }


        private void Start_Click(object sender, EventArgs e)
        {
            main_run();
        }


        void main_run()
        {
            clear_all();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;

            motor1.MxMotorSetPosition(initial_pos);

            if (Mipy.Checked)
            {
                cap2 = new Emgu.CV.VideoCapture(2, VideoCapture.API.DShow);

                mipy_socket = new SocketTool();
                mipy_socket.creatClient("127.0.0.1", 8888);
                Application.Idle += new EventHandler(Mipy_read_user);

            }
            else
            {
                read_py();
                stop_Face();
            }
        }


        void Mipy_read_user(object sender, EventArgs e)
        {

            mipy_socket.client_SendData("FaceNameRequest");
            Bitmap cap_mipy = cap2.QueryFrame().Bitmap;
            Image<Bgr, Byte> image_mipy = new Image<Bgr, byte>(cap_mipy);
            Mat img_mipy = image_mipy.Mat;
            imageBox1.Image = img_mipy;

            user_name = mipy_socket.client_ReadData();
            if (user_name != "None" && user_name != "Stranger!" && user_name != "")
            {
                stop_Face();
            }

        }

        void stop_Face()
        {
            if (Mipy.Checked)
            {
                Application.Idle -= new EventHandler(Mipy_read_user);
                textBox4.Text = user_name;
            }

            gesture_run();
        }

        void clear_all()
        {
            mode_1 = true;
            mode_1_first_answer = true;
            mode_2 = true;
            mode_3 = true;
            mode_4 = true;

            cup_1 = false;
            cup_2 = false;
            cup_3 = false;
            bottle_1 = false;
            bottle_2 = false;
            bottle_3 = false;
            drink_1 = false;
            drink_2 = false;
            drink_3 = false;
            drink_4 = false;
            gradual_check = false;

            move_1 = false;
            move_2 = false;
            move_3 = false;


            count_t = 0;
            count = 0;
            count1 = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;

            textBox1.Text = "";
            textBox2.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox7.Text = "";

        }


        void read_py()
        {
            //實體檔案名稱
            string fileName = "username.txt";

            //起一個Process執行Python程式
            Process pyProc = new Process();
            pyProc.EnableRaisingEvents = true;
            pyProc.StartInfo.UseShellExecute = false;
            pyProc.StartInfo.RedirectStandardOutput = true;
            pyProc.StartInfo.FileName = "python";
            pyProc.StartInfo.CreateNoWindow = true;

            string sArguments = @"test_noGUI.py";
            //將必要的參數丟進Python，其中test.py的路徑是放在與此UI執行路徑的同一個資料夾中
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + sArguments;// 获得python文件的绝对路径（将文件放在c#的debug文件夹中可以这样操作）
                                                                                                       // string path = @"D:\tensorflow-facenet\test" + sArguments;//(因为我没放debug下，所以直接写的绝对路径,替换掉上面的路径了)
            pyProc.StartInfo.FileName = @"C:\anaconda\python.exe";//没有配环境变量的话，可以像我这样写python.exe的绝对路径。如果配了，直接写"python.exe"即可
            string sArgu = path;
            pyProc.StartInfo.Arguments = sArgu;

            pyProc.Start();
            pyProc.BeginOutputReadLine();
            pyProc.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            Console.ReadLine();
            pyProc.WaitForExit();
            pyProc.Close();
            string user = System.IO.File.ReadAllText(fileName);
            textBox4.Text = user;
            //File.Delete(fileName);
        }

        static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                AppendText(e.Data + Environment.NewLine);
            }
        }

        public static void AppendText(string text)
        {
            Console.WriteLine(text);     //此处在控制台输出.py文件print的结果

        }

        void Show_capture_open(object sender, EventArgs e)
        {
            textBox2.Text = "請放置杯子";
            textBox5.Text = "";
            if (count == 0)
            {
                timer.Enabled = true;
                count++;
            }

            else if (count >= 3)
            {
                timer.Enabled = false;
                cup_detect = true;

                stop_run();

            }
            else
                count++;

        }

        void gesture_run()
        {
            count = 0;
            textBox2.Text = "請問您想喝哪種飲料?";
            textBox5.Text = "(1):" + DrinkName.first_drink_name + "   (2):" + DrinkName.second_drink_name + "   (3):" + DrinkName.gradual_name;

            cap = new Emgu.CV.VideoCapture(1, VideoCapture.API.DShow);
            gestureWrapper = new YoloWrapper("yolov2-tiny_number_test.cfg", "yolov2-tiny_number_90000.weights", "number.names");

            System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_g);
        }


        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        public byte[] BmpToBytes(Bitmap bmp)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] b = ms.GetBuffer();
            return b;
        }

        void stop_run()
        {
            count_t = 0;
            count1 = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;

            count = 0;

            if (mode_1 == true)
            {
                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_g);
                
                if (gradual_check)
                {                    
                    textBox2.Text = "請問要搭配何種漸層?";
                    textBox5.Text = "(1):" + DrinkName.second_drink_name + DrinkName.first_drink_name + "   (2):" + DrinkName.third_drink_name + DrinkName.first_drink_name;
                    System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_g);
                }
                else
                {
                    mode_1 = false;
                    motor1.MxMotorSetPosition(obj_pos);
                    System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_open);
                }


            }
            else if (mode_2 == true)
            {
                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_open);
                textBox2.Text = "偵測杯子中";
                textBox5.Text = "";
                mode_2 = false;
                objectWrapper_side = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");
                
                System.Windows.Forms.Application.Idle += new EventHandler(Show_capture);

            }

            else if (mode_3 == true)
            {
                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture);
                TextBoxWriteText(textBox2, "準備您的飲料中，請稍後~");
                //textBox2.Text = "準備您的飲料中，請稍後~";
                mode_3 = false;


                if (cup_1 == true)
                {
                    var c1x = cx1 + cw1 / 2;
                    var c1y = cy1 + ch1;

                    cup_one_rf = cam_set_right.I2W(c1x, c1y, OpenCamIndex_right, true);
                    cup_one_r[0] = cup_one_rf[0] / 1000;
                    cup_one_r[1] = cup_one_rf[2] / 1000;

                    cup_one_lf = cam_set_left.I2W(c1x, c1y, OpenCamIndex_left, true);
                    cup_one_l[0] = cup_one_lf[0] / 1000;
                    cup_one_l[1] = cup_one_lf[2] / 1000;


                }

                System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_b);

            }
            else if (mode_4 == true)
            {
                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_b);
                var prx = brx + brw / 2;
                var pry = bry + brh;
                var plx = blx + blw / 2;
                var ply = bly + blh;
                var pcx = bcx + bcw / 2;
                var pcy = bcy + bch;

                float[] bottle_right_rf = cam_set_right.I2W(prx, pry, OpenCamIndex_right, true);  //轉正後的世界座標
                bottle_right_r[0] = bottle_right_rf[0] / 1000;
                bottle_right_r[1] = bottle_right_rf[2] / 1000;

                float[] bottle_right_lf = cam_set_left.I2W(prx, pry, OpenCamIndex_left, true);
                bottle_right_l[0] = bottle_right_lf[0] / 1000;
                bottle_right_l[1] = bottle_right_lf[2] / 1000;


                float[] bottle_left_rf = cam_set_right.I2W(plx, ply, OpenCamIndex_right, true);  //轉正後的世界座標
                bottle_left_r[0] = bottle_left_rf[0] / 1000;
                bottle_left_r[1] = bottle_left_rf[2] / 1000;

                float[] bottle_left_lf = cam_set_left.I2W(plx, ply, OpenCamIndex_left, true);
                bottle_left_l[0] = bottle_left_lf[0] / 1000;
                bottle_left_l[1] = bottle_left_lf[2] / 1000;


                float[] bottle_center_rf = cam_set_right.I2W(pcx, pcy, OpenCamIndex_right, true);  //轉正後的世界座標
                bottle_center_r[0] = bottle_center_rf[0] / 1000;
                bottle_center_r[1] = bottle_center_rf[2] / 1000;

                float[] bottle_center_lf = cam_set_left.I2W(pcx, pcy, OpenCamIndex_left, true);
                bottle_center_l[0] = bottle_center_lf[0] / 1000;
                bottle_center_l[1] = bottle_center_lf[2] / 1000;

                if (drink_1)
                {
                    TextBoxWriteText(textBox2, DrinkName.first_drink_name);
                    //textBox2.Text = DrinkName.first_drink_name;
                    mode_4 = false;                  
                    robotMakeDrinkControl.rotate_bottle(bottle_right_r, cup_one_r, bottle_right_l);
                    move_1 = true;
                    stop_run();
                }
                else if (drink_2)
                {
                    TextBoxWriteText(textBox2, DrinkName.second_drink_name);
                    //textBox2.Text = DrinkName.second_drink_name;
                    mode_4 = false;
                    robotMakeDrinkControl.open_bottle(bottle_center_r, cup_one_r, cup_one_l, bottle_center_l);
                    move_2 = true;
                    stop_run();
                }
                else if (drink_3)
                {
                    TextBoxWriteText(textBox2, DrinkName.second_drink_name + DrinkName.first_drink_name);
                    //textBox2.Text = DrinkName.second_drink_name + DrinkName.first_drink_name;

                    if (move_1 == false)
                    {                      
                        robotMakeDrinkControl.rotate_bottle(bottle_right_r, cup_one_r, bottle_right_l);
                        move_1 = true;
                        stop_run();
                    }
                    else if (move_2 == false)
                    {
                        robotMakeDrinkControl.open_bottle(bottle_center_r, cup_one_r, cup_one_l, bottle_center_l);
                        move_2 = true;
                        stop_run();
                    }

                    else
                    {
                        mode_4 = false;
                        stop_run();
                    }
                }
                else if (drink_4)
                {
                    TextBoxWriteText(textBox2, DrinkName.third_drink_name + DrinkName.first_drink_name);
                    //textBox2.Text = DrinkName.third_drink_name + DrinkName.first_drink_name;

                    if (move_1 == false)
                    {             
                        robotMakeDrinkControl.rotate_bottle(bottle_right_r, cup_one_r, bottle_right_l);
                        move_1 = true;
                        stop_run();
                    }
                    else if (move_3 == false)
                    {
                        robotMakeDrinkControl.pick_bottle(bottle_left_l, cup_one_l, drink_2);
                        move_3 = true;
                        stop_run();
                    }

                    else
                    {
                        mode_4 = false;
                        stop_run();
                    }
                }

            }

            else if (mode_4 == false)
            {
                TextBoxWriteText(textBox2, "您的飲料已經準備好了，請拿取，謝謝~");
                //textBox2.Text = "您的飲料已經準備好了，請拿取，謝謝~";
              //  robotMakeDrinkControl.pick_cup(cup_one_r,cup_one_l);

                cap.Stop();
                cap.Dispose();

            }

        }


        void Show_capture_g(object sender, EventArgs e)
        {
            while (true)
            {
                Mat img;
                img = cap.QueryFrame();
                if (img != null && img.Width != 0)
                    break;
            }
            cap_img = cap.QueryFrame().Bitmap;

            System.Drawing.Rectangle rect1;

            Image<Bgr, Byte> imageCV1 = new Image<Bgr, byte>(cap_img);
            Mat img1 = imageCV1.Mat;


            imageBox1.SizeMode = PictureBoxSizeMode.Zoom;

            byte[] img2 = BmpToBytes(cap_img);
            items1 = gestureWrapper.Detect(img2);
            cap_img.Dispose();
            cap_img = null;

            if (mode_1 && mode_1_first_answer)
            {
                if (count1 >= 5 || count2 >= 5 || count3 >= 5)
                {
                    if (count1 >= 5)
                    {
                        textBox1.Text = DrinkName.first_drink_name;
                        drink_1 = true;
                    }
                    else if (count2 >= 5)
                    {
                        textBox1.Text = DrinkName.second_drink_name;
                        drink_2 = true;
                    }
                    else if (count3 >= 5)
                    {
                        textBox1.Text = DrinkName.gradual_name;
                        gradual_check = true;
                    }

                    //else if (count4 >=5)
                    //{
                    //    textBox1.Text = "中間+右邊瓶子";
                    //    drink_4 = true;
                    //}


                    if (count == 0)
                    {
                        timer.Enabled = true;
                        count++;
                    }

                    else if (count >= 5)
                    {
                        timer.Enabled = false;
                        mode_1_first_answer = false;
                        stop_run();
                    }

                    else
                        count++;

                }
                else
                {
                    foreach (YoloItem item in items1)
                    {
                        int confidence1 = (int)(item.Confidence * 100);
                        rect1 = new System.Drawing.Rectangle(item.X, item.Y, item.Width, item.Height);
                        CvInvoke.Rectangle(img1, rect1, new MCvScalar(65, 105, 255, 255), 3);
                        CvInvoke.PutText(img1, item.Type + "  " + confidence1, new System.Drawing.Point(item.X, item.Y - 10), 0, 0.5, new MCvScalar(65, 105, 255, 255));

                        if (item.Type == "one")
                            count1++;
                        if (item.Type == "two")
                            count2++;
                        if (item.Type == "three")
                            count3++;
                        if (item.Type == "four")
                            count4++;

                    }
                }
            }

            else if (mode_1 && gradual_check && !mode_1_first_answer)
            {
                if (count1 >= 5 || count2 >= 5)
                {
                    if (count1 >= 5)
                    {
                        textBox1.Text = DrinkName.second_drink_name + DrinkName.first_drink_name;
                        drink_3 = true;
                    }
                    else if (count2 >= 5)
                    {
                        textBox1.Text = DrinkName.third_drink_name + DrinkName.first_drink_name;
                        drink_4 = true;
                    }


                    if (count == 0)
                    {
                        timer.Enabled = true;
                        count++;
                    }

                    else if (count >= 5)
                    {
                        timer.Enabled = false;
                        gradual_check = false;
                        stop_run();
                    }

                    else
                        count++;

                }
                else
                {
                    foreach (YoloItem item in items1)
                    {
                        int confidence1 = (int)(item.Confidence * 100);
                        rect1 = new System.Drawing.Rectangle(item.X, item.Y, item.Width, item.Height);
                        CvInvoke.Rectangle(img1, rect1, new MCvScalar(65, 105, 255, 255), 3);
                        CvInvoke.PutText(img1, item.Type + "  " + confidence1, new System.Drawing.Point(item.X, item.Y - 10), 0, 0.5, new MCvScalar(65, 105, 255, 255));

                        if (item.Type == "one")
                            count1++;
                        if (item.Type == "two")
                            count2++;

                    }
                }
            }

            imageBox1.Image = img1;

        }

        void Show_capture(object sender, EventArgs e)
        {
            cap_img = cap.QueryFrame().Bitmap;

            System.Drawing.Rectangle rect1;

            Image<Bgr, Byte> imageCV1 = new Image<Bgr, byte>(cap_img);
            Mat img1 = imageCV1.Mat;
            Mat img1_2  = new Mat ();
            img1.CopyTo(img1_2);
           //Size A = new Size(416, 416);
           // CvInvoke.Resize(img1_2, img1_2,A);
;
            imageBox1.SizeMode = PictureBoxSizeMode.Zoom;

            byte[] img2 = BmpToBytes(img1_2.Bitmap);
            items2 = objectWrapper_side.Detect(img2);
            cap_img.Dispose();
            cap_img = null;

            if (count_t < 3)
            {

                foreach (YoloItem item in items2)
                {
                    int ry = item.Y + item.Height / 2;

                    if (ry < line)
                    {


                        if (item.Type == "cup" || item.Type == "bowl")
                        {
                            int confidence1 = (int)(item.Confidence * 100);
                            rect1 = new System.Drawing.Rectangle(item.X, item.Y, item.Width, item.Height);
                            CvInvoke.Rectangle(img1, rect1, new MCvScalar(65, 105, 255, 255), 3);
                            CvInvoke.PutText(img1, item.Type + "  " + confidence1, new System.Drawing.Point(item.X, item.Y - 10), 0, 0.5, new MCvScalar(65, 105, 255, 255));

                            if (count == 0)
                            {
                                cx1 = item.X;
                                cy1 = item.Y;
                                cw1 = item.Width;
                                ch1 = item.Height;
                                count++;
                                cup_1 = true;
                            }
                            else if (count == 1)
                            {
                                cx2 = item.X;
                                cy2 = item.Y;
                                cw2 = item.Width;
                                ch2 = item.Height;
                                if ((cx1 + 20 > cx2 && cx2 > cx1 - 20) && (cy1 + 20 > cy2 && cy2 > cy1 - 20))
                                {
                                    cx1 = cx2;
                                    cy1 = cy2;
                                    cw1 = cw2;
                                    ch1 = ch2;
                                }
                                else
                                {
                                    count++;
                                    cup_2 = true;
                                }
                            }
                            else if (count == 2)
                            {
                                cx3 = item.X;
                                cy3 = item.Y;
                                cw3 = item.Width;
                                ch3 = item.Height;
                                if ((cx1 + 20 > cx3 && cx3 > cx1 - 20) && (cy1 + 20 > cy3 && cy3 > cy1 - 20))
                                {
                                    cx1 = cx3;
                                    cy1 = cy3;
                                    cw1 = cw3;
                                    ch1 = ch3;
                                }

                                else if ((cx2 + 20 > cx3 && cx3 > cx2 - 20) && (cy2 + 20 > cy3 && cy3 > cy2 - 20))
                                {
                                    cx2 = cx3;
                                    cy2 = cy3;
                                    cw2 = cw3;
                                    ch2 = ch3;
                                }
                                else
                                {
                                    count++;
                                    cup_3 = true;
                                }
                            }

                            count_t++;

                        }
                    }


                }

            }
            else
            {
                TextBoxWriteText(textBox7, count.ToString());
                stop_run();
            }



            imageBox1.Image = img1_2;
        }
        void WriteTextSafe(Dictionary<TextBox,string> obj)
        {
            if (obj.ElementAt(0).Key.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                obj.ElementAt(0).Key.Invoke(d, new object[] { obj });
            }
            else
            {
                obj.ElementAt(0).Key.Text = obj.ElementAt(0).Value;
            }
        }
        void TextBoxWriteText(TextBox textBox, string text)
        {
            var d = new Dictionary<TextBox, string>();
            d.Add(textBox, text);
            WriteTextSafe(d);
        }

        void Show_capture_b(object sender, EventArgs e)
        {
            cap_img = cap.QueryFrame().Bitmap;
            Image<Bgr, Byte> imageCV1 = new Image<Bgr, byte>(cap_img);
            Mat img1 = imageCV1.Mat;
            System.Drawing.Rectangle rect1;

            imageBox1.SizeMode = PictureBoxSizeMode.Zoom;

            byte[] img2 = BmpToBytes(cap_img);
            items2 = objectWrapper_side.Detect(img2);
            cap_img.Dispose();
            cap_img = null;

            if (count < 3)
            {
                foreach (YoloItem item in items2)
                {
                    int ry = item.Y + item.Height / 2;

                    if (ry > line)
                    {
                        if (item.Type == "bottle" || item.Type == "cup")
                        {
                            int confidence1 = (int)(item.Confidence * 100);
                            rect1 = new System.Drawing.Rectangle(item.X, item.Y, item.Width, item.Height);
                            CvInvoke.Rectangle(img1, rect1, new MCvScalar(65, 105, 255, 255), 3);
                            CvInvoke.PutText(img1, item.Type + "  " + confidence1, new System.Drawing.Point(item.X, item.Y - 10), 0, 0.5, new MCvScalar(65, 105, 255, 255));
                            if (count == 0)
                            {
                                bx1 = item.X;
                                by1 = item.Y;
                                bw1 = item.Width;
                                bh1 = item.Height;

                                count++;
                                bottle_1 = true;
                            }
                            else if (count == 1)
                            {
                                bx2 = item.X;
                                by2 = item.Y;
                                bw2 = item.Width;
                                bh2 = item.Height;

                                if ((bx1 + 20 > bx2 && bx2 > bx1 - 20) && (by1 + 20 > by2 && by2 > by1 - 20))
                                {
                                    bx1 = bx2;
                                    by1 = by2;
                                    bw1 = bw2;
                                    bh1 = bh2;
                                }
                                else
                                {
                                    count++;
                                    bottle_2 = true;
                                }
                            }
                            else if (count == 2)
                            {
                                bx3 = item.X;
                                by3 = item.Y;
                                bw3 = item.Width;
                                bh3 = item.Height;

                                if ((bx1 + 20 > bx3 && bx3 > bx1 - 20) && (by1 + 20 > by3 && by3 > by1 - 20))
                                {
                                    bx1 = bx3;
                                    by1 = by3;
                                    bw1 = bw3;
                                    bh1 = bh3;

                                }

                                else if ((bx2 + 20 > bx3 && bx3 > bx2 - 20) && (by2 + 20 > by3 && by3 > by2 - 20))
                                {
                                    bx2 = bx3;
                                    by2 = by3;
                                    bw2 = bw3;
                                    bh2 = bh3;
                                }
                                else
                                {
                                    count++;
                                    bottle_3 = true;
                                }
                            }

                        }
                    }

                }
            }

            else
            {
                TextBoxWriteText(textBox2, "Done");
                //textBox2.Text = "Done";


                int[] array = new int[] { bx1, bx2, bx3 };
                var max = array.Max();
                var min = array.Min();

                if (max == bx1)
                {
                    brx = bx1;
                    bry = by1;
                    brw = bw1;
                    brh = bh1;

                    if (min == bx2)
                    {
                        blx = bx2;
                        bly = by2;
                        blw = bw2;
                        blh = bh2;


                        bcx = bx3;
                        bcy = by3;
                        bcw = bw3;
                        bch = bh3;
                    }

                    else
                    {
                        blx = bx3;
                        bly = by3;
                        blw = bw3;
                        blh = bh3;


                        bcx = bx2;
                        bcy = by2;
                        bcw = bw2;
                        bch = bh2;
                    }

                }
                else if (max == bx2)
                {
                    brx = bx2;
                    bry = by2;
                    brw = bw2;
                    brh = bh2;

                    if (min == bx1)
                    {
                        blx = bx1;
                        bly = by1;
                        blw = bw1;
                        blh = bh1;

                        bcx = bx3;
                        bcy = by3;
                        bcw = bw3;
                        bch = bh3;
                    }

                    else
                    {
                        blx = bx3;
                        bly = by3;
                        blw = bw3;
                        blh = bh3;

                        bcx = bx1;
                        bcy = by1;
                        bcw = bw1;
                        bch = bh1;
                    }
                }

                else if (max == bx3)
                {

                    brx = bx3;
                    bry = by3;
                    brw = bw3;
                    brh = bh3;

                    if (min == bx1)
                    {
                        blx = bx1;
                        bly = by1;
                        blw = bw1;
                        blh = bh1;

                        bcx = bx2;
                        bcy = by2;
                        bcw = bw2;
                        bch = bh2;
                    }

                    else
                    {
                        blx = bx2;
                        bly = by2;
                        blw = bw2;
                        blh = bh2;

                        bcx = bx1;
                        bcy = by1;
                        bcw = bw1;
                        bch = bh1;
                    }
                }

                stop_run();
            }


            imageBox1.Image = img1;
        }

        

        
    }


}
