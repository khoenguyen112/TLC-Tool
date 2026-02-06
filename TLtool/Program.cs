using System;
using System.Text;
using TLTool;
using TLTool.Modules;
using TLTool.Utils;

namespace TLtool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            ConsoleHelper.SetTitle("TLC TOOL - Windows Utility");

             //===== KIỂM TRA THAM SỐ DÒNG LỆNH =====
            if (args.Length > 0 && args[0] == "--install-apps")
            {
                UsbBatchInstallModule.RunInNewConsole();
                return;
            }
             //======================================

            // Kiểm tra license offline
            //if (!LicenseManager.IsPremium())
            //{
            //    Console.Clear();
            //    ConsoleHelper.Header("YÊU CẦU KEY PREMIUM");
            //    Console.WriteLine("Một số chức năng yêu cầu key Premium.");
            //    Console.WriteLine("Nhập key để unlock ngay bây giờ (dạng TL-YYYYMMDD-XXXXXX)");
            //    Console.Write("\nNhập key: ");
            //    string inputKey = Console.ReadLine()?.Trim();

            //    if (!string.IsNullOrEmpty(inputKey) && LicenseManager.IsValidKey(inputKey))
            //    {
            //        LicenseManager.SaveLicenseKey(inputKey);
            //        ConsoleHelper.Success("Key hợp lệ! Đã unlock Premium.");
            //    }
            //    else
            //    {
            //        ConsoleHelper.Error("Key không hợp lệ hoặc đã hết hạn!");
            //        ConsoleHelper.Pause();
            //        // Có thể thoát tool hoặc cho chạy chế độ free
            //        // return;
            //    }
            //}

            //// Set console size
            //try
            //{
            //    Console.SetWindowSize(90, 40);
            //    Console.SetBufferSize(90, 300);
            //}
            //catch { }

            //Console.CursorVisible = false;
            //Console.BackgroundColor = ConsoleColor.Black;
            //Console.ForegroundColor = ConsoleColor.Green;

            //// Intro
            //ConsoleHelper.MatrixRainIntro();
            //ConsoleHelper.HackerWelcome();

            // Menu chính
            while (true)
            {
                ConsoleHelper.ShowMainMenu(LicenseManager.IsPremium());
                string? choice = Console.ReadLine()?.Trim();

                // Nếu chọn Premium mà chưa unlock → yêu cầu key
                //if (!LicenseManager.IsPremium() && IsPremiumFeature(choice))
                //{
                //    ConsoleHelper.Error("Chức năng này yêu cầu Premium!");
                //    Console.Write("Nhập key Premium: ");
                //    string inputKey = Console.ReadLine()?.Trim();
                //    if (!string.IsNullOrEmpty(inputKey) && LicenseManager.IsValidKey(inputKey))
                //    {
                //        LicenseManager.SaveLicenseKey(inputKey);
                //        ConsoleHelper.Success("Đã unlock Premium!");
                //    }
                //    else
                //    {
                //        ConsoleHelper.Error("Key không hợp lệ!");
                //        continue;
                //    }
                //}

                switch (choice)
                {
                    case "1":
                        //ConsoleHelper.FakeProgress("Đang quét phần cứng ");
                        SystemInfoModule.Run();
                        break;
                    case "2":
                        //ConsoleHelper.FakeProgress("Đang tải payload ");
                        ActivationModule.Run();
                        break;
                    case "3":
                        QuickOptimizeModule.Run();
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
                    case "0":
                    case "q":
                    case "Q":
                        ConsoleHelper.Success("Thoát tool!");
                        return;
                    default:
                        ConsoleHelper.Error("Chức năng đang được phát triển");
                        break;
                }
            }
        }

        //private static bool IsPremiumFeature(string choice)
        //{
        //    return choice == "2" || choice == "3" || choice == "7" || choice == "5";
        //}
    }
}