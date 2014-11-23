﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Xml;
using Microsoft.VisualBasic;

namespace Lucid_Scribe_Data_Converter
{
  public partial class MainForm : Form
  {
    public MainForm()
    {
      InitializeComponent();
    }

    private void tmrLoad_Tick(object sender, EventArgs e)
    {
      tmrLoad.Enabled = false;
      ConvertToLSD();
      Application.Exit();
    }

    private void ConvertToLSD()
    {
      try
      {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Title = "Select file to convert";
        openFileDialog.Filter = "All files|*.*";

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
          SaveFileDialog saveFileDialog = new SaveFileDialog();
          saveFileDialog.Title = "Save converted file as";
          saveFileDialog.FileName = openFileDialog.SafeFileName.Replace(".txt", "");
          saveFileDialog.Filter = "Lucid Scribe Data|*.LSD";

          if (saveFileDialog.ShowDialog() == DialogResult.OK)
          {
            String csv = File.ReadAllText(openFileDialog.FileName);
            pgbSatus.Maximum = csv.Split('\n').Length;
            TextFieldParser parser = new TextFieldParser(new StringReader(csv));
            parser.SetDelimiters(" ");

            String lsd = "<LucidScribeData>";
            lsd += "\r\n  <Database Name=\"LSDBase\">";
            lsd += "\r\n    <Researcher Name=\"Nazrax\">";
            lsd += "\r\n      <Entry Name=\"" + openFileDialog.SafeFileName.Replace(".txt", "") + "\">";

            List<String> anthems = new List<String>();

            anthems.Add("Andrew Rayel feat Jano - How Do I Know");
            anthems.Add("Astral Projection - People Can Fly");
            anthems.Add("Delerium - Apparition");
            anthems.Add("Delerium feat. Sarah McLachlan - Silence");
            anthems.Add("Dj Shog - This Is My Sound");
            anthems.Add("DJ Tiesto - Adagio For Strings");
            anthems.Add("DT8 Project - Breathe");
            anthems.Add("Faithless - Long Way Home");
            anthems.Add("Faithless - Salva Mea");
            anthems.Add("Kai Tracid - 4 Just 1 Day");
            anthems.Add("Kai Tracid - Conscious");
            anthems.Add("Markus Schulz feat. Justine Suissa - Perception");
            anthems.Add("Oceanlab - Clear Blue Water");
            anthems.Add("Oceanlab - Sirens Of The Sea");
            anthems.Add("Orbital - Halcyon On On");
            anthems.Add("PPK - Resurrection");
            anthems.Add("Rank 1 - Airwave");
            anthems.Add("Rank 1 - Breathing");
            anthems.Add("Robert A. Monroe - Affirmation");
            anthems.Add("Robert A. Monroe - Aum");
            anthems.Add("Robert Miles - Children");
            anthems.Add("Yoda - Definitely");

            String currentHour = "-1";
            String currentMinute = "-1";

            String currentLeft = "";
            String currentREM = "";
            String currentRight = "";
            String currentClicks = "";

            String currentMinuteLeft = "";
            String currentMinuteREM = "";
            String currentMinuteRight = "";
            String currentMinuteClicks = "";

            String trimmedMinute = "";

            String comments = "";
            String lastClick = "000";

            int minuteLeft = 0;
            int minuteREM = 0;
            int minuteRight = 0;
            int minuteClicks = 0;

            double hourLeft = 0;
            double hourREM = 0;
            double hourRight = 0;
            double hourClicks = 0;
            int tick = 0;
            bool dreaming = false;

            String minute = "";

            List<int> historyLeft = new List<int>();
            List<int> historyRight = new List<int>();

            while (!parser.EndOfData)
            {
              string[] columns = parser.ReadFields();

              String timeInMinutes = columns[0];
              String somethingLegacy = columns[1];
              String clicks = columns[2];
              String readableTime = columns[3];
              String left = columns[4];
              String right = columns[5];

              String[] time = readableTime.Split(':');
              String hour = time[0];
              minute = time[1];
              String second = time[2].Split('.')[0];

              // Check if a new hour has started
              if (hour != currentHour)
              {
                // Close the previous hour
                if (currentHour != "-1")
                {
                  historyLeft = new List<int>();
                  historyRight = new List<int>();

                  trimmedMinute = currentMinute.TrimStart('0');
                  if (trimmedMinute == "") trimmedMinute = "0";

                  minuteREM = minuteREM / tick;
                  minuteLeft = minuteLeft / tick;
                  minuteRight = minuteRight / tick;
                  minuteClicks = minuteClicks / tick;

                  currentLeft += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteLeft + "\" >" + currentMinuteLeft + "</M>";
                  currentREM += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteREM + "\" >" + currentMinuteREM + "</M>";
                  currentRight += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteRight + "\" >" + currentMinuteRight + "</M>";
                  currentClicks += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteClicks + "\" >" + currentMinuteClicks + "</M>";

                  lsd += "\r\n          <Plugin Name=\"Left Eye\" Value=\"" + Convert.ToInt32(hourLeft / 60) + "\">" + currentLeft + "\r\n          </Plugin>";
                  lsd += "\r\n          <Plugin Name=\"Eye REM\" Value=\"" + Convert.ToInt32(hourREM / 60) + "\">" + currentREM + "\r\n          </Plugin>";
                  lsd += "\r\n          <Plugin Name=\"Right Eye\" Value=\"" + Convert.ToInt32(hourRight / 60) + "\">" + currentRight + "\r\n          </Plugin>";
                  lsd += "\r\n          <Plugin Name=\"Clicks\" Value=\"" + Convert.ToInt32(hourClicks / 60) + "\">" + currentClicks + "\r\n          </Plugin>";

                  lsd += "\r\n      </Plugins>";
                  if (comments != "")
                  {
                    lsd += "\r\n      <Comments>" + comments + "</Comments>";
                    comments = "";
                  }
                  lsd += "\r\n      </Hour>";
                }

                lsd += "\r\n      <Hour N=\"" + hour + "\">";
                lsd += "\r\n      <Plugins>";

                currentLeft = "";
                currentREM = "";
                currentRight = "";
                currentClicks = "";

                hourREM = 0;
                hourLeft = 0;
                hourRight = 0;
                hourClicks = 0;

                currentHour = hour;
              }

              if (minute != currentMinute)
              {
                if (currentMinute != "-1" & minute != "00")
                {
                  minuteREM = minuteREM / tick;
                  minuteLeft = minuteLeft / tick;
                  minuteRight = minuteRight / tick;
                  minuteClicks = minuteClicks / tick;

                  hourREM += minuteREM ;
                  hourLeft += minuteLeft ;
                  hourRight += minuteRight;
                  hourClicks += minuteClicks;

                  trimmedMinute = currentMinute.TrimStart('0');
                  if (trimmedMinute == "") trimmedMinute = "0";

                  currentLeft += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteLeft + "\" >" + currentMinuteLeft + "</M>";
                  currentREM += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteREM + "\" >" + currentMinuteREM + "</M>";
                  currentRight += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteRight + "\" >" + currentMinuteRight + "</M>";
                  currentClicks += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteClicks + "\" >" + currentMinuteClicks + "</M>";
                }

                currentMinuteREM = "";
                currentMinuteLeft = "";
                currentMinuteRight = "";
                currentMinuteClicks = "";

                minuteREM = 0;
                minuteLeft = 0;
                minuteRight = 0;
                minuteClicks = 0;

                currentMinute = minute;
                tick = 0;
              }

              int eyeMovements = 0;

              historyLeft.Add(Convert.ToInt32(left));
              if (historyLeft.Count > 500) { historyLeft.RemoveAt(0); }

              for (int i = 10; i < historyLeft.Count; i += 1)
              {
                // Check the last 10 ticks for a jump greater than fifty
                for (int x = i - 10; x < i; x++)
                {
                  if (historyLeft[x] - historyLeft[i] > 50)
                  {
                    eyeMovements = eyeMovements + 100;
                    i += 10;
                    break;
                  }
                }
              }

              historyRight.Add(Convert.ToInt32(right));
              if (historyRight.Count > 500) { historyRight.RemoveAt(0); }

              for (int i = 10; i < historyRight.Count; i += 1)
              {
                // Check the last 10 ticks for a jump greater than fifty
                for (int x = i - 10; x < i; x++)
                {
                  if (historyRight[x] - historyRight[i] > 50)
                  {
                    eyeMovements = eyeMovements + 100;
                    i += 10;
                    break;
                  }
                }
              }

              if (eyeMovements > 999)
              {
                eyeMovements = 888;
                if (!dreaming)
                {
                  dreaming = true;
                  //alert("Hello dream world");
                  comments += "<Comment Plugin=\"Eye REM\" Type=\"Anthem\" Minute=\"" + minute + "\" Second=\"" + second + "\" Tick=\"" + tick + "\">" + anthems[new Random().Next(anthems.Count-1)] + "</Comment>";
                }
              }
              else
              {
                dreaming = false;
              }

              minuteREM += eyeMovements;
              minuteLeft += Convert.ToInt32(left);
              minuteRight += Convert.ToInt32(right);
              minuteClicks += Convert.ToInt32(clicks);
              tick++;

              currentMinuteREM += eyeMovements + ",";
              currentMinuteLeft += left.TrimStart('0') + ",";
              currentMinuteRight += right.TrimStart('0') + ",";
              currentMinuteClicks += clicks + ",";

              if (lastClick != clicks)
              {
                comments += "<Comment Plugin=\"Clicks\" Type=\"Word\" Minute=\"" + minute + "\" Second=\"" + second + "\" Tick=\"" + tick + "\">Click " + clicks + "</Comment>";
                lastClick = clicks;
              }

              pgbSatus.Value++;
              Application.DoEvents();
            }

            trimmedMinute = minute.TrimStart('0');
            if (trimmedMinute == "") trimmedMinute = "0";

            minuteREM = minuteREM / tick;
            minuteLeft = minuteLeft / tick;
            minuteRight = minuteRight / tick;
            minuteClicks = minuteClicks / tick;

            currentREM += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteREM + "\" >" + currentMinuteREM + "</M>";
            currentLeft += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteLeft + "\" >" + currentMinuteLeft + "</M>";
            currentRight += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteRight + "\" >" + currentMinuteRight + "</M>";
            currentClicks += "\r\n            <M N=\"" + trimmedMinute + "\" V=\"" + minuteClicks + "\" >" + currentMinuteClicks + "</M>";

            lsd += "\r\n          <Plugin Name=\"Left Eye\" Value=\"" + Convert.ToInt32(hourLeft / 60) + "\">" + currentLeft + "\r\n          </Plugin>";
            lsd += "\r\n          <Plugin Name=\"Eye REM\" Value=\"" + Convert.ToInt32(hourREM / 60) + "\">" + currentREM + "\r\n          </Plugin>";
            lsd += "\r\n          <Plugin Name=\"Right Eye\" Value=\"" + Convert.ToInt32(hourRight / 60) + "\">" + currentRight + "\r\n          </Plugin>";
            lsd += "\r\n          <Plugin Name=\"Clicks\" Value=\"" + Convert.ToInt32(hourClicks / 60) + "\">" + currentClicks + "\r\n          </Plugin>";

            lsd += "\r\n          </Plugins>";
            if (comments != "")
            {
              lsd += "\r\n      <Comments>" + comments + "</Comments>";
            }
            lsd += "\r\n          </Hour>";
            lsd += "\r\n      </Entry>";
            lsd += "\r\n    </Researcher>";
            lsd += "\r\n  </Database>";
            lsd += "\r\n</LucidScribeData>";

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(lsd);
            xmlDocument.Save(saveFileDialog.FileName);
            Interaction.Shell("explorer \"" + saveFileDialog.FileName + "\"", AppWinStyle.NormalFocus, false, -1);
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
      }
    }

  }
}
