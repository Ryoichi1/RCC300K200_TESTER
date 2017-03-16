using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Management;
using System.Media;



namespace Project1
{
    public partial class Form1 : Form
    {
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Infomation data of [RCC300K_AT01]
        private string info_ver = "6.40";
        private string info_date = "2017/03/16";
        //
        //  Version     Update
        //  1.00        2015/10/02      新規作成
        //  1.01        2015/10/08      ｼﾘｱﾙNoのｲﾝｸﾘﾒﾝﾄを修正、誤記訂正(1箇所)
        //  1.10        2015/10/27      ｻﾝﾌﾟﾙの選択を追加
        //  1.11        2016/01/20      ﾃﾞｰﾄｺｰﾄﾞの桁数を訂正
        //  2.00        2016/02/12      出荷設定変更(51→53)に伴う修正
        //  3.00        2016/02/19      DMMの制御変更(RS-232C→GP-IB)
        //  4.00        2016/03/25      ｼｰｹﾝｽの時間ﾃﾞｰﾀの修正とﾛｸﾞ保存ﾃﾞｰﾀを追加
        //  4.10        2016/04/20      電圧の測定を10回の平均値に変更。ﾌﾚｰﾑﾚｽﾎﾟﾝｽの時間を補正
        //  5.00        2016/04/26      ﾌｧｲﾙの構成を全面変更
        //  6.00        2016/05/21      表示画面を全面変更
        //  6.10        2016/06/21      AFD 125KΩの範囲変更、読込ﾃﾞｰﾀを保存(5個)。
        //  6.11        2016/06/21      ﾌﾟﾛｾｽﾊﾞｰを追加。
        //  6.20        2016/07/26      電圧の測定を安定してからに変更。
        //  6.30        2017/03/10      アプリCPU FW変更 k_1.60.08.mot → k_1.61.0.mot
        //  6.40        2017/03/16      シリアルナンバーが6桁になる不具合を修正


// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public const int TEST = 28;

        // ｽﾄｯﾌﾟｳｫｯﾁ
        Stopwatch sw = new Stopwatch();

        // ｻｳﾝﾄﾞ
        public SoundPlayer player = null;


        // unicodeのEncodingｸﾗｽ作成
        Encoding encUni = Encoding.GetEncoding("utf-16");
        // s-jisのEncodingｸﾗｽ作成
        Encoding encSjis = Encoding.GetEncoding("shift-jis");

        public const byte STX = 0x02;
        public const byte ETX = 0x03;

        // IOﾎﾞｰﾄﾞ定義
        System.IntPtr hDv1;             // Device Handle
        public static int Status;       // Device Status (Return Code)
        public static int Number;       // Devices Number
        public static byte Direction;
        public static byte Port;
        public byte Value;

        private int SNo1;                                               // Serial No

        public static byte DataP10;                                     // Data of BD1 Port0 (out)
        public static byte DataP11;                                     // Data of BD1 Port1 (out)
        public static byte DataP12;                                     // Data of BD1 Port2 (out)
        //public static byte DataP13;                                    // Data of BD1 Port3 (in)
        public static byte DataP14;                                     // Data of BD1 Port4 (out)
        public static byte DataP15;                                     // Data of BD1 Port5 (out)
        public static byte DataP16;                                     // Data of BD1 Port6 (out)
        //public static byte DataP17;                                   　// Data of BD1 Port7 (in)



        private int ret;
        public int ret_num;
        public byte ret_ank;

        private int swin_fg;

        //public byte[] tx_data = new byte[100];
        //public byte[] rv_data = new byte[100];
        //public int rv_len;

        // FILE
        private String[,] param = new string[2, 20];                    //パラメータ用配列
        private String[,] log_dt = new string[14, 200];                 //ログデータ用配列
        private String[,] comp_dt = new string[2, 50];                  //比較データ用配列
        private String[,] romdtB1 = new string[3, 100];                //EEPROMデータ(BC-R)用配列1
        //private String[] item_name = new string[20];                  　//型番名データ用配列
        //private String[] tester_name = new string[20];                　//検査者名データ用配列
        private int test_mode;
        private int eeprom_bcr1_num;                                    // eepromﾃﾞｰﾀ数(BC-R1)
        private string path_log;                                        // 検査成績書用ﾛｸﾞのﾊﾟｽ


        // 
        private string[] save_dt = new string[50];
        private string[] judge = new string[20];
        private int error_code;
        private double VDC;
        private int step_no;
        private int click_cnt;
        //private double pass_time;
        private int sw_status;
        private int test_status;
        private int try_cnt;
        private int rst_cnt;
        private string rst_ret_code;
        //private string RVD;
        
        // 比較ﾃﾞｰﾀ(ﾌｧｲﾙからの読込値)
        private double VDDh;
        private double VDDl;
        private double VCCh;
        private double VCCl;
        private double Vrefh;
        private double Vrefl;
        private string AppVerT;
        private string AppSumT;
        private string AppVerN;
        private string AppSumN;
        private string BcrVerM;
        private string BcrSumM;
        private string BcrVerS;
        private string BcrSumS;
        // <V4.00> にて以下を追加
        private double pp_tm_3h;
        private double pp_tm_3l;
        private double pi_tm_3h;
        private double pi_tm_3l;
        private double mt_tm_3h;
        private double mt_tm_3l;
        private double fr_tm_3h;
        private double fr_tm_3l;
        private double pp_tm_Bh;
        private double pp_tm_Bl;
        private double pi_tm_Bh;
        private double pi_tm_Bl;
        private double fr_tm_Bh;
        private double fr_tm_Bl;


        private string serial_no;

        private string ver_dt;
        private string crc_dt;
        private string type_dt;
        private string di_dt;
        private string ram_dt;

        private string ver_dt_bcr_M;
        private string crc_dt_bcr_M;
        private string ver_dt_bcr_S;
        private string crc_dt_bcr_S;
        private string eeprom_dt_bcr_M;
        private string eeprom_dt_bcr_S;

        private string check_sum;

        private double ad_dt;

        private double tm1;
        private double tm2;
        private double start_tm;
        private double end_tm;

        private string pp_tm;                                           // ﾌﾟﾚﾊﾟｰｼﾞ時間
        private string pit_tm;                                          // ﾌﾟﾚｲｸﾞﾆｯｼｮﾝ遅延時間
        //private string it_tm;                                           // ｲｸﾞﾆｯｼｮﾝﾄﾗｲｱﾙ時間
        private string pi_tm;                                           // ﾌﾟﾚｲｸﾞﾆｯｼｮﾝ時間
        private string tc_tm;                                           // 点火確認時間
        private string mt_tm;                                           // ﾒｲﾝﾄﾗｲｱﾙ時間
        private string ma_tm;                                           // ﾒｲﾝ安定時間
        private string fr_tm;                                           // ﾌﾚｰﾑﾚｽﾎﾟﾝｽ時間

        // ****************************************************************************************
        // **** Form1 *****************************************************************************
        // ****************************************************************************************
        public Form1()
        {
            InitializeComponent();
        }


        // ****************************************************************************************
        //      フォームロード処理
        // ****************************************************************************************
        private void Form1_Load(object sender, EventArgs e)
        {
            int i, j;
            String filedata;

            // ｼｽﾃﾑの確認
            if (File.Exists("C:\\WINDOWS\\ossno.log") == false)
            {
                MessageBox.Show("システムの確認ができません！", "警告",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            else
            {
                StreamReader srInf0 = new StreamReader("C:\\WINDOWS\\ossno.log",
                                  Encoding.GetEncoding("shift_jis"));
                filedata = srInf0.ReadLine();
                ManagementClass mc = new ManagementClass("Win32_OperatingSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo["SerialNumber"].ToString() != filedata)
                    {
                        MessageBox.Show("正規のシステムではありません！", "警告",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }
                }
                srInf0.Close();
            }

            // ﾊﾟﾗﾒｰﾀの読み込み
            StreamReader srInf1 = new StreamReader("C:\\RCC300\\FC\\parameter.csv",
                                                    Encoding.GetEncoding("shift_jis"));
            j = 0;
            while ((filedata = srInf1.ReadLine()) != null)
            {
                string[] csvdata = filedata.Split(',');
                for (i = 0; i < 2; i++)
                {
                    param[i, j] = csvdata[i];
                }
                j++;
            }
            srInf1.Close();
            path_log = param[0, 0];                                    // 検査成績書用ﾛｸﾞのﾊﾟｽ
            SNo1 = int.Parse(param[0, 1]);                              // BD1のｼﾘｱﾙNo
            test_mode = int.Parse(param[0, 2]);                         // ﾃｽﾄﾓｰﾄﾞ(0=通常、1=ﾃﾞﾊﾞｯｸﾞ)

            // DIOﾎﾟｰﾄの初期化
            InitEXP_64S(SNo1);
            SelectDMMch(15);                                            // DMM[open]選択

            // GP-OBﾎﾟｰﾄの初期化
            InitDMM();                                                  // DMM初期化

            // 比較ﾃﾞｰﾀの読み込み
            StreamReader srInf2 = new StreamReader("C:\\RCC300\\FC\\\\comp_dt.csv",
                                                    Encoding.GetEncoding("shift_jis"));
            j = 0;
            while ((filedata = srInf2.ReadLine()) != null)
            {
                string[] csvdata = filedata.Split(',');
                for (i = 0; i < 2; i++)
                {
                    comp_dt[i, j] = csvdata[i];
                }
                switch (comp_dt[0,j])
                {
                    case "AppVerT":
                        AppVerT = comp_dt[1, j];
                        break;
                    case "AppSumT":
                        AppSumT = comp_dt[1, j];
                        break;
                    case "AppVerN":
                        AppVerN = comp_dt[1, j];
                        break;
                    case "AppSumN":
                        AppSumN = comp_dt[1, j];
                        break;
                    case "BcrVerM":
                        BcrVerM = comp_dt[1, j];
                        break;
                    case "BcrSumM":
                        BcrSumM = comp_dt[1, j];
                        break;
                    case "BcrVerS":
                        BcrVerS = comp_dt[1, j];
                        break;
                    case "BcrSumS":
                        BcrSumS = comp_dt[1, j];
                        break;
                    case "VDDh":
                        VDDh = Double.Parse(comp_dt[1, j]);
                        break;
                    case "VDDl":
                        VDDl = Double.Parse(comp_dt[1, j]);
                        break;
                    case "VCCh":
                        VCCh = Double.Parse(comp_dt[1, j]);
                        break;
                    case "VCCl":
                        VCCl = Double.Parse(comp_dt[1, j]);
                        break;
                    case "Vrefh":
                        Vrefh = Double.Parse(comp_dt[1, j]);
                        break;
                    case "Vrefl":
                        Vrefl = Double.Parse(comp_dt[1, j]);
                        break;
                    // <V4.00> にて以下を追加
                    case "pp_tm_3h":
                        pp_tm_3h = Double.Parse(comp_dt[1, j]);
                        break;
                    case "pp_tm_3l":
                        pp_tm_3l = Double.Parse(comp_dt[1, j]);
                        break;
                    case "pi_tm_3h":
                        pi_tm_3h = Double.Parse(comp_dt[1, j]);
                        break;
                    case "pi_tm_3l":
                        pi_tm_3l = Double.Parse(comp_dt[1, j]);
                        break;
                    case "mt_tm_3h":
                        mt_tm_3h = Double.Parse(comp_dt[1, j]);
                        break;
                    case "mt_tm_3l":
                        mt_tm_3l = Double.Parse(comp_dt[1, j]);
                        break;
                    case "fr_tm_3h":
                        fr_tm_3h = Double.Parse(comp_dt[1, j]);
                        break;
                    case "fr_tm_3l":
                        fr_tm_3l = Double.Parse(comp_dt[1, j]);
                        break;
                    case "pp_tm_Bh":
                        pp_tm_Bh = Double.Parse(comp_dt[1, j]);
                        break;
                    case "pp_tm_Bl":
                        pp_tm_Bl = Double.Parse(comp_dt[1, j]);
                        break;
                    case "pi_tm_Bh":
                        pi_tm_Bh = Double.Parse(comp_dt[1, j]);
                        break;
                    case "pi_tm_Bl":
                        pi_tm_Bl = Double.Parse(comp_dt[1, j]);
                        break;
                    case "fr_tm_Bh":
                        fr_tm_Bh = Double.Parse(comp_dt[1, j]);
                        break;
                    case "fr_tm_Bl":
                        fr_tm_Bl = Double.Parse(comp_dt[1, j]);
                        break;
                }
                j++;
            }
            srInf2.Close();
            txtAppCpuVer.Text = AppVerN;
            txtAppCpuSum.Text = AppSumN;
            txtBCRmVer.Text = BcrVerM;
            txtBCRmSum.Text = BcrSumM;
            txtBCRsVer.Text = BcrVerS;
            txtBCRsSum.Text = BcrSumS;

            // EEPROMﾃﾞｰﾀ(BC-Rｺｱ)の読み込み
            StreamReader srInf3 = new StreamReader("C:\\RCC300\\FC\\\\EepromDataBcr1.csv",
                                                    Encoding.GetEncoding("shift_jis"));
            j = 0;
            while ((filedata = srInf3.ReadLine()) != null)
            {
                string[] csvdata = filedata.Split(',');
                for (i = 0; i < 3; i++)
                {
                    romdtB1[i, j] = csvdata[i];
                }
                j++;
            }
            srInf3.Close();
            eeprom_bcr1_num = j;

            // 検査者名の読み込み
            StreamReader srInf4 = new StreamReader("C:\\RCC300\\FC\\\\testername.dat",
                                                    Encoding.GetEncoding("shift_jis"));
            j = 0;
            while ((filedata = srInf4.ReadLine()) != null)
            {
                cmbTesterName.Items.Add(filedata);
                j++;
            }
            srInf4.Close();
            cmbTesterName.SelectedIndex = 0;

            // ｼﾘｱﾙNoの読み込み
            StreamReader srInf5 = new StreamReader("C:\\RCC300\\FC\\\\SerialNo.dat",
                                                    Encoding.GetEncoding("shift_jis"));
            filedata = srInf5.ReadLine();
            serial_no = filedata;
            srInf5.Close();
            txtSerialNoData.Text = serial_no;

            pgbDoing.Value = 0;

            btnAbort.Visible = false;
            btnCheck.Visible = true;
            pnlGo.Visible = false;

            // ﾛｸﾞ画面のｻｲｽﾞ変更
            grpManual.Visible = false;
            grpLog.Height = 720;
            txtLog.Height = 690;
            lblInfo1.Text = "工番,型番(履歴表のﾊﾞｰｺｰﾄﾞから),ｼﾘｱﾙNoと検査者を決定してから、";
            lblInfo2.Text = "｢確認｣ﾎﾞﾀﾝ　をｸﾘｯｸして下さい！ ";
            sw_status = 0;
            test_status = 0;
            swin_fg = 0;
            pnlGo.Enabled = true;

            picJudge.Visible = false;
            txtErrorCode.Visible = false;
            txtErrorMessege.Visible = false;

            rdbTerget2.Checked = true;                                  // 製品を選択　<V1.10>にて追加

            rdbDMM0.Checked = true;                                     // DMM Open
            rdbSensFL0.Checked = true;                                  // ｾﾝｻｰ(FL) Open
            rdbSensAFD0.Checked = true;                                 // ｾﾝｻｰAFD) Open
            rdbDamperDS10.Checked = true;                               // ﾀﾞﾝﾊﾟｰ1 Open
            rdbDamperDS20.Checked = true;                               // ﾀﾞﾝﾊﾟｰ2 Open
            rdbType1.Checked = true;                                    // 燃料種 ｶﾞｽ
            rdbIGtm1.Checked = true;                                    // 着火ﾄﾗｲｱﾙ時間　5s
            rdbMethod1.Checked = true;                                  // 点火方式 ﾊﾟｲﾛｯﾄ
            rdbAPPwr1.Checked = true;                                   // ｱﾌﾟﾘ書込 試験版
            rdbBCRwr0.Checked = true;                                   // BC-R書込 Open

            chkTextLogSave.Checked = true;

            //grpInput.Enabled = false;

            //chkAppCpu.Checked = true;
            //chkBcrCore.Checked = true;

            timer2.Interval = 200;
            timer2.Enabled = true;
            click_cnt = 0;

        }


        // ****************************************************************************************
        //      「閉じる」
        // ****************************************************************************************
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer2.Enabled = false;
            timer1.Enabled = false;

            // Close EPX-64S
            AllOffEXP_64S_BD1();                                        // BD1 All OFF
            CloseEXP_64S();

            // Close Serial Port
            if (serialPort1.IsOpen == true)
            {
                serialPort1.Close();
            }
            if (serialPort2.IsOpen == true)
            {
                serialPort2.Close();
            }
            if (serialPort3.IsOpen == true)
            {
                serialPort3.Close();
            }
        }


        // ****************************************************************************************
        //      ﾒﾆｭｰから閉じる
        // ****************************************************************************************
        private void mnuClose_Click(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            timer1.Enabled = false;

            // Close EPX-64S
            AllOffEXP_64S_BD1();                                        // BD1 All OFF
            CloseEXP_64S();

            // Close Serial Port
            if (serialPort1.IsOpen == true)
            {
                serialPort1.Close();
            }
            if (serialPort2.IsOpen == true)
            {
                serialPort2.Close();
            }
            if (serialPort3.IsOpen == true)
            {
                serialPort3.Close();
            }

            this.Close();
        }


        // ****************************************************************************************
        //      手　動
        // ****************************************************************************************
        private void mnuManual_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer2.Enabled = false;

            grpLog.Height = 535;
            txtLog.Height = 500;
            //grpInput.Enabled = true;
            grpManual.Visible = true;
        }


        // ****************************************************************************************
        //      情　報
        // ****************************************************************************************
        private void mnuInfomation_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Version  : " + info_ver + "\r\n" + "Update   : " + info_date + "\r\n",
                             "Infomation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        // ****************************************************************************************
        //      タイマー１（ステップアップ）
        // ****************************************************************************************
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            step_no++;
            Test();
        }


        // ****************************************************************************************
        //      タイマー２（スイッチ入力）
        // ****************************************************************************************
        private void timer2_Tick(object sender, EventArgs e)
        {
            int i;

            //toolSSL1.Text = test_status.ToString();

            toolSSL5.Text = DateTime.Now.ToString(" yyyy/MM/dd ");
            toolSSL6.Text = DateTime.Now.ToString(" HH:mm:ss ");
            
            // ﾒｯｾｰｼﾞの点滅
            lblInfo1.Location = new Point((770 - lblInfo1.Size.Width) / 2, 15);
            lblInfo2.Location = new Point((770 - lblInfo2.Size.Width) / 2, 45);
            click_cnt++;
            if (click_cnt > 5)
            {
                click_cnt = 0;
                if (pnlMsg.BackColor == Color.Gray)
                {
                    if (test_status == 5)
                    {
                        pnlMsg.BackColor = Color.Salmon;
                        lblInfo1.BackColor = Color.Salmon;
                        lblInfo2.BackColor = Color.Salmon;
                    }
                    else
                    {
                        pnlMsg.BackColor = Color.SteelBlue;
                        lblInfo1.BackColor = Color.SteelBlue;
                        lblInfo2.BackColor = Color.SteelBlue;
                    }
                }
                else
                {
                    pnlMsg.BackColor = Color.Gray;
                    lblInfo1.BackColor = Color.Gray;
                    lblInfo2.BackColor = Color.Gray;
                }
            }

            if (swin_fg == 0)
            { 
                // ｽｲｯﾁ入力
                Value = InputEPX64Port13();
                if ((Value & 0x02) == 0x00)
                {
                    btnSW1.Top = 30;
                    sw_status = 1;
                }
                else
                {
                    btnSW1.Top = 5;
                    sw_status = 0;
                }

                // 試験開始へ
                if (test_status == 0 && sw_status == 1)
                {
                    test_status = 1;
                }

                if (test_status == 1 && sw_status == 0)
                {
                    picJudge.Image = null;
                    picJudge.Refresh();

                    txtErrorCode.Visible = false;
                    txtErrorMessege.Visible = false;

                    ClearDisplay();                                         //  表示ｸﾘｱ

                    btnAbort.Visible = true;

                    lblInfo1.Text = "◆◇◆◇◆◇◆ 　試  　験  　中 　◆◇◆◇◆◇◆";
                    lblInfo2.Text = "---中断する場合は、｢中断｣ﾎﾞﾀﾝをｸﾘｯｸしてください---";
                    System.Media.SystemSounds.Beep.Play();

                    for (i = 0; i < 50; i++)
                    {
                        save_dt[i] = "***";
                    }
                    step_no = 0;
                    error_code = 0;
                    test_status = 2;
                    StepUpTimer(10);
                }

                // 結果確認処理
                if (test_status == 3 && sw_status == 1)
                {
                    test_status = 4;
                }

                if (test_status == 4 && sw_status == 0)
                {
                    if (error_code == 0)
                    {
                        // ｼﾘｱﾙNoのｲﾝｸﾘﾒﾝﾄ
                        if (txtSerialNo.Text != "SA000")
                        {
                            var currentSerial = Int32.Parse(txtSerialNo.Text);
                            var nextSerial = (currentSerial + 1).ToString("D5");
                            txtSerialNo.Text = nextSerial;

                            StreamWriter srInf8 = new StreamWriter("C:\\RCC300\\FC\\\\SerialNo.dat",
                                                    false, Encoding.GetEncoding("shift_jis"));
                            srInf8.WriteLine(nextSerial);
                            srInf8.Close();
                        }
                    }

                    ClearDisplay();                                         // 画面ｸﾘｱ

                    DisplayDipSw("OFF", "OFF", "OFF");
                    lblInfo1.Text = "S800の1,3のみON / S2の1,2をON / JP1,JP2,JP22を全てOFF";
                    lblInfo2.Text = " にをｾｯﾄしてから、｢Go｣sw を押して下さい!";
                    test_status = 0;
                }

                //  試験開始確認処理
                if (test_status ==  5 && sw_status == 1)
                {
                    test_status = 6;
                }

                if (test_status == 6 && sw_status == 0)
                {
                    lblInfo1.Text = "◆◇◆◇◆◇◆ 　試  　験  　中 　◆◇◆◇◆◇◆";
                    lblInfo2.Text = "---中断する場合は、｢中断｣ﾎﾞﾀﾝをｸﾘｯｸしてください---";
                    test_status = 2;
                }

                // 中断許可処理
                if (test_status == 99 && sw_status == 1)
                {
                    test_status = 4;
                }
            }
        }


        // ****************************************************************************************
        //      ｻﾝﾌﾟﾙを選択　<V1.10>にて追加
        // ****************************************************************************************
        private void rdbTerget1_CheckedChanged(object sender, EventArgs e)
        {
            txtKoubanData.Text = "Sample";
            txtItemData.Text = "RCC300K200-1/C";
            txtSerialNoData.Text = "SA000";
        }


        // ****************************************************************************************
        //      製品を選択  <V1.10>にて追加
        // ****************************************************************************************
        private void rdbTerget2_CheckedChanged(object sender, EventArgs e)
        {
            txtKoubanData.Text = "";
            txtItemData.Text = "";
            txtSerialNoData.Text = serial_no;
            txtKoubanData.Focus();
        }

        
        // ****************************************************************************************
        //      再選択
        // ****************************************************************************************
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (test_status == 0)
            {
                ClearDisplay();                                         // 画面ｸﾘｱ
                picJudge.Enabled = true;
                pnlGo.Visible = false;
                pnlSetData.Visible = true;
                rdbTerget2.Checked = true;
                txtKoubanData.Text = "";
                txtItemData.Text = "";
                txtSerialNoData.Text = serial_no;
                txtKoubanData.Focus();

                btnAbort.Visible = false;
                btnCheck.Visible = true;
                lblInfo1.Text = "工番,型番(履歴表のﾊﾞｰｺｰﾄﾞから),ｼﾘｱﾙNoと検査者を決定してから、";
                lblInfo2.Text = "｢確認｣ﾎﾞﾀﾝ　をｸﾘｯｸして下さい！ ";
            }
        }

        
        // ****************************************************************************************
        //      データ決定
        // ****************************************************************************************
        private void btnSetData_Click(object sender, EventArgs e)
        {
            txtItem.Text = txtItemData.Text;
            txtKouban.Text = txtKoubanData.Text;
            txtSerialNo.Text = txtSerialNoData.Text;
            txtDateCode.Text = DateTime.Now.ToString("yy") + 
                                (int.Parse(DateTime.Now.ToString("MM")) * 4).ToString("D2") + "Ne";
                                                                        // <V1.11>にて　"D"→"D2"
            toolSSL3.Text = (cmbTesterName.Text + "　　　　").Substring(0, 4);
        }


