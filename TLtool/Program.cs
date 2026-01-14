using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

using TLTool.Modules;

using TLTool.Utils;

namespace TLTool
{
    internal class Program
    {
        private static string serverUrl = "https://script.google.com/macros/s/AKfycbyOr5rApLLLEu2mKwpO63R9XRsoW8puzKwWd2E_QFclbw1Z_UV_ymkhbxbiY4ALSico/exec";
        private static string licenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.key");
        private static bool isPremium = false;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            ConsoleHelper.SetTitle("TLC TOOL - Windows Utility");

            LoadLicenseStatus(); // Đọc key và check online (không hiện gì nếu sai)

            // Set console size
            try
            {
                Console.SetWindowSize(90, 40);
                Console.SetBufferSize(90, 300);
            }
            catch { }

            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;

            // Intro bình thường, không hỏi key
            ConsoleHelper.MatrixRainIntro();
            ConsoleHelper.HackerWelcome();

            // Menu chính
            while (true)
            {
                ConsoleHelper.ShowMainMenu(isPremium); // Truyền isPremium để hiện banner Free/Premium

                string? choice = Console.ReadLine()?.Trim();

                // Nếu chọn chức năng Premium mà chưa có key → hỏi nhập key
                if (!isPremium && IsPremiumFeature(choice))
                {
                    if (AskForPremiumKey())
                    {
                        isPremium = true;
                        RestartTool(); // Restart để áp dụng ngay
                    }
                    continue;
                }

                switch (choice)
                {
                    case "1":
                        ConsoleHelper.FakeProgress("Đang quét phần cứng ");
                        SystemInfoModule.Run();
                        break;

                    case "2":
                        ConsoleHelper.FakeProgress("Đang tải payload ");
                        ActivationModule.Run();
                        break;

                    case "4":
                        HardwareDiagnosticModule.Run();
                        break;

                    case "6":
                        LicenseManager.Manage();
                        break;

                    case "7":
                        UsbBatchInstallModule.Run();
                        break;
                    case "8":
                        ChocolateyBatchInstallModule.Run();
                        break;

                    case "0":
                    case "q":
                    case "Q":
                        ConsoleHelper.Success("Thoát tool. Bye bye!");
                        return;

                    default:
                        ConsoleHelper.Error("Chức năng đang được phát triển");
                        break;
                }
            }
        }

        private static void LoadLicenseStatus()
        {
            isPremium = false;

            if (File.Exists(licenseFile))
            {
                string currentKey = File.ReadAllText(licenseFile).Trim();

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        string url = $"{serverUrl}?key={Uri.EscapeDataString(currentKey)}";
                        string result = client.GetStringAsync(url).Result.Trim();
                        isPremium = result == "VALID";
                    }
                }
                catch
                {
                    // Không mạng → tạm cho Premium nếu có key cũ
                    isPremium = !string.IsNullOrEmpty(currentKey);
                }
            }
        }

        private static bool AskForPremiumKey()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("══════════════════════════════════════════════════");
            Console.WriteLine("   Chức năng này yêu cầu phiên bản PREMIUM");
            Console.WriteLine("   Nhập key để unlock full tool ngay bây giờ");
            Console.WriteLine("══════════════════════════════════════════════════");
            Console.ResetColor();

            while (true)
            {
                Console.Write("Nhập key Premium: ");
                string inputKey = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(inputKey))
                {
                    ConsoleHelper.Error("Key không được để trống! Nhấn Enter để bỏ qua.");
                    if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        return false;
                    continue;
                }

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        string url = $"{serverUrl}?key={Uri.EscapeDataString(inputKey)}";
                        string result = client.GetStringAsync(url).Result.Trim();

                        if (result == "VALID")
                        {
                            File.WriteAllText(licenseFile, inputKey);
                            ConsoleHelper.Success("Key hợp lệ! Đã unlock Premium. Tool sẽ restart...");
                            Thread.Sleep(2000);
                            return true;
                        }
                        else
                        {
                            ConsoleHelper.Error("Key không hợp lệ hoặc hết hạn! Thử lại.");
                        }
                    }
                }
                catch
                {
                    ConsoleHelper.Error("Không kết nối server! Cần mạng để kích hoạt key.");
                }
            }
        }

        private static bool IsPremiumFeature(string choice)
        {
            // Các chức năng Premium
            return choice == "2" || choice == "3" || choice == "7" || choice == "5";
        }

        private static void RestartTool()
        {
            string exePath = Process.GetCurrentProcess().MainModule!.FileName;
            Process.Start(exePath);
            Environment.Exit(0);
        }
    }
}