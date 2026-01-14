using System;
using System.Diagnostics;
using System.IO;
using TLTool.Utils;

namespace TLTool.Modules
{
    public static class HardwareDiagnosticModule
    {
        public static void Run()
        {
            while (true)
            {
                ConsoleHelper.Header("CHẨN ĐOÁN PHẦN CỨNG");

                Console.WriteLine("1. Test bàn phím (Switch Hitter)");
                Console.WriteLine("2. Test camere ");
                Console.WriteLine("3. Test âm thanh ");
                Console.WriteLine("4. Test mic");
                Console.WriteLine("5. Test pin");
                Console.WriteLine();
                Console.WriteLine("0. Quay lại menu chính");

                Console.Write("\n>> Chọn: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        LaunchKeyboardTest();
                        break;
                    case "2":
                    case "3":
                    case "4":
                        ConsoleHelper.Warning("Chức năng đang phát triển...");
                        break;
                    case "5":
                        // Test pin
                        ConsoleHelper.Warning("Chức năng đang phát triển...");
                        break;
                    case "0":
                        return;
                    default:
                        ConsoleHelper.Error("Lựa chọn không hợp lệ!");
                        break;
                }
            }
        }

        private static void LaunchKeyboardTest()
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Switch Hitter.exe");

            if (File.Exists(exePath))
            {
                Console.Clear();
                Console.WriteLine("Đang mở Switch Hitter...");
                Console.WriteLine("Sử dụng phần mềm để test phím. Đóng cửa sổ để quay lại tool.");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true // Mở như app bình thường
                };

                Process.Start(psi);

                Console.WriteLine("\nNhấn phím bất kỳ để tiếp tục khi đóng Keyboard Test...");
                Console.ReadKey();
            }
            else
            {
                ConsoleHelper.Error("Không tìm thấy Switch Hitter.exe trong thư mục tool!");
                Console.WriteLine("Vui lòng đặt file EXE vào thư mục publish.");
            }
        }
    }
}