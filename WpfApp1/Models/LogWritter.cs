using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class LogWritter
    {
        private static string _url = "";
        public static void SetUrl(string url)
        {
            _url = url;
        }
        public static void Write(string message, LogType type = LogType.INFO)
        {
            Write(message, _url, type);
        }
        public static void Write(string message,string path, LogType type= LogType.INFO)
        {
            if (IsValidFolderPath(path))
            {
                string filePath = Path.Combine(path, "log.txt");
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    if (type == LogType.INFO)
                    {
                        writer.WriteLine($"----INFO----:  {message} - {DateTime.Now} ");
                    }
                    else if (type == LogType.WARNING)
                    {
                        writer.WriteLine($"----WARNING---:  {message} - {DateTime.Now} ");
                    }
                    else if (type == LogType.ERROR)
                    {
                        writer.WriteLine($"----ERROR----:  {message} - {DateTime.Now} ");
                    }
         
                }
            }
            else
            {
                throw new ArgumentException("Invalid folder path.");
            }
        }
        public static void WriteInfro(string message)
        {

        }
        private static bool IsValidFolderPath(string path)
        {
            try
            {
                return Directory.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    public enum LogType
    {
        INFO,
        WARNING,
        ERROR
    }
}
