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
using CGpCmDeclCs;

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++ 測定器関連(GP-IB) +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

namespace Project1
{
    public partial class Form1 : Form
    {

        // DMM
        public static int Ret;
        public static int DevDMM;

        // GP-IB
        CGpCmDecl GpCmDecl = new CGpCmDecl();

        // ****************************************************************************************
        //      名　称：InitDMM
        //      説　明：DMMを初期化する (Agilent 34401A)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:初期化ｴﾗｰ,-2:[*RST]ｺﾏﾝﾄﾞｴﾗｰ,-3:[*CLS]ｺﾏﾝﾄﾞｴﾗｰ)
        // ****************************************************************************************
        public int InitDMM()
        {
            DevDMM = GpCmDecl.Pkibdev(0, 22, 0, (int)CGpCmDeclConst.T10s, 1, 0); // ｱﾄﾞﾚｽ=22
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -1;                                              // 初期化 ｴﾗｰ！
            }

            // ﾏﾙﾁﾒｰﾀのﾘｾｯﾄ

            GpCmDecl.Pkibwrt(DevDMM, "*RST", 4);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -2;                                              // [*RST] ｺﾏﾝﾄﾞ ｴﾗｰ
            }

            GpCmDecl.Pkibwrt(DevDMM, "*CLS", 4);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -3;                                              // [*CLS] ｺﾏﾝﾄﾞ ｴﾗｰ
            }

            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetDCVolt_Stable   <V6.20>にて追加
        //      説　明：直流電圧値取得 (Agilent 34401A)   <注.安定した状態の電圧を取得する>
        //      引　数：to_data:ﾀｲﾑｱｳﾄ時間<秒>,sa_value:差,DCV:電圧値(参照)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾚﾝｼﾞﾌﾟﾘｾｯﾄｴﾗｰ,-2:ｺﾏﾝﾄﾞｴﾗｰ,-3:ﾃﾞｰﾀ受信ｴﾗｰ,-4:ﾀｲﾑｱｳﾄ)
        // ****************************************************************************************
        public int GetDCVolt_Stable(int to_data, double sa_value, ref double DCV)
        {
            StringBuilder rdbuf = new StringBuilder("", 100);
            string rvdata;
            int cnt_N = 5;
            int cnt_L = to_data * 2;
            double vn = 0.0;
            double vo = 0.0;
            double sa = 0;
            long ttm;
            long tm;

            GpCmDecl.Pkibwrt(DevDMM, ":CONF:VOLT:DC 100", 17);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -1;                                              // ﾚﾝｼﾞﾌﾟﾘｾｯﾄ ｴﾗｰ!
            }

            do
            {
                ttm = (Environment.TickCount & Int32.MaxValue) + 500;   // 500ms wait
                do
                {
                    tm = (Environment.TickCount & Int32.MaxValue);
                } while (tm < ttm);

                GpCmDecl.Pkibwrt(DevDMM, "READ?", 5);
                Ret = GpCmDecl.PkThreadIbsta();
                if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
                {
                    return -2;                                          // ｺﾏﾝﾄﾞ ｴﾗｰ!
                }

                GpCmDecl.Pkibrd(DevDMM, rdbuf, rdbuf.Capacity);
                Ret = GpCmDecl.PkThreadIbsta();
                if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
                {
                    return -3;                                          // ﾃﾞｰﾀ受信 ｴﾗｰ!
                }

                rvdata = rdbuf.ToString();
                vn = double.Parse(rvdata.Substring(0, 15));
                txtDMM.Text = vn.ToString("#0.000");
                txtDMM.Refresh();
                sa = vn - vo;
                vo = vn;
                if (sa > sa_value)
                {
                    cnt_N = 5;
                }
                else
                {
                    //PlaySound(@"C:\AUR\FC\PIsound.wav");   // Pi Buzzer
                    cnt_N--;
                    if (cnt_N < 0)
                    {
                        DCV = vn;
                        return 0;
                    }
                }
                cnt_L--;
            } while (cnt_L > 0);
            return -4;
        }


        // ****************************************************************************************
        //      名　称：GetDCVolt
        //      説　明：直流電圧を取得する (Agilent 34401A) *10回のｻﾝﾌﾟﾘﾝｸﾞ値の平均値
        //      引　数：DCV：直流電圧値<参照渡し>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾚﾝｼﾞﾌﾟﾘｾｯﾄｴﾗｰ,-2/-3:ｺﾏﾝﾄﾞｴﾗｰ)
        // ****************************************************************************************
        public int GetDCVolt(ref double DCV)
        {
            StringBuilder rdbuf = new StringBuilder("", 100);
            string rvdata;
            double v_bar;
            int cnt;

            GpCmDecl.Pkibwrt(DevDMM, ":CONF:VOLT:DC 100", 17);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -1;                                              // ﾚﾝｼﾞﾌﾟﾘｾｯﾄ ｴﾗｰ!
            }

            GpCmDecl.Pkibwrt(DevDMM, "READ?", 5);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -2;                                              // ｺﾏﾝﾄﾞ ｴﾗｰ!
            }

            GpCmDecl.Pkibrd(DevDMM, rdbuf, rdbuf.Capacity);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -3;                                              // ﾃﾞｰﾀ受信 ｴﾗｰ!
            }

            rvdata = rdbuf.ToString();
            v_bar = double.Parse(rvdata.Substring(0, 15));

            for (cnt = 0; cnt < 9; cnt++)
            {
                GpCmDecl.Pkibwrt(DevDMM, "READ?", 5);
                Ret = GpCmDecl.PkThreadIbsta();
                if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
                {
                    return -2;                                          // ｺﾏﾝﾄﾞ ｴﾗｰ!
                }

                GpCmDecl.Pkibrd(DevDMM, rdbuf, rdbuf.Capacity);
                Ret = GpCmDecl.PkThreadIbsta();
                if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
                {
                    return -3;                                          // ﾃﾞｰﾀ受信 ｴﾗｰ!
                }

                rvdata = rdbuf.ToString();
                v_bar += double.Parse(rvdata.Substring(0, 15));
            }

            DCV = v_bar / 10;

            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetACVolt
        //      説　明：交流電圧を取得する (Agilent 34401A) *10回のｻﾝﾌﾟﾘﾝｸﾞ値の平均値
        //      引　数：ACV：直流電圧値<参照渡し>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾚﾝｼﾞﾌﾟﾘｾｯﾄｴﾗｰ,-2/-3:ｺﾏﾝﾄﾞｴﾗｰ)
        // ****************************************************************************************
        public int GetACVolt(ref double ACV)
        {
            StringBuilder rdbuf = new StringBuilder("", 100);
            string rvdata;
            double v_bar;
            int cnt;

            GpCmDecl.Pkibwrt(DevDMM, ":CONF:VOLT:AC", 13);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -1;                                              // ﾚﾝｼﾞﾌﾟﾘｾｯﾄ ｴﾗｰ!
            }

            GpCmDecl.Pkibwrt(DevDMM, "READ?", 5);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -2;                                              // ｺﾏﾝﾄﾞ ｴﾗｰ!
            }

            GpCmDecl.Pkibrd(DevDMM, rdbuf, rdbuf.Capacity);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -3;                                              // ﾃﾞｰﾀ受信 ｴﾗｰ!
            }

            rvdata = rdbuf.ToString();
            v_bar = double.Parse(rvdata.Substring(0, 15));

            for (cnt = 0; cnt < 9; cnt++)
            {
                GpCmDecl.Pkibwrt(DevDMM, "READ?", 5);
                Ret = GpCmDecl.PkThreadIbsta();
                if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
                {
                    return -2;                                          // ｺﾏﾝﾄﾞ ｴﾗｰ!
                }

                GpCmDecl.Pkibrd(DevDMM, rdbuf, rdbuf.Capacity);
                Ret = GpCmDecl.PkThreadIbsta();
                if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
                {
                    return -3;                                          // ﾃﾞｰﾀ受信 ｴﾗｰ!
                }

                rvdata = rdbuf.ToString();
                v_bar = double.Parse(rvdata.Substring(0, 15));
            }

            ACV = v_bar / 10;

            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetDCCurrent
        //      説　明：直流電流を取得する (Agilent 34401A) *10回のｻﾝﾌﾟﾘﾝｸﾞ値の平均値
        //      引　数：DCA：直流電流値<参照渡し>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾚﾝｼﾞﾌﾟﾘｾｯﾄｴﾗｰ,-2/-3:ｺﾏﾝﾄﾞｴﾗｰ)
        // ****************************************************************************************
        public int GetDCCurrent(ref double DCA)
        {
            StringBuilder rdbuf = new StringBuilder("", 100);
            string rvdata;
            double a_bar;
            int cnt;

            GpCmDecl.Pkibwrt(DevDMM, ":CONF:CURR:DC", 17);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -1;                                              // ﾚﾝｼﾞﾌﾟﾘｾｯﾄ ｴﾗｰ!
            }

            GpCmDecl.Pkibwrt(DevDMM, "READ?", 5);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -2;                                              // ｺﾏﾝﾄﾞ ｴﾗｰ!
            }

            GpCmDecl.Pkibrd(DevDMM, rdbuf, rdbuf.Capacity);
            Ret = GpCmDecl.PkThreadIbsta();
            if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
            {
                return -3;                                              // ﾃﾞｰﾀ受信 ｴﾗｰ!
            }

            rvdata = rdbuf.ToString();
            a_bar = double.Parse(rvdata.Substring(0, 12));

            for (cnt = 0; cnt < 9; cnt++)
            {
                GpCmDecl.Pkibwrt(DevDMM, "READ?", 5);
                Ret = GpCmDecl.PkThreadIbsta();
                if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
                {
                    return -2;                                          // ｺﾏﾝﾄﾞ ｴﾗｰ!
                }

                GpCmDecl.Pkibrd(DevDMM, rdbuf, rdbuf.Capacity);
                Ret = GpCmDecl.PkThreadIbsta();
                if ((Ret & (int)CGpCmDeclConst.ERR) != 0)
                {
                    return -3;                                          // ﾃﾞｰﾀ受信 ｴﾗｰ!
                }

                rvdata = rdbuf.ToString();
                a_bar += double.Parse(rvdata.Substring(0, 12));
            }

            DCA = a_bar / 10;
            DCA = DCA * 1000;   // mAにする
            return 0;
        }

    }
}
