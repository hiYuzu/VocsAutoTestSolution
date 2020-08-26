using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocsAutoTest.Algorithm;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Tools
{
    public class SpecDataSave
    {
        private ArrayList dataList = new ArrayList();
        private ArrayList intervalDataList = new ArrayList();
        private ArrayList fbDataList = new ArrayList();
        private ArrayList fbintervalDataList = new ArrayList();
        private DateTime oldTime1 = DateTime.Now;
        private DateTime fbDate1 = DateTime.Now; 
        //当前读取的光谱数据时间
        public static DateTime timeInterval = DateTime.Now;
        public static DateTime startdate = DateTime.Now;
        private const int saveChangeFileTimes = 24;
        private readonly string saveName = "SpecDataFiles";
        private string specDataSavePath;
        private bool startSave = false;
        private bool isCreatFile = true;
        public int saveCount;
        public int intervalTime;
        public bool isIntervalSave = false;
        #region 单例
        private static readonly object _obj = new object();
        private static SpecDataSave instance = null;
        public static SpecDataSave Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (_obj)
                    {
                        instance = new SpecDataSave();
                    }
                }
                return instance;
            }
        }
        #endregion
        public string SpecDataSavePath
        {
            get
            {
                if (string.IsNullOrEmpty(specDataSavePath))
                {
                    string path = ConstConfig.GetValue(saveName);
                    if (string.IsNullOrEmpty(path))
                    {
                        path = ConstConfig.AppPath + @"\SpecDataFiles";
                    }
                    specDataSavePath = path;
                    if (!System.IO.Directory.Exists(specDataSavePath))
                    {
                        System.IO.Directory.CreateDirectory(specDataSavePath);
                    }
                }
                return specDataSavePath;
            }
            set
            {
                specDataSavePath = value;
                ConstConfig.SaveValue(saveName, specDataSavePath);
            }
        }
        public bool StartSave
        {
            get { return startSave; }
            set
            {
                startSave = value;
                if (!startSave)
                {
                    dataList.Clear();
                    intervalDataList.Clear();
                    fbDataList.Clear();
                    fbintervalDataList.Clear();
                }
            }
        }
        /// <summary>
        /// 单次保存
        /// </summary>
        /// <param name="data"></param>
        public void SaveCurrentData(string[] data)
        {
            if (data == null || data.Length < 1) return;
            ArrayList list = new ArrayList();
            list.Add(data);
            string path;
            if (SpecDataSavePath.EndsWith(@"\"))
            {
                path = SpecDataSavePath + "raw-Current" + @"\";
            }
            else
            {
                path = SpecDataSavePath + @"\" + "raw-Current" + @"\";
            }
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            FileControl.SaveRawFile(path, list);
            list.Clear();
        }
        public void SaveSpecDataContin(string[] data)
        {
            //无间隔保存
            if (!isIntervalSave)
            {
                if (fbDataList.Count == 0)
                {
                    fbDate1 = DateTime.Now;
                }
                fbDataList.Add(data);

                timeInterval = DateTime.Now;
                if (timeInterval > startdate.AddHours(saveChangeFileTimes))
                {
                    isCreatFile = true;
                    FileControl.SaveRawFile(specDataSavePath, fbDataList, fbDate1); ;
                    fbDataList.Clear();
                    startdate = startdate.AddHours(saveChangeFileTimes);
                }
                else
                {
                    if (fbDataList.Count >= saveCount)
                    {
                        string path;
                        if (specDataSavePath.EndsWith(@"\"))
                        {
                            path = specDataSavePath + startdate.ToString("yyyyMMddhhmmss");
                        }
                        else
                        {
                            path = specDataSavePath + @"\" + startdate.ToString("yyyyMMddhhmmss"); ;
                        }
                        if (!System.IO.Directory.Exists(path) && isCreatFile == true)
                        {
                            System.IO.Directory.CreateDirectory(path);
                            isCreatFile = false;
                        }
                        FileControl.SaveRawFile(path, fbDataList, fbDate1);
                        fbDataList.Clear();
                    }
                }
            }
            //间隔保存
            else
            {
                DateTime dt = DateTime.Now;
                long past = dt.Ticks - oldTime1.Ticks;
                past = (long)(past / 10000);
                if (past >= intervalTime)
                {
                    oldTime1 = dt;
                    if (fbintervalDataList.Count == 0)
                    {
                        fbDate1 = DateTime.Now;
                    }
                    fbintervalDataList.Add(data);
                    timeInterval = timeInterval.AddSeconds(intervalTime / 1000);
                }
                if (timeInterval > startdate.AddHours(saveChangeFileTimes))
                {
                    isCreatFile = true;
                    FileControl.SaveRawFile(specDataSavePath, fbintervalDataList, fbDate1);
                    fbintervalDataList.Clear();
                    startdate = startdate.AddHours(saveChangeFileTimes);
                }
                else
                {
                    if (fbintervalDataList.Count >= saveCount)
                    {
                        string path;
                        if (specDataSavePath.EndsWith(@"\"))
                        {
                            path = specDataSavePath + startdate.ToString("yyyyMMddhhmmss");
                        }
                        else
                        {
                            path = specDataSavePath + @"\" + startdate.ToString("yyyyMMddhhmmss"); ;
                        }
                        if (!System.IO.Directory.Exists(path) && isCreatFile == true)
                        {
                            System.IO.Directory.CreateDirectory(path);
                            isCreatFile = false;
                        }
                        FileControl.SaveRawFile(path, fbintervalDataList, fbDate1);
                        fbintervalDataList.Clear();
                    }
                }
            }
        }
    }
}
