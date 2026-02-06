using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TLTool.Utils;

namespace TLTool.Modules
{
    public static class UsbBatchInstallModule
    {
        public static void Run()
        {
            // Mở cửa sổ console mới để cài app
            LaunchInNewConsole();
        }

        private static void LaunchInNewConsole()
        {
            try
            {
                // Lấy đường dẫn exe hiện tại
                string exePath = Process.GetCurrentProcess().MainModule.FileName;

                // Tạo process mới với cửa sổ console riêng
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "--install-apps", // Tham số đặc biệt
                    UseShellExecute = true, // Mở cửa sổ mới
                    CreateNoWindow = false
                };

                Process.Start(psi);

                Console.WriteLine("Đã mở cửa sổ cài đặt riêng!");
                Console.WriteLine("Bạn có thể tiếp tục dùng tool này bình thường.\n");
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error("Không thể mở cửa sổ mới: " + ex.Message);
                ConsoleHelper.Pause();
            }
        }

        // Hàm này sẽ chạy khi được gọi từ cửa sổ mới
        public static void RunInNewConsole()
        {
            ConsoleHelper.Header("CÀI APP NHANH");

            if (!TryAutoInstall())
                Environment.Exit(0);

            Console.WriteLine("\nHoàn tất! Cửa sổ sẽ tự động đóng sau 3 giây...");
            Thread.Sleep(3000);
            Environment.Exit(0); 
        }

        // ===== TÌM VÀ XỬ LÝ AUTO KEY =====
        private static bool TryAutoInstall()
        {
            try
            {
                // Quét tất cả ổ ngoài tìm file autoinstall.key
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (var drive in drives)
                {
                    if (!drive.IsReady || drive.Name == "C:\\") continue;

                    string keyFilePath = Path.Combine(drive.RootDirectory.FullName, "autoinstall.key");

                    if (File.Exists(keyFilePath))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"🔑 Tìm thấy file autoinstall.key trong ổ {drive.Name}");
                        Console.ResetColor();

                        // Đọc nội dung file
                        string content = File.ReadAllText(keyFilePath).Trim();

                        // Parse: KEY=abc123|FOLDER=MyFolder
                        var parts = content.Split('|');
                        if (parts.Length != 2)
                        {
                            ConsoleHelper.Warning("Format file key không đúng!");
                            continue;
                        }

                        string keyPart = parts[0].Trim();
                        string pathPart = parts[1].Trim();

                        if (!keyPart.StartsWith("KEY=") || !pathPart.StartsWith("FOLDER="))
                        {
                            ConsoleHelper.Warning("Format file key không đúng! Cần: KEY=xxx|FOLDER=yyy");
                            continue;
                        }

                        string expectedKey = keyPart.Substring(4).Trim();
                        string folderName = pathPart.Substring(7).Trim();

                        // Ghép đường dẫn đầy đủ
                        string targetPath = Path.Combine(drive.RootDirectory.FullName, folderName);

                        // Vòng lặp cho phép nhập lại key
                        while (true)
                        {
                            Console.Write($"\nNhập key để cài tự động (hoặc Enter để thoát): ");
                            string inputKey = Console.ReadLine()?.Trim();

                            // Nếu người dùng nhấn Enter mà không nhập gì → Thoát
                            if (string.IsNullOrEmpty(inputKey))
                            {
                                ConsoleHelper.Warning("Đã hủy! Nhấn phím bất kỳ để quay lại...");
                                Console.ReadKey();
                                return false;
                            }

                            // Kiểm tra key
                            if (inputKey != expectedKey)
                            {
                                ConsoleHelper.Error("❌ Key không đúng! Vui lòng thử lại.", pause: false);
                                continue; // Cho nhập lại
                            }

                            // Key đúng → Kiểm tra folder
                            if (!Directory.Exists(targetPath))
                            {
                                ConsoleHelper.Error($"Folder không tồn tại: {targetPath}");
                                ConsoleHelper.Warning("Nhấn phím bất kỳ để quay lại...");
                                Console.ReadKey();
                                return false;
                            }

                            // Bắt đầu cài tự động
                            ConsoleHelper.Success("✅ Key hợp lệ! Bắt đầu cài tự động...\n", pause: false);
                            System.Threading.Thread.Sleep(1000);

                            InstallFromFolder(targetPath);
                            return true; // Đã cài xong
                        }
                    }
                }

