﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KillService
{
    partial class Service1 : ServiceBase
    {
        static System.Timers.Timer Timer_Get = new System.Timers.Timer();
        public Process[] process = Process.GetProcesses();
        public String[] processname;
        public String[] forbiddenprocess;
        System.Diagnostics.Process builderprocess;
        bool state = false;
        public Service1()
        {
            InitializeComponent();
            //processname = new string[] { "OneDrive","csrss", "svchost","3DPrintService","RuntimeBroker", "services", "WmiPrvSE", 
            //    "sihost","nvvsvc","WmiApSrv","spoolsv","nvxdsync","SearchFilterHost","SystemSettings","igfxEM","smss","esif_uf",
            //    "explorer","SearchProtocolHost","MsMpEng","Builder3D","ChsIME","wininit","WinStore.Mobile","MpCmdRun","audiodg",
            //    "NisSrv","winlogon","esif_assist","spoolsv","ApplicationFrameHost","SearchUI","WUDFHost","igfxCUIService","dwm",
            //    "dllhost","IntelCpHeciSvc","lsass","LoveAppWU","SearchFilterHost","ShellExperienceHost","SkypeHost","inetinfo",
            //    "SearchIndexer","igfxTray","taskhostw","HxTsr","ChsIME","audiodg","TabTip","fontdrvhost","WUDFHost","HxMail",
            //    "explorer","System","dasHost","KillService","Idle","NcdAutoSetup","WlanSvc","WpnService","WcsPluglnService",
            //    "winmgmt","Taskmgr","BITS","Brokerlnfrastructure","taskhostw","系统中断","mstask","系统空闲进程",
            //    "系统和压缩内存","Shutdown"};
            forbiddenprocess = new string[] { "MicrosoftEdge", "MicrosoftEdgeCP", "InstallUtil", "QQProtect",
                "LoveAppWU", "ShellExperienceHost","igfxTray","HxTsr","HxMail","WinStore.Mobile"};
            processname = new string[process.Length];
            for (int i = 0; i < process.Length; i++)
            {
                processname[i] = process[i].ProcessName;
            }

            //string serverpath = "C:\\Users\\Marco\\Desktop\\Z++Server.exe";
            //builderprocess = new System.Diagnostics.Process()
            //{
            //    StartInfo = new System.Diagnostics.ProcessStartInfo()
            //    {
            //        FileName = serverpath
            //    }
            //};
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // TODO: 在此处添加代码以启动服务。
            AutoLog = false;
            FileLog.Success("服务已启动");
            Timer_Get.Enabled = true;
            Timer_Get.Interval = 1000;
            Timer_Get.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        }

        public void GetProcessEvent()
        {
            process = Process.GetProcesses();
        }

        public void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            FileLog.Success("开始发送");
            Timer_Get.Enabled = false;
            try
            {
                GetProcessEvent();

                //foreach (Process anyprocess in process)
                //{
                //    if (!processname.Contains(anyprocess.ProcessName))
                //    {
                //        anyprocess.Kill();
                //    }
                //}

                if (System.Diagnostics.Process.GetProcessesByName("Z++Server").Length <= 0)
                {
                    System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
                    Info.FileName = "Z++Server.exe";
                    Info.WorkingDirectory = @"C:\Users\Marco\Desktop";
                    //Info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                    System.Diagnostics.Process Proc;
                    try
                    {
                        Proc = System.Diagnostics.Process.Start(Info);
                        System.Threading.Thread.Sleep(500);
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        return;
                    }
                }

                for (int i = 0; i < process.Length; i++)
                {
                    //if (System.Diagnostics.Process.GetProcessesByName("2").Length <= 0)
                    //{ }
                    if (process[i].ProcessName == "Z++Server")
                    {
                        state = true;
                        break;
                    }
                    else
                    {
                        state = false;
                    }
                }
                if (state != true)
                {
                    builderprocess.Start();
                }
                foreach (Process anyprocess in process)
                {
                    if (forbiddenprocess.Contains(anyprocess.ProcessName))
                    {
                        anyprocess.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                FileLog.Error(ex.Source + "。" + ex.Message);
            }
            Timer_Get.Enabled = true;
            FileLog.Success("结束发送");
        }

        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            FileLog.Success("服务已停止");
            Timer_Get.Enabled = false;
        }
    }

    /// <summary>
    /// 文件型日志记录
    /// </summary>
    public static class FileLog
    {
        private static string sFilePath = System.Configuration.ConfigurationSettings.AppSettings["UserLog"];
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="Message">错误内容</param>
        public static void Error(string Message)
        {
            try
            {
                if (!System.IO.Directory.Exists(sFilePath))
                {
                    System.IO.Directory.CreateDirectory(sFilePath);
                }
                string sFileName = sFilePath + "\\" + string.Format("{0}-Error.txt", DateTime.Now.ToString("yyyy-MM-dd"));
                string sContent = string.Format("{0}-- {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Message);
                System.IO.FileStream fs = new System.IO.FileStream(sFileName, System.IO.FileMode.Append);
                Byte[] b = Encoding.Default.GetBytes(sContent);
                fs.Write(b, 0, b.Length);
                fs.Close();
            }
            catch { }
        }

        /// <summary>
        /// 正确日志
        /// </summary>
        /// <param name="Message">正确内容</param>
        public static void Success(string Message)
        {
            try
            {
                if (!System.IO.Directory.Exists(sFilePath))
                {
                    System.IO.Directory.CreateDirectory(sFilePath);
                }
                string sFileName = sFilePath + "\\" + string.Format("{0}-Success.txt", DateTime.Now.ToString("yyyy-MM-dd"));
                string sContent = string.Format("{0}-- {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Message);
                System.IO.FileStream fs = new System.IO.FileStream(sFileName, System.IO.FileMode.Append);
                Byte[] b = Encoding.Default.GetBytes(sContent);
                fs.Write(b, 0, b.Length);
                fs.Close();
            }
            catch { }
        }
    }
}


