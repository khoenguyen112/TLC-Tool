using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using TLTool.Utils;

namespace TLTool.Modules
{
    public static class SystemInfoModule
    {
        public static void Run()
        {
            ConsoleHelper.Header("THÔNG TIN HỆ THỐNG CHI TIẾT");

            try
            {
                // ═══════════════════════════════════════════════════════════
                // THÔNG TIN MÁY TÍNH & MAINBOARD
                // ═══════════════════════════════════════════════════════════
                PrintSectionHeader("THÔNG TIN MÁY TÍNH & MAINBOARD");

                string manufacturer = "Unknown";
                string model = "Unknown";
                string userName = Environment.UserName;
                string computerName = Environment.MachineName;
                string serialNumber = "Unknown";

                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "Unknown";
                        model = obj["Model"]?.ToString()?.Trim() ?? "Unknown";
                        break;
                    }
                }

                // Lấy Serial Number
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            serialNumber = obj["SerialNumber"]?.ToString()?.Trim() ?? "Unknown";
                            break;
                        }
                    }
                }
                catch { }

                PrintInfo("Tên máy tính", computerName, ConsoleColor.Cyan);
                PrintInfo("Người dùng", userName, ConsoleColor.Cyan);
                PrintInfo("Hãng sản xuất", manufacturer);
                PrintInfo("Model", model);
                PrintInfo("Serial Number", serialNumber, ConsoleColor.Green);

                // BIOS Info
                string bios = "Unknown";
                string biosDate = "Unknown";
                using (var searcher = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        bios = obj["SMBIOSBIOSVersion"]?.ToString()?.Trim() ?? "Unknown";
                        string releaseDateRaw = obj["ReleaseDate"]?.ToString();
                        if (!string.IsNullOrEmpty(releaseDateRaw) && releaseDateRaw.Length >= 8)
                        {
                            biosDate = $"{releaseDateRaw.Substring(6, 2)}/{releaseDateRaw.Substring(4, 2)}/{releaseDateRaw.Substring(0, 4)}";
                        }
                        break;
                    }
                }
                PrintInfo("BIOS Version", bios);
                PrintInfo("BIOS Date", biosDate);

                // Mainboard
                string mainboardManufacturer = "Unknown";
                string mainboardModel = "Unknown";
                string mainboardSerial = "Unknown";
                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product, SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        mainboardManufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "Unknown";
                        mainboardModel = obj["Product"]?.ToString()?.Trim() ?? "Unknown";
                        mainboardSerial = obj["SerialNumber"]?.ToString()?.Trim() ?? "Unknown";
                        break;
                    }
                }
                PrintInfo("Mainboard", $"{mainboardManufacturer} {mainboardModel}", ConsoleColor.Yellow);
                PrintInfo("Mainboard S/N", mainboardSerial);

                Console.WriteLine();

                // ═══════════════════════════════════════════════════════════
                // HỆ ĐIỀU HÀNH WINDOWS
                // ═══════════════════════════════════════════════════════════
                PrintSectionHeader("HỆ ĐIỀU HÀNH");

                string osName = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "Unknown")?.ToString() ?? "Unknown";
                string displayVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DisplayVersion", "")?.ToString() ?? "";
                string build = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild", "")?.ToString() ?? "";
                string ubr = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "UBR", "")?.ToString() ?? "";

                // Phát hiện Windows 11 dựa trên build number (Windows 11 bắt đầu từ build 22000)
                if (!string.IsNullOrEmpty(build) && int.TryParse(build, out int buildNumber))
                {
                    if (buildNumber >= 22000 && osName.Contains("Windows 10"))
                    {
                        osName = osName.Replace("Windows 10", "Windows 11");
                    }
                }

                string installDate = "";

                try
                {
                    int installDateInt = Convert.ToInt32(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallDate", 0));
                    if (installDateInt > 0)
                    {
                        DateTime installDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(installDateInt).ToLocalTime();
                        installDate = installDateTime.ToString("dd/MM/yyyy HH:mm");
                    }
                }
                catch { }

                string architecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
                string fullBuild = string.IsNullOrEmpty(ubr) ? build : $"{build}.{ubr}";

                PrintInfo("Hệ điều hành", osName, ConsoleColor.Cyan);
                PrintInfo("Phiên bản", displayVersion);
                PrintInfo("Build", fullBuild, ConsoleColor.Green);
                PrintInfo("Kiến trúc", architecture);
                PrintInfo("Ngày cài đặt", installDate);
                PrintInfo("Thư mục Windows", Environment.GetFolderPath(Environment.SpecialFolder.Windows));

                Console.WriteLine();

                // ═══════════════════════════════════════════════════════════
                // BỘ XỬ LÝ (CPU)
                // ═══════════════════════════════════════════════════════════
                PrintSectionHeader("BỘ XỬ LÝ (CPU)");

                using (var searcher = new ManagementObjectSearcher("SELECT Name, MaxClockSpeed, NumberOfCores, NumberOfLogicalProcessors, CurrentClockSpeed FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString()?.Trim() ?? "Unknown";
                        uint maxSpeedMHz = Convert.ToUInt32(obj["MaxClockSpeed"] ?? 0);
                        uint currentSpeedMHz = Convert.ToUInt32(obj["CurrentClockSpeed"] ?? 0);
                        double speedGHz = Math.Round(maxSpeedMHz / 1000.0, 2);
                        double currentGHz = Math.Round(currentSpeedMHz / 1000.0, 2);
                        uint cores = Convert.ToUInt32(obj["NumberOfCores"] ?? 0);
                        uint threads = Convert.ToUInt32(obj["NumberOfLogicalProcessors"] ?? 0);

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"  {name}");
                        Console.ResetColor();
                        PrintInfo("Số nhân", $"{cores} cores");
                        PrintInfo("Số luồng", $"{threads} threads", ConsoleColor.Yellow);
                        PrintInfo("Tốc độ tối đa", $"{speedGHz} GHz");
                        PrintInfo("Tốc độ hiện tại", $"{currentGHz} GHz", ConsoleColor.Green);
                        break;
                    }
                }
                Console.WriteLine();

                // ═══════════════════════════════════════════════════════════
                // CARD ĐỒ HỌA (GPU)
                // ═══════════════════════════════════════════════════════════
                PrintSectionHeader("CARD ĐỒ HỌA (GPU)");

                int gpuCount = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, DriverVersion, VideoModeDescription FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(name) && !name.Contains("Basic") && !name.Contains("Microsoft Remote"))
                        {
                            gpuCount++;
                            ulong vramBytes = Convert.ToUInt64(obj["AdapterRAM"] ?? 0);
                            double vramGB = Math.Round(vramBytes / 1024.0 / 1024.0 / 1024.0, 2);
                            string driver = obj["DriverVersion"]?.ToString()?.Trim() ?? "Unknown";
                            string resolution = obj["VideoModeDescription"]?.ToString()?.Trim() ?? "Unknown";

                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"  GPU #{gpuCount}: {name}");
                            Console.ResetColor();
                            if (vramGB > 0)
                            {
                                PrintInfo("VRAM", $"{vramGB} GB", ConsoleColor.Yellow);
                            }
                            PrintInfo("Driver", driver);
                            PrintInfo("Độ phân giải", resolution);
                            Console.WriteLine();
                        }
                    }
                }

                // ═══════════════════════════════════════════════════════════
                // BỘ NHỚ RAM
                // ═══════════════════════════════════════════════════════════
                PrintSectionHeader("BỘ NHỚ RAM");

                ulong totalRamKB = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalRamKB = Convert.ToUInt64(obj["TotalVisibleMemorySize"]);
                        ulong freeRamKB = Convert.ToUInt64(obj["FreePhysicalMemory"]);
                        double totalGB = Math.Round(totalRamKB / 1024.0 / 1024.0, 1);
                        double freeGB = Math.Round(freeRamKB / 1024.0 / 1024.0, 1);
                        double usedGB = Math.Round((totalRamKB - freeRamKB) / 1024.0 / 1024.0, 1);

                        PrintInfo("Tổng RAM", $"{totalGB} GB", ConsoleColor.Cyan);
                        PrintInfo("Đang sử dụng", $"{usedGB} GB", ConsoleColor.Yellow);
                        PrintInfo("Còn trống", $"{freeGB} GB", ConsoleColor.Green);
                        break;
                    }
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Chi tiết các thanh RAM:");
                Console.ResetColor();

                int usedSlots = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, DeviceLocator, Manufacturer, PartNumber FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong capacityBytes = Convert.ToUInt64(obj["Capacity"] ?? 0);
                        double capacityGB = Math.Round(capacityBytes / 1024.0 / 1024.0 / 1024.0, 1);
                        uint speed = Convert.ToUInt32(obj["Speed"] ?? 0);
                        string locator = obj["DeviceLocator"]?.ToString()?.Trim() ?? "Unknown";
                        string mfr = obj["Manufacturer"]?.ToString()?.Trim() ?? "";
                        string partNumber = obj["PartNumber"]?.ToString()?.Trim() ?? "";

                        Console.Write("  ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"{locator}:");
                        Console.ResetColor();
                        Console.Write($" {capacityGB} GB @ {speed} MHz");
                        if (!string.IsNullOrEmpty(mfr) && !string.IsNullOrEmpty(partNumber))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write($" ({mfr} {partNumber})");
                            Console.ResetColor();
                        }
                        Console.WriteLine();
                        usedSlots++;
                    }
                }

                int totalSlots = 0;
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            totalSlots += Convert.ToInt32(obj["MemoryDevices"] ?? 0);
                        }
                    }
                }
                catch { totalSlots = usedSlots > 0 ? usedSlots * 2 : 2; }

                Console.WriteLine();
                PrintInfo("Tổng số khe RAM", totalSlots.ToString());
                PrintInfo("Đã sử dụng", usedSlots.ToString());

                int emptySlots = totalSlots - usedSlots;
                if (emptySlots > 0)
                {
                    PrintInfo("Còn trống", $"{emptySlots} khe", ConsoleColor.Green);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ✓ Có thể nâng cấp thêm RAM");
                    Console.ResetColor();
                }
                else
                {
                    PrintInfo("Còn trống", "0 khe", ConsoleColor.Red);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ✗ Đã dùng hết khe RAM");
                    Console.ResetColor();
                }
                Console.WriteLine();

                // ═══════════════════════════════════════════════════════════
                // Ổ LƯU TRỮ (STORAGE)
                // ═══════════════════════════════════════════════════════════
                PrintSectionHeader("Ổ LƯU TRỮ (STORAGE)");

                using (var searcher = new ManagementObjectSearcher("SELECT Model, Size, InterfaceType, MediaType, DeviceID FROM Win32_DiskDrive"))
                {
                    int diskNum = 0;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        diskNum++;
                        string diskModel = obj["Model"]?.ToString()?.Trim() ?? "Unknown";
                        ulong sizeBytes = Convert.ToUInt64(obj["Size"] ?? 0);
                        double sizeGB = Math.Round(sizeBytes / 1024.0 / 1024.0 / 1024.0, 1);
                        string interfaceType = obj["InterfaceType"]?.ToString()?.Trim() ?? "Unknown";
                        string mediaType = obj["MediaType"]?.ToString()?.Trim() ?? "Unknown";
                        string deviceID = obj["DeviceID"]?.ToString()?.Trim() ?? "";

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"  Ổ cứng #{diskNum}: {diskModel}");
                        Console.ResetColor();
                        PrintInfo("Dung lượng", $"{sizeGB} GB", ConsoleColor.Yellow);
                        PrintInfo("Loại kết nối", interfaceType);

                        // Phân loại SSD/HDD - cải tiến
                        string driveType = DetectDriveType(diskModel, mediaType, deviceID);
                        PrintInfo("Loại ổ đĩa", driveType, driveType == "SSD" || driveType == "NVMe SSD" ? ConsoleColor.Green : ConsoleColor.White);
                        Console.WriteLine();
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Các phân vùng (Partitions):");
                Console.ResetColor();

                DriveInfo[] drives = DriveInfo.GetDrives();
                ulong totalPartitionBytes = 0;
                ulong totalFreeBytes = 0;

                foreach (DriveInfo drive in drives)
                {
                    if (drive.IsReady)
                    {
                        double totalGBDrive = Math.Round(drive.TotalSize / 1024.0 / 1024.0 / 1024.0, 1);
                        double freeGB = Math.Round(drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0, 1);
                        double usedGB = totalGBDrive - freeGB;
                        double percentUsed = Math.Round((usedGB / totalGBDrive) * 100, 1);

                        string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "No Label" : drive.VolumeLabel;

                        Console.Write("  ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"{drive.Name}");
                        Console.ResetColor();
                        Console.Write($" [{label}] ");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write($"({drive.DriveType})");
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine($"    Tổng: {totalGBDrive} GB | Dùng: {usedGB:F1} GB ({percentUsed}%) | Trống: {freeGB} GB | {drive.DriveFormat}");

                        // Cảnh báo nếu ổ sắp đầy
                        if (percentUsed > 90)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"    ⚠ Cảnh báo: Ổ đĩa sắp đầy!");
                            Console.ResetColor();
                        }
                        else if (percentUsed > 80)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"    ⚠ Lưu ý: Dung lượng ổ đĩa đang cao");
                            Console.ResetColor();
                        }
                    }
                }

                Console.WriteLine();

                // ═══════════════════════════════════════════════════════════
                // MẠNG (NETWORK)
                // ═══════════════════════════════════════════════════════════
                PrintSectionHeader("MẠNG (NETWORK)");

                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                int adapterCount = 0;

                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        adapterCount++;
                        IPInterfaceProperties ipProps = ni.GetIPProperties();

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"  Adapter #{adapterCount}: {ni.Name}");
                        Console.ResetColor();

                        PrintInfo("Loại", ni.NetworkInterfaceType.ToString());
                        PrintInfo("Trạng thái", ni.OperationalStatus.ToString(), ConsoleColor.Green);
                        PrintInfo("Tốc độ", $"{ni.Speed / 1000000} Mbps", ConsoleColor.Yellow);
                        PrintInfo("MAC Address", ni.GetPhysicalAddress().ToString(), ConsoleColor.Magenta);

                        foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                PrintInfo("IPv4", ip.Address.ToString(), ConsoleColor.Green);
                                PrintInfo("Subnet Mask", ip.IPv4Mask.ToString());
                            }
                        }

                        if (ipProps.GatewayAddresses.Count > 0)
                        {
                            PrintInfo("Gateway", ipProps.GatewayAddresses[0].Address.ToString());
                        }

                        if (ipProps.DnsAddresses.Count > 0)
                        {
                            PrintInfo("DNS", string.Join(", ", ipProps.DnsAddresses.Take(2).Select(d => d.ToString())));
                        }

                        Console.WriteLine();
                    }
                }

                // ═══════════════════════════════════════════════════════════
                // THỜI GIAN HOẠT ĐỘNG
                // ═══════════════════════════════════════════════════════════
                PrintSectionHeader("THỜI GIAN HOẠT ĐỘNG (UPTIME)");

                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                DateTime bootTime = DateTime.Now - uptime;

                PrintInfo("Thời gian bật máy", bootTime.ToString("dd/MM/yyyy HH:mm:ss"));
                PrintInfo("Uptime", $"{uptime.Days} ngày {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}", ConsoleColor.Cyan);

                if (uptime.Days > 7)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ⚠ Lưu ý: Máy chưa khởi động lại lâu rồi, nên restart để tối ưu hiệu suất");
                    Console.ResetColor();
                }
                Console.WriteLine();

                // ═══════════════════════════════════════════════════════════
                // PIN (Battery - nếu là laptop)
                // ═══════════════════════════════════════════════════════════
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT EstimatedChargeRemaining, BatteryStatus, EstimatedRunTime FROM Win32_Battery"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            PrintSectionHeader("PIN (BATTERY)");

                            uint charge = Convert.ToUInt32(obj["EstimatedChargeRemaining"] ?? 0);
                            ushort status = Convert.ToUInt16(obj["BatteryStatus"] ?? 0);
                            uint runTime = Convert.ToUInt32(obj["EstimatedRunTime"] ?? 0);

                            string statusText = status switch
                            {
                                1 => "Khác",
                                2 => "Đang sạc",
                                3 => "Đầy",
                                4 => "Thấp",
                                5 => "Rất thấp",
                                6 => "Đang sạc (Thấp)",
                                7 => "Đang sạc (Cao)",
                                8 => "Đang sạc (Đầy)",
                                9 => "Chưa xác định",
                                10 => "Đã sạc một phần",
                                _ => "Không xác định"
                            };

                            ConsoleColor batteryColor = charge > 80 ? ConsoleColor.Green :
                                                       charge > 30 ? ConsoleColor.Yellow :
                                                       ConsoleColor.Red;

                            PrintInfo("Mức pin", $"{charge}%", batteryColor);
                            PrintInfo("Trạng thái", statusText, status == 2 || status >= 6 ? ConsoleColor.Cyan : ConsoleColor.White);

                            if (runTime > 0 && runTime != 71582788)
                            {
                                int hours = (int)(runTime / 60);
                                int minutes = (int)(runTime % 60);
                                PrintInfo("Thời gian còn lại", $"{hours}h {minutes}m");
                            }

                            Console.WriteLine();
                            break;
                        }
                    }
                }
                catch { /* Không có pin hoặc không phải laptop */ }

            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Lỗi lấy thông tin: {ex.Message}");
            }

            ConsoleHelper.Pause();
        }

        private static void PrintSectionHeader(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ {title.PadRight(56)} ║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private static void PrintInfo(string label, string value, ConsoleColor valueColor = ConsoleColor.White)
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{label.PadRight(25)}: ");
            Console.ForegroundColor = valueColor;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        private static string DetectDriveType(string model, string mediaType, string deviceID)
        {
            string modelUpper = model.ToUpper();
            string mediaUpper = mediaType.ToUpper();

            // Kiểm tra NVMe
            if (modelUpper.Contains("NVME") || modelUpper.Contains("NVM EXPRESS"))
            {
                return "NVMe SSD";
            }

            // Danh sách thương hiệu SSD phổ biến
            string[] ssdBrands = { "SAMSUNG", "KINGSTON", "CRUCIAL", "SANDISK", "WD", "WESTERN DIGITAL",
                                   "INTEL", "CORSAIR", "ADATA", "TRANSCEND", "TOSHIBA", "SK HYNIX",
                                   "MICRON", "PLEXTOR", "MUSHKIN", "PATRIOT", "PNY", "LEXAR",
                                   "GIGABYTE", "APACER", "TEAM", "SILICON POWER" };

            // Kiểm tra từ khóa SSD trong tên model
            if (modelUpper.Contains("SSD") || modelUpper.Contains(" SSD ") || modelUpper.Contains("SOLID STATE"))
            {
                return "SSD";
            }

            // Kiểm tra thương hiệu SSD nổi tiếng
            foreach (string brand in ssdBrands)
            {
                if (modelUpper.Contains(brand))
                {
                    // Nếu có thương hiệu SSD nhưng không chắc chắn 100%
                    // Kiểm tra thêm một số từ khóa loại trừ HDD
                    if (!modelUpper.Contains("HDD") && !modelUpper.Contains("HARD DISK"))
                    {
                        // Đặc biệt với Kingston, WD, Toshiba - kiểm tra thêm
                        if (brand == "KINGSTON" || brand == "WD" || brand == "WESTERN DIGITAL" || brand == "TOSHIBA")
                        {
                            // Các model HDD thường có số vòng quay hoặc chữ "RPM"
                            if (modelUpper.Contains("7200") || modelUpper.Contains("5400") ||
                                modelUpper.Contains("RPM") || modelUpper.Contains("BLUE") && modelUpper.Contains("WD"))
                            {
                                return "HDD";
                            }
                        }
                        return "SSD";
                    }
                }
            }

            // Kiểm tra qua MediaType
            if (mediaUpper.Contains("SSD"))
            {
                return "SSD";
            }

            // Kiểm tra qua DeviceID (thường ổ USB/External)
            if (!string.IsNullOrEmpty(deviceID))
            {
                if (modelUpper.Contains("USB") || modelUpper.Contains("EXTERNAL"))
                {
                    return "External HDD";
                }
            }

            // Mặc định là HDD nếu không xác định được
            return "HDD";
        }
    }
}