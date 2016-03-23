//-----------------------------------------------------------------------

// <copyright file="Program.cs" company="Beatsbury Software">

//     Copyright (c) Beatsbury Software. All rights reserved.

// </copyright>

// <author>Beatsbury</author>

//-----------------------------------------------------------------------

namespace FileSystemLogger
{
    //using System.Threading;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using System.Text;
    using System.Timers;

    //using System.Timers;

    internal class Program
    {
        #region Fields

        internal static List<string> FileRegistry = new List<string>();
        internal static List<string> LogLines = new List<string>();
        internal static List<string> ProcessedDirectories = new List<string>();
        internal static Dictionary<Timer, bool> TimerPool = new Dictionary<Timer, bool>();

        #endregion Fields

        //MailSender _mailSender = new MailSender();

        #region Methods

        // ReSharper disable once UnusedParameter.Local
        private static void AlarmTimerElapsed(object sender, ElapsedEventArgs args, string dirPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(dirPath);
                var files = dirInfo.GetFiles();
                if (files.Length != 0)
                {
                    var filesToMessage = new List<string>();
                    foreach (var file in files.Where(file => FileRegistry.Contains(file.FullName)))
                    {
                        filesToMessage.Add(file.Name);
                        FileRegistry.Remove(file.FullName);
                    }
                    if (filesToMessage.Count != 0)
                    {
                        var mailSender = new MailSender();
                        var messageBuilder = new StringBuilder(mailSender.MessageBody);
                        mailSender.MessageSubject = $"{DateTime.Now} : Задержка обмена данными [{dirPath}]";
                        messageBuilder.AppendLine("Следующие файлы не были обработаны обменом вовремя!");
                        messageBuilder.AppendLine("=======================================================\r\n");
                        messageBuilder.AppendLine($"Путь папки : {dirPath}\r\n");
                        messageBuilder.AppendLine("-------------------------------------------------------");
                        messageBuilder.AppendLine("Файлы :");
                        foreach (var fileToMessage in filesToMessage)
                        {
                            messageBuilder.AppendLine($"\t{fileToMessage}");
                        }
                        messageBuilder.AppendLine("=======================================================\r\n\r\n");
                        filesToMessage.Clear();
                        ProcessedDirectories.Remove(dirPath);
                        Console.WriteLine(messageBuilder);
                        LogLines.Add(messageBuilder.ToString());
                        mailSender.MessageBody = messageBuilder.ToString();
                        mailSender.SendMail();
                        mailSender.MessageBody = string.Empty;
                        mailSender.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                LogLines.Add(e.Message);
            }
            var timer = (Timer)sender;
            timer?.Close();
            Console.WriteLine($"Dispose() method has been called for {timer}");
        }

        //private static Timer GetTimer()
        //{
        //    var timerSlot = TimerPool.First(pair => !pair.Value);
        //    TimerPool[timerSlot.Key] = true;
        //    Console.WriteLine($"{timerSlot.Key}'s value in TimerPool has been changed to {timerSlot.Value}..");
        //    return timerSlot.Key;
        //}

        private static void InitializeLogTimer()
        {
            var logTimer = new Timer();
            logTimer.Interval = 180000;
            logTimer.Elapsed += LogTimer_Elapsed;
            logTimer.AutoReset = true;
            logTimer.Start();
        }

        //private static void InitializeTimerPool(string path)
        //{
        //    var info = new DirectoryInfo(path);
        //    try
        //    {
        //        var subDirInfos = info.GetDirectories();
        //        foreach (var subDirInfo in subDirInfos)
        //        {
        //            TimerPool.Add(new Timer(), false);
        //            Console.WriteLine($"Added timer for {subDirInfo.FullName}");
        //        }
        //        foreach (var pair in TimerPool)
        //        {
        //            Console.WriteLine($"{pair.Key} : {pair.Value}");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogLines.Add("Failed to create TimerPool:");
        //        LogLines.Add(e.Message);
        //    }
        //}

        private static void InitializeWatcher(string path)
        {
            var watcher = new FileSystemWatcher();
            try
            {
                watcher.Path = path;
            }
            catch (Exception e)
            {
                string errorMessage = $"{DateTime.Now} : Invalid path. {e.Message}";
                LogLines.Add(errorMessage);
            }

            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime;
            watcher.Created += OnCreated;
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += OnDeleted;
            try
            {
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                string errorMessage = $"{DateTime.Now} : {e.Message}";
                LogLines.Add(errorMessage);
            }
        }

        private static void LogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                using (var file = new StreamWriter(@"D:\Клиенты\monetkaFiles.log", true))
                {
                    foreach (var logLine in LogLines)
                    {
                        file.WriteLine(logLine);
                    }
                    LogLines.Clear();
                    //file.Dispose();
                }
            }
            catch (Exception error)
            {
                string errorMessage = $"{DateTime.Now} : {error.Message}";
                LogLines.Add(errorMessage);
            }
        }

        private static void Main()
        {
            const string rootPath = @"D:\Клиенты\Монетка";
            Run(rootPath);
        }

