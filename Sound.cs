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
// ++++++++ �T�E���h�֘A ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

namespace Project1
{
    public partial class Form1 : Form
    {

        // ****************************************************************************************
        //      ���@�́FPlaySound
        //      ���@���F�������ڲ����
        //      ���@���Fsound_name:����ޖ�
        //      �߂�l�F�Ȃ�
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
        //      ���@�́FStopSound
        //      ���@���F����ނ��~����
        //      ���@���F�Ȃ�
        //      �߂�l�F�Ȃ�
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
