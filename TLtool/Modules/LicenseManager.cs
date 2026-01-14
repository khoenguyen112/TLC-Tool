using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using TLTool.Utils;

namespace TLTool
{
    public static class LicenseManager
    {
        private static readonly string licenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.key");

        public static void Manage()
        {
            while (true)
            {
                Console.Clear();
                ConsoleHelper.Header("QUẢN LÝ LICENSE KEY");

                if (File.Exists(licenseFile))
                {
                    string currentKey = File.ReadAllText(licenseFile).Trim();
                    Console.WriteLine($"Key hiện tại: {currentKey}");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Hiện chưa có key nào được lưu.");
                    Console.WriteLine();
                }

                Console.WriteLine("1. Đổi key mới");
                Console.WriteLine("2. Xóa key hiện tại");
                Console.WriteLine("0. Quay lại menu chính");

                Console.Write("\n➤ Chọn: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": // Đổi key mới
                        Console.Write("Nhập key mới: ");
                        string newKey = Console.ReadLine()?.Trim();

                        if (!string.IsNullOrEmpty(newKey))
                        {
                            File.WriteAllText(licenseFile, newKey);
                            ConsoleHelper.Success("Đổi key thành công! Tool sẽ restart để áp dụng.");
                            Thread.Sleep(2000);
                           RestartTool(); // Restart tool
                        }
                        else
                        {
                            ConsoleHelper.Error("Key không được để trống!");
                        }
                        break;

                    case "2": // Xóa key
                        if (File.Exists(licenseFile))
                        {
                            File.Delete(licenseFile);
                            ConsoleHelper.Success("Đã xóa key! Tool sẽ restart để áp dụng.");
                            Thread.Sleep(2000);
                            RestartTool(); // Restart tool
                        }
                        else
                        {
                            ConsoleHelper.Warning("Không có key nào để xóa.");
                        }
                        break;

                    case "0":
                        return;

                    default:
                        ConsoleHelper.Error("Lựa chọn không hợp lệ!");
                        Thread.Sleep(1000);
                        break;
                }
            }
        }
        private static void RestartTool()
        {
            string exePath = Process.GetCurrentProcess().MainModule!.FileName;
            Process.Start(exePath);
            Environment.Exit(0); // Thoát tool cũ
        }


    }
}