        private static void OnCreated(object sender, FileSystemEventArgs args)
        {
            string logLine = $"{DateTime.Now} : New file : {args.FullPath}";
            LogLines.Add(logLine);
            FileRegistry.Add(args.FullPath);
            //Console.WriteLine(dirPath); //diagnostic message
            var dirPath = args.FullPath.Remove(args.FullPath.LastIndexOf('\\') + 1);
            if (args.FullPath.Contains("UPLOAD") || ProcessedDirectories.Contains(dirPath)) return;
            ProcessedDirectories.Add(dirPath);
            //var alarmTimer = new Timer(TimerElapsed, dirPath, 66666, 0);
            var alarmTimer = new Timer();
            //var alarmTimer = GetTimer();
            alarmTimer.Interval = 666666;
            Console.WriteLine($"Alarm timer for directory path {dirPath} created..");
            //alarmTimer.Dispose();
            alarmTimer.Start();
            //alarmTimer.Elapsed += AlarmTimer_Elapsed;
            alarmTimer.Elapsed += (a, e) => AlarmTimerElapsed(a, e, dirPath);
        }

        //private static void AlarmTimer_Elapsed(object a, ElapsedEventArgs e, string dirPath)
        //{
        //    Console.WriteLine(dirPath);
        //    Console.WriteLine($"Alarm timer for directory path {dirPath} elapsed..");
        //    try
        //    {
        //        var dirInfo = new DirectoryInfo(dirPath);
        //        Console.WriteLine($"Dir Info for {dirPath} created..");
        //        var files = dirInfo.GetFiles();
        //        if (files.Length == 0) return;
        //        var filesToMessage = new List<string>();
        //        foreach (var file in files.Where(file => FileRegistry.Contains(file.FullName)))
        //        {
        //            filesToMessage.Add(file.Name);
        //            FileRegistry.Remove(file.FullName);
        //        }
        //        if (filesToMessage.Count == 0) return;
        //        var mailSender = new MailSender();
        //        var messageBuilder = new StringBuilder(mailSender.MessageBody);
        //        mailSender.MessageSubject = $"{DateTime.Now} : FILE EXCHANGE DELAY [{dirPath}]";
        //        messageBuilder.AppendLine("The following files has not been processed by exchange!");
        //        messageBuilder.AppendLine("=======================================================\r\n");
        //        messageBuilder.AppendLine($"Directory path : {dirPath}\r\n");
        //        messageBuilder.AppendLine("-------------------------------------------------------");
        //        messageBuilder.AppendLine("Files :");
        //        foreach (var fileToMessage in filesToMessage)
        //        {
        //            messageBuilder.AppendLine($"\t{fileToMessage}");
        //        }
        //        filesToMessage.Clear();
        //        ProcessedDirectories.Remove(dirPath);
        //        Console.WriteLine(messageBuilder);
        //        LogLines.Add(messageBuilder.ToString());
        //        mailSender.MessageBody = messageBuilder.ToString();
        //        mailSender.SendMail();
        //        mailSender.MessageBody = string.Empty;
        //        mailSender.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogLines.Add(ex.Message);
        //    }
        //}

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            string logLine = $"{DateTime.Now} : File removed : {e.FullPath}";
            LogLines.Add(logLine);
        }

        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Run(string path)
        {
            //InitializeTimerPool(path);
            InitializeWatcher(path);
            InitializeLogTimer();
            Console.Read();
        }

        #endregion Methods

        //private static void TimerElapsed(object info)
        //{
        //    Console.WriteLine(state as string);
        //    var dirPath = (string)info;
        //    Console.WriteLine($"Alarm timer for directory path {dirPath} elapsed..");
        //    try
        //    {
        //        var dirInfo = new DirectoryInfo(dirPath);
        //        Console.WriteLine($"Dir Info for {dirPath} created..");
        //        var files = dirInfo.GetFiles();
        //        if (files.Length == 0) return;
        //        var filesToMessage = new List<string>();
        //        foreach (var file in files.Where(file => FileRegistry.Contains(file.FullName)))
        //        {
        //            filesToMessage.Add(file.Name);
        //            FileRegistry.Remove(file.FullName);
        //        }
        //        if (filesToMessage.Count == 0) return;
        //        var mailSender = new MailSender();
        //        var messageBuilder = new StringBuilder(mailSender.MessageBody);
        //        mailSender.MessageSubject = $"{DateTime.Now} : FILE EXCHANGE DELAY [{dirPath}]";
        //        messageBuilder.AppendLine("The following files has not been processed by exchange!");
        //        messageBuilder.AppendLine("=======================================================\r\n");
        //        messageBuilder.AppendLine($"Directory path : {dirPath}\r\n");
        //        messageBuilder.AppendLine("-------------------------------------------------------");
        //        messageBuilder.AppendLine("Files :");
        //        foreach (var fileToMessage in filesToMessage)
        //        {
        //            messageBuilder.AppendLine($"\t{fileToMessage}");
        //        }
        //        filesToMessage.Clear();
        //        ProcessedDirectories.Remove(dirPath);
        //        Console.WriteLine(messageBuilder);
        //        LogLines.Add(messageBuilder.ToString());
        //        mailSender.MessageBody = messageBuilder.ToString();
        //        mailSender.SendMail();
        //        mailSender.MessageBody = string.Empty;
        //        mailSender.Dispose();
        //    }
        //    catch (Exception e)
        //    {
        //        LogLines.Add(e.Message);
        //    }
        //}

        //private static void AlarmTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    Console.WriteLine(sender.GetType());
        //    Console.WriteLine(sender.ToString());
        //}
    }
}