        // ****************************************************************************************
        //      型番と検査者の決定確認
        // ****************************************************************************************
        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (txtItem.Text != "" && txtKouban.Text != "")
            {
                DisplayDipSw("OFF", "OFF", "OFF");
                lblInfo1.Text = "S800の1,3のみON / S2の1,2をON / JP1,JP2,JP22を全てOFF";
                lblInfo2.Text = " にをｾｯﾄしてから、｢Go｣sw を押して下さい!";
                pnlSetData.Visible = false;
                btnCheck.Visible = false;
                pnlGo.Visible = true;
            }
        }


        // ****************************************************************************************
        //      クリア
        // ****************************************************************************************
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearDisplay();     // 画面ｸﾘｱ
        }


        // ****************************************************************************************
        //      ステップアップ処理用タイマー
        // ****************************************************************************************
        private void StepUpTimer(short time_data)
        {
            timer1.Interval = time_data;
            timer1.Enabled = true;
        }


        // ****************************************************************************************
        //      次のステップへ
        // ****************************************************************************************
        private void NextStep()
        {
            timer1.Interval = 100;
            timer1.Enabled = true;
        }




        // ****************************************************************************************
        //      画面クリア
        // ****************************************************************************************
        private void ClearDisplay()
        {
            pgbDoing.Value = 0;
            picJudge.Image = null;
            txtLog.Text = "";
            txtTm1.Text = "";
            txtRv1.Text = "";
            txtTm2.Text = "";
            txtRv2.Text = "";
            txtDMM.Text = "";
            txtStopWatch.Text = "";
            //txtAppCpuVer.Text = "";
            //txtAppCpuSum.Text = "";
            //txtBCRmVer.Text = "";
            //txtBCRmSum.Text = "";
            //txtBCRsVer.Text = "";
            //txtBCRsSum.Text = "";
            txtOut1.BackColor = Color.Gray;
            txtOut2.BackColor = Color.Gray;
            txtOut3.BackColor = Color.Gray;
            txtOut4.BackColor = Color.Gray;
            txtOut5.BackColor = Color.Gray;
            txtOut6.BackColor = Color.Gray;
            txtOut7.BackColor = Color.Gray;
            txtOut8.BackColor = Color.Gray;
            txtOut9.BackColor = Color.Gray;
            txtOut10.BackColor = Color.Gray;
            txtOut11.BackColor = Color.Gray;
            txtOut12.BackColor = Color.Gray;
            txtOut13.BackColor = Color.Gray;
            txtOut14.BackColor = Color.Gray;
            txtOut15.BackColor = Color.Gray;
            txtOut16.BackColor = Color.Gray;
            txtLED.BackColor = Color.Black;
            txtLED.Text = "";
            txtErrorCode.Visible = false;
            txtErrorMessege.Visible = false;
            chkBcrWrite.Checked = false;
            DisplayDipSw("OFF", "OFF", "OFF");
            timer1.Enabled = false;
        }


        // ****************************************************************************************
        //      中断処理
        // ****************************************************************************************
        private void btnAbort_Click(object sender, EventArgs e)
        {
                AllOffEXP_64S_BD1();                                    // BD1 All OFF
                PrintLogData("\r\n");
                PrintLogData("    *** 中断しました！***\r\n");

                lblInfo1.Text = "結果を確認してから、｢Go｣sw を押して下さい！";
                lblInfo2.Text = "";
                test_status = 99;

                error_code = 99;
                step_no = 3099;
                Test();
        }

        
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //      手動操作関連
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        //// ****************************************************************************************
        ////      手動操作終了処理
        //// ****************************************************************************************
        //private void btnExit_Click(object sender, EventArgs e)
        //{
        //    grpLog.Height = 810;
        //    txtLog.Height = 780;
        //}




        // ****************************************************************************************
        //      btnStampのｸﾘｯｸ処理
        // ****************************************************************************************
        private void btnStamp_Click(object sender, EventArgs e)
        {
            if (btnStamp.BackColor == Color.Gray)
            {
                ControlStamp(1);
                btnStamp.BackColor = Color.Green;
            }
            else
            {
                ControlStamp(0);
                btnStamp.BackColor = Color.Gray;
            }
        }


        // ****************************************************************************************
        //      btnDCPowerのｸﾘｯｸ処理
        // ****************************************************************************************
        private void btnDCPower_Click(object sender, EventArgs e)
        {
            if (btnDCPower.BackColor == Color.Gray)
            {
                ControlDCPower(1);
                btnDCPower.BackColor = Color.Green;
            }
            else
            {
                ControlDCPower(0);
                btnDCPower.BackColor = Color.Gray;
            }
        }

        // ****************************************************************************************
        //      btnACPowerのｸﾘｯｸ処理
        // ****************************************************************************************
        private void btnACPower_Click(object sender, EventArgs e)
        {
            if (btnACPower.BackColor == Color.Gray)
            {
                ControlACPower(1);
                btnACPower.BackColor = Color.Green;
            }
            else
            {
                ControlACPower(0);
                btnACPower.BackColor = Color.Gray;
            }
        }

        // ****************************************************************************************
        //      btnRamMonitor
        // ****************************************************************************************
        private void btnMonRam_Click(object sender, EventArgs e)
        {
            string rvd;

            ret = MonitorRAM_APP(txtMonCmd.Text, out rvd);
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      btnDMMのｸﾘｯｸ処理 (電圧測定)
        // ****************************************************************************************
        private void btnDMM_Click(object sender, EventArgs e)
        {
            int ret;

            ret = GetDCVolt(ref VDC);
            txtDMM.Text = VDC.ToString("#00.00");
            lblDMM.Text = "[DCV]";

        }

        // ****************************************************************************************
        //      rdbDMM0のｸﾘｯｸ処理  <Open>
        // ****************************************************************************************
        private void rdbDMM0_CheckedChanged(object sender, EventArgs e)
        {
            SelectDMMch(0);
        }

        // ****************************************************************************************
        //      rdbDMM1のｸﾘｯｸ処理 <VDD> [VDC]
        // ****************************************************************************************
        private void rdbDMM1_CheckedChanged(object sender, EventArgs e)
        {
            SelectDMMch(1);
        }

        // ****************************************************************************************
        //      rdbDMM2のｸﾘｯｸ処理 <VCC> [VDC]
        // ****************************************************************************************
        private void rdbDMM2_CheckedChanged(object sender, EventArgs e)
        {
            SelectDMMch(2);
        }

        // ****************************************************************************************
        //      rdbDMM3のｸﾘｯｸ処理 <Vref> [VDC]
        // ****************************************************************************************
        private void rdbDMM3_CheckedChanged(object sender, EventArgs e)
        {
            SelectDMMch(3);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        // ****************************************************************************************
        //      btnAPPwrｸﾘｯｸ処理
        // ****************************************************************************************
        private void btnAPPwr_Click(object sender, EventArgs e)
        {
            //timer2.Enabled = false;
            toolSSL1.Text = "";
            if (rdbAPPwr1.Checked == true)
            {
                ret = WriteAppCPU(@"C:\Program Files\Renesas\FDT4.09\FDT.exe",
                                    @"/DISCRETESTARTUP ""SimpleInterfaceMode /r""",
                                        @"C:\RCC300\FW\K\AppCPU\k_fire_test_5.mot",
                                        ref check_sum);
            }
            if (rdbAPPwr2.Checked == true)
            {
                ret = WriteAppCPU(@"C:\Program Files\Renesas\FDT4.09\FDT.exe",
                                    @"/DISCRETESTARTUP ""SimpleInterfaceMode /r""",
                                        @"C:\RCC300\FW\K\AppCPU\k_1.61.0.mot",
                                        ref check_sum);
            }
            if (ret == 0)
            {
                toolSSL1.Text = "*** 書き込み完了！ <ﾁｪｯｸｻﾑ: " + check_sum + "> ***";
            }
            else
            {
                toolSSL1.Text = "*** 書き込みエラー！***";
            }
            //timer2.Enabled = true;
        }

        // ****************************************************************************************
        //      btnBCRwrのｸﾘｯｸ処理
        // ****************************************************************************************
        private void btnBCRwr_Click(object sender, EventArgs e)
        {
            //Timer2.Enabled = false;
            if (rdbBCRwr1.Checked == true)
            {
                ret = WriteBCRcore(@"C:\RCC300\FW\K\BC-Rcore\BC-Rcore.rws",
                                            @"C:\RCC300\FW\K\BC-Rcore\4.1.1\BCR_Core_crc.hex");
            }
            if (rdbBCRwr2.Checked == true)
            {
                ret = WriteBCRcore(@"C:\RCC300\FW\K\BC-Rcore\BC-Rcore.rws",
                                            @"C:\RCC300\FW\K\BC-Rcore\4.1.1\BCR_SUB_Core_crc.hex");
            }
            if (ret == 0)
            {
                toolSSL1.Text = "*** 書き込み完了！***";
            }
            else
            {
                toolSSL1.Text = "*** 書き込みエラー！***";
            }
            //timer2.Enabled = true;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      rdbSensFL0のｸﾘｯｸ処理 <Open>
        // ****************************************************************************************
        private void rdbSensFL0_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(1, 0);
        }

        // ****************************************************************************************
        //      rdbSensFL1のｸﾘｯｸ処理 <抵抗1>
        // ****************************************************************************************
        private void rdbSensFL1_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(1, 1);
        }

        // ****************************************************************************************
        //      rdbSensFL2のｸﾘｯｸ処理 <抵抗2>
        // ****************************************************************************************
        private void rdbSensFL2_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(1, 2);
        }

        // ****************************************************************************************
        //      rdbSensFL3のｸﾘｯｸ処理 <抵抗3>
        // ****************************************************************************************
        private void rdbSensFL3_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(1, 3);
        }

        // ****************************************************************************************
        //      rdbSensFL4のｸﾘｯｸ処理 <抵抗4>
        // ****************************************************************************************
        private void rdbSensFL4_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(1, 4);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      rdbSensAFD0のｸﾘｯｸ処理 <Open>
        // ****************************************************************************************
        private void rdbSensAFD0_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(2, 0);
        }

        // ****************************************************************************************
        //      rdbSensAFD1のｸﾘｯｸ処理 <抵抗1>
        // ****************************************************************************************
        private void rdbSensAFD1_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(2, 1);
        }

        // ****************************************************************************************
        //      rdbSensAFD2のｸﾘｯｸ処理 <抵抗2>
        // ****************************************************************************************
        private void rdbSensAFD2_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(2, 2);
        }

        // ****************************************************************************************
        //      rdbSensAFD3のｸﾘｯｸ処理 <抵抗3>
        // ****************************************************************************************
        private void rdbSensAFD3_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(2, 3);
        }

        // ****************************************************************************************
        //      rdbSensAFD4のｸﾘｯｸ処理 <抵抗4>
        // ****************************************************************************************
        private void rdbSensAFD4_CheckedChanged(object sender, EventArgs e)
        {
            SetSensor(2, 4);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      rdbDamperDS10のｸﾘｯｸ処理 <DS1-Open>
        // ****************************************************************************************
        private void rdbDamperDS10_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(1, 0);
        }

        // ****************************************************************************************
        //      rdbDamperDS11のｸﾘｯｸ処理 <DS1-HH>
        // ****************************************************************************************
        private void rdbDamperDS11_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(1, 1);
        }

        // ****************************************************************************************
        //      rdbDamperDS12のｸﾘｯｸ処理 <DS1-H>
        // ****************************************************************************************
        private void rdbDamperDS12_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(1, 2);
        }

        // ****************************************************************************************
        //      rdbDamperDS13のｸﾘｯｸ処理 <DS1-M>
        // ****************************************************************************************
        private void rdbDamperDS13_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(1, 3);
        }

        // ****************************************************************************************
        //      rdbDamperDS14のｸﾘｯｸ処理 <DS1-L>
        // ****************************************************************************************
        private void rdbDamperDS14_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(1, 4);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      rdbDamperDS20のｸﾘｯｸ処理 <DS2-Open>
        // ****************************************************************************************
        private void rdbDamperDS20_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(2, 0);
        }

        // ****************************************************************************************
        //      rdbDamperDS21のｸﾘｯｸ処理 <DS2-HH>
        // ****************************************************************************************
        private void rdbDamperDS21_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(2, 1);
        }

        // ****************************************************************************************
        //      rdbDamperDS22のｸﾘｯｸ処理 <DS2-H>
        // ****************************************************************************************
        private void rdbDamperDS22_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(2, 2);
        }

        // ****************************************************************************************
        //      rdbDamperDS23のｸﾘｯｸ処理 <DS2-M>
        // ****************************************************************************************
        private void rdbDamperDS23_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(2, 3);
        }

        // ****************************************************************************************
        //      rdbDamperDS24のｸﾘｯｸ処理 <DS2-L>
        // ****************************************************************************************
        private void rdbDamperDS24_CheckedChanged(object sender, EventArgs e)
        {
            SetDamper(2, 4);
        }




        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      btnAirSWのｸﾘｯｸ処理 <AirSW>
        // ****************************************************************************************
        private void btnAirSW_Click(object sender, EventArgs e)
        {
            if (test_status != 2)
            {
                if (btnAirSW.BackColor == Color.Gray)
                {
                    ControlAirSW(1);
                }
                else
                {
                    ControlAirSW(0);
                }
            }
        }

        // ****************************************************************************************
        //      btnGusSWのｸﾘｯｸ処理 <GusSW>
        // ****************************************************************************************
        private void btnGusSW_Click(object sender, EventArgs e)
        {
            if (test_status != 2)
            {
                if (btnGusSW.BackColor == Color.Gray)
                {
                    ControlGusSW(1);
                }
                else
                {
                    ControlGusSW(0);
                }
            }
        }

        // ****************************************************************************************
        //      btnBlwSWのｸﾘｯｸ処理 <BlwSW>
        // ****************************************************************************************
        private void btnBlwSW_Click(object sender, EventArgs e)
        {
            if (test_status != 2)
            {
                if (btnBlwSW.BackColor == Color.Gray)
                {
                    ControlBlwSW(1);
                }
                else
                {
                    ControlBlwSW(0);
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      rdbType1のｸﾘｯｸ処理 <ﾀｲﾌﾟ:油>
        // ****************************************************************************************
        private void rdbType1_CheckedChanged(object sender, EventArgs e)
        {
            SetHarnessType(0);
        }

        // ****************************************************************************************
        //      rdbType2のｸﾘｯｸ処理 <ﾀｲﾌﾟ:ｶﾞｽ>
        // ****************************************************************************************
        private void rdbType2_CheckedChanged(object sender, EventArgs e)
        {
            SetHarnessType(1);
        }

        // ****************************************************************************************
        //      rdbIGtm1のｸﾘｯｸ処理 <ｲｸﾞﾅｲﾀ時間:5s>
        // ****************************************************************************************
        private void rdbIGtm1_CheckedChanged(object sender, EventArgs e)
        {
            SetHarnessTime(0);
        }

        // ****************************************************************************************
        //      rdbIGtm2のｸﾘｯｸ処理 <ｲｸﾞﾅｲﾀ時間:10s>
        // ****************************************************************************************
        private void rdbIGtm2_CheckedChanged(object sender, EventArgs e)
        {
            SetHarnessTime(1);
        }

        // ****************************************************************************************
        //      rdbMethod1のｸﾘｯｸ処理 <点火方式:ﾊﾟｲﾛｯﾄ>
        // ****************************************************************************************
        private void rdbMethod1_CheckedChanged(object sender, EventArgs e)
        {
            SetHarnessMethod(0);
        }

        // ****************************************************************************************
        //      rdbMethod2のｸﾘｯｸ処理 <点火方式:ﾀﾞｲﾚｸﾄ>
        // ****************************************************************************************
        private void rdbMethod2_CheckedChanged(object sender, EventArgs e)
        {
            SetHarnessMethod(1);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      rdbBCRwr0のｸﾘｯｸ処理
        // ****************************************************************************************
        private void rdbBCRwr0_CheckedChanged(object sender, EventArgs e)
        {
            SelectBCRwriter(0);
        }

        // ****************************************************************************************
        //      rdbBCRwr0のｸﾘｯｸ処理
        // ****************************************************************************************
        private void rdbBCRwr1_CheckedChanged(object sender, EventArgs e)
        {
            SelectBCRwriter(1);
        }

        // ****************************************************************************************
        //      rdbBCRwr0のｸﾘｯｸ処理
        // ****************************************************************************************
        private void rdbBCRwr2_CheckedChanged(object sender, EventArgs e)
        {
            SelectBCRwriter(2);
        }

        // ****************************************************************************************
        //      btnInitLoaderのｸﾘｯｸ処理
        // ****************************************************************************************
        private void btnInitLoader_Click(object sender, EventArgs e)
        {
            InitialSerialPortLoader();
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        // ****************************************************************************************
        //      指定した時間だけ待つ（mSec）
        // ****************************************************************************************
        public void WaitMsec(long tm_data)
        {
            long ttm;
            long ntm;

            ttm = (Environment.TickCount & Int32.MaxValue) + tm_data;
            do
            {
                ntm = Environment.TickCount & Int32.MaxValue;
            } while (ntm < ttm);
        }


        // ****************************************************************************************
        //      経過時間の表示（デバック用）
        // ****************************************************************************************
        public void PrintTime()
        {
            if (test_mode != 0)
            {
                sw.Stop();
                PrintLogData("  ............................... <time "
                                            + sw.Elapsed.ToString().Substring(3, 8) + ">\r\n");
                sw.Start();
            }
        }


        // ****************************************************************************************
        //      ログデータの表示
        // ****************************************************************************************
        public void PrintLogData(string dt)
        {
            txtLog.Text += dt;
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.Focus();
            txtLog.ScrollToCaret();
        }


        // ****************************************************************************************
        //      ディップスイッチの表示
        // ****************************************************************************************
        public void DisplayDipSw(string jp1, string jp2, string jp22)
        {
            if (jp1 == "ON")
            {
                txtJP1On.BackColor = Color.White;
                txtJP1Off.BackColor = Color.Black;
            }
            else
            {
                txtJP1On.BackColor = Color.Black;
                txtJP1Off.BackColor = Color.White;
            }
            if (jp2 == "ON")
            {
                txtJP2On.BackColor = Color.White;
                txtJP2Off.BackColor = Color.Black;
            }
            else
            {
                txtJP2On.BackColor = Color.Black;
                txtJP2Off.BackColor = Color.White;
            }
            if (jp22 == "ON")
            {
                txtJP22On.BackColor = Color.White;
                txtJP22Off.BackColor = Color.Black;
            }
            else
            {
                txtJP22On.BackColor = Color.Black;
                txtJP22Off.BackColor = Color.White;
            }
        }



        // ****************************************************************************************
        //      試　験
        // ****************************************************************************************
        private void Test()
        {
            switch (step_no)
            {
                case 1:
                    if (test_mode != 0)
                    {
                        sw.Reset();
                        sw.Start();
                    }

                    ret = GetCnData();
                    if ((ret & 0x10) != 0)
                    {
                        PrintLogData("  ﾌﾞﾛｯｸ1(CN3,CN6,CN7)のｺﾈｸﾀ実装異常!\r\n");
                        error_code = 1;                                 // ｢ｺﾈｸﾀ実装異常｣(1)
                    }
                    if ((ret & 0x20) != 0)
                    {
                        PrintLogData("  ﾌﾞﾛｯｸ2(CN9,CN10)のｺﾈｸﾀ実装異常!\r\n");
                        error_code = 1;                                 // ｢ｺﾈｸﾀ実装異常｣(1)
                    }
                    if ((ret & 0x40) != 0)
                    {
                        PrintLogData("  ﾌﾞﾛｯｸ3(CN1,CN2,CN4,CN5)のｺﾈｸﾀ実装異常!\r\n");
                        error_code = 1;                                 // ｢ｺﾈｸﾀ実装異常｣(1)
                    }
                    if ((ret & 0x80) != 0)
                    {
                        PrintLogData("  ﾌﾞﾛｯｸ4(CN8,CN28,CN29,CN900)のｺﾈｸﾀ実装異常!\r\n");
                        error_code = 1;                                 // ｢ｺﾈｸﾀ実装異常｣(1)
                    }
                    step_no = 99;
                    NextStep();
                    break;

                // ++++ 1.電源ＯＮテスト ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 100:
                    pgbDoing.Value = (int)(step_no / TEST);
                    swin_fg = 1;                                        // SW入力禁止
                    pnlGo.Enabled = false;
                    if (error_code != 0)
                    {
                        step_no = 199;
                        NextStep();
                        break;
                    }

                    PrintLogData(" -------------------------------------------------\r\n");
                    PrintLogData("    型番: " + txtItem.Text + "    ｼﾘｱﾙNo: " + txtSerialNo.Text +
                                "\r\n");

                    PrintLogData("  1.電源ＯＮテスト\r\n");
                    PlaySound("C:\\RCC300\\FC\\PIsound.wav");           // PI Buzzer
                    AllOffEXP_64S_BD1();                                // BD1 All OFF
                    InitialSerialPortRS485();                           // RS-485ﾎﾟｰﾄ 初期化,ｵｰﾌﾟﾝ 
                    NextStep();
                    break;

                case 101:
                    PrintLogData("    電源 ON\r\n");
                    ControlDCPower(1);                                  // DC POwer ON
                    ControlACPower(1);                                  // AC POwer ON
                    StepUpTimer(500);
                    break;

                // ---- VDD ----------------------------------------------------------------------
                case 102:
                    PrintLogData("    VDD  : ");
                    SelectDMMch(1);                                     // [VDD]選択
                    StepUpTimer(500);
                    break;

                case 103:
                    if (GetDCVolt_Stable(20, 0.5, ref VDC) != 0)        // <V6.20>にて変更
                    {
                        PrintLogData("...DMMエラー！\r\n");
                        error_code = 51;
                        step_no = 198;
                        NextStep();
                        break;

                    }
                    txtDMM.Text = VDC.ToString("#0.000");
                    save_dt[9] = txtDMM.Text;
                    lblDMM.Text = "[DCV]";
                    PrintLogData(VDC.ToString("#0.000") + "[V] ★9 ");
                    if (VDDl > VDC || VDC > VDDh)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + VDDl.ToString("#0.000～")
                                                       + VDDh.ToString("#0.000[V]") + " >\r\n");
                        error_code = 101;                               // ｢VDD電圧異常｣(101)
                        step_no = 198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    SelectDMMch(0);                                     // [open]選択
                    StepUpTimer(500);
                    break;

                // ---- VCC -----------------------------------------------------------------------
                case 104:
                    PrintLogData("    VCC  : ");
                    SelectDMMch(2);                                     // [VCC]選択
                    StepUpTimer(500);
                    break;

                case 105:
                    if (GetDCVolt_Stable(20, 0.5, ref VDC) != 0)        // <V6.20>にて変更
                    {
                        PrintLogData("...DMMエラー！\r\n");
                        error_code = 51;
                        step_no = 198;
                        NextStep();
                        break;
                    }
                    txtDMM.Text = VDC.ToString("#0.000");
                    save_dt[10] = txtDMM.Text;
                    lblDMM.Text = "[DCV]";
                    PrintLogData(VDC.ToString("#0.000") + "[V] ★10 ");
                    if (VCCl > VDC || VDC > VCCh)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + VCCl.ToString("#0.000～")
                                                       + VCCh.ToString("#0.000[V]") + " >\r\n");
                        error_code = 102;                               // ｢VCC電圧異常｣(102)
                        step_no = 198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    SelectDMMch(0);                                     // [open]選択
                    StepUpTimer(500);
                    break;

                // ---- Vref ----------------------------------------------------------------------
                case 106:
                    PrintLogData("    Vref : ");
                    SelectDMMch(3);                                     // [Vref]選択
                    StepUpTimer(500);
                    break;

                case 107:
                    if (GetDCVolt_Stable(20, 0.01, ref VDC) != 0)       // <V6.20>にて変更
                    {
                        PrintLogData("...DMMエラー！\r\n");
                        error_code = 51;
                        step_no = 198;
                        NextStep();
                        break;
                    }
                    txtDMM.Text = VDC.ToString("#0.000");
                    save_dt[11] = txtDMM.Text;
                    lblDMM.Text = "[DCV]";
                    PrintLogData(VDC.ToString("#0.000") + "[V] ★11 ");
                    if (Vrefl > VDC || VDC > Vrefh)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + Vrefl.ToString("#0.000～")
                                                       + Vrefh.ToString("#0.000[V]") + " >\r\n");
                        error_code = 103;                               // ｢Vref電圧異常｣(103)
                        step_no = 198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    SelectDMMch(0);                                     // [open]選択
                    step_no = 198;
                    StepUpTimer(500);
                    break;

                case 199:

                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    //step_no = 2099; //@@@@@@@@@@@@@@@@@@@@@@
                    NextStep();
                    break;


                // ++++ 2.F/W書込(BC-Rｺｱ) +++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 200:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 299;
                        NextStep();
                        break;
                    }

                    PrintLogData("  2.F/W書込(BC-Rｺｱ)\r\n");
                    PrintLogData("    F/W書込");
                    if (chkBcrWrite.Checked == true)
                    {
                        PrintLogData(" -[pass]-\r\n");
                        step_no = 299;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(2000);
                    break;

                case 201:
                    PrintLogData("     Main ");
                    SelectBCRwriter(1);                                 // BC-R(main) 選択
                    StepUpTimer(500);
                    break;

                case 202:
                    ret = WriteBCRcore(@"C:\RCC300\FW\K\BC-Rcore\BC-Rcore.rws",
                                                @"C:\RCC300\FW\K\BC-Rcore\4.1.1\BCR_Core_crc.hex");
                    if (ret != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 201;                           // ｢BC-Rｺｱ<ﾒｲﾝ>書込異常｣(201)
                        step_no = 298;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 203:
                    PrintLogData("      Sub ");
                    SelectBCRwriter(2);                                 // BC-R(sub) 選択
                    StepUpTimer(500);
                    break;

                case 204:
                    ret = WriteBCRcore(@"C:\RCC300\FW\K\BC-Rcore\BC-Rcore.rws",
                                            @"C:\RCC300\FW\K\BC-Rcore\4.1.1\BCR_SUB_Core_crc.hex");
                    if (ret != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 202;                           // ｢BC-Rｺｱ<ｻﾌﾞ>書込異常｣(202)
                        step_no = 298;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    SelectBCRwriter(0);                                 // BC-R(open) 選択
                    step_no = 298;
                    NextStep();
                    break;

                case 299:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 3.F/W書込(ｱﾌﾟﾘCPU) ++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 300:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 399;
                        NextStep();
                        break;
                    }

                    PrintLogData("  3.F/W書込(ｱﾌﾟﾘCPU)\r\n");
                    PrintLogData("    F/W書込 ");
                    StepUpTimer(1000);
                    break;

                case 301:
                    ret = WriteAppCPU(@"C:\Program Files\Renesas\FDT4.09\FDT.exe",
                                    @"/DISCRETESTARTUP ""SimpleInterfaceMode /r""",
                                        @"C:\RCC300\FW\K\AppCPU\k_fire_test_5.mot",
                                        ref check_sum);
                    if (ret != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 301;                               // ｢ｱﾌﾟﾘCPU書込異常｣(301)
                        step_no = 398;
                        NextStep();
                        break;
                    }
                    if (check_sum != AppSumT)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 302;                               // ｢ｱﾌﾟﾘCPUﾁｪｯｸｻﾑ異常｣(302)
                        step_no = 398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 398;
                    StepUpTimer(1000);
                    break;

                case 399:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 4.試験開始 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 400:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 499;
                        NextStep();
                        break;
                    }

                    PrintLogData("  4.試験開始\r\n");
                    PrintLogData("    電源 OFF\r\n");
                    ControlDCPower(0);                                  // DC Power OFF
                    ControlACPower(0);                                  // AC Power OFF
                    swin_fg = 0;                                        // SW入力許可
                    pnlGo.Enabled = true;
                    StepUpTimer(2000);
                    break;

                case 401:
                    lblInfo1.Text = "S2の1,2をOFF にしてから、";
                    lblInfo2.Text = "｢Go｣sw を押して下さい！";
                    test_status = 5;
                    NextStep();
                    break;

                case 402:
                    if (test_status != 2)
                    {
                        PlaySound("C:\\RCC300\\FC\\WAITsound.wav");     // Wait Buzzer
                        step_no--;
                        StepUpTimer(2000);
                        break;
                    }
                    swin_fg = 1;                                        // SW入力禁止
                    StepUpTimer(500);
                    break;

                case 403:
                    PlaySound("C:\\RCC300\\FC\\PIsound.wav");           // PI Buzzer
                    PrintLogData("    電源 ON\r\n");
                    ControlDCPower(1);                                  // DC Power ON
                    ControlACPower(1);                                  // AC Power ON
                    pnlGo.Enabled = false;
                    StepUpTimer(5000);
                    break;

                case 404:
                    PrintLogData("    LED点灯確認 ");
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret == 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 401;                               // 「LEDが点灯している｣(401)
                        step_no = 498;
                        NextStep();
                        break;
                    }
                    try_cnt = 100;                                      // ﾀｲﾑｵｰﾊﾞｰ 100s
                    StepUpTimer(500);
                    break;

                case 405:
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 402;                           // 「LEDが点灯しない｣(402)
                            step_no = 498;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 498;
                    StepUpTimer(5000);
                    break;

                case 499:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 5.ｴﾗｰﾘｾｯﾄ +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 500:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 599;
                        NextStep();
                        break;
                    }

                    PrintLogData("  5.ｴﾗｰﾘｾｯﾄ\r\n");
                    NextStep();
                    break;

                case 501:
                    PrintLogData("    ﾘｾｯﾄ ");
                    try_cnt = 20;
                    NextStep();
                    break;

                case 502:
                    ret = ResetErrorCode_Normal("53", out rst_ret_code);    // ｴﾗｰｺｰﾄﾞﾘｾｯﾄ(通常)
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 501;                           // ｢ﾘｾｯﾄできない｣(501)
                            step_no = 598;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 598;
                    NextStep();
                    break;

                case 599:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 6.ﾃｽﾄﾓｰﾄﾞ設定 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 600:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 699;
                        NextStep();
                        break;
                    }

                    PrintLogData("  6.ﾃｽﾄﾓｰﾄﾞ設定\r\n");
                    InitialSerialPortLoader();                          // ﾛｰﾀﾞｰﾎﾟｰﾄ 初期化,ｵｰﾌﾟﾝ   
                    StepUpTimer(500);
                    break;

                case 601:
                    PrintLogData("    ﾃｽﾄﾓｰﾄﾞ(ｱﾌﾟﾘCPU)設定 ");
                    ret = SetTestMode_APP();                            // ﾃｽﾄﾓｰﾄﾞ設定(AppCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 698;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 602:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 698;
                        NextStep();
                    }
                    PrintLogData("    ﾃｽﾄﾓｰﾄﾞ(BC-Rｺｱ)設定 ");
                    ret = SetTestMode_BCR();                            // ﾃｽﾄﾓｰﾄﾞ設定(BCRｺｱ)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 698;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 603:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 698;
                    NextStep();
                    break;

                case 699:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 7.Version,ﾁｪｯｸｻﾑ,基板ﾀｲﾌﾟ確認 +++++++++++++++++++++++++++++++++++++++++++++

                case 700:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 799;
                        NextStep();
                        break;
                    }

                    PrintLogData("  7.Version,ﾁｪｯｸｻﾑ,基板ﾀｲﾌﾟ確認\r\n");
                    NextStep();
                    break;

                // ---- K43069 5.12 Version,ﾁｪｯｸｻﾑ,基板ﾀｲﾌﾟの確認(ｱﾌﾟﾘCPU) ----

                case 701:
                    PrintLogData("    <ｱﾌﾟﾘCPU>\r\n");
                    ret = GetVerCrc_APP();                              // Ver,CRC確認 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 798;
                        NextStep();
                        break;
                    }
                    save_dt[1] = ver_dt;
                    save_dt[2] = crc_dt;
                    PrintLogData("         Ver : " + ver_dt + " ★1\r\n");
                    PrintLogData("      ﾁｪｯｸｻﾑ : " + crc_dt + " ★2\r\n");
                    PrintLogData("    基板ﾀｲﾌﾟ : " + type_dt + " ");
                    if (ver_dt != AppVerT || crc_dt != AppSumT || type_dt != "4B")
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 701;                      // ｢ｱﾌﾟﾘCPU<Ver,ﾁｪｯｸｻﾑ>異常｣(701)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ---- K43069 5.13 Version,CRCの確認(BC-Rｺｱ) -----------------

                case 702:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 798;
                        NextStep();
                    }
                    PrintLogData("    <BC-Rｺｱ>\r\n");
                    ret = GetVerCrc_BCR();                              // Ver,CRC確認 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 798;
                        NextStep();
                        break;
                    }
                    save_dt[5] = ver_dt_bcr_M;
                    save_dt[6] = ver_dt_bcr_S;
                    save_dt[7] = crc_dt_bcr_M;
                    save_dt[8] = crc_dt_bcr_S;
                    PrintLogData("       Ver(main) : " + ver_dt_bcr_M + " ★5\r\n");
                    PrintLogData("       Ver(sub ) : " + ver_dt_bcr_S + " ★6\r\n");
                    PrintLogData("    ﾁｪｯｸｻﾑ(main) : " + crc_dt_bcr_M + " ★7\r\n");
                    PrintLogData("    ﾁｪｯｸｻﾑ(sub ) : " + crc_dt_bcr_S + " ★8");
                    if (ver_dt_bcr_M != BcrVerM || ver_dt_bcr_S != BcrVerS ||
                        crc_dt_bcr_M != BcrSumM || crc_dt_bcr_S != BcrSumS)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 702;                       // ｢BC-Rｺｱ<Ver,ﾁｪｯｸｻﾑ>異常｣(702)
                        step_no = 798;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 703:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 798;
                    NextStep();
                    break;

                case 799:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 8.ｼﾘｱﾙNo,ﾃﾞｰﾄｺｰﾄﾞ書込 +++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 800:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 899;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.24 D/C,S/N設定 ---------------------------

                    PrintLogData("  8.ｼﾘｱﾙNo,ﾃﾞｰﾄｺｰﾄﾞ書込\r\n");
                    NextStep();
                    break;

                case 801:
                    PrintLogData("    ｼﾘｱﾙNo書込<ｱﾌﾟﾘCPU>");
                    ret = WriteSno_APP();                               // Sno書込
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 898;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 802:
                    PrintLogData("    ﾃﾞｰﾄｺｰﾄﾞ書込<ｱﾌﾟﾘCPU>");
                    ret = WriteDcode_APP();                             // DC書込
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 898;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 803:
                    PrintLogData("    EEPROM書込<ｱﾌﾟﾘCPU>");
                    ret = WriteEEprom_APP();                            // EEPROM書込
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 898;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 898;
                    NextStep();
                    break;

                case 899:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 9.ﾃﾞｼﾞﾀﾙ入力確認 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 900:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 999;
                        NextStep();
                        break;
                    }

                    PrintLogData("  9.ﾃﾞｼﾞﾀﾙ入力確認\r\n");
                    // pppppppppppppppppppppppppppppppppppppppppppppppppppppppp
                    //PrintLogData(" ---[ ﾊﾟｽ ]---\r\n");
                    //step_no = 999;
                    //NextStep();
                    //break;
                    // pppppppppppppppppppppppppppppppppppppppppppppppppppppppp
                    NextStep();
                    break;

                // ---- K43069 5.15 ﾃﾞｼﾞﾀﾙ入力確認(ｱﾌﾟﾘCPU) ---------------------------------------

                // ---- K43069 5.15.1 外部入力信号 ----------------------------

                case 901:
                    PrintLogData("    <ｱﾌﾟﾘCPU>\r\n");
                    PrintLogData("    初期値    : ");
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C1C0EEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C1C0EEE >\r\n");
                        error_code = 901;                               // ｢初期値異常｣(901)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 902:
                    PrintLogData("    風圧ｽｲｯﾁ  : ");
                    ControlAirSW(1);                                    // ｴｱｰSW ON
                    StepUpTimer(500);
                    break;

                case 903:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C1C0EEC")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C1C0EEC >\r\n");
                        error_code = 902;                               // ｢風圧ｽｲｯﾁ入力異常｣(902)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 904:
                    ControlAirSW(0);                                    // ｴｱｰSW OFF
                    PrintLogData("    K10 ON");
                    ret = SetRamBit_K10_APP(1);                         // K10 ON
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    PrintLogData("    ｶﾞｽ圧ｽｲｯﾁ : ");
                    ControlGusSW(1);                                    // ｶﾞｽSW ON
                    StepUpTimer(500);
                    break;

                case 905:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C1C0CEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C1C0CEE >\r\n");
                        error_code = 903;                               // ｢ｶﾞｽ圧ｽｲｯﾁ入力異常｣(903)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 906:
                    ControlGusSW(0);                                    // ｶﾞｽSW OFF
                    PrintLogData("    ﾌﾞﾛｱｻｰﾏﾙ  : ");
                    ControlBlwSW(1);                                    // BLW ON
                    StepUpTimer(500);
                    break;

                case 907:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C0C0EEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C0C0EEE >\r\n");
                        error_code = 904;                               // ｢ﾌﾞﾛｱｻｰﾏﾙ入力異常｣(904)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 908:
                    PrintLogData("    K10 OFF");
                    ret = SetRamBit_K10_APP(0);                         // K10 OFF
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    ControlBlwSW(0);                                    // BLW OFF
                    PrintLogData("    DS1_HH    : ");
                    rdbDamperDS11.Checked = true;                       // DH1_HH ON
                    StepUpTimer(500);
                    break;

                case 909:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C1406E6")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C1406E6 >\r\n");
                        error_code = 905;                               // ｢DS1<HH>入力異常｣(905)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 910:
                    PrintLogData("    DS1_H     : ");
                    rdbDamperDS12.Checked = true;                       // DH1_H ON
                    StepUpTimer(500);
                    break;

                case 911:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C1406EE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C1406EE >\r\n");
                        error_code = 906;                               // ｢DS1<H>入力異常｣(906)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 912:
                    PrintLogData("    DS1_M     : ");
                    rdbDamperDS13.Checked = true;                       // DH1_M ON
                    StepUpTimer(500);
                    break;

                case 913:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C140EEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C140EEE >\r\n");
                        error_code = 907;                               // ｢DS1<M>入力異常｣(907)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 914:
                    PrintLogData("    DS1_L     : ");
                    rdbDamperDS14.Checked = true;                       // DH1_L ON
                    StepUpTimer(500);
                    break;

                case 915:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "141C0EEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:141C0EEE >\r\n");
                        error_code = 908;                               // ｢DS1<L>入力異常｣(908)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;


                // ------------------------------------------------------------

                case 916:
                    rdbDamperDS10.Checked = true;                       // DH1 Open
                    PrintLogData("    DS2_HH    : ");
                    rdbDamperDS21.Checked = true;                       // DH2_HH ON
                    StepUpTimer(500);
                    break;

                case 917:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C180AEA")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C180AEA >\r\n");
                        error_code = 909;                               // ｢DS2<HH>入力異常｣(909)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 918:
                    PrintLogData("    DS2_H     : ");
                    rdbDamperDS22.Checked = true;                       // DH2_H ON
                    StepUpTimer(500);
                    break;

                case 919:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C180AEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C180AEE >\r\n");
                        error_code = 910;                               // ｢DS2<H>入力異常｣(910)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 920:
                    PrintLogData("    DS2_M     : ");
                    rdbDamperDS23.Checked = true;                       // DH2_M ON
                    StepUpTimer(500);
                    break;

                case 921:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "1C180EEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:1C180EEE >\r\n");
                        error_code = 911;                               // ｢DS2<M>入力異常｣(911)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 922:
                    PrintLogData("    DS2_L     : ");
                    rdbDamperDS24.Checked = true;                       // DH2_L ON
                    StepUpTimer(500);
                    break;

                case 923:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "181C0EEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:181C0EEE >\r\n");
                        error_code = 912;                               // ｢DS2<L>入力異常｣(912)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 924:
                    rdbDamperDS20.Checked = true;                       // DH2 Open
                    PrintLogData("    燃焼開始信号 : ");
                    SetFromC2(1);                                       // 燃焼開始信号 ON
                    StepUpTimer(500);
                    break;

                case 925:
                    ret = GetDI_APP();                                  // DIﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(di_dt);
                    if (di_dt != "0C1C0EEE")
                    {
                        PrintLogData(" ...ＮＧ! < ○:0C1C0EEE >\r\n");
                        error_code = 913;                           // ｢燃焼開始信号入力異常｣(913)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 926:
                    SetFromC2(0);                                       // 燃焼開始信号 OFF
                    step_no = 930;
                    StepUpTimer(500);
                    break;
                
                // ---- 5.15.2 & 5.16.1 ﾘﾚｰF/B --------------------------------

                case 931:
                    PrintLogData("    <ﾘﾚｰF/B>\r\n");
                    PrintLogData("    K10 ON\r\n");
                    SetRamBit_K10_APP(1);                               // K10 ON
                    PrintLogData("    ｶﾞｽ圧sw ON\r\n");
                    ControlGusSW(1);                                    // ｶﾞｽSW ON
                    StepUpTimer(500);
                    break;

                case 932:
                    PrintLogData("    K13,K14,K15 OFF");
                    ret = SetRamBit_K13_APP(0);                         // K13 OFF
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K14_APP(0);                         // K14 OFF
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K15_APP(0);                         // K15 OFF
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 933:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1,K2,K3,K4,K5 全てOFF / K6 RESET");
                    ret = SetRamBit_K1_BCR(0, 0);                       // K1(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("      ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(0, 0);                       // K2(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("      ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K3_BCR(0, 0);                       // K3(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("      ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K4_BCR(0, 0);                       // K4(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("      ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K5_BCR(0, 0);                       // K5(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("      ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6R_BCR(0, 0);                      // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("      ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6S_BCR(0, 0);                      // K6S(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("      ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6R_BCR(1, 1);                         // K6R(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("      ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(3000);
                    break;

                case 934:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    初期値 ");
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xE000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 914;                               // ｢F/B初期値異常｣(914)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 935:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1 ON");
                    ret = SetRamBit_K1_BCR(1, 1);                       // K1(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 936:
                    PrintLogData("    K1 F/B(BC-Rｺｱ) ");
                    ret = GetRamAdrData_BCR("0BCA");                    // ｱﾄﾞﾚｽ(3018)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0xffff) != 0x0041 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0xffff) != 0x0041)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 915;                               // ｢K1 F/B異常<BC-Rｺｱ>｣(915)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");                    
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xE100)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 916;                               // ｢K1 F/B異常<ｱﾌﾟﾘCPU>｣(916)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 937:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1 OFF,K2 ON");
                    ret = SetRamBit_K1_BCR(0, 0);                       // K1(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(1, 1);                       // K2(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 938:
                    PrintLogData("    K2 F/B(BC-Rｺｱ) ");
                    ret = GetRamAdrData_BCR("0BCA");                    // ｱﾄﾞﾚｽ(3018)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0xffff) != 0x0042 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0xffff) != 0x0042)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 917;                               // ｢K2 F/B異常<BC-Rｺｱ>｣(917)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");                    
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K2 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xE200)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 918;                               // ｢K2 F/B異常<ｱﾌﾟﾘCPU>｣(918)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 939:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1 ON,K2 ON");
                    ret = SetRamBit_K1_BCR(1, 1);                       // K1(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(1, 1);                       // K2(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 940:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1,K2 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xE300)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 919;                          // ｢K1,K2 F/B異常<ｱﾌﾟﾘCPU>｣(919)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 941:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K3 ON");
                    ret = SetRamBit_K3_BCR(1, 1);                       // K3(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 942:
                    PrintLogData("    K1,K2,K3 F/B(BC-Rｺｱ) ");
                    ret = GetRamAdrData_BCR("0BCA");                    // ｱﾄﾞﾚｽ(3018)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0xffff) != 0x004B ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0xffff) != 0x004B)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 920;                       // ｢K1,K2,K3 F/B異常<BC-Rｺｱ>｣(920)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");                    
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1,K2,K3 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得????
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xE700)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 921;                       // ｢K1,K2,K3 F/B異常<ｱﾌﾟﾘCPU>｣(921)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 943:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K3 OFF,K4 ON");
                    ret = SetRamBit_K3_BCR(0, 0);                       // K3(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K4_BCR(1, 1);                       // K4(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 944:
                    PrintLogData("    K1,K2,K4 F/B(BC-Rｺｱ) ");
                    ret = GetRamAdrData_BCR("0BCA");                    // ｱﾄﾞﾚｽ(3018)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0xffff) != 0x0053 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0xffff) != 0x0053)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 922;                       // ｢K1,K2,K4 F/B異常<BC-Rｺｱ>｣(922)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");                    
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1,K2,K4 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得????
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xEB00)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 923;                       // ｢K1,K2,K4 F/B異常<ｱﾌﾟﾘCPU>｣(923)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 945:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K4 OFF,K5 ON");
                    ret = SetRamBit_K4_BCR(0, 0);                       // K4(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K5_BCR(1, 1);                       // K5(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 946:
                    PrintLogData("    K1,K2,K5 F/B(BC-Rｺｱ) ");
                    ret = GetRamAdrData_BCR("0BCA");                    // ｱﾄﾞﾚｽ(3018)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0xffff) != 0x0047 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0xffff) != 0x0047)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 924;                     // ｢K1,K2,K5 F/B異常<BC-Rｺｱ>｣(924)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");                    
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1,K2,K5 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得????
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xF300)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 925;                       // ｢K1,K2,K5 F/B異常<ｱﾌﾟﾘCPU>｣(925)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 947:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K5 OFF,K4 ON");
                    ret = SetRamBit_K5_BCR(0, 0);                       // K5(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K4_BCR(1, 1);                       // K4(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K13 ON");
                    ret = SetRamBit_K13_APP(1);                         // K13 ON
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 948:
                    PrintLogData("    K1,K2,K4,K13 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得????
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xAB00)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 926;               // ｢K1,K2,K4,K13 F/B異常<ｱﾌﾟﾘCPU>｣(926)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 949:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K4 OFF");
                    ret = SetRamBit_K4_BCR(0, 0);                       // K4(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K13 OFF,K14 ON");
                    ret = SetRamBit_K13_APP(0);                         // K13 OFF
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K14_APP(1);                         // K14 ON
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 950:
                    PrintLogData("    K1,K2,K14 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得????
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0x6300)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 927;                       // ｢K1,K2,K14 F/B異常<ｱﾌﾟﾘCPU>｣(927)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 951:
                    PrintLogData("    K14 OFF,K15 ON");
                    ret = SetRamBit_K14_APP(0);                         // K14 OFF
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K15_APP(1);                         // K15 ON
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 952:
                    PrintLogData("    K1,K2,K15 F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得????
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xC300)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 928;                       // ｢K1,K2,K15 F/B異常<ｱﾌﾟﾘCPU>｣(928)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 953:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K1,K2 OFF");
                    ret = SetRamBit_K1_BCR(0, 0);                       // K1(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(0, 0);                       // K2(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K15 OFF");
                    ret = SetRamBit_K15_APP(0);                         // K15 OFF
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 954:
                    PrintLogData("    F/B(all OFF)(ｱﾌﾟﾘCPU) ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得????
                    ret = GetRamData_APP("0703", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xE000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 929;                           // ｢全OFF F/B異常<ｱﾌﾟﾘCPU>｣(929)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 955:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K6S ON");
                    ret = SetRamBit_K6S_BCR(0, 0);                      // K6S(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6R_BCR(0, 0);                      // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6S_BCR(1, 1);                      // K6S(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(3000);
                    break;

                case 956:
                    PrintLogData("    K6 ON F/B(BC-Rｺｱ) ");
                    ret = GetRamAdrData_BCR("0BCA");                    // ｱﾄﾞﾚｽ(3018)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0xffff) != 0x0000 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0xffff) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 930;                           // ｢K6 ON F/B異常<BC-Rｺｱ>｣(930)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");                    
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K6 ON F/B(ｱﾌﾟﾘCPU) ");
                    ret = GetRamData_APP("0703", 2);                    // ｱﾄﾞﾚｽ(0703)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xE001)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 931;                           // ｢K6 ON F/B異常<ｱﾌﾟﾘCPU>｣(931)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------
                case 957:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K6 RESET");
                    ret = SetRamBit_K6R_BCR(0, 0);                      // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6S_BCR(0, 0);                      // K6S(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6R_BCR(1, 1);                      // K6R(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(3000);
                    break;

                case 958:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    PrintLogData("    K6 OFF F/B(ｱﾌﾟﾘCPU)  ");
                    ret = GetRamData_APP("0703", 2);                    // ｱﾄﾞﾚｽ(0703)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0xff01) != 0xE000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 932;                       // ｢K6 OFF F/B異常<ｱﾌﾟﾘCPU>｣(932)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;
                
                case 959:
                    step_no = 960;
                    StepUpTimer(500);
                    break;

                // ---- K43069 5.15.3 設定 ------------------------------------

                case 961:
                    PrintLogData("    <設 定>\r\n");
                    PrintLogData("    ﾊｰﾈｽ(燃料種) ");
                    rdbType2.Checked = true;                            // ﾊｰﾈｽ(燃料種) ON 
                    StepUpTimer(500);
                    break;

                case 962:
                    ret = GetRamData_APP("0208", 2);                    // ｱﾄﾞﾚｽ(0208)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x1A5A)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 933;                          // ｢ﾊｰﾈｽ燃料種異常｣(933)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 963:
                    PrintLogData("    ﾊｰﾈｽ(点火方式) ");
                    rdbType1.Checked = true;                            // ﾊｰﾈｽ(燃料種) OFF 
                    rdbIGtm2.Checked= true;                             // ﾊｰﾈｽ(点火方式) ON 
                    StepUpTimer(500);
                    break;

                case 964:
                    ret = GetRamData_APP("0210", 2);                    // ｱﾄﾞﾚｽ(0210)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x2A5A)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 934;                               // ｢ﾊｰﾈｽ点火方式異常｣(934)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 965:
                    PrintLogData("    ﾊｰﾈｽ(着火ﾄﾗｲｱﾙ時間) ");
                    rdbIGtm1.Checked = true;                            // ﾊｰﾈｽ(点火方式) OFF 
                    rdbMethod2.Checked = true;                          // ﾊｰﾈｽ(着火ﾄﾗｲｱﾙ時間) ON 
                    StepUpTimer(500);
                    break;

                case 966:
                    ret = GetRamData_APP("0212", 2);                    // ｱﾄﾞﾚｽ(0212)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x4C3C)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 935;                           // ｢ﾊｰﾈｽ着火ﾄﾗｲｱﾙ時間異常｣(935)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 967:
                    rdbMethod1.Checked = true;                          // ﾊｰﾈｽ(着火ﾄﾗｲｱﾙ時間) OFF 
                    StepUpTimer(500);
                    break;

                case 968:
                    PrintLogData("    JP1,JP2,JP22 全OFF　");
                    ret = GetRamData_APP("0202", 2);                    // ｱﾄﾞﾚｽ(0202)のﾃﾞｰﾀ
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x0C3C)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 936;                               // ｢JP1 OFF 異常｣(936)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = GetRamData_APP("0204", 2);                    // ｱﾄﾞﾚｽ(0204)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x3A5A)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 937;                               // ｢JP2 OFF 異常｣(937)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = GetRamData_APP("0206", 2);                    // ｱﾄﾞﾚｽ(0206)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x6C3C)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 938;                               // ｢JP22 OFF 異常｣(938)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    swin_fg = 0;                                        // SW入力許可
                    NextStep();
                    break;

                case 969:
                    PrintLogData("    JP1,JP2,JP22 全ON　");
                    DisplayDipSw("ON", "ON", "ON");
                    lblInfo1.Text = "JP1=ON / JP2=ON / JP22=ON にしてから、";
                    lblInfo2.Text = "｢Go｣sw を押して下さい！";
                    test_status = 5;
                    NextStep();
                    break;

                case 970:
                    if (test_status != 2)
                    {
                        PlaySound("C:\\RCC300\\FC\\WAITsound.wav");     // Wait Buzzer
                        step_no--;
                        StepUpTimer(2000);
                        break;
                    }
                    swin_fg = 1;                                        // SW入力禁止
                    StepUpTimer(500);
                    break;

                case 971:
                    PlaySound("C:\\RCC300\\FC\\PIsound.wav");           // PIPO Buzzer
                    ret = GetRamData_APP("0202", 2);                    // ｱﾄﾞﾚｽ(0202)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x0A5A)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 939;                               // ｢JP1 ON 異常｣(939)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = GetRamData_APP("0204", 2);                    // ｱﾄﾞﾚｽ(0204)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x3C3C)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 940;                               // ｢JP2 ON 異常｣(940)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    ret = GetRamData_APP("0206", 2);                    // 指定ｱﾄﾞﾚｽﾃﾞｰﾀ取得(ｱﾌﾟﾘCPU)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x6C3C)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 941;                               // ｢JP22 ON 異常｣(941)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 980;
                    NextStep();
                    break;

                // ---- K43069 5.16 ﾃﾞｼﾞﾀﾙ入力確認(BC-Rｺｱ) --------------------

                case 981:
                    PrintLogData("    <BC-Rｺｱ>\r\n");
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 998;
                        NextStep();
                    }
                    NextStep();
                    break;

                case 982:
                    PrintLogData("    風圧ｽｲｯﾁ ");
                    ControlAirSW(1);                                    // ｴｱｰSW ON
                    StepUpTimer(500);
                    break;

                case 983:
                    ret = GetRamAdrData_BCR("0BC9");                    // ｱﾄﾞﾚｽ(3017)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    // 風圧SW(b0)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0001) != 0x0001 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0001) != 0x0001)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 942;                               // ｢風圧ｽｲｯﾁ ﾋﾞｯﾄ異常｣(942)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 984:
                    PrintLogData("    高燃焼ｲﾝﾀｰﾛｯｸ ");
                    ControlAirSW(0);                                    // ｴｱｰSW OFF
                    rdbDamperDS11.Checked = true;                       // 高燃焼ｲﾝﾀｰﾛｯｸ(DS1_HH) ON
                    StepUpTimer(500);
                    break;

                case 985:
                    ret = GetRamAdrData_BCR("0BC9");                          // 指定ｱﾄﾞﾚｽ(3017)のﾃﾞｰﾀ取得(BC-Rｺｱ)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    // 高燃焼ｲﾝﾀｰﾛｯｸ(b1)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0002) != 0x0002 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0002) != 0x0002)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 943;                          // ｢高燃焼ｲﾝﾀｰﾛｯｸ ﾋﾞｯﾄ異常｣(943)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 986:
                    PrintLogData("    低燃焼ｲﾝﾀｰﾛｯｸ ");
                    rdbDamperDS14.Checked = true;                       // 低燃焼ｲﾝﾀｰﾛｯｸ(DS1_L) ON
                    StepUpTimer(500);
                    break;

                case 987:
                    ret = GetRamAdrData_BCR("0BC9");                          // 指定ｱﾄﾞﾚｽ(3017)のﾃﾞｰﾀ取得(BC-Rｺｱ)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    // 低燃焼ｲﾝﾀｰﾛｯｸ(b2)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0004) != 0x0004 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0004) != 0x0004)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 944;                           // ｢低燃焼ｲﾝﾀｰﾛｯｸ ﾋﾞｯﾄ異常｣(944)
                        step_no = 998;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 988:
                    rdbDamperDS10.Checked = true;                       // DS1 Open
                    PrintLogData("    ｶﾞｽ圧sw OFF\r\n");
                    ControlGusSW(0);                                    // ｶﾞｽSW OFF
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 998;
                    NextStep();
                    break;

                case 999:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 10.ﾃﾞｼﾞﾀﾙ出力確認 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1000:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1099;
                        NextStep();
                        break;
                    }

                    PrintLogData(" 10.ﾃﾞｼﾞﾀﾙ出力確認\r\n");
                    // pppppppppppppppppppppppppppppppppppppppppppppppppppppppp
                    //PrintLogData(" ---[ ﾊﾟｽ ]---\r\n");
                    //step_no = 1099;
                    //NextStep();
                    //break;
                    // pppppppppppppppppppppppppppppppppppppppppppppppppppppppp
                    NextStep();
                    break;

                // ---- K43069 5.17 ﾃﾞｼﾞﾀﾙ出力確認(ｱﾌﾟﾘCPU) -------------------

                case 1001:
                    PrintLogData("    <ｱﾌﾟﾘCPU>\r\n");
                    PrintLogData("    初期値 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1001;                              // ｢初期値異常｣(1001)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1002:
                    PrintLogData("    BLWsw ON\r\n");
                    ControlBlwSW(1);                                    // BLW ON
                    PrintLogData("    K10 ON");
                    ret = SetRamBit_K10_APP(1);                         // K10 ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(3000);
                    break;

                case 1003:
                    PrintLogData("    MBL ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0200)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1002;                              // ｢BLWsw異常｣(1002)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    BLWsw OFF\r\n");
                    ControlBlwSW(0);                                    // BLW OFF
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1004:
                    PrintLogData("    ｶﾞｽ圧sw ON\r\n");
                    ControlGusSW(1);                                    // ｶﾞｽSW ON
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1098;
                        NextStep();
                    }
                    PrintLogData("    K1,K2,K3,K4 ON");
                    ret = SetRamBit_K1_BCR(1, 1);                       // K1(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(1, 1);                       // K2(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K3_BCR(1, 1);                       // K3(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K4_BCR(1, 1);                       // K4(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1005:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 1098;
                        NextStep();
                    }
                    PrintLogData("    K13 ON");
                    ret = SetRamBit_K13_APP(1);                         // K13(SV) ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1006:
                    PrintLogData("    SV1,SV2 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x003F)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1003;                              // ｢SV1,SV2異常｣(1003)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K13 OFF");
                    ret = SetRamBit_K13_APP(0);                         // K13(SV) OFF    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1007:
                    PrintLogData("    K14 ON");
                    ret = SetRamBit_K14_APP(1);                         // K14(HV) ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1008:
                    PrintLogData("    HV ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x004F)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1004;                              // ｢HV異常｣(1004)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K14 OFF");
                    ret = SetRamBit_K14_APP(0);                         // K14(HV) OFF    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1009:
                    PrintLogData("    K15 ON");
                    ret = SetRamBit_K15_APP(1);                         // K15(CV) ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1010:
                    PrintLogData("    CV ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x008F)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1005;                              // ｢CV異常｣(1005)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K15 OFF");
                    ret = SetRamBit_K15_APP(0);                         // K15(CV) OFF    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                // ------------------------------------------------------------

                case 1011:
                    PrintLogData("    ｶﾞｽ圧sw OFF\r\n");
                    ControlGusSW(0);                                    // ｶﾞｽSW OFF
                    PrintLogData("    K10 OFF");
                    ret = SetRamBit_K10_APP(0);                         // K10 OFF    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1098;
                        NextStep();
                    }
                    PrintLogData("    K1,K2,K3,K4 OFF");
                    ret = SetRamBit_K1_BCR(0, 0);                       // K1(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(0, 0);                       // K2(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K3_BCR(0, 0);                       // K3(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K4_BCR(0, 0);                       // K4(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                // ------------------------------------------------------------

                case 1012:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 1098;
                        NextStep();
                    }
                    PrintLogData("    K18 ON");
                    ret = SetRamBit_K18_APP(1);                         // K18(Dp1p) ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1013:
                    PrintLogData("    MDU12 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0800)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1006;                              // ｢MDU12異常｣(1006)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1014:
                    PrintLogData("    K16 ON");
                    ret = SetRamBit_K16_APP(1);                         // K16(Dp1d) ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1015:
                    PrintLogData("    MDU11 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0400)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1007;                              // ｢MDU11異常｣(1007)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K18,K16 OFF");
                    ret = SetRamBit_K18_APP(0);                         // K18(Dp1p) OFF    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K16_APP(0);                         // K16(Dp1d) OFF    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1016:
                    PrintLogData("    K19 ON");
                    ret = SetRamBit_K19_APP(1);                         // K19(Dp2p) ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1017:
                    PrintLogData("    MDU22 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x2000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1008;                              // ｢MDU22異常｣(1008)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1018:
                    PrintLogData("    K17 ON");
                    ret = SetRamBit_K17_APP(1);                         // K17(Dp2d) ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1019:
                    PrintLogData("    MDU21 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x1000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1009;                              // ｢MDU21異常｣(1009)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1020:
                    PrintLogData("    K19,K17 OFF");
                    ret = SetRamBit_K19_APP(0);                         // K19(Dp2p) OFF    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K17_APP(0);                         // K17(Dp2d) OFF    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    step_no = 1030;
                    StepUpTimer(500);
                    break;

                // ---- K43069 5.18 ﾃﾞｼﾞﾀﾙ出力確認(BC-Rｺｱ) --------------------

                case 1031:
                    PrintLogData("    <BC-Rｺｱ>\r\n");
                    PrintLogData("    ｶﾞｽ圧sw ON\r\n");
                    ControlGusSW(1);                                    // ｶﾞｽSW ON
                    PrintLogData("    K10 ON");
                    ret = SetRamBit_K10_APP(1);                         // K10 ON    
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                // ------------------------------------------------------------

                case 1032:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1098;
                        NextStep();
                    }
                    PrintLogData("    K1,K2,K3,K4,K5 全てOFF");
                    ret = SetRamBit_K1_BCR(0, 0);                       // K1(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(0, 0);                       // K2(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K3_BCR(0, 0);                       // K3(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K4_BCR(0, 0);                       // K4(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K5_BCR(0, 0);                       // K4(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(500);
                    break;

                case 1033:
                    PrintLogData("    初期値 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1010;                              // ｢初期値異常｣(1010)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1034:
                    PrintLogData("    K1(M:ON,S:OFF) ");
                    ret = SetRamBit_K1_BCR(1, 0);                         // K1(M:ON,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1035:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1011;                              // ｢K1<M:ON,S:OFF>異常｣(1011)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1036:
                    PrintLogData("    K1(M:OFF,S:ON) ");
                    ret = SetRamBit_K1_BCR(0, 1);                       // K1(M:OFF,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1037:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1012;                              // ｢K1<M:OFF,S:ON>異常｣(1012)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K1(M:OFF,S:OFF) ");
                    ret = SetRamBit_K1_BCR(0, 0);                       // K1(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1038:
                    PrintLogData("    K2(M:ON,S:OFF) ");
                    ret = SetRamBit_K2_BCR(1, 0);                       // K2(M:ON,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1039:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1013;                              // ｢K2<M:ON,S:OFF>異常｣(1013)                          
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1040:
                    PrintLogData("    K2(M:OFF,S:ON) ");
                    ret = SetRamBit_K2_BCR(0, 1);                       // K2(M:OFF,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1041:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1014;                              // ｢K2<M:OFF,S:ON>異常｣(1014)  
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1042:
                    PrintLogData("    K1(M:ON,S:ON),K2(M:ON,S:ON) ");
                    ret = SetRamBit_K1_BCR(1, 1);                       // K1(main) ON
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(1, 1);                       // K2(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1043:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1015;                     // ｢K1<M:ON,S:ON>,K2<M:ON,S:ON>異常｣(1015)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1044:
                    PrintLogData("    K3(M:ON,S:OFF) ");
                    ret = SetRamBit_K3_BCR(1, 0);                       // K3(M:ON,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1045:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1016;                              // ｢K3<M:ON,S:OFF>異常｣(1016)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1046:
                    PrintLogData("    K3(M:OFF,S:ON) ");
                    ret = SetRamBit_K3_BCR(0, 1);                       // K3(M:OFF,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1047:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1017;                              // ｢K3<M:OFF,S:ON>異常｣(1017)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K3(M:OFF,S:OFF) ");
                    ret = SetRamBit_K3_BCR(0, 0);                       // K3(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1048:
                    PrintLogData("    K4(M:ON,S:OFF) ");
                    ret = SetRamBit_K4_BCR(1, 0);                       // K4(M:ON,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1049:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1018;                          // ｢K4<M:ON,S:OFF>異常｣(1017)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1050:
                    PrintLogData("    K4(M:OFF,S:ON) ");
                    ret = SetRamBit_K4_BCR(0, 1);                       // K4(M:OFF,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1051:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1019;                              // ｢K4<M:OFF,S:ON>異常｣(1018)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K4(M:OFF,S:OFF) ");
                    ret = SetRamBit_K4_BCR(0, 0);                       // K4(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1052:
                    PrintLogData("    K5(M:ON,S:OFF) ");
                    ret = SetRamBit_K5_BCR(1, 0);                       // K5(M:ON,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1053:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1020;                              // ｢K5<M:ON,S:OFF>異常｣(1019)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1054:
                    PrintLogData("    K5(M:OFF,S:ON) ");
                    ret = SetRamBit_K5_BCR(0, 1);                         // K5(M:OFF,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1055:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1021;                              // ｢K5<M:OFF,S:ON>異常｣(1021)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1056:
                    PrintLogData("    K5(M:OFF,S:OFF) ");
                    ret = SetRamBit_K5_BCR(0, 0);                         // K5(sub) OFF
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1057:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1022;                              // ｢K5<M:OFF,S:OFF>異常｣(1022)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1058:
                    PrintLogData("    K3(M:ON,S:ON) ");
                    ret = SetRamBit_K3_BCR(1, 1);                       // K3(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1059:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0003)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1023;                              // ｢K3<M:ON,S:ON>異常｣(1023)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K3(M:OFF,S:OFF) ");
                    ret = SetRamBit_K3_BCR(0, 0);                       // K3(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1060:
                    PrintLogData("    K4(M:ON,S:ON) ");
                    ret = SetRamBit_K4_BCR(1, 1);                       // K4(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1061:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x000C)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1024;                              // ｢K4<M:ON,S:ON>異常｣(1024)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K4(M:OFF,S:OFF) ");
                    ret = SetRamBit_K4_BCR(0, 0);                       // K4(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1062:
                    PrintLogData("    K5(M:ON,S:ON) ");
                    ret = SetRamBit_K5_BCR(1, 1);                       // K5(main) ON
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1063:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0100)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1025;                              // ｢K5<M:ON,S:ON>異常｣(1025)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K5(M:OFF,S:OFF) ");
                    ret = SetRamBit_K5_BCR(0, 0);                       // K5(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1064:
                    PrintLogData("    K1,K2,K5,K10 全てOFF ");
                    ret = SetRamBit_K1_BCR(0, 0);                       // K1(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K2_BCR(0, 0);                       // K2(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K5_BCR(0, 0);                       // K5(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    // 部品(R305)が未実装のため出力制御は不可
                    //ret = SetRamBit_K10_BCR(0);                         // K10 OFF
                    //if (ret < 0)
                    //{
                    //    PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                    //    error_code = 52;
                    //    step_no = 1098;
                    //    NextStep();
                    //    break;
                    //}
                    StepUpTimer(500);
                    break;

                case 1065:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1026;                              // ｢K1,K2,K5,K10 全てOFF 異常｣(1026)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // --- 警報ﾘﾚｰ(K6) --------------------------------------------

                case 1066:
                    PrintLogData("    K6 Reset ");
                    ret = SetRamBit_K6R_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6S_BCR(0, 0);                       // K6S(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6R_BCR(1, 1);                       // K6R(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1067:
                    ret = SetRamBit_K6R_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1068:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1027;                              // ｢R6 ﾘｾｯﾄ 異常｣(1027)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1069:
                    PrintLogData("    K6S(M:ON,S:OFF) ");
                    ret = SetRamBit_K6S_BCR(1, 0);                       // K6S(M:ON,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1070:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x4000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1028;                               // ｢K6S(M:ON,S:OFF) 異常｣(1028)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1071:
                    PrintLogData("    K6 Reset ");
                    ret = SetRamBit_K6R_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6S_BCR(0, 0);                       // K6S(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6R_BCR(1, 1);                       // K6R(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1072:
                    ret = SetRamBit_K6R_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1073:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1029;                               // ｢R6 ﾘｾｯﾄ 異常｣(1029)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1074:
                    PrintLogData("    K6S(M:OFF,S:ON) "); 
                    ret = SetRamBit_K6S_BCR(0, 1);                       // K6S(M:OFF,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1075:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x4000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1030;                               // ｢K6S(M:OFF,S:ON) 異常｣(1030)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    K6S(M:OFF,S:OFF) ");
                    ret = SetRamBit_K6S_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1076:
                    PrintLogData("    K6R(M:ON,S:OFF) ");
                    ret = SetRamBit_K6R_BCR(1, 0);                       // K6S(M:ON,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1077:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x4000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1031;                               // ｢K6R(M:ON,S:OFF) 異常｣(1031)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1078:
                    PrintLogData("    K6R(M:OFF,S:ON) ");
                    ret = SetRamBit_K6R_BCR(0, 1);                      // K6R(M:OFF,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1079:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x4000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1032;                               // ｢K6R(M:OFF,S:ON)異常｣(1032)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1080:
                    PrintLogData("    K6R(M:ON,S:ON) ");
                    ret = SetRamBit_K6R_BCR(1, 1);                      // K6R(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1081:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1033;                               // ｢K6R(M:ON,S:ON) 異常｣(1033)
                        step_no = 1098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ------------------------------------------------------------

                case 1082:
                    PrintLogData("    ｶﾞｽ圧sw OFF\r\n");
                    ControlGusSW(0);                                    // ｶﾞｽSW OFF
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 1098;
                    NextStep();
                    break;

                case 1099:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 11.CPU間信号確認 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1100:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1199;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.19 ｱﾌﾟﾘCPU、BC-Rｺｱ間の信号確認 -----------

                    PrintLogData(" 11.CPU間信号確認\r\n");
                    // pppppppppppppppppppppppppppppppppppppppppppppppppppppppp
                    //PrintLogData(" ---[ ﾊﾟｽ ]---\r\n");
                    //step_no = 1199;
                    //NextStep();
                    //break;
                    // pppppppppppppppppppppppppppppppppppppppppppppppppppppppp
                    NextStep();
                    break;

                // ---- K43069 5.19.1 ｱﾌﾟﾘCPU → BC-Rｺｱ -----------------------

                case 1101:
                    PrintLogData("    [ｱﾌﾟﾘCPU → BC-Rｺｱ]\r\n");
                    PrintLogData("    ｲﾝﾀｰﾛｯｸ,起動,ﾘｾｯﾄのﾋﾞｯﾄ → '1'");
                    ret = SetRamBit_CPU_APP(1);                         // ｾｯﾄ(IR,起動､ﾘｾｯﾄ)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(2000);
                    break;

                case 1102:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1198;
                        NextStep();
                    }
                    PrintLogData("    遮断弁閉,ｲﾝﾀｰﾛｯｸのﾋﾞｯﾄ ");
                    ret = GetRamAdrData_BCR("0BC9");                    // ｱﾄﾞﾚｽ(3017)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    // 遮断弁閉(b4)=0,ｲﾝﾀｰﾛｯｸ(b3)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0018) != 0x0008 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0018) != 0x0008)
                    {
                         PrintLogData("...ＮＧ!\r\n");
                         error_code = 1101;                 // ｢遮断弁閉,ｲﾝﾀｰﾛｯｸのﾋﾞｯﾄ異常<1>｣(1101)
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1103:
                    PrintLogData("    RSTsw,DISPsw,起動,ﾘｾｯﾄのﾋﾞｯﾄ ");
                    ret = GetRamAdrData_BCR("0BC8");                    // ｱﾄﾞﾚｽ(3016)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    // RSTsw(b3)=0,DISPsw(b2)=0,起動(b1)=1,ﾘｾｯﾄ(b0)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x00f3) != 0x0003 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x00f3) != 0x0003)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1102;           // ｢RSTsw,DISPsw,起動,ﾘｾｯﾄのﾋﾞｯﾄ異常<1>｣(1102)

                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1104:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 1198;
                        NextStep();
                    }
                    PrintLogData("    ｲﾝﾀｰﾛｯｸ,起動,ﾘｾｯﾄのﾋﾞｯﾄ → '0'");
                    ret = SetRamBit_CPU_APP(0);                         // ｾｯﾄ(IR,起動､ﾘｾｯﾄ)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(2000);
                    break;

                case 1105:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1198;
                        NextStep();
                    }
                    PrintLogData("    遮断弁閉,ｲﾝﾀｰﾛｯｸのﾋﾞｯﾄ ");
                    ret = GetRamAdrData_BCR("0BC9");                    // ｱﾄﾞﾚｽ(3017)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    // 遮断弁閉(b4)=0,ｲﾝﾀｰﾛｯｸ(b3)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0018) != 0x0000 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0018) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1103;                      // ｢遮断弁閉,ｲﾝﾀｰﾛｯｸのﾋﾞｯﾄ異常<0>｣(1103)
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1106:
                    PrintLogData("    RSTsw,DISPsw,起動,ﾘｾｯﾄのﾋﾞｯﾄ ");
                    ret = GetRamAdrData_BCR("0BC8");                    // ｱﾄﾞﾚｽ(3016)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    // RSTsw(b3)=0,DISPsw(b2)=0,起動(b1)=1,ﾘｾｯﾄ(b0)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x00f3) != 0x0000 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x00f3) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1104;              // ｢RSTsw,DISPsw,起動,ﾘｾｯﾄのﾋﾞｯﾄ異常<0>｣(1104)   
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1107:
                    PrintLogData("    商用電源ﾊﾟﾙｽ ");
                    ret = GetRamAdrData_BCR("0E61");                    // ｱﾄﾞﾚｽ(3681)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(eeprom_dt_bcr_M, 16)  == 0x005A ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) == 0x005A)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1105;                              // ｢商用電源ﾊﾟﾙｽ異常｣(1105)
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                // ---- K43069 5.19.2 BC-Rｺｱ → ｱﾌﾟﾘCPU -----------------------

                case 1108:
                    PrintLogData("    [BC-Rｺｱ → ｱﾌﾟﾘCPU]\r\n");
                    PrintLogData("    BLM,DmpH/L,Dmp比例,PV,MVのﾋﾞｯﾄ → '1'");
                    ret = SetRamBit_K10_BCR(1);                         // K10 ON
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K7_BCR(1);                          // set K7(ﾀﾞﾝﾊﾟH/L)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K8_BCR(1);                          // set K8(ﾀﾞﾝﾊﾟ比例)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_PV_BCR(1);                          // set PV同期信号
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_MV_BCR(1);                          // set MV同期信号
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(2000);
                    break;

                case 1109:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 1198;
                        NextStep();
                    }
                    PrintLogData("    BLM,DmpH/L,Dmp比例,PV,MVのﾋﾞｯﾄ ");
                    ret = GetRamData_APP("0705", 1);                    // ｱﾄﾞﾚｽ(0705)のﾃﾞｰﾀ取得                       if (ret < 0)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0x7f) != 0x7f)
                    {
                        PrintLogData("...NG! <○:0x7f>\r\n");
                        error_code = 1106;            // ｢BLM,DmpH/L,Dmp比例,PV,MVのﾋﾞｯﾄ異常<1>｣(1106)
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1110:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1198;
                        NextStep();
                    }
                    PrintLogData("    BLM,DmpH/L,Dmp比例,PV,MVのﾋﾞｯﾄ → '0'");
                    ret = SetRamBit_K10_BCR(0);                         // K10 OFF
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K7_BCR(0);                          // set K7(ﾀﾞﾝﾊﾟH/L)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K8_BCR(0);                          // set K8(ﾀﾞﾝﾊﾟ比例)
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_PV_BCR(0);                          // set PV同期信号
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_MV_BCR(0);                          // set MV同期信号
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    StepUpTimer(2000);
                    break;

                case 1111:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 1198;
                        NextStep();
                    }
                    PrintLogData("    BLM,DmpH/L,Dmp比例,PV,MVのﾋﾞｯﾄ ");
                    ret = GetRamData_APP("0705", 1);                    // ｱﾄﾞﾚｽ(0705)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0x7f) != 0x00)
                    {
                        PrintLogData("...NG! <○:0x00>\r\n");
                        error_code = 1107;          // ｢BLM,DmpH/L,Dmp比例,PV,MVのﾋﾞｯﾄ異常<0>｣(1107)
                        step_no = 1198;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 1198;
                    NextStep();
                    break;
                
                case 1199:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 12.ﾌﾚｰﾑ電圧確認 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1200:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1299;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.20 ﾌﾚｰﾑ電圧入力 --------------------------

                    PrintLogData(" 12.ﾌﾚｰﾑ電圧確認\r\n");
                    NextStep();
                    break;

                case 1201:
                    PrintLogData("    [ﾌﾚｰﾑﾛｯﾄﾞ]\r\n");
                    PrintLogData("    28.7MΩ : ");
                    rdbSensFL3.Checked = true;
                    StepUpTimer(3000);
                    break;

                case 1202:
                    ret = GetRamData_APP("0760", 2);                    // ｱﾄﾞﾚｽ(0760)のﾃﾞｰﾀ取得                        if (ret < 0)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1298;
                        NextStep();
                        break;
                    }
                    ad_dt = Convert.ToInt32(ram_dt, 16);
                    ad_dt = ad_dt / 10;
                    PrintLogData(ad_dt.ToString("#0.0") + "[μA] ★19");
                    save_dt[19] = ad_dt.ToString("#0.0");               // <V6.10>にて追加
                    if (1.5 > ad_dt || ad_dt > 2.5)
                    {
                        PrintLogData("...NG!\r\n");
                        PrintLogData("                    <○:1.5～2.5μA>\r\n");
                        error_code = 1201;                              // ｢ﾌﾚｰﾑﾛｯﾄﾞ 28.7MΩ のﾌﾚｰﾑ電圧異常｣(1201)
                        step_no = 1298;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1203:
                    PrintLogData("    Open    : ");
                    rdbSensFL0.Checked = true;
                    StepUpTimer(3000);
                    break;

                case 1204:
                    ret = GetRamData_APP("0760", 2);                    // ｱﾄﾞﾚｽ(0760)のﾃﾞｰﾀ取得                        if (ret < 0)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1298;
                        NextStep();
                        break;
                    }
                    ad_dt = Convert.ToInt32(ram_dt, 16);
                    ad_dt = ad_dt / 10;
                    PrintLogData(ad_dt.ToString("#0.0") + "[μA] ★20");
                    save_dt[20] = ad_dt.ToString("#0.0");               // <V6.10>にて追加
                    if (-0.5 > ad_dt || ad_dt > 0.5)
                    {
                        PrintLogData("...NG!\r\n");
                        PrintLogData("                    <○:-0.5～0.5μA>\r\n");
                        error_code = 1202;                              // ｢ﾌﾚｰﾑﾛｯﾄﾞ Open のﾌﾚｰﾑ電圧異常｣(1202)
                        step_no = 1298;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1205:
                    PrintLogData("    [AFD]\r\n");
                    PrintLogData("    125KΩ  : ");
                    rdbSensAFD1.Checked = true;
                    StepUpTimer(3000);
                    break;

                case 1206:
                    ret = GetRamData_APP("0750", 2);                    // ｱﾄﾞﾚｽ(0750)のﾃﾞｰﾀ取得                        if (ret < 0)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1298;
                        NextStep();
                        break;
                    }
                    ad_dt = Convert.ToInt32(ram_dt, 16);
                    ad_dt = ad_dt / 10;
                    PrintLogData(ad_dt.ToString("#0.0") + "[μA] ★21");
                    save_dt[21] = ad_dt.ToString("#0.0");               // <V6.10>にて追加
                    if (26.8 > ad_dt || ad_dt > 32.0)                   // <V6.10>にて 31.8 -> 32.0
                    {
                        PrintLogData("...NG!\r\n");
                        PrintLogData("                    <○:26.8～32.0μA>\r\n");
                        error_code = 1203;                              // ｢AFD 125KΩ のﾌﾚｰﾑ電圧異常｣(1203)
                        step_no = 1298;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1207:
                    PrintLogData("    Open    : ");
                    rdbSensAFD0.Checked = true;
                    StepUpTimer(3000);
                    break;

                case 1208:
                    ret = GetRamData_APP("0750", 2);                    // ｱﾄﾞﾚｽ(0750)のﾃﾞｰﾀ取得                        if (ret < 0)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1298;
                        NextStep();
                        break;
                    }
                    ad_dt = Convert.ToInt32(ram_dt, 16);
                    ad_dt = ad_dt / 10;
                    PrintLogData(ad_dt.ToString("#0.0") + "[μA] ★22");
                    save_dt[22] = ad_dt.ToString("#0.0");               // <V6.10>にて追加
                    if (-2.5 > ad_dt || ad_dt > 2.5)
                    {
                        PrintLogData("...NG!\r\n");
                        PrintLogData("                    <○:-2.5～2.5μA>\r\n");
                        error_code = 1204;                              // ｢AFD Open のﾌﾚｰﾑ電圧異常｣(1204)
                        step_no = 1298;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 1298;
                    NextStep();
                    break;

                case 1299:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 13.基板ｻｰﾐｽﾀ確認 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1300:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1399;
                        NextStep();
                        break;
                    }

                // ---- K43069 5.21 基板ｻｰﾐｽﾀ検査 -----------------------------

                    PrintLogData(" 13.基板ｻｰﾐｽﾀ確認\r\n");
                    NextStep();
                    break;

                case 1301:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1398;
                        NextStep();
                    }
                    PrintLogData("    温度 : ");
                    ret = GetRamAdrData_BCR("0BCF");                    // ｱﾄﾞﾚｽ(3023)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1398;
                        NextStep();
                        break;
                    }

                    ad_dt = Convert.ToInt32(eeprom_dt_bcr_M, 16) - 400;
                    ad_dt = ad_dt /10;
                    PrintLogData(ad_dt.ToString("#0.0") + "[℃] ★23");
                    save_dt[23] = ad_dt.ToString("#0.0");               // <V6.10>にて追加
                    if (10.0 > ad_dt || ad_dt > 45.0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1301;                              // ｢温度異常｣(1301)
                        step_no = 1398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1302:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 1398;
                    NextStep();
                    break;

                case 1399:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 14.ﾌﾚｰﾑ入力確認 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1400:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1499;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.22 ﾌﾚｰﾑ入力確認 --------------------------

                    PrintLogData(" 14.ﾌﾚｰﾑ入力確認\r\n");
                    NextStep();
                    break;

                case 1401:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1498;
                        NextStep();
                    }
                    NextStep();
                    break;

                case 1402:
                    PrintLogData("    <ﾌﾚｰﾑﾛｯﾄﾞ>\r\n");
                    PrintLogData("     着火(110MΩ) ");
                    rdbSensFL1.Checked = true;
                    //PrintLogData("     着火(28.7MΩ) ");
                    //rdbSensFL3.Checked = true;
                    StepUpTimer(5000);
                    break;

                case 1403:
                    ret = GetRamAdrData_BCR("0BCB");                      // ｱﾄﾞﾚｽ(3019)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1498;
                        NextStep();
                        break;
                    }
                    // 火炎入力情報(b0)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0001) != 0x0001 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0001) != 0x0001)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1401;                              // ｢ﾌﾚｰﾑﾛｯﾄﾞ着火入力異常｣(1401)
                        step_no = 1498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1404:
                    PrintLogData("     消炎(220MΩ) ");
                    rdbSensFL2.Checked = true;
                    StepUpTimer(5000);
                    break;

                case 1405:
                    ret = GetRamAdrData_BCR("0BCB");                      // ｱﾄﾞﾚｽ(3019)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1498;
                        NextStep();
                        break;
                    }
                    // 火炎入力情報(b0)=0のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0001) != 0x0000 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0001) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1402;                              // ｢ﾌﾚｰﾑﾛｯﾄﾞ消炎入力異常｣(1402)
                        step_no = 1498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1406:
                    PrintLogData("     Open\r\n");
                    rdbSensFL0.Checked = true;
                    StepUpTimer(500);
                    break;

                case 1407:
                    PrintLogData("    <AFD>\r\n");
                    PrintLogData("     着火(125KΩ) ");
                    rdbSensAFD1.Checked = true;
                    StepUpTimer(5000);
                    break;

                case 1408:
                    ret = GetRamAdrData_BCR("0BCB");                      // ｱﾄﾞﾚｽ(3019)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1498;
                        NextStep();
                        break;
                    }
                    // 火炎入力情報(b0)=1のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0001) != 0x0001 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0001) != 0x0001)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1403;                              // ｢AFD着火入力異常｣(1403)
                        step_no = 1498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1409:
                    PrintLogData("     消炎(140KΩ) ");
                    rdbSensAFD2.Checked = true;
                    StepUpTimer(5000);
                    break;

                case 1410:
                    ret = GetRamAdrData_BCR("0BCB");                      // ｱﾄﾞﾚｽ(3019)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1498;
                        NextStep();
                        break;
                    }
                    // 火炎入力情報(b0)=0のﾋﾞｯﾄを確認
                    if ((Convert.ToInt32(eeprom_dt_bcr_M, 16) & 0x0001) != 0x0000 ||
                        (Convert.ToInt32(eeprom_dt_bcr_S, 16) & 0x0001) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1404;                              // ｢AFD消炎入力異常｣(1404)
                        step_no = 1498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1411:
                    PrintLogData("     Open\r\n");
                    rdbSensAFD0.Checked = true;
                    StepUpTimer(500);
                    break;

                case 1412:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 1498;
                    NextStep();
                    break;

                case 1499:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 15.LED確認 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1500:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1599;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.23 LED確認 -------------------------------

                    PrintLogData(" 15.LED確認\r\n");
                    NextStep();
                    break;

                case 1501:
                    PrintLogData("    LED点灯確認 ");
                    ret = SetRamBit_LED_APP(1);                         // LED 点灯
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1598;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1502:
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1501;                               // ｢LEDが点灯しない｣(1501)
                        step_no = 1598;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1503:
                    PrintLogData("    LED消灯確認 ");
                    ret = SetRamBit_LED_APP(0);                         // LED 消灯
                    if (ret < 0)
                    {
                        PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1598;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1504:
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret == 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1502;                              // ｢LEDが消灯しない｣(1502)
                        step_no = 1598;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 1598;
                    NextStep();
                    break;

                case 1599:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 16.基板間通信確認 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1600:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1699;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.26 基板間通信確認 ------------------------

                    PrintLogData(" 16.基板間通信確認\r\n");
                    NextStep();
                    break;

                case 1601:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1698;
                        NextStep();
                    }
                    NextStep();
                    break;

                case 1602:
                    PrintLogData("    BC-Rｺｱ ");
                    try_cnt = 10;
                    NextStep();
                    break;

                case 1603:
                    ret = GetVerCrc_BCR();                              // Ver,CRC確認 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1698;
                        NextStep();
                        break;
                    }
                    if (ver_dt_bcr_M != BcrVerM || ver_dt_bcr_S != BcrVerS ||
                        crc_dt_bcr_M != BcrSumM || crc_dt_bcr_S != BcrSumS)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1601;                              // ｢通信異常<BC-Rｺｱ>｣(1601)
                        step_no = 1698;
                        NextStep();
                        break;
                    }
                    try_cnt--;
                    if (try_cnt > 0)
                    {
                        PrintLogData("o");
                        step_no--;
                        StepUpTimer(200);
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1604:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 1698;
                        NextStep();
                    }
                    NextStep();
                    break;

                case 1605:
                    PrintLogData("    ｱﾌﾟﾘCPU ");
                    try_cnt = 10;
                    NextStep();
                    break;

                case 1606:
                    ret = GetVerCrc_APP();                              // Ver,CRC確認 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1698;
                        NextStep();
                        break;
                    }
                    //if (ver_dt != AppVerN || crc_dt != AppSumN || type_dt != "4B")
                    if (ver_dt != AppVerT || crc_dt != AppSumT || type_dt != "4B")
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1602;                              // ｢通信異常<ｱﾌﾟﾘCPU>｣(1602)
                        step_no = 1698;
                        NextStep();
                        break;
                    }
                    try_cnt--;
                    if (try_cnt > 0)
                    {
                        PrintLogData("o");
                        step_no--;
                        StepUpTimer(200);
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    step_no = 1698;
                    NextStep();
                    break;

                case 1699:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 17.基板間IO確認 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1700:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1799;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.27 基板間IO確認 --------------------------

                    PrintLogData(" 17.基板間IO確認\r\n");
                    NextStep();
                    break;

                case 1701:
                    PrintLogData("    火炎信号 ");
                    ret = SetRamBit_Kaen_APP(1);                        // 火炎信号ﾋﾞｯﾄ = 1    
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1702:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x8000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1701;                              // ｢火炎信号<ON>異常｣(1701)
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    NextStep();
                    break;

                case 1703:
                    ret = SetRamBit_Kaen_APP(0);                        // 火炎信号ﾋﾞｯﾄ = 0    
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1704:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1702;                              // ｢火炎信号<OFF>異常｣(1702)
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1705:
                    PrintLogData("    [ﾘﾚｰ制御]\r\n");
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1798;
                        NextStep();
                    }
                    PrintLogData("    警報ﾘﾚｰ(K6) OFF確認 ");
                    ret = SetRamBit_K6R_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6S_BCR(0, 0);                       // K6S(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6R_BCR(1, 1);                       // K6R(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1706:
                    ret = SetRamBit_K6R_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1707:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1703;                              // ｢警報<OFF>異常｣(1703)
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1708:
                    PrintLogData("    警報入力ｾｯﾄ ");
                    SetFromC1(1);                                       // set 警報入力(1s)
                    StepUpTimer(1000);
                    break;

                case 1709:
                    SetFromC1(0);                                       // reset 警報入力
                    StepUpTimer(3000);
                    break;

                case 1710:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x4000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1704;                              // ｢警報<ON>異常｣(1704)
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1711:
                    PrintLogData("    警報ﾘﾚｰ(K6) Reset ");
                    ret = SetRamBit_K6R_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6S_BCR(0, 0);                       // K6S(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    ret = SetRamBit_K6R_BCR(1, 1);                       // K6R(M:ON,S:ON)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    StepUpTimer(3000);
                    break;

                case 1712:
                    ret = SetRamBit_K6R_BCR(0, 0);                       // K6R(M:OFF,S:OFF)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 1713:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1705;                              // ｢警報<OFF>異常｣(1705)
                        step_no = 1798;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1714:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 1798;
                    NextStep();
                    break;

                case 1799:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 18.ｸﾛｯｸ確認 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1800:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1899;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.29 ｸﾛｯｸ検査 ------------------------------

                    PrintLogData(" 18.ｸﾛｯｸ確認\r\n");
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 1898;
                    }
                    NextStep();
                    break;


                case 1801:
                    PrintLogData("    Sub→Main ");
                    ret = GetRamAdrData_BCR("0E62");                    // ｱﾄﾞﾚｽ(3682)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1898;
                        NextStep();
                        break;
                    }
                    if (41 > Convert.ToInt32(eeprom_dt_bcr_M, 16) ||
                        Convert.ToInt32(eeprom_dt_bcr_M, 16) > 45 )
                    //if (41 > Convert.ToInt32(eeprom_dt_bcr_M, 16) ||
                    //    Convert.ToInt32(eeprom_dt_bcr_M, 16) > 45 ||
                    //    41 > Convert.ToInt32(eeprom_dt_bcr_S, 16) ||
                    //    Convert.ToInt32(eeprom_dt_bcr_S, 16) > 45)
                        {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1801;                              // ｢ｸﾛｯｸ値異常<Sub→Main>｣(1801)
                        step_no = 1898;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1802:
                    PrintLogData("    Main→Sub ");
                    ret = GetRamAdrData_BCR("0E63");                    // ｱﾄﾞﾚｽ(3683)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1898;
                        NextStep();
                        break;
                    }
                    if (41 > Convert.ToInt32(eeprom_dt_bcr_S, 16) ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) > 45)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 1802;                              // ｢ｸﾛｯｸ値異常<Main→Sub>｣(1802)
                        step_no = 1898;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 1803:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 1898;
                    NextStep();
                    break;

                case 1899:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 19.電源周期確認 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 1900:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 1999;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.30 電源周期の確認 ------------------------

                    PrintLogData(" 19.電源周期確認\r\n");
                    PrintLogData("    AC電源OFF ");
                    ControlACPower(0);                                  // AC Power OFF
                    StepUpTimer(3000);
                    break;

                case 1901:
                    ret = GetRamData_APP("0706", 1);                    // ｱﾄﾞﾚｽ(0706)ﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0x01) != 0x00)
                    {
                        PrintLogData(" ...ＮＧ!\r\n");
                        error_code = 1901;                              // ｢AC電源<OFF>異常｣(1901)
                        step_no = 1998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    StepUpTimer(500);
                    break;
                
                case 1902:
                    PrintLogData("    AC電源ON ");
                    ControlACPower(1);                                  // AC Power ON
                    StepUpTimer(3000);
                    break;

                case 1903:
                    ret = GetRamData_APP("0706", 1);                    // ｱﾄﾞﾚｽ(0706)のﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 1998;
                        NextStep();
                        break;
                    }
                    if ((Convert.ToInt32(ram_dt, 16) & 0x01) != 0x01)
                    {
                        PrintLogData(" ...ＮＧ!\r\n");
                        error_code = 1902;                              // ｢AC電源<ON>異常｣(1902)
                        step_no = 1998;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    step_no = 1998;
                    StepUpTimer(500);
                    break;

                case 1999:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 20.F/W書込(ｱﾌﾟﾘCPU<通常ﾓｰﾄﾞ>) +++++++++++++++++++++++++++++++++++++++++++++

                case 2000:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 2099;
                        NextStep();
                        break;
                    }

                    PrintLogData(" 20.ｱﾌﾟﾘCPUのFW書換\r\n");
                    PrintLogData("    電源 OFF\r\n");
                    ControlDCPower(0);                                  // DC Power OFF   
                    ControlACPower(0);                                  // AC Power OFF
                    if (serialPort2.IsOpen == true)                     // ﾛｰﾀﾞｰ通信ﾎﾟｰﾄ　ｸﾛｰｽﾞ
                    {
                        serialPort2.Close();
                    }
                    swin_fg = 0;                                        // SW入力許可
                    pnlGo.Enabled = true;
                    StepUpTimer(2000);
                    break;

                case 2001:
                    lblInfo1.Text = "S2の1,2をON にしてから、";
                    lblInfo2.Text = "｢Go｣sw を押して下さい！";
                    test_status = 5;
                    NextStep();
                    break;

                case 2002:
                    if (test_status != 2)
                    {
                        PlaySound("C:\\RCC300\\FC\\WAITsound.wav");     // WAIT Buzzer
                        step_no--;
                        StepUpTimer(2000);
                        break;
                    }
                    swin_fg = 1;                                        // SW入力禁止
                    StepUpTimer(500);
                    break;

                case 2003:
                    PlaySound("C:\\RCC300\\FC\\PIsound.wav");           // PI Buzzer
                    PrintLogData("    電源 ON\r\n");
                    ControlDCPower(1);                                  // DC Power ON
                    pnlGo.Enabled = false;
                    StepUpTimer(5000);
                    break;
                
                case 2004:
                    PrintLogData("    F/W書込 ");
                    ret = WriteAppCPU(@"C:\Program Files\Renesas\FDT4.09\FDT.exe",
                                    @"/DISCRETESTARTUP ""SimpleInterfaceMode /r""",
                                        @"C:\RCC300\FW\K\AppCPU\k_1.61.0.mot",
                                        ref check_sum);
                    if (ret != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2001;                              // ｢書込異常<ｱﾌﾟﾘCPU>｣(2001)
                        step_no = 2098;
                        NextStep();
                        break;
                    }
                    if (check_sum != AppSumN)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2002;                          // ｢ﾁｪｯｸｻﾑ異常<ｱﾌﾟﾘCPU>｣(2002)
                        step_no = 2098;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 2098;
                    NextStep();
                    break;

                case 2099:
                    PrintTime();    // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;


                // ++++ 21.試験再開 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 2100:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 2199;
                        NextStep();
                        break;
                    }

                    PrintLogData(" 21.試験再開\r\n");
                    PrintLogData("    電源 OFF\r\n");
                    ControlDCPower(0);                                  // DC Power OFF   
                    swin_fg = 0;                                        // SW入力許可
                    pnlGo.Enabled = true;
                    StepUpTimer(2000);
                    break;

                case 2101:
                    lblInfo1.Text = "S2の1,2をOFF にしてから、";
                    lblInfo2.Text = "｢Go｣sw を押して下さい！";
                    test_status = 5;
                    NextStep();
                    break;

                case 2102:
                    if (test_status != 2)
                    {
                        PlaySound("C:\\RCC300\\FC\\WAITsound.wav");     // WAIT Buzzer
                        step_no--;
                        StepUpTimer(2000);
                        break;
                    }
                    swin_fg = 1;                                        // SW入力禁止
                    StepUpTimer(500);
                    break;

                case 2103:
                    PlaySound("C:\\RCC300\\FC\\PIsound.wav");           // PI Buzzer
                    PrintLogData("    電源 ON\r\n");
                    ControlDCPower(1);                                  // DC Power ON
                    ControlACPower(1);                                  // AC Power ON
                    pnlGo.Enabled = false;
                    StepUpTimer(5000);
                    break;
                
                case 2104:
                    PrintLogData("    LED点灯確認 ");
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret == 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2101;                              // ｢LEDが点灯している｣(2101)
                        step_no = 2198;
                        NextStep();
                        break;
                    }
                    try_cnt = 20;                                       // ﾀｲﾑｵｰﾊﾞｰ　20s
                    StepUpTimer(500);
                    break;

                case 2105:
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2102;                          // ｢LEDが点灯しない｣(2102)
                            step_no = 2198;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 2198;
                    NextStep();
                    break;

                case 2199:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 22.Version,ﾁｪｯｸｻﾑ,基板ﾀｲﾌﾟ確認 ++++++++++++++++++++++++++++++++++++++++++++

                case 2200:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 2299;
                        NextStep();
                        break;
                    }

                    PrintLogData(" 22.Version,ﾁｪｯｸｻﾑ,基板ﾀｲﾌﾟ確認\r\n");
                    InitialSerialPortLoader();                          // ﾛｰﾀﾞｰ通信ﾎﾟｰﾄ 初期化,ｵｰﾌﾟﾝ 
                    StepUpTimer(500);
                    break;

                case 2201:
                    PrintLogData("    <ｱﾌﾟﾘCPU>\r\n");
                    ret = GetVerCrc_APP();                              // Ver,CRC確認 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2298;
                        NextStep();
                        break;
                    }
                    save_dt[3] = ver_dt;
                    save_dt[4] = crc_dt;
                    PrintLogData("              Ver : " + ver_dt + " ★3\r\n");
                    PrintLogData("           ﾁｪｯｸｻﾑ : " + crc_dt + " ★4\r\n");
                    PrintLogData("         基板ﾀｲﾌﾟ : " + type_dt + " ");
                    if (ver_dt != AppVerN || crc_dt != AppSumN || type_dt != "4B")
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2201;                      // ｢Ver,ﾁｪｯｸｻﾑ,基板ﾀｲﾌﾟ異常｣(2201)
                        step_no = 2298;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 2298;
                    NextStep();
                    break;

                case 2299:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 23.ｼｰｹﾝｽ、ﾀｲﾐﾝｸﾞ確認 ++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 2300:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 2399;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.25 ｼｰｹﾝｽ､ﾀｲﾐﾝｸﾞ確認 ------------------------

                    PrintLogData(" 23.ｼｰｹﾝｽ、ﾀｲﾐﾝｸﾞ確認\r\n");
                    PrintLogData("    ｴﾗｰｺｰﾄﾞ確認");
                    ret = GetRamData_APP("0255", 1);                    // ｱﾄﾞﾚｽ(0255)ﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("\r\n");
                    if (ram_dt == "3E")
                    {
                        PrintLogData("    ｼﾞｪﾈﾗﾙﾘｾｯﾄ");
                        try_cnt = 5;
                        NextStep();
                        break;
                    }
                    else
                    {
                        step_no = 2310;
                        NextStep();
                        break;
                    }

                case 2301:
                    ret = ResetErrorCode_GenRst("53", out rst_ret_code);    // ｼﾞｪﾈﾗﾙﾘｾｯﾄ
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2300;                      // ｢ｼﾞｪﾈﾗﾙﾘｾｯﾄ異常｣(2300)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    step_no = 2310;
                    StepUpTimer(1000);
                    break;

                // ---- ｼｰｹﾝｽ(5B) -----------------------------------------

                case 2311:
                    PrintLogData("   -[ｼｰｹﾝｽ<B>]-\r\n");
                    PrintLogData("    ｼｰｹﾝｽ設定(5B) ");
                    ret = SetSequence_APP("5B");                       // 燃焼ｼｰｹﾝｽ設定(5B)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2312:
                    PrintLogData("    書込完了確認 ");
                    try_cnt = 20;
                    NextStep();
                    break;

                case 2313:
                    ret = GetRamData_APP("0570", 1);                    // ｱﾄﾞﾚｽ(0570)のﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x1F)
                    {
                        try_cnt--;
                        if (try_cnt > 0)
                        {
                            step_no--;
                            StepUpTimer(1000);
                            break;
                        }
                        else
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2301;                          // ｢書込が終了ない｣(2301)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(500);
                    break;

                case 2314:
                    PrintLogData("    電源 OFF\r\n");
                    ControlDCPower(0);                                  // DC Power OFF   
                    ControlACPower(0);                                  // AC Power OFF   
                    swin_fg = 0;                                        // SW入力許可
                    pnlGo.Enabled = true;
                    StepUpTimer(500);
                    break;

                case 2315:
                    DisplayDipSw("OFF", "ON", "OFF");
                    lblInfo1.Text = "JP1=OFF / JP2=ON / JP22=OFF にをｾｯﾄしてから、";
                    lblInfo2.Text = "｢Go｣sw を押して下さい！";
                    test_status = 5;
                    NextStep();
                    break;

                case 2316:
                    if (test_status != 2)
                    {
                        PlaySound("C:\\RCC300\\FC\\WAITsound.wav");     // Wait Buzzer
                        step_no--;
                        StepUpTimer(2000);
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 2317:
                    PlaySound("C:\\RCC300\\FC\\PIsound.wav");           // PI Buzzer
                    swin_fg = 1;                                        // SW入力禁止
                    StepUpTimer(500);
                    break;

                case 2318:
                    PrintLogData("    燃料種(ﾊｰﾈｽ) 短絡\r\n");
                    rdbType2.Checked = true;                            // 燃料種　油
                    PrintLogData("    着火ﾄﾗｲｱﾙ時間(ﾊｰﾈｽ) 開放\r\n");
                    rdbIGtm1.Checked = true;                            // 着火ﾄﾗｲｱﾙ時間 5s
                    PrintLogData("    点火方式(ﾊｰﾈｽ) 短絡\r\n");
                    rdbMethod2.Checked = true;                          // 点火方式　ﾀﾞｲﾚｸﾄ
                    PrintLogData("    ｶﾞｽ圧ｽｲｯﾁ ON\r\n");
                    ControlGusSW(1);                                    // ｶﾞｽSW ON
                    PrintLogData("    DS1_L ON\r\n");
                    rdbDamperDS14.Checked = true;                       // DS1_L ON
                    PrintLogData("    DS2_L ON\r\n");
                    rdbDamperDS24.Checked = true;                       // DS2_L ON <V1.01> 23→24
                    StepUpTimer(500);
                    break;

                case 2319:
                    PrintLogData("    電源 ON\r\n");
                    ControlDCPower(1);                                  // DC Power ON
                    ControlACPower(1);                                  // AC Power ON
                    StepUpTimer(5000);
                    break;

                case 2320:
                    PrintLogData("    LED点灯確認 ");
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret == 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2302;                          // ｢LEDが点灯している｣(2302)
                        step_no = 2198;
                        NextStep();
                        break;
                    }
                    try_cnt = 20;                                       // ﾀｲﾑｵｰﾊﾞｰ　20s
                    StepUpTimer(500);
                    break;

                case 2321:
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2303;                          // ｢LEDが点灯しない｣(2303)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2322:
                    PrintLogData("    ｺﾏﾝﾄﾞﾘｾｯﾄ ");
                    rst_cnt = 5;
                    try_cnt = 10;
                    NextStep();
                    break;

                case 2323:
                    ret = ResetErrorCode_Normal("5B", out rst_ret_code);    // ｴﾗｰｺｰﾄﾞﾘｾｯﾄ(通常)
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2304;                          // ｢ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常｣(2304)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    NextStep();
                    break;

                case 2324:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x2800)
                    {
                        rst_cnt--;
                        if (rst_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2305;                              // ｢停止出力異常｣(2305)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        try_cnt = 10;
                        step_no -= 2;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2325:
                    PrintLogData("    ｼｰｹﾝｽNo : ");
                    ret = GetRamData_APP("0201", 1);                    // ｱﾄﾞﾚｽ(0201)ﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData(ram_dt);
                    if (ram_dt != "0B")
                    {
                        PrintLogData(" ...ＮＧ!\r\n");
                        error_code = 2306;                              // ｢ｼｰｹﾝｽNo異常｣(2306)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    ｴﾗｰｺｰﾄﾞ : ");
                    ret = GetRamData_APP("0255", 1);                    // ｱﾄﾞﾚｽ(0255)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData(ram_dt);
                    if (ram_dt != "00")
                    {
                        PrintLogData(" ...ＮＧ!\r\n");
                        error_code = 2307;                              // ｢ｴﾗｰがある｣(2307)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2326:
                    PrintLogData("    燃焼要求 ");
                    try_cnt = 10;
                    NextStep();
                    break;

                case 2327:
                    ret = RequestNensyou("5B");                         // 燃焼要求
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2308;                          // ｢燃焼要求応答異常｣(2308)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2328:
                    PrintLogData("    BLWｽｲｯﾁ ON\r\n");
                    ControlBlwSW(1);
                    PrintLogData("    ﾌﾟﾚﾊﾟｰｼﾞ開始待ち(S08) ");
                    SetFromC2(1);                                       // 燃焼開始信号 ON
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 5000;                              // timeover = 5s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2309;                          // ｢ﾌﾟﾚﾊﾟｰｼﾞ開始待ちﾀｲﾑｵｰﾊﾞｰ｣(2309)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("5B");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0x1600);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2329:
                    PrintLogData("    DS1_HH ON\r\n");
                    rdbDamperDS11.Checked = true;                       // DS1_HH ON
                    PrintLogData("    DS2_HH ON\r\n");
                    rdbDamperDS21.Checked = true;                       // DS2_HH ON
                    PrintLogData("    ｴｱｰｽｲｯﾁ ON\r\n");
                    ControlAirSW(1);                                    // ｴｱｰSW ON
                    PrintLogData("    ﾌﾟﾚｲｸﾞﾆｯｼｮﾝ遅延(S09) : ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 25000;                             // timeover = 25s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2310;                          // ｢ﾌﾟﾚｲｸﾞﾆｯｼｮﾝ遅延ﾀｲﾑｵｰﾊﾞｰ｣(2310)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("5B");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0x1700);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    pit_tm = txtStopWatch.Text;
                    PrintLogData(pit_tm + "[秒]");
                    if (16.0 > Double.Parse(pit_tm) || Double.Parse(pit_tm) > 24.0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2311;                              // ｢ﾌﾟﾚｲｸﾞﾆｯｼｮﾝ遅延ﾀｲﾑ異常｣(2311)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");

                    PrintLogData("    ﾌﾟﾚﾊﾟｰｼﾞ(S09+S10) : ");
                    tm1 = start_tm + 45000;                                  // timeover = 45s
                    //tm2 = tm2 + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2312;                          // ｢ﾌﾟﾚﾊﾟｰｼﾞﾀｲﾑｵｰﾊﾞｰ｣(2312)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("5B");                 // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0x2B00);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    pp_tm = txtStopWatch.Text;
                    save_dt[16] = pp_tm;                                // <V4.00> にて追加
                    PrintLogData(pp_tm + "[秒] ★16 ");
                    // ｢-- <V4.00>にて以下に変更 --------------------------------------------------
                    //if (30.0 > Double.Parse(pp_tm) || Double.Parse(pp_tm) > 40.0)
                    //{
                    //    PrintLogData("...ＮＧ!\r\n");
                    if (pp_tm_Bl > Double.Parse(pp_tm) || Double.Parse(pp_tm) > pp_tm_Bh)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + pp_tm_Bl.ToString("#0.0～")
                                                       + pp_tm_Bh.ToString("#0.0[S]") + " >\r\n");
                    // ---------------------------------------------------------------------------｣
                        error_code = 2313;                              // ｢ﾌﾟﾚﾊﾟｰｼﾞﾀｲﾑ異常｣(2313)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2330:
                    PrintLogData("    DS1_L ON\r\n");
                    rdbDamperDS14.Checked = true;                       // DS1_L ON
                    PrintLogData("    DS2_L ON\r\n");
                    rdbDamperDS24.Checked = true;                       // DS2_L ON
                    PrintLogData("    着火条件待ち(S12) ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 6000;                              // timeover = 6s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2314;                      // ｢着火条件待ちﾀｲﾑｵｰﾊﾞｰ｣(2314)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("5B");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0x2BC3);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2331:
                    PrintLogData("    AFD(125KΩ)\r\n");
                    rdbSensAFD1.Checked = true;
                    NextStep();
                    break;

                case 2332:
                    PrintLogData("    ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝ(S14) : ");
                    // ｢--- <V4.00> にて以下追加　---------------------------------------------
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 5000;                              // timeover = 5s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2315;                      // ｢ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑｵｰﾊﾞｰ｣(2315)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("5B");                 // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0xABC3);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    // -----------------------------------------------------------------------｣
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 15000;                             // timeover = 15s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2315;                      // ｢ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑｵｰﾊﾞｰ｣(2315)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("5B");                 // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0xAAC3);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    pi_tm = txtStopWatch.Text;
                    save_dt[17] = pi_tm;                                // <V4.00> にて追加
                    PrintLogData(pi_tm + "[秒] ★17 ");
                    // ｢-- <V4.00>にて以下に変更 --------------------------------------------------
                    //if (8.0 > Double.Parse(pi_tm) || Double.Parse(pi_tm) > 14.0)
                    //{
                    //    PrintLogData("...ＮＧ!\r\n");
                    if (pi_tm_Bl > Double.Parse(pi_tm) || Double.Parse(pi_tm) > pi_tm_Bh)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + pi_tm_Bl.ToString("#0.0～")
                                                       + pi_tm_Bh.ToString("#0.0[S]") + " >\r\n");
                    // ---------------------------------------------------------------------------｣
                        error_code = 2316;                          // ｢ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑ異常｣(2316)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2333:
                    PrintLogData("    AFD(0Ω)\r\n");
                    rdbSensAFD3.Checked = true;
                    PrintLogData("    着火確認(S15) ");
                    try_cnt = 5;
                    NextStep();
                    break;

                case 2334:
                    ret = RequestNensyou("5B");                         // 燃焼要求
                    try_cnt--;
                    if (try_cnt > 0)
                    {
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                    grpOutput.Refresh();
                    if (ret == 0xAAC3)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2317;                          // ｢着火確認異常｣(2317)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2335:
                    PrintLogData("    AFD(Open)\r\n");
                    //rdbSensAFD0.Checked = true;
                    SetSensor(2, 0);                                    // AFD Open
                    // <V4.10>にて下記に修正
                    //NextStep();
                    StepUpTimer(200);
                    break;

                case 2336:
                    PrintLogData("    ﾌﾚｰﾑﾚｽﾎﾟﾝｽ : ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 3000;                              // timeout 3s
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.00");
                        txtStopWatch.Refresh();
                        if (end_tm > tm1)
                        {
                            error_code = 2318;                      // ｢ﾌﾚｰﾑﾚｽﾎﾟﾝｽﾀｲﾑｵｰﾊﾞｰ｣(2318)
                            break;
                        }
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while ((ret & 0x003f) != 0x0000);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    fr_tm = txtStopWatch.Text;
                    // <V4.00>にて 13 -> 18
                    //save_dt[13] = fr_tm;
                    //PrintLogData(fr_tm + "[秒] ★13 ");
                    save_dt[18] = fr_tm;
                    PrintLogData(fr_tm + "[秒] ★18 ");
                    // ｢-- <V4.00>にて以下に変更 --------------------------------------------------
                    //if (1.0 > Double.Parse(fr_tm) || Double.Parse(fr_tm) > 3.0)
                    //{
                    //    PrintLogData("...ＮＧ!\r\n");
                    if (fr_tm_Bl > Double.Parse(fr_tm) || Double.Parse(fr_tm) > fr_tm_Bh)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + fr_tm_Bl.ToString("#0.0～")
                                                       + fr_tm_Bh.ToString("#0.0[S]") + " >\r\n");
                    // ---------------------------------------------------------------------------｣
                        error_code = 2319;                          // ｢ﾌﾚｰﾑﾚｽﾎﾟﾝｽﾀｲﾑ異常｣(2319)
                        step_no = 2398;                                 // ｴﾗｰｺｰﾄﾞ表示
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(500);
                    break;

                case 2337:
                    PrintLogData("    出力確認 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    grpOutput.Refresh();
                    if ((ret & 0x4000) != 0x4000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2320;                          // ｢警報がONしていない｣(2320)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2338:
                    rdbSensAFD0.Checked = true;
                    SetFromC2(0);                                       // 燃焼開始信号 OFF
                    PrintLogData("    BLWｽｲｯﾁ OFF\r\n");                // BLWSW OFF
                    ControlBlwSW(0);
                    PrintLogData("    ｴｱｰｽｲｯﾁ OFF\r\n");
                    ControlAirSW(0);                                    // ｴｱｰSW OFF
                    PrintLogData("    ｶﾞｽ圧ｽｲｯﾁ OFF\r\n");
                    ControlGusSW(0);                                    // ｶﾞｽSW OFF
                    NextStep();
                    break;

                case 2339:
                    PrintLogData("    電源 OFF\r\n");
                    ControlDCPower(0);                                  // DC Power OFF   
                    ControlACPower(0);                                  // AC Power OFF
                    pnlGo.Enabled = true;
                    StepUpTimer(2000);
                    break;

                case 2340:
                    PrintLogData("    電源 ON\r\n");
                    ControlDCPower(1);                                  // DC Power ON
                    ControlACPower(1);                                  // AC Power ON
                    StepUpTimer(5000);
                    break;

                case 2341:
                    PrintLogData("    LED点灯確認 ");
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret == 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2321;                              // ｢LEDが点灯している｣(2321)
                        step_no = 2198;
                        NextStep();
                        break;
                    }
                    try_cnt = 20;                                       // ﾀｲﾑｵｰﾊﾞｰ　20s
                    StepUpTimer(500);
                    break;

                case 2342:
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2322;                          // ｢LEDが点灯しない｣(2322)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2343:
                    PrintLogData("    ｺﾏﾝﾄﾞﾘｾｯﾄ ");
                    try_cnt = 10;
                    NextStep();
                    break;

                case 2344:
                    ret = ResetErrorCode_Normal("5B", out rst_ret_code);    // ｴﾗｰｺｰﾄﾞﾘｾｯﾄ(通常)
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2323;                          // ｢ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常｣(2323)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 2351;
                    NextStep();
                    break;

                // ---- ｼｰｹﾝｽ(53) -----------------------------------------

                case 2352:
                    PrintLogData("   -[ｼｰｹﾝｽ<3>]-\r\n");
                    PrintLogData("    ｼｰｹﾝｽ設定(53) ");
                    ret = SetSequence_APP("53");                       // 燃焼ｼｰｹﾝｽ設定(53)
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2353:
                    PrintLogData("    書込完了確認 ");
                    try_cnt = 20;
                    NextStep();
                    break;

                case 2354:
                    ret = GetRamData_APP("0570", 1);                    // ｱﾄﾞﾚｽ(0570)のﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x1F)
                    {
                        try_cnt--;
                        if (try_cnt > 0)
                        {
                            step_no--;
                            StepUpTimer(1000);
                            break;
                        }
                        else
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2351;                          // ｢書込が終了ない｣(2351)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(500);
                    break;

                case 2355:
                    PrintLogData("    電源 OFF\r\n");
                    ControlDCPower(0);                                  // DC Power OFF   
                    ControlACPower(0);                                  // AC Power OFF   
                    swin_fg = 0;                                        // SW入力許可
                    pnlGo.Enabled = true;
                    StepUpTimer(500);
                    break;

                case 2356:
                    DisplayDipSw("ON", "OFF", "OFF");
                    lblInfo1.Text = "JP1=ON / JP2=OFF / JP22=OFF にをｾｯﾄしてから、";
                    lblInfo2.Text = "｢Go｣sw を押して下さい！";
                    test_status = 5;
                    NextStep();
                    break;

                case 2357:
                    if (test_status != 2)
                    {
                        PlaySound("C:\\RCC300\\FC\\WAITsound.wav");     // Wait Buzzer
                        step_no--;
                        StepUpTimer(2000);
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 2358:
                    PlaySound("C:\\RCC300\\FC\\PIsound.wav");           // PI Buzzer
                    swin_fg = 1;                                        // SW入力禁止
                    StepUpTimer(500);
                    break;

                case 2359:
                    PrintLogData("    燃料種(ﾊｰﾈｽ) 開放\r\n");
                    rdbType1.Checked = true;                            // 燃料種　ｶﾞｽ
                    PrintLogData("    着火ﾄﾗｲｱﾙ時間(ﾊｰﾈｽ) 短絡\r\n");
                    rdbIGtm2.Checked = true;                            // 着火ﾄﾗｲｱﾙ時間 10s
                    PrintLogData("    点火方式(ﾊｰﾈｽ) 開放\r\n");
                    rdbMethod1.Checked = true;                          // 点火方式　ﾊﾟｲﾛｯﾄ
                    PrintLogData("    ｶﾞｽ圧ｽｲｯﾁ ON\r\n");
                    ControlGusSW(1);                                    // ｶﾞｽSW ON
                    PrintLogData("    DS1_L ON\r\n");
                    rdbDamperDS14.Checked = true;                       // DS1_L ON
                    StepUpTimer(500);
                    break;

                case 2360:
                    PrintLogData("    電源 ON\r\n");
                    ControlDCPower(1);                                  // DC Power ON
                    ControlACPower(1);                                  // AC Power ON
                    StepUpTimer(5000);
                    break;

                case 2361:
                    PrintLogData("    LED点灯確認 ");
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret == 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2352;                              // ｢LEDが点灯している｣(2352)
                        step_no = 2198;
                        NextStep();
                        break;
                    }
                    try_cnt = 20;                                       // ﾀｲﾑｵｰﾊﾞｰ　20s
                    StepUpTimer(500);
                    break;

                case 2362:
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2353;                          // ｢LEDが点灯しない｣(2353)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2363:
                    PrintLogData("    ｺﾏﾝﾄﾞﾘｾｯﾄ ");
                    rst_cnt = 5;
                    try_cnt = 10;
                    NextStep();
                    break;

                case 2364:
                    ret = ResetErrorCode_Normal("53", out rst_ret_code);    // ｴﾗｰｺｰﾄﾞﾘｾｯﾄ(通常)
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2354;                          // ｢ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常｣(2354)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    StepUpTimer(1000);
                    break;

                case 2365:
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if (ret != 0x2800)
                    {
                        rst_cnt--;
                        if (rst_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2355;                              // ｢停止出力異常｣(2355)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        try_cnt = 10;
                        step_no -= 2;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2366:
                    PrintLogData("    ｼｰｹﾝｽNo : ");
                    ret = GetRamData_APP("0201", 1);                    // ｱﾄﾞﾚｽ(0201)ﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData(ram_dt);
                    if (ram_dt != "03")
                    {
                        PrintLogData(" ...ＮＧ!\r\n");
                        error_code = 2356;                              // ｢ｼｰｹﾝｽNo異常｣(2356)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    PrintLogData("    ｴﾗｰｺｰﾄﾞ : ");
                    ret = GetRamData_APP("0255", 1);                    // ｱﾄﾞﾚｽ(0255)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData(ram_dt);
                    if (ram_dt != "00")
                    {
                        PrintLogData(" ...ＮＧ!\r\n");
                        error_code = 2357;                              // ｢ｴﾗｰがある｣(2357)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2367:
                    PrintLogData("    燃焼要求 ");
                    try_cnt = 10;
                    NextStep();
                    break;

                case 2368:
                    ret = RequestNensyou("53");                         // 燃焼要求
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2358;                          // ｢燃焼要求応答異常｣(2358)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2369:
                    PrintLogData("    BLWｽｲｯﾁ ON\r\n");
                    ControlBlwSW(1);
                    PrintLogData("    ﾌﾟﾚﾊﾟｰｼﾞ開始待ち(S08) ");
                    SetFromC2(1);                                       // 燃焼開始信号 ON
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 5000;                              // timeover = 5s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2359;                          // ｢ﾌﾟﾚﾊﾟｰｼﾞ開始待ちﾀｲﾑｵｰﾊﾞｰ｣(2359)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("53");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0x2600);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2370:
                    PrintLogData("    DS1_M ON\r\n");
                    rdbDamperDS13.Checked = true;                       // DS1_M ON
                    StepUpTimer(200);
                    break;

                case 2371:
                    PrintLogData("    DS1_H ON\r\n");
                    rdbDamperDS12.Checked = true;                       // DS1_H ON
                    StepUpTimer(200);
                    break;

                case 2372:
                    PrintLogData("    DS1_HH ON\r\n");
                    rdbDamperDS11.Checked = true;                       // DS1_HH ON
                    PrintLogData("    ｴｱｰｽｲｯﾁ ON\r\n");
                    ControlAirSW(1);                                    // ｴｱｰSW ON
                    PrintLogData("    ﾌﾟﾚﾊﾟｰｼﾞ(S09) : ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 45000;                             // timeover = 45s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2360;                          // ｢ﾌﾟﾚﾊﾟｰｼﾞﾀｲﾑｵｰﾊﾞｰ｣(2360)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("53");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0x2A00);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    pp_tm = txtStopWatch.Text;
                    save_dt[12] = pp_tm;                                // <V4.00> にて追加
                    PrintLogData(pp_tm + "[秒] ★12 ");
                    // ｢-- <V4.00>にて以下に変更 --------------------------------------------------
                    //if (30.0 > Double.Parse(pp_tm) || Double.Parse(pp_tm) > 40.0)
                    //{
                    //    PrintLogData("...ＮＧ!\r\n");
                    if (pp_tm_3l > Double.Parse(pp_tm) || Double.Parse(pp_tm) > pp_tm_3h)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + pp_tm_3l.ToString("#0.0～")
                                                       + pp_tm_3h.ToString("#0.0[S]") + " >\r\n");
                    // ---------------------------------------------------------------------------｣
                        error_code = 2361;                              // ｢ﾌﾟﾚﾊﾟｰｼﾞﾀｲﾑ異常｣(2361)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2373:
                    PrintLogData("    DS1_H ON\r\n");
                    rdbDamperDS12.Checked = true;                       // DS1_H ON
                    StepUpTimer(200);
                    break;

                case 2374:
                    PrintLogData("    DS1_M ON\r\n");
                    rdbDamperDS13.Checked = true;                       // DS1_M ON
                    StepUpTimer(200);
                    break;

                case 2375:
                    PrintLogData("    DS1_L ON\r\n");
                    rdbDamperDS14.Checked = true;                       // DS1_L ON
                    PrintLogData("    着火条件待ち(S12) ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 8000;                              // timeover = 8s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2362;                      // ｢着火条件待ちﾀｲﾑｵｰﾊﾞｰ｣(2362)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("53");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0x2B03);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2376:
                    PrintLogData("    ﾌﾚｰﾑﾛｯﾄﾞ(110MΩ)\r\n");
                    rdbSensFL1.Checked = true;
                    NextStep();
                    break;

                case 2377:
                    PrintLogData("    ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝ(S14) : ");
                    // ｢--- <V4.00> にて以下追加 ----------------------------------------------
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 5000;                              // timeover = 5s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2363;                      // ｢ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑｵｰﾊﾞｰ｣(2363)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("53");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0xAB03);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    // -----------------------------------------------------------------------｣
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 8000;                              // timeover = 8s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2363;                      // ｢ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑｵｰﾊﾞｰ｣(2363)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("53");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0xAA03);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    pi_tm = txtStopWatch.Text;
                    save_dt[13] = pi_tm;                                // <V4.00> にて追加
                    PrintLogData(pi_tm + "[秒] ★13 ");
                    // ｢-- <V4.00>にて以下に変更 --------------------------------------------------
                    //if (4.0 > Double.Parse(pi_tm) || Double.Parse(pi_tm) > 7.0)
                    //{
                    //    PrintLogData("...ＮＧ!\r\n");
                    if (pi_tm_3l > Double.Parse(pi_tm) || Double.Parse(pi_tm) > pi_tm_3h)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + pi_tm_3l.ToString("#0.0～")
                                                       + pi_tm_3h.ToString("#0.0[S]") + " >\r\n");
                    // ---------------------------------------------------------------------------｣
                        error_code = 2364;                          // ｢ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑ異常｣(2364)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2378:
                    PrintLogData("    着火確認(S15) : ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 8000;                          // timeover = 8s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2365;                      // ｢着火確認ﾀｲﾑｵｰﾊﾞｰ｣(2365)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("53");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0xAA0F);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    tc_tm = txtStopWatch.Text;
                    PrintLogData(tc_tm + "[秒]");
                    if (2.0 > Double.Parse(tc_tm) || Double.Parse(tc_tm) > 6.0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2366;                              // ｢着火確認ﾀｲﾑ異常｣(2366)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2379:
                    PrintLogData("    ﾒｲﾝﾄﾗｲｱﾙ(S16) : ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 10000;                             // timeover = 10s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2367;                          // ｢ﾒｲﾝﾄﾗｲｱﾙﾀｲﾑｵｰﾊﾞｰ｣(2367)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("53");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0xAA0C);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    mt_tm = txtStopWatch.Text;
                    save_dt[14] = mt_tm;                                // <V4.00> にて追加
                    PrintLogData(mt_tm + "[秒] ★14 ");
                    // ｢-- <V4.00>にて以下に変更 --------------------------------------------------
                    //if (5.0 > Double.Parse(mt_tm) || Double.Parse(mt_tm) > 9.0)
                    //{
                    //    PrintLogData("...ＮＧ!\r\n");
                    if (mt_tm_3l > Double.Parse(mt_tm) || Double.Parse(mt_tm) > mt_tm_3h)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + mt_tm_3l.ToString("#0.0～")
                                                       + mt_tm_3h.ToString("#0.0[S]") + " >\r\n");
                    // ---------------------------------------------------------------------------｣
                        error_code = 2368;                          // ｢ﾒｲﾝﾄﾗｲｱﾙﾀｲﾑ異常｣(2368)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2380:
                    PrintLogData("    ﾒｲﾝ安定(S17) : ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 5000;                              // timeover = 5s
                    tm2 = start_tm + 1000;
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2369;                          // ｢ﾒｲﾝ安定ﾀｲﾑｵｰﾊﾞｰ｣(2369)
                            break;
                        }
                        if (end_tm > tm2)
                        {
                            tm2 += 1000;
                            ret = RequestNensyou("53");                         // 燃焼要求
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.0");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while (ret != 0xA60C);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    ma_tm = txtStopWatch.Text;
                    PrintLogData(ma_tm + "[秒]");
                    if (2.0 > Double.Parse(ma_tm) || Double.Parse(ma_tm) > 4.0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2370;                          // ｢ﾒｲﾝ安定ﾀｲﾑ異常｣(2370)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2381:
                    PrintLogData("    ﾌﾚｰﾑﾛｯﾄﾞ(28.7MΩ)\r\n");
                    rdbSensFL3.Checked = true;
                    PrintLogData("    燃焼要求 ");
                    ret = RequestNensyou("53");                         // 燃焼要求
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(500);
                    break;

                case 2382:
                    PrintLogData("    ﾌﾚｰﾑﾛｯﾄﾞ(Open)\r\n");
                    //rdbSensFL0.Checked = true;
                    SetSensor(1, 0);                                    // ﾌﾚｰﾑﾛｯﾄﾞ　Open
                    // <V4.10>にて下記に修正
                    //NextStep();
                    StepUpTimer(200);
                    break;

                case 2383:
                    PrintLogData("    ﾌﾚｰﾑﾚｽﾎﾟﾝｽ : ");
                    start_tm = Environment.TickCount & Int32.MaxValue;  // start time
                    tm1 = start_tm + 3000;                              // timeout 3s
                    do
                    {
                        end_tm = Environment.TickCount & Int32.MaxValue;
                        if (end_tm > tm1)
                        {
                            error_code = 2371;                          // ｢ﾌﾚｰﾑﾚｽﾎﾟﾝｽﾀｲﾑｵｰﾊﾞｰ｣(2371)
                            break;
                        }
                        txtStopWatch.Text = ((end_tm - start_tm) / 1000).ToString("#0.00");
                        txtStopWatch.Refresh();
                        ret = GetOutputData();                          // 出力ﾃﾞｰﾀ取得
                        grpOutput.Refresh();
                    } while ((ret & 0x003f) != 0x0000);
                    if (error_code != 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    fr_tm = txtStopWatch.Text;
                    // <V4.00>にて 12 -> 15
                    //save_dt[12] = fr_tm;
                    //PrintLogData(fr_tm + "[秒] ★12 ");
                    save_dt[15] = fr_tm;
                    PrintLogData(fr_tm + "[秒] ★15 ");
                    // ｢-- <V4.00>にて以下に変更 --------------------------------------------------
                    //if (1.0 > Double.Parse(fr_tm) || Double.Parse(fr_tm) > 3.0)
                    //{
                    //    PrintLogData("...ＮＧ!\r\n");
                    if (fr_tm_3l > Double.Parse(fr_tm) || Double.Parse(fr_tm) > fr_tm_3h)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        PrintLogData("          < ○:" + fr_tm_3l.ToString("#0.0～")
                                                       + fr_tm_3h.ToString("#0.0[S]") + " >\r\n");
                    // ---------------------------------------------------------------------------｣
                        error_code = 2372;                              // ｢ﾌﾚｰﾑﾚｽﾎﾟﾝｽﾀｲﾑ異常｣(2372)
                        step_no = 2398;                                 // ｴﾗｰｺｰﾄﾞ表示
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(500);
                    break;

                case 2384:
                    PrintLogData("    出力確認 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    grpOutput.Refresh();
                    if ((ret & 0x4000) != 0x4000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2373;                          // ｢警報がONしていない｣(2373)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(1000);
                    break;

                case 2385:
                    rdbSensFL0.Checked = true;
                    SetFromC2(0);                                       // 燃焼開始信号 OFF
                    PrintLogData("    BLWｽｲｯﾁ OFF\r\n");                // BLWSW OFF
                    ControlBlwSW(0);
                    PrintLogData("    ｴｱｰｽｲｯﾁ OFF\r\n");
                    ControlAirSW(0);                                    // ｴｱｰSW OFF
                    PrintLogData("    ｶﾞｽ圧ｽｲｯﾁ OFF\r\n");
                    ControlGusSW(0);                                    // ｶﾞｽSW OFF
                    PrintLogData("    ﾎﾟｽﾄﾊﾟｰｼﾞ待ち\r\n");
                    try_cnt = 15;
                    StepUpTimer(500);
                    break;

                case 2386:
                    ret = StopNensyou("53");                         // 燃焼停止
                    try_cnt--;
                    if (try_cnt > 0)
                    {
                        step_no--;
                    }
                    StepUpTimer(1000);
                    break;

                case 2387:
                    PrintLogData("    ｺﾏﾝﾄﾞﾘｾｯﾄ ");
                    try_cnt = 10;
                    NextStep();
                    break;

                case 2388:
                    ret = ResetErrorCode_Normal("53", out rst_ret_code);    // ｴﾗｰｺｰﾄﾞﾘｾｯﾄ(通常)
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2374;                          // ｢ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常｣(2374)
                            step_no = 2398;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2389:
                    PrintLogData("    出力確認 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    grpOutput.Refresh();
                    if ((ret & 0x4000) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2375;                              // ｢ﾘｾｯﾄされない｣(2375)
                        step_no = 2398;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(1000);
                    break;

                case 2390:
                    step_no = 2398;
                    StepUpTimer(500);
                    break;

                case 2399:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 24.CPU間通信確認 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 2400:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 2499;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.28 CPU間通信 -----------------------------

                    PrintLogData(" 24.CPU間通信確認\r\n");
                    NextStep();
                    break;

                case 2401:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 2498;
                        NextStep();
                    }
                    PrintLogData("    内部診断① ");
                    ret = GetRamAdrData_BCR("0E2A");                    // ｱﾄﾞﾚｽ(3626)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2401;                              // ｢0000でない｣(2401)
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2402:
                    PrintLogData("    内部診断② ");
                    ret = GetRamAdrData_BCR("0E2B");                    // ｱﾄﾞﾚｽ(3627)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2401;                              // ｢0000でない｣(2401)
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2403:
                    PrintLogData("    内部診断③ ");
                    ret = GetRamAdrData_BCR("0E2C");                    // ｱﾄﾞﾚｽ(3628)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2401;                              // ｢0000でない｣(2401)
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2404:
                    PrintLogData("    内部診断④ ");
                    ret = GetRamAdrData_BCR("0E2D");                    // ｱﾄﾞﾚｽ(3629)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2401;                              // ｢0000でない｣(2401)
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2405:
                    PrintLogData("    内部診断⑤ ");
                    ret = GetRamAdrData_BCR("0E2E");                    // ｱﾄﾞﾚｽ(3630)のﾃﾞｰﾀ取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2401;                              // ｢0000でない｣(2401)
                        step_no = 2498;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2406:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 2498;
                    NextStep();
                    break;

                case 2499:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 25.EEPROM初期化 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 2500:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 2599;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.31 EEPROM初期化 --------------------------

                    PrintLogData(" 25.EEPROM初期化\r\n");
                    //InitialSerialPortLoader();      // ?????? ﾛｰﾀﾞｰ通信ﾎﾟｰﾄ 初期化,ｵｰﾌﾟﾝ 
                    step_no = 2503;                                     // <V2.00>にて追加
                    NextStep();
                    break;

                // <v2.00>にて削除
                //case 2501:
                //    PrintLogData("    燃焼ｼｰｹﾝｽ設定<ｱﾌﾟﾘCPU>\r\n");
                //    PrintLogData("    ｼｰｹﾝｽ設定(51) ");
                //    ret = SetSequence_APP("51");                       // 燃焼ｼｰｹﾝｽ設定(51)
                //    if (ret < 0)
                //    {
                //        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                //        error_code = 52;
                //        step_no = 2598;
                //        NextStep();
                //        break;
                //    }
                //    PrintLogData("...ＯＫ!\r\n");
                //    NextStep();
                //    break;

                //case 2502:
                //    PrintLogData("    書込完了確認 ");
                //    try_cnt = 20;
                //    NextStep();
                //    break;

                //case 2503:
                //    ret = GetRamData_APP("0570", 1);                    // ｱﾄﾞﾚｽ(0570)のﾃﾞｰﾀ取得 
                //    if (ret < 0)
                //    {
                //        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                //        error_code = 52;
                //        step_no = 2598;
                //        NextStep();
                //        break;
                //    }
                //    if (Convert.ToInt32(ram_dt, 16) != 0x1F)
                //    {
                //        try_cnt--;
                //        if (try_cnt > 0)
                //        {
                //            step_no--;
                //            StepUpTimer(1000);
                //            break;
                //        }
                //        else
                //        {
                //            PrintLogData("...ＮＧ!\r\n");
                //            error_code = 2501;                          // ｢書込完了しない<ｼｰｹﾝｽ設定>｣(2501)
                //            step_no = 2498;
                //            NextStep();
                //            break;
                //        }
                //    }
                //    PrintLogData("...ＯＫ!\r\n");
                //    NextStep();
                //    break;

                case 2504:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 2598;
                        NextStep();
                    }
                    NextStep();
                    break;

                case 2505:
                    PrintLogData("    設定ﾓｰﾄﾞ開始<BC-Rｺｱ> ");
                    ret = StartSettingMode_BCR();                       // 設定ﾓｰﾄﾞ開始
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2598;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2506:
                    PrintLogData("    EEPROM初期化<BC-Rｺｱ> ");
                    ret = InitEEprom_BCR();                             // EEPROM初期化
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2598;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2507:
                    PrintLogData("    設定ﾓｰﾄﾞ終了<BC-Rｺｱ> ");
                    ret = StopSettingMode_BCR();                       // 設定ﾓｰﾄﾞ終了
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2598;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2508:
                    PrintLogData("    書込開始確認<BC-Rｺｱ> ");
                    try_cnt = 10;
                    NextStep();
                    break;

                case 2509:
                    ret = GetEEpromWriteEnd_BCR();                      // 書込完了情報を取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2598;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0001 ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0001)
                    {
                        try_cnt--;
                        if (try_cnt > 0)
                        {
                            step_no--;
                            StepUpTimer(500);
                            break;
                        }
                        else
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2502;                          // ｢書込開始しない<BC-Rｺｱ>｣(2502)
                            step_no = 2598;
                            NextStep();
                            break;
                        }
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(500);
                    break;

                case 2510:
                    PrintLogData("    書込完了確認<BC-Rｺｱ> ");
                    try_cnt = 20;
                    NextStep();
                    break;

                case 2511:
                    ret = GetEEpromWriteEnd_BCR();                      // 書込完了情報を取得
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2598;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                        Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                    {
                        try_cnt--;
                        if (try_cnt > 0)
                        {
                            step_no--;
                            StepUpTimer(500);
                            break;
                        }
                        else
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2503;                          // ｢書込完了しない<BC-Rｺｱ>｣(2503)
                            step_no = 2598;
                            NextStep();
                            break;
                        }
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(500);
                    break;

                case 2512:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    step_no = 2598;
                    NextStep();
                    break;

                case 2599:
                    PrintTime();    // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 26.ｴﾗｰﾘｾｯﾄ ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 2600:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 2699;
                        NextStep();
                        break;
                    }

                    PrintLogData(" 26.ｴﾗｰﾘｾｯﾄ\r\n");
                    NextStep();
                    break;

                case 2601:
                    PrintLogData("    ﾘｾｯﾄ ");
                    try_cnt = 10;
                    NextStep();
                    break;

                case 2602:
                    ret = ResetErrorCode_Normal("53", out rst_ret_code);    // ｴﾗｰｺｰﾄﾞﾘｾｯﾄ(通常)
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2601;                      // ｢ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常｣(2601)
                            step_no = 2698;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(1000);
                    break;

                case 2603:
                    PrintLogData("    出力確認 ");
                    ret = GetOutputData();                              // 出力ﾃﾞｰﾀ取得
                    if ((ret & 0x4000) != 0x0000)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2602;                              // ｢警報がｸﾘｱできない｣(2602)
                        step_no = 2698;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 2698;
                    NextStep();
                    break;

                case 2699:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++ 27.出荷設定 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 2700:
                    pgbDoing.Value = (int)(step_no / TEST);
                    if (error_code != 0)
                    {
                        step_no = 2799;
                        NextStep();
                        break;
                    }

                    // ---- K43069 5.32 出荷設定 ------------------------------

                    PrintLogData(" 27.出荷設定\r\n");
                    NextStep();
                    break;

                case 2701:
                    PrintLogData("    電源 OFF\r\n");
                    ControlDCPower(0);                                  // DC Power OFF   
                    ControlACPower(0);                                  // AC Power OFF
                    swin_fg = 0;                                        // SW入力許可
                    StepUpTimer(500);
                    break;

                case 2702:
                    DisplayDipSw("ON", "OFF", "OFF");
                    lblInfo1.Text = "S800を全てOFF / (JP1,JP2,JP22はそのまま) にをｾｯﾄしてから、";
                    lblInfo2.Text = "｢Go｣sw を押して下さい！";
                    test_status = 5;
                    NextStep();
                    break;

                case 2703:
                    if (test_status != 2)
                    {
                        PlaySound("C:\\RCC300\\FC\\WAITsound.wav");     // Wait Buzzer
                        step_no--;
                        StepUpTimer(2000);
                        break;
                    }
                    StepUpTimer(500);
                    break;

                case 2704:
                    PlaySound("C:\\RCC300\\FC\\PIsound.wav");           // PIPO Buzzer
                    swin_fg = 1;                                        // SW入力禁止
                    StepUpTimer(500);
                    break;

                case 2705:
                    PrintLogData("    燃料種(ﾊｰﾈｽ) 開放\r\n");
                    rdbType1.Checked = true;                            // 燃料種　ｶﾞｽ
                    PrintLogData("    着火ﾄﾗｲｱﾙ時間(ﾊｰﾈｽ) 短絡\r\n");
                    rdbIGtm2.Checked = true;                            // 着火ﾄﾗｲｱﾙ時間 10s
                    PrintLogData("    点火方式(ﾊｰﾈｽ) 開放\r\n");
                    rdbMethod1.Checked = true;                          // 点火方式　ﾊﾟｲﾛｯﾄ
                    PrintLogData("    ｶﾞｽ圧ｽｲｯﾁ ON\r\n");
                    ControlGusSW(1);                                    // ｶﾞｽSW ON
                    PrintLogData("    DS1_L ON\r\n");
                    rdbDamperDS14.Checked = true;                       // DS1_L ON
                    StepUpTimer(500);
                    break;

                case 2706:
                    PrintLogData("    電源 ON\r\n");
                    ControlDCPower(1);                                  // DC Power ON
                    ControlACPower(1);                                  // AC Power ON
                    StepUpTimer(5000);
                    break;

                case 2707:
                    PrintLogData("    LED点灯確認 ");
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret == 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2701;                              // ｢LEDが点灯している｣(2701)
                        step_no = 2798;
                        NextStep();
                        break;
                    }
                    try_cnt = 20;                                       // ﾀｲﾑｵｰﾊﾞｰ　20s
                    StepUpTimer(500);
                    break;

                case 2708:
                    ret = CheckLedON();                                 // LED点灯確認
                    if (ret != 0)
                    {
                        try_cnt--;
                        if (try_cnt < 0)
                        {
                            PrintLogData("...ＮＧ!\r\n");
                            error_code = 2702;                          // ｢LEDが点灯しない｣(2702)
                            step_no = 2798;
                            NextStep();
                            break;
                        }
                        step_no--;
                        StepUpTimer(1000);
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    StepUpTimer(3000);
                    break;

                case 2709:
                    ChangeCPU("A", "B");                                // CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)
                    if (error_code != 0)
                    {
                        step_no = 2798;
                    }
                    NextStep();
                    break;

                case 2710:
                    PrintLogData("    内部ﾃﾞｰﾀ確認<BC-Rｺｱ> ");
                    ret = CheckEEpromData_BCR();                        // EEPROMﾃﾞｰﾀﾁｪｯｸ
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 2703;                              // ｢内部ﾃﾞｰﾀ異常｣(2703)
                        step_no = 2798;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2711:
                    PrintLogData("    内部ﾃﾞｰﾀ(ｾﾞﾛ)確認<BC-Rｺｱ> ");
                    ret = CheckEEpromZero_BCR();                        // EEPROMﾃﾞｰﾀｾﾞﾛﾁｪｯｸ
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 2704;                              // ｢内部ﾃﾞｰﾀ<ｾﾞﾛ>異常｣(2704)
                        step_no = 2798;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2712:
                    ChangeCPU("B", "A");                                // CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)
                    if (error_code != 0)
                    {
                        step_no = 2798;
                    }
                    NextStep();
                    break;

                case 2713:
                    PrintLogData("    内部ﾃﾞｰﾀ(ｼｰｹﾝｽNo)確認<ｱﾌﾟﾘCPU> ");
                    ret = GetRamData_APP("0201", 1);                    // ｱﾄﾞﾚｽ(0201)のﾃﾞｰﾀ取得 
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                        error_code = 52;
                        step_no = 2798;
                        NextStep();
                        break;
                    }
                    if (Convert.ToInt32(ram_dt, 16) != 0x03)
                    {
                        PrintLogData(" ...ＮＧ!\r\n");
                        error_code = 2705;                       // ｢内部ﾃﾞｰﾀ<ｼｰｹﾝｽNo>異常｣(2705)
                        step_no = 2798;
                        NextStep();
                        break;
                    }
                    PrintLogData(" ...ＯＫ!\r\n");
                    NextStep();
                    break;
                
                case 2714:
                    PrintLogData("    内部ﾃﾞｰﾀ(ｼﾘｱﾙNo)確認<ｱﾌﾟﾘCPU> ");
                    ret = CheckSno_APP();                               // ｼﾘｱﾙNo確認
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2706;                          // ｢内部ﾃﾞｰﾀ<ｼﾘｱﾙNo>異常｣(2706)
                        step_no = 2798;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    NextStep();
                    break;

                case 2715:
                    PrintLogData("    内部ﾃﾞｰﾀ(ﾃﾞｰﾄｺｰﾄﾞ)確認<ｱﾌﾟﾘCPU> ");
                    ret = CheckDcode_APP();                               // ﾃﾞｰﾄｺｰﾄﾞ確認
                    if (ret < 0)
                    {
                        PrintLogData("...ＮＧ!\r\n");
                        error_code = 2707;                      // ｢内部ﾃﾞｰﾀ<ﾃﾞｰﾄｺｰﾄﾞ>異常｣(2707)
                        step_no = 2798;
                        NextStep();
                        break;
                    }
                    PrintLogData("...ＯＫ!\r\n");
                    step_no = 2798;
                    NextStep();
                    break;

                case 2799:
                    PrintTime();    // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

 
                // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ 
                
                case 2800:
                    pgbDoing.Value = (int)(step_no / TEST);
                    step_no = 2999;
                    NextStep();
                    break;

                // end
                case 3000:
                    PrintLogData("    試験終了\r\n");
                    ControlDCPower(0);                                  // DC Power OFF           
                    ControlACPower(0);                                  // AC Power OFF           
                    SelectDMMch(0);                                     // DMM[open]選択
                    step_no = 3098;
                    StepUpTimer(500);
                    break;

                case 3099:
                    PrintTime();                                        // #### ﾃﾞﾊﾞｯｸ用 ####
                    NextStep();
                    break;

                // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                case 3100:
                    // 試験結果ﾛｸﾞﾃﾞｰﾀ書込 <V2.00>にてNGの場合も保存するように変更。
                    if (File.Exists("C:\\RCC300\\FC\\Log\\" + txtKouban.Text + ".txt"))
                    {
                        // 追加
                        StreamWriter srInf6 = new StreamWriter("C:\\RCC300\\FC\\Log\\" +
                                              txtKouban.Text + ".txt", true,
                                              Encoding.GetEncoding("shift_jis"));
                        srInf6.WriteLine(txtDateCode.Text + "," +
                                            txtSerialNo.Text + "," +
                                            txtKouban.Text + "," +
                                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "," +
                                            toolSSL3.Text + "," +
                                            error_code.ToString() + "," +
                                            save_dt[1] + "," + save_dt[2] + "," +
                                            save_dt[3] + "," + save_dt[4] + "," +
                                            save_dt[5] + "," + save_dt[6] + "," +
                                            save_dt[7] + "," + save_dt[8] + "," +
                                            save_dt[9] + "," + save_dt[10] + "," +
                                            save_dt[11] + "," +
                                            save_dt[12] + "," + save_dt[13] + "," +
                                            save_dt[14] + "," + save_dt[15] + "," +
                                            save_dt[16] + "," + save_dt[17] + "," +
                                            save_dt[18] + "," + save_dt[19] + "," +
                                            save_dt[20] + "," + save_dt[21] + "," +
                                            save_dt[22] + "," + save_dt[23]);
                        srInf6.Close();
                    }
                    else
                    {
                        // 新規
                        StreamWriter srInf6 = new StreamWriter("C:\\RCC300\\FC\\Log\\" +
                                              txtKouban.Text + ".txt", false,
                                              Encoding.GetEncoding("shift_jis"));
                        // <V4.00> にてﾌﾟﾚﾊﾟｰｼﾞ､ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝ､ﾒｲﾝﾄﾗｲｱﾙを追加
                        srInf6.WriteLine("ﾃﾞｰﾄｺｰﾄﾞ" + "," +
                                            "ｼﾘｱﾙNo" + "," +
                                            "工  番" + "," +
                                            "試験日時" + "," +
                                            "検査者" + "," +
                                            "ｴﾗｰｺｰﾄﾞ" + "," +
                                            "ｱﾌﾟﾘCPU<ﾃｽﾄ-ﾊﾞｰｼﾞｮﾝ>" + "," +
                                            "ｱﾌﾟﾘCPU<ﾃｽﾄ-ﾁｪｯｸｻﾑ>" + "," +
                                            "ｱﾌﾟﾘCPU<製品-ﾊﾞｰｼﾞｮﾝ>" + "," +
                                            "ｱﾌﾟﾘCPU<製品-ﾁｪｯｸｻﾑ>" + "," +
                                            "BC-Rｺｱ<ﾒｲﾝ-ﾊﾞｰｼﾞｮﾝ>" + "," +
                                            "BC-Rｺｱ<ｻﾌﾞ-ﾊﾞｰｼﾞｮﾝ>" + "," +
                                            "BC-Rｺｱ<ﾒｲﾝ-ﾁｪｯｸｻﾑ>" + "," +
                                            "BC-Rｺｱ<ｻﾌﾞ-ﾁｪｯｸｻﾑ>" + "," +
                                            "VDD電圧[V]" + "," +
                                            "VCC電圧[V]" + "," +
                                            "Vref電圧[V]" + "," +
                                            "ﾌﾟﾚﾊﾟｰｼﾞ[S]<3>" + "," +
                                            "ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝ[S]<3>" + "," +
                                            "ﾒｲﾝﾄﾗｲｱﾙ[S]<3>" + "," +
                                            "ﾌﾚｰﾑﾚｽﾎﾟﾝｽ[S]<3>" + "," +
                                            "ﾌﾟﾚﾊﾟｰｼﾞ[S]<B>" + "," +
                                            "ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝ[S]<B>" + "," +
                                            "ﾌﾚｰﾑﾚｽﾎﾟﾝｽ[S]<B>" +
                                            "ﾌﾚｰﾑﾛｯﾄﾞ<28.7MΩ>[uA]" +
                                            "ﾌﾚｰﾑﾛｯﾄﾞ<ｵｰﾌﾟﾝ>[uA]" +
                                            "AFD<125KΩ>[uA]" +
                                            "AFD<ｵｰﾌﾟﾝ>[uA]" +
                                            "温度[℃]" +
                                            "\r\n" +
                                            txtDateCode.Text + "," +
                                            txtSerialNo.Text + "," +
                                            txtKouban.Text + "," +
                                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "," +
                                            toolSSL3.Text + "," +
                                            error_code.ToString() + "," +
                                            save_dt[1] + "," + save_dt[2] + "," +
                                            save_dt[3] + "," + save_dt[4] + "," +
                                            save_dt[5] + "," + save_dt[6] + "," +
                                            save_dt[7] + "," + save_dt[8] + "," +
                                            save_dt[9] + "," + save_dt[10] + "," +
                                            save_dt[11] + "," +
                                            save_dt[12] + "," + save_dt[13] + "," +
                                            save_dt[14] + "," + save_dt[15] + "," +
                                            save_dt[16] + "," + save_dt[17] + "," +
                                            save_dt[18] + "," + save_dt[19] + "," +
                                            save_dt[20] + "," + save_dt[21] + "," +
                                            save_dt[22] + "," + save_dt[23]);
                        srInf6.Close();
                    }

                    // NG
                    if (error_code > 0)
                    {
                        pnlSetData.Visible = false;
                        picJudge.Visible = true;
                        picJudge.Image = System.Drawing.Image.FromFile("C:\\RCC300\\FC\\NG.bmp");
                        txtErrorCode.Text = error_code.ToString();
                        // ｴﾗｰ内容表示
                        switch (error_code)
                        {
                            case 51:
                                txtErrorMessege.Text = "ﾏﾙﾁﾒｰﾀ異常"; break;
                            case 52:
                                txtErrorMessege.Text = "ﾛｰﾀﾞｰ通信異常"; break;
                            case 99:
                                txtErrorMessege.Text = "試験中断"; break;
                            case 1:
                                txtErrorMessege.Text = "ｺﾈｸﾀ実装異常"; break;
                            case 101:
                                txtErrorMessege.Text = "VDD電圧異常"; break;
                            case 102:
                                txtErrorMessege.Text = "VCC電圧異常"; break;
                            case 103:
                                txtErrorMessege.Text = "Vref電圧異常"; break;
                            case 201:
                                txtErrorMessege.Text = "BC-Rｺｱ<ﾒｲﾝ>書込異常"; break;
                            case 202:
                                txtErrorMessege.Text = "BC-Rｺｱ<ｻﾌﾞ>書込異常"; break;
                            case 301:
                                txtErrorMessege.Text = "ｱﾌﾟﾘCPU書込異常"; break;
                            case 302:
                                txtErrorMessege.Text = "ｱﾌﾟﾘCPUﾁｪｯｸｻﾑ異常"; break;
                            case 401:
                                txtErrorMessege.Text = "LEDが点灯している"; break;
                            case 402:
                                txtErrorMessege.Text = "LEDが点灯しない"; break;
                            case 501:
                                txtErrorMessege.Text = "ﾘｾｯﾄできない"; break;
                            case 701:
                                txtErrorMessege.Text = "ｱﾌﾟﾘCPU<Ver,ﾁｪｯｸｻﾑ>異常"; break;
                            case 702:
                                txtErrorMessege.Text = "BC-Rｺｱ<Ver,ﾁｪｯｸｻﾑ>異常"; break;
                            case 901:
                                txtErrorMessege.Text = "初期値異常"; break;
                            case 902:
                                txtErrorMessege.Text = "風圧ｽｲｯﾁ入力異常"; break;
                            case 903:
                                txtErrorMessege.Text = "ｶﾞｽ圧ｽｲｯﾁ入力異常"; break;
                            case 904:
                                txtErrorMessege.Text = "ﾌﾞﾛｱｻｰﾏﾙ入力異常"; break;
                            case 905:
                                txtErrorMessege.Text = "DS1<HH>入力異常"; break;
                            case 906:
                                txtErrorMessege.Text = "DS1<H>入力異常"; break;
                            case 907:
                                txtErrorMessege.Text = "DS1<M>入力異常"; break;
                            case 908:
                                txtErrorMessege.Text = "DS1<L>入力異常"; break;
                            case 909:
                                txtErrorMessege.Text = "DS2<HH>入力異常"; break;
                            case 910:
                                txtErrorMessege.Text = "DS2<H>入力異常"; break;
                            case 911:
                                txtErrorMessege.Text = "DS2<M>入力異常"; break;
                            case 912:
                                txtErrorMessege.Text = "DS2<L>入力異常"; break;
                            case 913:
                                txtErrorMessege.Text = "燃焼開始信号入力異常"; break;
                            case 914:
                                txtErrorMessege.Text = "F/B初期値異常"; break;
                            case 915:
                                txtErrorMessege.Text = "K1 F/B異常<BC-Rｺｱ>"; break;
                            case 916:
                                txtErrorMessege.Text = "K1 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 917:
                                txtErrorMessege.Text = "K2 F/B異常<BC-Rｺｱ>"; break;
                            case 918:
                                txtErrorMessege.Text = "K2 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 919:
                                txtErrorMessege.Text = "K1,K2 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 920:
                                txtErrorMessege.Text = "K1,K2,K3 F/B異常<BC-Rｺｱ>"; break;
                            case 921:
                                txtErrorMessege.Text = "K1,K2,K3 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 922:
                                txtErrorMessege.Text = "K1,K2,K4 F/B異常<BC-Rｺｱ>"; break;
                            case 923:
                                txtErrorMessege.Text = "K1,K2,K4 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 924:
                                txtErrorMessege.Text = "K1,K2,K5 F/B異常<BC-Rｺｱ>"; break;
                            case 925:
                                txtErrorMessege.Text = "K1,K2,K5 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 926:
                                txtErrorMessege.Text = "K1,K2,K4,K13 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 927:
                                txtErrorMessege.Text = "K1,K2,K14 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 928:
                                txtErrorMessege.Text = "K1,K2,K15 F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 929:
                                txtErrorMessege.Text = "全OFF F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 930:
                                txtErrorMessege.Text = "K6 ON F/B異常<BC-Rｺｱ>"; break;
                            case 931:
                                txtErrorMessege.Text = "K6 ON F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 932:
                                txtErrorMessege.Text = "K6 OFF F/B異常<ｱﾌﾟﾘCPU>"; break;
                            case 933:
                                txtErrorMessege.Text = "ﾊｰﾈｽ燃料種異常"; break;
                            case 934:
                                txtErrorMessege.Text = "ﾊｰﾈｽ点火方式異常"; break;
                            case 935:
                                txtErrorMessege.Text = "ﾊｰﾈｽ着火ﾄﾗｲｱﾙ時間異常"; break;
                            case 936:
                                txtErrorMessege.Text = "JP1 OFF 異常"; break;
                            case 937:
                                txtErrorMessege.Text = "JP2 OFF 異常"; break;
                            case 938:
                                txtErrorMessege.Text = "JP22 OFF 異常"; break;
                            case 939:
                                txtErrorMessege.Text = "JP1 ON 異常"; break;
                            case 940:
                                txtErrorMessege.Text = "JP2 ON 異常"; break;
                            case 941:
                                txtErrorMessege.Text = "JP22 ON 異常"; break;
                            case 942:
                                txtErrorMessege.Text = "風圧ｽｲｯﾁ ﾋﾞｯﾄ異常"; break;
                            case 943:
                                txtErrorMessege.Text = "高燃焼ｲﾝﾀｰﾛｯｸ ﾋﾞｯﾄ異常"; break;
                            case 944:
                                txtErrorMessege.Text = "低燃焼ｲﾝﾀｰﾛｯｸ ﾋﾞｯﾄ異常"; break;
                            case 1001:
                                txtErrorMessege.Text = "初期値異常"; break;
                            case 1002:
                                txtErrorMessege.Text = "BLWsw異常"; break;
                            case 1003:
                                txtErrorMessege.Text = "SV1,SV2異常"; break;
                            case 1004:
                                txtErrorMessege.Text = "HV異常"; break;
                            case 1005:
                                txtErrorMessege.Text = "CV異常"; break;
                            case 1006:
                                txtErrorMessege.Text = "MDU12異常"; break;
                            case 1007:
                                txtErrorMessege.Text = "MDU11異常"; break;
                            case 1008:
                                txtErrorMessege.Text = "MDU22異常"; break;
                            case 1009:
                                txtErrorMessege.Text = "MDU21異常"; break;
                            case 1010:
                                txtErrorMessege.Text = "初期値異常"; break;
                            case 1011:
                                txtErrorMessege.Text = "K1<M:ON,S:OFF>異常"; break;
                            case 1012:
                                txtErrorMessege.Text = "K1<M:OFF,S:ON>異常"; break;
                            case 1013:
                                txtErrorMessege.Text = "K2<M:ON,S:OFF>異常"; break;
                            case 1014:
                                txtErrorMessege.Text = "K2<M:OFF,S:ON>異常"; break;
                            case 1015:
                                txtErrorMessege.Text = "K1<M:ON,S:ON>K2<M:ON,S:ON>異常"; break;
                            case 1016:
                                txtErrorMessege.Text = "K3<M:ON,S:OFF>異常"; break;
                            case 1017:
                                txtErrorMessege.Text = "K3<M:OFF,S:ON>異常"; break;
                            case 1018:
                                txtErrorMessege.Text = "K4<M:ON,S:OFF>異常"; break;
                            case 1019:
                                txtErrorMessege.Text = "K4<M:OFF,S:ON>異常"; break;
                            case 1020:
                                txtErrorMessege.Text = "K5<M:ON,S:OFF>異常"; break;
                            case 1021:
                                txtErrorMessege.Text = "K5<M:OFF,S:ON>異常"; break;
                            case 1022:
                                txtErrorMessege.Text = "K5<M:OFF,S:OFF>異常"; break;
                            case 1023:
                                txtErrorMessege.Text = "K3<M:ON,S:ON>異常"; break;
                            case 1024:
                                txtErrorMessege.Text = "K4<M:ON,S:ON>異常"; break;
                            case 1025:
                                txtErrorMessege.Text = "K5<M:ON,S:ON>異常"; break;
                            case 1026:
                                txtErrorMessege.Text = "K1,K2,K5,K10 全てOFF 異常"; break;
                            case 1027:
                                txtErrorMessege.Text = "R6 ﾘｾｯﾄ 異常"; break;
                            case 1028:
                                txtErrorMessege.Text = "R6S<M:ON,S:OFF>異常"; break;
                            case 1029:
                                txtErrorMessege.Text = "R6 ﾘｾｯﾄ 異常"; break;
                            case 1030:
                                txtErrorMessege.Text = "R6S<M:OFF,S:ON>異常"; break;
                            case 1031:
                                txtErrorMessege.Text = "R6R<M:ON,S:OFF>異常"; break;
                            case 1032:
                                txtErrorMessege.Text = "R6R<M:OFF,S:ON>異常"; break;
                            case 1033:
                                txtErrorMessege.Text = "R6R<M:ON,S:ON>異常"; break;
                            case 1101:
                                txtErrorMessege.Text = "遮断弁閉,ｲﾝﾀｰﾛｯｸのﾋﾞｯﾄ異常<1>"; break;
                            case 1102:
                                txtErrorMessege.Text = "RSTsw,DISPsw,起動,ﾘｾｯﾄのﾋﾞｯﾄ異常<1>"; break;
                            case 1103:
                                txtErrorMessege.Text = "遮断弁閉,ｲﾝﾀｰﾛｯｸのﾋﾞｯﾄ異常<0>"; break;
                            case 1104:
                                txtErrorMessege.Text = "RSTsw,DISPsw,起動,ﾘｾｯﾄのﾋﾞｯﾄ異常<0>"; break;
                            case 1105:
                                txtErrorMessege.Text = "商用電源ﾊﾟﾙｽ異常"; break;
                            case 1106:
                                txtErrorMessege.Text = "BLM,DmpH/L,Dmp比例,PV,MVのﾋﾞｯﾄ異常<1>"; break;
                            case 1107:
                                txtErrorMessege.Text = "BLM,DmpH/L,Dmp比例,PV,MVのﾋﾞｯﾄ異常<0>"; break;
                            case 1201:
                                txtErrorMessege.Text = "ﾌﾚｰﾑﾛｯﾄﾞ 28.7MΩのﾌﾚｰﾑ電圧異常"; break;
                            case 1202:
                                txtErrorMessege.Text = "ﾌﾚｰﾑﾛｯﾄﾞ ｵｰﾌﾟﾝの電圧異常"; break;
                            case 1203:
                                txtErrorMessege.Text = "AFD 125KΩのﾌﾚｰﾑ電圧異常"; break;
                            case 1204:
                                txtErrorMessege.Text = "AFD ｵｰﾌﾟﾝの電圧異常"; break;
                            case 1301:
                                txtErrorMessege.Text = "温度異常"; break;
                            case 1401:
                                txtErrorMessege.Text = "ﾌﾚｰﾑﾛｯﾄﾞ 着火入力異常"; break;
                            case 1402:
                                txtErrorMessege.Text = "ﾌﾚｰﾑﾛｯﾄﾞ 消炎入力異常"; break;
                            case 1403:
                                txtErrorMessege.Text = "AFD 着火入力異常"; break;
                            case 1404:
                                txtErrorMessege.Text = "AFD 消炎入力異常"; break;
                            case 1501:
                                txtErrorMessege.Text = "LEDが点灯しない"; break;
                            case 1502:
                                txtErrorMessege.Text = "LEDが消灯しない"; break;
                            case 1601:
                                txtErrorMessege.Text = "通信異常<BC-Rｺｱ>"; break;
                            case 1602:
                                txtErrorMessege.Text = "通信異常<ｱﾌﾟﾘCPU>"; break;
                            case 1701:
                                txtErrorMessege.Text = "火炎信号<ON>異常"; break;
                            case 1702:
                                txtErrorMessege.Text = "火炎信号<OFF>異常"; break;
                            case 1703:
                                txtErrorMessege.Text = "警報<OFF>異常"; break;
                            case 1704:
                                txtErrorMessege.Text = "警報<ON>異常"; break;
                            case 1705:
                                txtErrorMessege.Text = "警報<OFF>異常"; break;
                            case 1801:
                                txtErrorMessege.Text = "ｸﾛｯｸ値異常<Sub→Main>"; break;
                            case 1802:
                                txtErrorMessege.Text = "ｸﾛｯｸ値異常<Main→Sub>"; break;
                            case 1901:
                                txtErrorMessege.Text = "AC電源<OFF>異常"; break;
                            case 1902:
                                txtErrorMessege.Text = "AC電源<ON>異常"; break;
                            case 2001:
                                txtErrorMessege.Text = "書込異常<ｱﾌﾟﾘCPU>"; break;
                            case 2002:
                                txtErrorMessege.Text = "ﾁｪｯｸｻﾑ異常<ｱﾌﾟﾘCPU>"; break;
                            case 2101:
                                txtErrorMessege.Text = "LEDが点灯している"; break;
                            case 2102:
                                txtErrorMessege.Text = "LEDが点灯しない"; break;
                            case 2201:
                                txtErrorMessege.Text = "Ver,ﾁｪｯｸｻﾑ,基板ﾀｲﾌﾟ異常"; break;

                            case 2300:
                                txtErrorMessege.Text = "ｼﾞｪﾈﾗﾙﾘｾｯﾄ異常"; break;
                            case 2301:
                                txtErrorMessege.Text = "書込が終了ない"; break;
                            case 2302:
                                txtErrorMessege.Text = "LEDが点灯している"; break;
                            case 2303:
                                txtErrorMessege.Text = "LEDが点灯しない"; break;
                            case 2304:
                                txtErrorMessege.Text = "ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常"; break;
                            case 2305:
                                txtErrorMessege.Text = "停止出力異常"; break;
                            case 2306:
                                txtErrorMessege.Text = "ｼｰｹﾝｽNo異常"; break;
                            case 2307:
                                txtErrorMessege.Text = "ｴﾗｰがある"; break;
                            case 2308:
                                txtErrorMessege.Text = "燃焼要求応答異常"; break;
                            case 2309:
                                txtErrorMessege.Text = "ﾌﾟﾚﾊﾟｰｼﾞ開始待ちﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2310:
                                txtErrorMessege.Text = "ﾌﾟﾚｲｸﾞﾆｯｼｮﾝ遅延ﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2311:
                                txtErrorMessege.Text = "ﾌﾟﾚｲｸﾞﾆｯｼｮﾝ遅延ﾀｲﾑ異常"; break;
                            case 2312:
                                txtErrorMessege.Text = "ﾌﾟﾚﾊﾟｰｼﾞﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2313:
                                txtErrorMessege.Text = "ﾌﾟﾚﾊﾟｰｼﾞﾀｲﾑ異常"; break;
                            case 2314:
                                txtErrorMessege.Text = "着火条件待ちﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2315:
                                txtErrorMessege.Text = "ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2316:
                                txtErrorMessege.Text = "ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑ異常"; break;
                            case 2317:
                                txtErrorMessege.Text = "着火確認異常"; break;
                            case 2318:
                                txtErrorMessege.Text = "ﾌﾚｰﾑﾚｽﾎﾟﾝｽﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2319:
                                txtErrorMessege.Text = "ﾌﾚｰﾑﾚｽﾎﾟﾝｽﾀｲﾑ異常"; break;
                            case 2320:
                                txtErrorMessege.Text = "警報がONしていない"; break;
                            case 2321:
                                txtErrorMessege.Text = "LEDが点灯している"; break;
                            case 2322:
                                txtErrorMessege.Text = "LEDが点灯しない"; break;
                            case 2323:
                                txtErrorMessege.Text = "ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常"; break;

                            case 2351:
                                txtErrorMessege.Text = "書込が終了ない"; break;
                            case 2352:
                                txtErrorMessege.Text = "LEDが点灯している"; break;
                            case 2353:
                                txtErrorMessege.Text = "LEDが点灯しない"; break;
                            case 2354:
                                txtErrorMessege.Text = "ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常"; break;
                            case 2355:
                                txtErrorMessege.Text = "停止出力異常"; break;
                            case 2356:
                                txtErrorMessege.Text = "ｼｰｹﾝｽNo異常"; break;
                            case 2357:
                                txtErrorMessege.Text = "ｴﾗｰがある"; break;
                            case 2358:
                                txtErrorMessege.Text = "燃焼要求応答異常"; break;
                            case 2359:
                                txtErrorMessege.Text = "ﾌﾟﾚﾊﾟｰｼﾞ開始待ちﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2360:
                                txtErrorMessege.Text = "ﾌﾟﾚﾊﾟｰｼﾞﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2361:
                                txtErrorMessege.Text = "ﾌﾟﾚﾊﾟｰｼﾞﾀｲﾑ異常"; break;
                            case 2362:
                                txtErrorMessege.Text = "着火条件待ちﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2363:
                                txtErrorMessege.Text = "ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2364:
                                txtErrorMessege.Text = "ﾎﾟｽﾄｲｸﾞﾆｯｼｮﾝﾀｲﾑ異常"; break;
                            case 2365:
                                txtErrorMessege.Text = "着火確認ﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2366:
                                txtErrorMessege.Text = "着火確認ﾀｲﾑ異常"; break;
                            case 2367:
                                txtErrorMessege.Text = "ﾒｲﾝﾄﾗｲｱﾙﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2368:
                                txtErrorMessege.Text = "ﾒｲﾝﾄﾗｲｱﾙﾀｲﾑ異常"; break;
                            case 2369:
                                txtErrorMessege.Text = "ﾒｲﾝ安定ﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2370:
                                txtErrorMessege.Text = "ﾒｲﾝ安定ﾀｲﾑ異常"; break;
                            case 2371:
                                txtErrorMessege.Text = "ﾌﾚｰﾑﾚｽﾎﾟﾝｽﾀｲﾑｵｰﾊﾞｰ"; break;
                            case 2372:
                                txtErrorMessege.Text = "ﾌﾚｰﾑﾚｽﾎﾟﾝｽﾀｲﾑ異常"; break;
                            case 2373:
                                txtErrorMessege.Text = "警報がONしていない"; break;
                            case 2374:
                                txtErrorMessege.Text = "ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常"; break;
                            case 2375:
                                txtErrorMessege.Text = "ﾘｾｯﾄされない"; break;

                            case 2401:
                                txtErrorMessege.Text = "0000でない"; break;
                            case 2501:
                                txtErrorMessege.Text = "書込完了しない<ｼｰｹﾝｽ設定>"; break;
                            case 2502:
                                txtErrorMessege.Text = "書込開始しない<BC-Rｺｱ>"; break;
                            case 2503:
                                txtErrorMessege.Text = "書込完了しない<BC-Rｺｱ>"; break;
                            case 2601:
                                txtErrorMessege.Text = "ﾘｾｯﾄｺﾏﾝﾄﾞ応答異常"; break;
                            case 2602:
                                txtErrorMessege.Text = "警報がｸﾘｱできない"; break;
                            case 2701:
                                txtErrorMessege.Text = "LEDが点灯している"; break;
                            case 2702:
                                txtErrorMessege.Text = "LEDが点灯しない"; break;
                            case 2703:
                                txtErrorMessege.Text = "内部ﾃﾞｰﾀ異常"; break;
                            case 2704:
                                txtErrorMessege.Text = "内部ﾃﾞｰﾀ<ｾﾞﾛ>異常"; break;
                            case 2705:
                                txtErrorMessege.Text = "内部ﾃﾞｰﾀ<ｼｰｹﾝｽNo>異常"; break;
                            case 2706:
                                txtErrorMessege.Text = "内部ﾃﾞｰﾀ<ｼﾘｱﾙNo>異常"; break;
                            case 2707:
                                txtErrorMessege.Text = "内部ﾃﾞｰﾀ<ﾃﾞｰﾄｺｰﾄﾞ>異常"; break;
                        }
                        PrintLogData("    xxxx NG品 xxxx   ｴﾗｰｺｰﾄﾞ[" + txtErrorCode.Text + "]\r\n");
                        PrintLogData("  ........................ "
                                                + toolSSL5.Text + " " + toolSSL6.Text + "\r\n");
                        txtErrorCode.Show();
                        txtErrorMessege.Show();
                        System.Media.SystemSounds.Exclamation.Play();   // NG Buzzer
                        step_no = 3102;
                        NextStep();
                        break;
                    }

                    // OK
                    pnlSetData.Visible = false;
                    picJudge.Visible = true;
                    picJudge.Image = System.Drawing.Image.FromFile("C:\\RCC300\\FC\\OK.bmp");
                    PrintLogData("    oooo 良品 oooo\r\n");
                    PrintLogData("  ........................ "
                                            + toolSSL5.Text + " " + toolSSL6.Text + "\r\n");
                    txtErrorCode.Hide();
                    txtErrorMessege.Hide();
                    System.Media.SystemSounds.Asterisk.Play();          // OK Buzzer

                    // 成績書印刷用ﾛｸﾞﾃﾞｰﾀ書込
                    if (File.Exists(path_log + txtKouban.Text + ".txt"))
                    {
                        StreamWriter srInf7 = new StreamWriter(path_log +
                                              txtKouban.Text + ".txt", true,
                                              Encoding.GetEncoding("shift_jis"));
                        srInf7.WriteLine(txtDateCode.Text + "," +
                                            txtSerialNo.Text + "," +
                                            txtKouban.Text + "," +
                                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                        srInf7.Close();
                    }
                    else
                    {
                        StreamWriter srInf7 = new StreamWriter(path_log +
                                              txtKouban.Text + ".txt", false,
                                              Encoding.GetEncoding("shift_jis"));
                        srInf7.WriteLine(txtDateCode.Text + "," +
                                            txtSerialNo.Text + "," +
                                            txtKouban.Text + "," +
                                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                        srInf7.Close();
                    }
                    NextStep();
                    break;

                case 3101:
                    ControlStamp(1);                                    // 合格印　ON
                    StepUpTimer(500);
                    break;

                case 3102:
                    ControlStamp(0);                                    // 合格印　OFF
                    StepUpTimer(100);
                    break;

                case 3103:
                    AllOffEXP_64S_BD1();                                // BD1 All OFF

                    if (serialPort1.IsOpen == true)                     // DMM通信ﾎﾟｰﾄ　ｸﾛｰｽﾞ
                    {
                        serialPort1.Close();
                    }
                    if (serialPort2.IsOpen == true)                     // ﾛｰﾀﾞｰ通信ﾎﾟｰﾄ　ｸﾛｰｽﾞ
                    {
                        serialPort2.Close();
                    }
                    if (serialPort3.IsOpen == true)                     // RS-485通信ﾎﾟｰﾄ　ｸﾛｰｽﾞ
                    {
                        serialPort3.Close();
                    }

                    if (test_mode != 0)
                    {
                        sw.Stop();
                    }

                    // ﾃｷｽﾄﾛｸﾞﾃﾞｰﾀ保存
                    if (chkTextLogSave.Checked == true)
                    {
                        StreamWriter srInf9 = new StreamWriter("C:\\RCC300\\FC\\TextLog\\" +
                                              txtItem.Text.Substring(0, 10) + "_" +
                                              txtSerialNo.Text + ".txt", true,
                                              Encoding.GetEncoding("shift_jis"));
                        srInf9.WriteLine(txtLog.Text);
                        srInf9.Close();
                    }

                    StepUpTimer(3000);
                    break;

                case 3104:
                    btnAbort.Visible = false;
                    btnCheck.Visible = false;
                    if (error_code == 0)
                    {
                        lblInfo1.Text = "結果を確認し、ｼｰﾙを貼ってから、";
                    }
                    else
                    {
                        lblInfo1.Text = "結果を確認してから、";
                    }
                    lblInfo2.Text = "｢Go｣sw を押して下さい！";
                    test_status = 3;
                    swin_fg = 0;                                        // SW入力許可
                    pnlGo.Enabled = true;
                    NextStep();
                    break;

                case 3105:
                    // 判定
                    if (error_code > 0)
                    {
                        PlaySound("C:\\RCC300\\FC\\NGsound.wav");       // NG Buzzer
                    }
                    else
                    {
                        PlaySound("C:\\RCC300\\FC\\OKsound.wav");       // OK Buzzer
                    }
                    step_no--;
                    StepUpTimer(3000);
                    break;

            }

        }



    // **** End Of Program ************************************************************************

    }
}
