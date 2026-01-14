using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TLTool.Utils;

namespace TLTool.Modules
{
    public static class UsbBatchInstallModule
    {
        public static void Run()
        {
            ConsoleHelper.Header("CÀI APP NHANH TỪ Ổ NGOÀI");

            Console.WriteLine("Tool sẽ liệt kê các thư mục trong ổ.");
            Console.WriteLine("Sau khi chọn thư mục → cài silent tất cả .exe/.msi trong đó.\n");

            // Lấy ổ
            DriveInfo[] drives = DriveInfo.GetDrives();
            List<DriveInfo> validDrives = drives
                .Where(d => d.IsReady &&
                           (d.DriveType == DriveType.Removable ||
                            d.DriveType == DriveType.CDRom ||
                            d.DriveType == DriveType.Fixed) &&
                           d.Name != "C:\\")
                .ToList();

            if (validDrives.Count == 0)
            {
                ConsoleHelper.Error("Không tìm thấy ổ nào phù hợp!");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine("Chọn ổ:");
            for (int i = 0; i < validDrives.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. Ổ {validDrives[i].Name} ({validDrives[i].VolumeLabel})");
            }

            Console.Write("\nChọn số ổ (0 hủy): ");
            if (!int.TryParse(Console.ReadLine(), out int driveChoice) || driveChoice == 0 || driveChoice > validDrives.Count)
            {
                return;
            }

            string rootPath = validDrives[driveChoice - 1].RootDirectory.FullName;

            // Liệt kê thư mục con
            List<string> folders = new List<string>();
            try
            {
                folders = Directory.GetDirectories(rootPath)
                    .OrderBy(d => d)
                    .ToList();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error("Lỗi đọc ổ: " + ex.Message);
                ConsoleHelper.Pause();
                return;
            }

            if (folders.Count == 0)
            {
                ConsoleHelper.Warning("Không có thư mục nào trong ổ!");
                ConsoleHelper.Pause();
                return;
            }

            Console.Clear();
            Console.WriteLine($"Ổ {rootPath} có {folders.Count} thư mục:\n");

            for (int i = 0; i < folders.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {Path.GetFileName(folders[i])}");
            }

            Console.Write("\nChọn số thư mục để cài (0 hủy): ");
            if (!int.TryParse(Console.ReadLine(), out int folderChoice) || folderChoice == 0 || folderChoice > folders.Count)
            {
                return;
            }

            string selectedFolder = folders[folderChoice - 1];

            List<string> installers = new List<string>();
            try
            {
                installers.AddRange(SafeEnumerateFiles(selectedFolder, "*.exe"));
                installers.AddRange(SafeEnumerateFiles(selectedFolder, "*.msi"));
            }
            catch (Exception ex)
            {
                ConsoleHelper.Warning("Lỗi quét thư mục: " + ex.Message);
            }

            if (installers.Count == 0)
            {
                ConsoleHelper.Warning("Không tìm thấy file .exe hoặc .msi trong thư mục này!");
                ConsoleHelper.Pause();
                return;
            }

            Console.Clear();
            Console.WriteLine($"Tìm thấy {installers.Count} installer trong:");
            Console.WriteLine($"   {selectedFolder}\n");

            Console.WriteLine("Bắt đầu cài silent (tiến trình thật)...\n");

            int success = 0;
            int total = installers.Count;

            for (int i = 0; i < total; i++)
            {
                string file = installers[i];
                string fileName = Path.GetFileName(file);

                Console.WriteLine($"[{i + 1}/{total}] {fileName}");
                Console.WriteLine("  Trạng thái: Đang chạy...");

                bool installed = InstallSilent(file);

                if (installed)
                {
                    success++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  Trạng thái: Thành công!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  Trạng thái: Thất bại hoặc đã cài");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            ConsoleHelper.Success($"Hoàn tất! Cài thành công {success}/{total} app.");
            ConsoleHelper.Pause();
        }

        // ... SafeEnumerateFiles giữ nguyên như cũ

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
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using Process process = Process.Start(psi);

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine($"  {e.Data}");
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine($"  {e.Data}");
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit(300000); // Timeout 5 phút

                return process.ExitCode == 0 || process.ExitCode == 3010 || process.ExitCode == 1641; // Các code thành công phổ biến
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Lỗi: {ex.Message}");
                return false;
            }
        }

        private static string GetSilentArguments(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();

            if (filePath.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
                return "/quiet /norestart";

            string strong = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /CLOSEAPPLICATIONS /FORCECLOSEAPPLICATIONS";

            if (fileName.Contains("chrome")) return "/silent /install";
            if (fileName.Contains("firefox")) return "-ms";
            if (fileName.Contains("coccoc") || fileName.Contains("coc coc")) return "/silent /install";
            if (fileName.Contains("winrar")) return "/s";
            if (fileName.Contains("ultraviewer")) return "/VERYSILENT /NORESTART";

            return strong;
        }
        // Hàm quét file an toàn, bỏ qua thư mục bị khóa quyền (System Volume Information, $RECYCLE.BIN...)
        private static IEnumerable<string> SafeEnumerateFiles(string path, string searchPattern)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);

            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                string[] files = null;
                string[] subDirs = null;

                try
                {
                    files = Directory.GetFiles(path, searchPattern);
                }
                catch { /* Bỏ qua lỗi quyền hoặc lỗi khác */ }

                if (files != null)
                {
                    foreach (string file in files)
                        yield return file;
                }

                try
                {
                    subDirs = Directory.GetDirectories(path);
                }
                catch { /* Bỏ qua */ }

                if (subDirs != null)
                {
                    foreach (string subDir in subDirs)
                        queue.Enqueue(subDir);
                }
            }
        }
    }
}