using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VocsAutoTest.Tools
{
    public class ParamInfo
    {
        private string machId = string.Empty;

        public string MachId
        {
            get { return machId; }
            set { machId = value; }
        }
        private string instrId = string.Empty;

        public string InstrId
        {
            get { return instrId; }
            set { instrId = value; }
        }
        private string temp = string.Empty;

        public string Temp
        {
            get { return temp; }
            set { temp = value; }
        }
        private string press = string.Empty;

        public string Press
        {
            get { return press; }
            set { press = value; }
        }
        private string inFine = string.Empty;

        public string InFine
        {
            get { return inFine; }
            set { inFine = value; }
        }
        private string outFine = string.Empty;

        public string OutFine
        {
            get { return outFine; }
            set { outFine = value; }
        }
        private string roomId = string.Empty;

        public string RoomId
        {
            get { return roomId; }
            set { roomId = value; }
        }
        private string lightId = string.Empty;

        public string LightId
        {
            get { return lightId; }
            set { lightId = value; }
        }
        private string vol = string.Empty;

        public string Vol
        {
            get { return vol; }
            set { vol = value; }
        }
        private string avgTimes = string.Empty;

        public string AvgTimes
        {
            get { return avgTimes; }
            set { avgTimes = value; }
        }
        private string person = string.Empty;

        public string Person
        {
            get { return person; }
            set { person = value; }
        }
     

        //加载测量信息设定
        public void LoadParameterInfo(string fileName)
        {
            TextReader textReader = null;
            try
            {
                FileInfo file = new FileInfo(fileName);
                textReader = file.OpenText();
                string line = null;
                //read Info
                while ((line = textReader.ReadLine()) != null)
                {
                    string info = line.Trim();
                    if (info.StartsWith("整机ID:"))
                    {
                        string tmp = "整机ID:";
                        machId = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("光谱仪ID:"))
                    {
                        string tmp = "光谱仪ID:";
                        instrId = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("温度:"))
                    {
                        string tmp = "温度:";
                        temp = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("压力:"))
                    {
                        string tmp = "压力:";
                        press = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("输入光纤ID:"))
                    {
                        string tmp = "输入光纤ID:";
                        inFine = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("输出光纤ID:"))
                    {
                        string tmp = "输出光纤ID:";
                        outFine = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("气体室编号:"))
                    {
                        string tmp = "气体室编号:";
                        roomId = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("氙灯ID:"))
                    {
                        string tmp = "氙灯ID:";
                        lightId = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("电压:"))
                    {
                        string tmp = "电压:";
                        vol = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("光谱平均次数:"))
                    {
                        string tmp = "光谱平均次数:";
                        avgTimes = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("实验人员"))
                    {
                        string tmp = "实验人员:";
                        person = info.Substring(tmp.Length);
                    }
                }

            }
            catch (Exception e)
            {
                //throw new Exception(CustomResource.ImpFileErr + e.Message);
            }
            finally
            {
                if (textReader != null)
                    textReader.Close();
            }
        }

        //保存测量信息设定
        public void SaveParameterInfo(string fileName)
        {
            TextWriter textWriter = null;
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                textWriter = File.CreateText(fileName);
                StringBuilder sb = new StringBuilder();
                sb.Append("整机ID:").Append(this.machId).Append("\r\n");
                sb.Append("光谱仪ID:").Append(this.instrId).Append("\r\n");
                sb.Append("温度:").Append(this.temp).Append("\r\n");
                sb.Append("压力:").Append(this.press).Append("\r\n");
                sb.Append("输入光纤ID:").Append(this.inFine).Append("\r\n");
                sb.Append("输出光纤ID:").Append(this.outFine).Append("\r\n");
                sb.Append("气体室编号:").Append(this.roomId).Append("\r\n");
                sb.Append("氙灯ID:").Append(this.lightId).Append("\r\n");
                sb.Append("电压:").Append(this.vol).Append("\r\n");
                sb.Append("光谱平均次数:").Append(this.avgTimes).Append("\r\n");
                sb.Append("实验人员:").Append(this.person).Append("\r\n");
                textWriter.Write(sb.ToString());

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }



        public void Check()
        {
            if (string.IsNullOrEmpty(this.machId))
            {
                throw new Exception("没有找到整机ID");
            }
            if (string.IsNullOrEmpty(this.instrId))
            {
                throw new Exception("没有找到光谱仪ID");
            }

            float tmpTemp;//温度
            float tmpPress;//压力
            try
            {
                tmpTemp = System.Single.Parse(this.temp);
                tmpPress = System.Single.Parse(this.press);
            }
            catch (Exception)
            {
                throw new Exception("没有找到温度和压力");
            }


            if (string.IsNullOrEmpty(this.inFine))
            {
                throw new Exception("没有找到输入光纤ID");
            }
            if (string.IsNullOrEmpty(this.outFine))
            {
                throw new Exception("没有找到输出光纤ID");
            }
            if (string.IsNullOrEmpty(this.roomId))
            {
                throw new Exception("没有找到气体室编号");
            }
            if (string.IsNullOrEmpty(this.lightId))
            {
                throw new Exception("没有找到氙灯ID");
            }
            try
            {
                System.Single.Parse(this.vol);
            }
            catch (Exception)
            {
                throw new Exception("没有找到电压");
            }
            try
            {
                System.UInt32.Parse(this.avgTimes);
            }
            catch (Exception)
            {
                throw new Exception("没有找到光谱平均次数");
            }
            if (string.IsNullOrEmpty(this.person))
            {
                throw new Exception("没有找到实验人员");
            }

        }
    }
}
