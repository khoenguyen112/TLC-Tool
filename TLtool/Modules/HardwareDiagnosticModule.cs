using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
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
                        TestBattery();
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
                    UseShellExecute = true
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

        private static void TestBattery()
        {
            Console.Clear();
            ConsoleHelper.Header("KIỂM TRA PIN");

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
                ManagementObjectCollection batteries = searcher.Get();

                if (batteries.Count == 0)
                {
                    ConsoleHelper.Warning("Không phát hiện pin (có thể là máy bàn hoặc pin bị tháo)");
                    Console.WriteLine("\nNhấn phím bất kỳ để quay lại...");
                    Console.ReadKey();
                    return;
                }

                foreach (ManagementObject battery in batteries)
                {
                    DisplayBatteryInfo(battery);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Lỗi khi đọc thông tin pin: {ex.Message}");
            }

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Nhấn phím bất kỳ để quay lại...");
            Console.ReadKey();
        }

        private static void DisplayBatteryInfo(ManagementObject battery)
        {
            // Lấy thông tin cơ bản
            string name = battery["Name"]?.ToString() ?? "Unknown";
            ushort status = Convert.ToUInt16(battery["BatteryStatus"] ?? 0);
            ushort chargeRemaining = Convert.ToUInt16(battery["EstimatedChargeRemaining"] ?? 0);
            uint runTime = Convert.ToUInt32(battery["EstimatedRunTime"] ?? 0);

            // Lấy dung lượng từ WMI
            uint designCapacity = Convert.ToUInt32(battery["DesignCapacity"] ?? 0);
            uint fullChargeCapacity = Convert.ToUInt32(battery["FullChargeCapacity"] ?? 0);

            // Hiển thị thông tin
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Tên pin: {name}");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();

            // Trạng thái
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- TRẠNG THÁI HIỆN TẠI ---");
            Console.ResetColor();

            string statusText = GetBatteryStatusText(status);
            Console.WriteLine($"Trạng thái: {statusText}");
            Console.WriteLine($"Pin hiện tại: {chargeRemaining}%");

            // Vẽ thanh pin
            DrawBatteryBar(chargeRemaining);

            if (runTime != 71582788 && runTime > 0)
            {
                int hours = (int)runTime / 60;
                int minutes = (int)runTime % 60;
                Console.WriteLine($"Thời gian còn lại: {hours} giờ {minutes} phút");
            }
            else if (status == 2)
            {
                Console.WriteLine("Thời gian còn lại: Đang sạc...");
            }
            else
            {
                Console.WriteLine("Thời gian còn lại: Không xác định");
            }

            Console.WriteLine();

            // Dung lượng và sức khỏe pin
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- CHI TIẾT DUNG LƯỢNG ---");
            Console.ResetColor();

            // Nếu WMI không có dữ liệu, dùng PowerCfg
            if (designCapacity == 0 || fullChargeCapacity == 0)
            {
                Console.WriteLine("WMI không cung cấp dung lượng pin, đang lấy từ PowerCfg...");
                Console.WriteLine();
                GetBatteryInfoFromPowerCfg(out designCapacity, out fullChargeCapacity);
            }

            if (designCapacity > 0 && fullChargeCapacity > 0)
            {
                Console.WriteLine($"Dung lượng thiết kế: {designCapacity:N0} mWh");
                Console.WriteLine($"Dung lượng thực tế: {fullChargeCapacity:N0} mWh");

                // Tính sức khỏe pin
                double healthPercent = ((double)fullChargeCapacity / designCapacity) * 100;
                double wearPercent = 100 - healthPercent;
                int capacityLost = (int)(designCapacity - fullChargeCapacity);

                Console.WriteLine($"Sức khỏe pin: {healthPercent:F1}%");
                Console.WriteLine($"Độ chai pin: {wearPercent:F1}%");
                Console.WriteLine($"Dung lượng mất đi: {capacityLost:N0} mWh");

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("--- ĐÁNH GIÁ ---");
                Console.ResetColor();

                // Đánh giá tình trạng
                if (healthPercent >= 90)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Sức khỏe pin: RẤT TỐT (≥90%)");
                }
                else if (healthPercent >= 80)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Sức khỏe pin: TỐT (80-90%)");
                }
                else if (healthPercent >= 60)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ Sức khỏe pin: TRUNG BÌNH (60-80%)");
                    Console.WriteLine("  → Nên cân nhắc thay pin trong tương lai gần");
                }
                else if (healthPercent >= 40)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("⚠ Sức khỏe pin: YẾU (40-60%)");
                    Console.WriteLine("  → Nên thay pin sớm");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✗ Sức khỏe pin: RẤT YẾU (<40%)");
                    Console.WriteLine("  → Cần thay pin ngay");
                }
                Console.ResetColor();
            }
            else
            {
                ConsoleHelper.Warning("Không thể đọc thông tin dung lượng pin");
                Console.WriteLine("(Hệ thống không cung cấp thông tin này)");
            }

            // Thông tin bổ sung
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- THÔNG TIN BỔ SUNG ---");
            Console.ResetColor();

            if (battery["Chemistry"] != null)
            {
                ushort chemistry = Convert.ToUInt16(battery["Chemistry"]);
                string chemistryText = GetChemistryText(chemistry);
                Console.WriteLine($"Công nghệ pin: {chemistryText}");
            }

            if (battery["DesignVoltage"] != null)
            {
                uint voltage = Convert.ToUInt32(battery["DesignVoltage"]);
                Console.WriteLine($"Điện áp thiết kế: {voltage} mV");
            }
        }

        private static void GetBatteryInfoFromPowerCfg(out uint designCapacity, out uint fullChargeCapacity)
        {
            designCapacity = 0;
            fullChargeCapacity = 0;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "/batteryreport /duration 1 /output battery-report.html",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                Process process = Process.Start(psi);
                process.WaitForExit();

                // Đọc file HTML được tạo
                string reportPath = Path.Combine(Environment.CurrentDirectory, "battery-report.html");
                if (File.Exists(reportPath))
                {
                    string htmlContent = File.ReadAllText(reportPath);

                    // Parse HTML để lấy Design Capacity và Full Charge Capacity
                    // Tìm dòng chứa "DESIGN CAPACITY" và "FULL CHARGE CAPACITY"
                    Match designMatch = Regex.Match(htmlContent, @"DESIGN CAPACITY.*?(\d+,?\d*)\s*mWh", RegexOptions.IgnoreCase);
                    Match fullChargeMatch = Regex.Match(htmlContent, @"FULL CHARGE CAPACITY.*?(\d+,?\d*)\s*mWh", RegexOptions.IgnoreCase);

                    if (designMatch.Success)
                    {
                        string designStr = designMatch.Groups[1].Value.Replace(",", "");
                        designCapacity = uint.Parse(designStr);
                    }

                    if (fullChargeMatch.Success)
                    {
                        string fullChargeStr = fullChargeMatch.Groups[1].Value.Replace(",", "");
                        fullChargeCapacity = uint.Parse(fullChargeStr);
                    }

                    // Xóa file tạm
                    try { File.Delete(reportPath); } catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi chạy PowerCfg: {ex.Message}");
            }
        }

        private static void DrawBatteryBar(int percent)
        {
            int barLength = 40;
            int filled = (int)((percent / 100.0) * barLength);

            Console.Write("[");

            // Chọn màu dựa theo %
            if (percent > 60)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (percent > 20)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.Write(new string('█', filled));
            Console.ResetColor();
            Console.Write(new string('░', barLength - filled));
            Console.WriteLine("]");
        }

        private static string GetBatteryStatusText(ushort status)
        {
            return status switch
            {
                1 => "Pin đang xả (không sạc)",
                2 => "Đang sạc",
                3 => "Đã sạc đầy",
                4 => "Thấp",
                5 => "Rất thấp (Critical)",
                6 => "Đang sạc và thấp",
                7 => "Đang sạc và rất thấp",
                8 => "Đang sạc và rất thấp (Critical)",
                9 => "Không xác định",
                10 => "Đã sạc một phần",
                11 => "Hoàn toàn sạc",
                _ => "Không xác định"
            };
        }

        private static string GetChemistryText(ushort chemistry)
        {
            return chemistry switch
            {
                1 => "Khác",
                2 => "Không biết",
                3 => "Lead Acid",
                4 => "Nickel Cadmium",
                5 => "Nickel Metal Hydride",
                6 => "Lithium-ion",
                7 => "Zinc Air",
                8 => "Lithium Polymer",
                _ => "Không xác định"
            };
        }
    }
}