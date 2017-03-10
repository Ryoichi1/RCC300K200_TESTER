using System;
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
using System.Collections.Generic;
using System.Management;
using System.Media;


// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++ サウンド関連 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

namespace Project1
{
    public partial class Form1 : Form
    {

        // ****************************************************************************************
        //      名　称：PlaySound
        //      説　明：ｻｳﾝﾄﾞﾌﾟﾚｲする
        //      引　数：sound_name:ｻｳﾝﾄﾞ名
        //      戻り値：なし
        // ****************************************************************************************
        public void PlaySound(string sound_name)
        {
            if (player != null)
            {
                StopSound();
            }
            player = new System.Media.SoundPlayer(sound_name);
            player.Play();
        }


        // ****************************************************************************************
        //      名　称：StopSound
        //      説　明：ｻｳﾝﾄﾞを停止する
        //      引　数：なし
        //      戻り値：なし
        // ****************************************************************************************
        public void StopSound()
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }
        }

    }
}