                // Không tìm thấy file key nào
                ConsoleHelper.Error("Không tìm thấy file autoinstall.key trong các ổ đĩa!");
                Console.WriteLine("\nCách sử dụng:");
                Console.WriteLine("1. Tạo file 'autoinstall.key' ở box");
                Console.WriteLine("2. Nội dung: KEY=matkhaucuaban|FOLDER=TenFolder");
                Console.WriteLine("3. Ví dụ: KEY=tlc123|FOLDER=app\n");
                ConsoleHelper.Warning("Nhấn phím bất kỳ để quay lại...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error("Lỗi đọc auto key: " + ex.Message);
                ConsoleHelper.Warning("Nhấn phím bất kỳ để quay lại...");
                Console.ReadKey();
            }

            return false; // Không tìm thấy key
        }

        private static void InstallFromFolder(string folderPath)
        {
            // Quét file .exe/.msi
            List<string> installers = new List<string>();
            try
            {
                installers.AddRange(Directory.GetFiles(folderPath, "*.exe"));
                installers.AddRange(Directory.GetFiles(folderPath, "*.msi"));
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error("Lỗi quét folder: " + ex.Message);
                return;
            }

            if (installers.Count == 0)
            {
                ConsoleHelper.Warning("Không tìm thấy file .exe hoặc .msi trong folder!");
                return;
            }

            Console.Clear();
            Console.WriteLine($"📁 Folder: {folderPath}");
            Console.WriteLine($"📦 Tìm thấy {installers.Count} installer\n");
            Console.WriteLine("Bắt đầu cài SONG SONG (nhanh x3-5 lần)...\n");

            int success = 0;
            int total = installers.Count;
            object lockObj = new object();

            DateTime startAll = DateTime.Now;

            // Cài song song tối đa 4 app cùng lúc
            Parallel.ForEach(installers, new ParallelOptions { MaxDegreeOfParallelism = 2 },
                (file, state, index) =>
                {
                    string fileName = Path.GetFileName(file);

                    lock (lockObj)
                    {
                        Console.WriteLine($"[{index + 1}/{total}] {fileName}");
                        Console.WriteLine("  Trạng thái: Đang chạy...");
                    }

                    DateTime start = DateTime.Now;
                    bool installed = InstallSilent(file);
                    TimeSpan duration = DateTime.Now - start;

                    lock (lockObj)
                    {
                        if (installed)
                        {
                            success++;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"  Trạng thái: Thành công! (Thời gian: {duration.TotalSeconds:F1}s)");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"  Trạng thái: Thất bại hoặc đã cài (Thời gian: {duration.TotalSeconds:F1}s)");
                            Console.ResetColor();
                        }
                        Console.WriteLine();
                    }
                });

            TimeSpan totalDuration = DateTime.Now - startAll;

            ConsoleHelper.Success($"Hoàn tất! Cài thành công {success}/{total} app.");
            Console.WriteLine($"Tổng thời gian: {totalDuration.TotalSeconds:F1}s (trung bình {totalDuration.TotalSeconds / total:F1}s/app)");
            // ===== BỎ DÒNG ConsoleHelper.Pause() Ở ĐÂY =====
        }

        private static bool InstallSilent(string filePath)
        {
            string arguments = GetSilentArguments(filePath);

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    // Bỏ redirect để tăng tốc
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                using Process process = Process.Start(psi);

                // Timeout 2 phút
                process.WaitForExit(120000);

                // Exit code 0 = thành công
                // Exit code 3010 = cần restart
                // Exit code 1641 = restart đã bắt đầu
                return process.ExitCode == 0 || process.ExitCode == 3010 || process.ExitCode == 1641;
            }
            catch
            {
                return false;
            }
        }

        private static string GetSilentArguments(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();

            // MSI files
            if (filePath.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
                return "/qn /norestart ALLUSERS=1";

            // Zalo - QUAN TRỌNG: /S phải viết HOA
            if (fileName.Contains("zalo"))
                return "/S /NCRC";

            // Zoom - arguments chuẩn
            if (fileName.Contains("zoom"))
                return "/silent /install";

            // Cốc Cốc
            if (fileName.Contains("coccoc") || fileName.Contains("coc coc"))
                return "/silent /install";

            // Chrome
            if (fileName.Contains("chrome"))
                return "/silent /install";

            // WinRAR - /S viết HOA
            if (fileName.Contains("winrar"))
                return "/S";

            // UltraViewer
            if (fileName.Contains("ultraviewer"))
                return "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-";

            // Foxit Reader
            if (fileName.Contains("foxit"))
                return "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-";

            // Unikey
            if (fileName.Contains("unikey"))
                return "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-";

            // K-Lite Codec
            if (fileName.Contains("k-lite") || fileName.Contains("klite"))
                return "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-";

            // Default cho Inno Setup installers
            return "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /CLOSEAPPLICATIONS /FORCECLOSEAPPLICATIONS";
        }
    }
}