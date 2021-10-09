using HandySub.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HandySub.Common
{
    public class SrtHelper
    {
        //Define a list of ModelList to accept the content read from the file
        private static List<SrtModel> mySrtModelList;

        //Define a method to get the string displayed at the current time
        public static string GetTimeString(int timeMile)
        {
            String currentTimeTxt = "";
            if (mySrtModelList != null)
            {
                foreach (SrtModel sm in mySrtModelList)
                {
                    if (timeMile > sm.BeginTime && timeMile < sm.EndTime)
                    {
                        currentTimeTxt = sm.SrtString;
                    }
                }

            }
            return currentTimeTxt;
        }

        //Read the contents of the file to the mySrtModelList list
        public static List<SrtModel> ParseSrt(string srtPath)
        {
            mySrtModelList = new List<SrtModel>();
            string line;
            using (FileStream fs = new FileStream(srtPath, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                {
                    StringBuilder sb = new StringBuilder();
                    while ((line = sr.ReadLine()) != null)
                    {

                        if (!line.Equals(""))
                        {
                            sb.Append(line).Append("@");
                            continue;
                        }

                        string[] parseStrs = sb.ToString().Split('@');
                        if (parseStrs.Length < 3)
                        {
                            sb.Remove(0, sb.Length);// Clear, otherwise it will affect the analysis of the next subtitle element</i>  
                            continue;
                        }

                        SrtModel srt = new SrtModel();
                        string strToTime = parseStrs[1];
                        int beginHour = int.Parse(strToTime.Substring(0, 2));
                        int beginMintue = int.Parse(strToTime.Substring(3, 2));
                        int beginSecond = int.Parse(strToTime.Substring(6, 2));
                        int beginMSecond = int.Parse(strToTime.Substring(9, 3));
                        int beginTime = (beginHour * 3600 + beginMintue * 60 + beginSecond) * 1000 + beginMSecond;

                        int endHour = int.Parse(strToTime.Substring(17, 2));
                        int endMintue = int.Parse(strToTime.Substring(20, 2));
                        int endSecond = int.Parse(strToTime.Substring(23, 2));
                        int endMSecond = int.Parse(strToTime.Substring(26, 2));
                        int endTime = (endHour * 3600 + endMintue * 60 + endSecond) * 1000 + endMSecond;

                        srt.BeginHour = beginHour;
                        srt.BeginMintue = beginMintue;
                        srt.BeginSecond = beginSecond;
                        srt.BeginMSecond = beginMSecond;
                        srt.BeginTime = beginTime;

                        srt.EndHour = endHour;
                        srt.EndMintue = endMintue;
                        srt.EndSecond = endSecond;
                        srt.EndMSecond = endMSecond;
                        srt.EndTime = endTime;
                        string strBody = null;
                        for (int i = 2; i < parseStrs.Length; i++)
                        {
                            strBody += parseStrs[i];
                        }
                        srt.SrtString = strBody;
                        mySrtModelList.Add(srt);
                        sb.Remove(0, sb.Length);
                    }
                }
            }
            return mySrtModelList;
        }
    }
}
