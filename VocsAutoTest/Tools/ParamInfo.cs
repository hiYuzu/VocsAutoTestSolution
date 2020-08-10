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

        public int LrType { get; set; }
        public int SensorType { get; set; }
        public int LightPath { get; set; }
        public int Pixel { get; set; }
        public string LightDistance { get; set; }
        public int GasChamberType { get; set; }


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
                    else if (info.StartsWith("光源类型"))
                    {
                        string tmp = "光源类型:";
                        string lrTpe = info.Substring(tmp.Length);
                        LrType = 0;
                        if ("氘灯".Equals(lrTpe)) {
                            LrType = 1;
                        }
                    }
                    else if (info.StartsWith("传感器类型"))
                    {
                        string tmp = "传感器类型:";
                        string sensorType = info.Substring(tmp.Length);
                        SensorType = 0;
                        if ("CCD".Equals(sensorType))
                        {
                            SensorType = 1;
                        }
                    }
                    else if (info.StartsWith("像素"))
                    {
                        string tmp = "像素:";
                        string pixel = info.Substring(tmp.Length);
                        switch(pixel.Trim())
                        {
                            case "256":
                                Pixel = 0;
                                break;
                            case "512":
                                Pixel = 1;
                                break;
                            case "1024":
                                Pixel = 2;
                                break;
                            case "2048":
                                Pixel = 3;
                                break;
                        }
                    }
                    else if (info.StartsWith("光路"))
                    {
                        string tmp = "光路:";
                        string lightPath = info.Substring(tmp.Length);
                        LightPath = 0;
                        if("向量文件仅适用于光路2".Equals(lightPath))
                        {
                            LightPath = 1;
                        }
                    }
                    else if (info.StartsWith("光程(mm)"))
                    {
                        string tmp = "光程(mm):";
                        LightDistance = info.Substring(tmp.Length);
                    }
                    else if (info.StartsWith("气体室类型"))
                    {
                        string tmp = "气体室类型:";
                        string gasChamberType = info.Substring(tmp.Length);
                        GasChamberType = 1;
                        if("折返式".Equals(gasChamberType))
                        {
                            GasChamberType = 0;
                        }
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
                sb.Append("光源类型:").Append(LrType).Append("\r\n");
                sb.Append("传感器类型:").Append(SensorType).Append("\r\n");
                sb.Append("像素:").Append(Pixel).Append("\r\n");
                sb.Append("光路:").Append(LightPath).Append("\r\n");
                sb.Append("光程(mm):").Append(LightDistance).Append("\r\n");
                sb.Append("气体室类型:").Append(GasChamberType).Append("\r\n");
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
