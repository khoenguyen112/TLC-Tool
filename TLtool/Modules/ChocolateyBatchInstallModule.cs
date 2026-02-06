//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Threading;
//using TLTool.Utils;

//namespace TLtool.Modules
//{
//    public static class ChocolateyBatchInstallModule
//    {
//        public static void Run()
//        {
//            ConsoleHelper.Header("CÀI APP HÀNG LOẠT QUA .NUPKG OFFLINE (TRONG TOOL)");

//            // Check và cài Chocolatey nếu chưa có
//            if (!IsChocoReady())
//            {
//                InstallChocolatey();
//            }

//            while (true)
//            {
//                Console.WriteLine("1. Cài hàng loạt app phổ biến");
//                Console.WriteLine("2. Chọn app để cài (nhiều app cùng lúc)");
//                Console.WriteLine("0. Quay lại");
//                Console.Write("\nChọn: ");
//                string choice = Console.ReadLine()?.Trim();

//                if (choice == "1")
//                {
//                    InstallPredefinedBatch();
//                }
//                else if (choice == "2")
//                {
//                    InstallCustomSelection();
//                }
//                else if (choice == "0")
//                {
//                    return;
//                }
//                else
//                {
//                    ConsoleHelper.Error("Lựa chọn không hợp lệ!");
//                }
//            }
//        }

//        private static bool IsChocoReady()
//        {
//            try
//            {
//                ProcessStartInfo psi = new ProcessStartInfo("choco", "--version")
//                {
//                    UseShellExecute = false,
//                    RedirectStandardOutput = true,
//                    CreateNoWindow = true
//                };
//                using Process p = Process.Start(psi);
//                p.WaitForExit(5000);
//                return p.ExitCode == 0;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private static void InstallChocolatey()
//        {
//            Console.WriteLine("Chocolatey chưa có - Đang cài tự động...\n");
//            try
//            {
//                ProcessStartInfo psi = new ProcessStartInfo("powershell", "-NoProfile -ExecutionPolicy Bypass -Command \"Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))\"")
//                {
//                    UseShellExecute = true,
//                    Verb = "runas"
//                };
//                Process.Start(psi).WaitForExit();
//                Thread.Sleep(5000);
//                ConsoleHelper.Success("Đã cài Chocolatey thành công! Bắt đầu cài app...");
//            }
//            catch (Exception ex)
//            {
//                ConsoleHelper.Error("Không cài được Chocolatey: " + ex.Message);
//                ConsoleHelper.Warning("Chạy tool với quyền Admin và kết nối mạng ổn định.");
//            }
//        }

//        private static void InstallPredefinedBatch()
//        {
//            var apps = GetAppList();
//            Console.Clear();
//            Console.WriteLine("Bắt đầu cài hàng loạt từ .nupkg trong tool...\n");

//            int success = 0;
//            for (int i = 0; i < apps.Count; i++)
//            {
//                string appName = apps[i].Name;
//                string appId = apps[i].Id;

//                Console.WriteLine($"[{i + 1}/{apps.Count}] Đang cài {appName}...");

//                bool installed = InstallFromEmbeddedNupkg(appId);

//                if (installed)
//                    success++;
//            }

//            ConsoleHelper.Success($"Hoàn tất! Cài thành công {success}/{apps.Count} app.");
//            ConsoleHelper.Pause();
//        }

//        private static void InstallCustomSelection()
//        {
//            var apps = GetAppList();
//            Console.Clear();
//            Console.WriteLine("CHỌN APP ĐỂ CÀI (nhiều app cùng lúc)\n");

//            for (int i = 0; i < apps.Count; i++)
//            {
//                Console.WriteLine($" {i + 1,2}. {apps[i].Name}");
//            }

//            Console.WriteLine("\nCách chọn: 1,3,5 hoặc 1-5 hoặc all hoặc 0 hủy");
//            Console.Write("Nhập: ");
//            string input = Console.ReadLine()?.Trim().ToLower();

//            if (string.IsNullOrEmpty(input) || input == "0") return;

//            List<string> selected = new List<string>();

//            if (input == "all")
//            {
//                selected.AddRange(apps.Select(a => a.Id));
//            }
//            else
//            {
//                foreach (string part in input.Split(','))
//                {
//                    if (part.Contains("-"))
//                    {
//                        var range = part.Split('-');
//                        if (int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
//                        {
//                            for (int j = start; j <= end; j++)
//                            {
//                                var item = apps.ElementAtOrDefault(j - 1);
//                                if (item != default)
//                                {
//                                    selected.Add(item.Id);
//                                }
//                            }
//                        }
//                    }
//                    else if (int.TryParse(part, out int idx))
//                    {
//                        var item = apps.ElementAtOrDefault(idx - 1);
//                        if (item != default)
//                        {
//                            selected.Add(item.Id);
//                        }
//                    }
//                }
//            }

//            if (selected.Count == 0)
//            {
//                ConsoleHelper.Error("Không chọn app nào!");
//                return;
//            }

//            Console.Clear();
//            Console.WriteLine($"Bắt đầu cài {selected.Count} app từ .nupkg trong tool...\n");

//            int success = 0;
//            int count = 0;
//            foreach (string id in selected)
//            {
//                count++;
//                string appName = apps.FirstOrDefault(a => a.Id == id).Name ?? id;
//                Console.WriteLine($"[{count}/{selected.Count}] Đang cài {appName}...");
//                if (InstallFromEmbeddedNupkg(id))
//                    success++;
//            }

//            ConsoleHelper.Success($"Hoàn tất! Cài thành công {success}/{selected.Count} app.");
//            ConsoleHelper.Pause();
//        }

//        private static bool InstallFromEmbeddedNupkg(string appId)
//        {
//            try
//            {
//                string tempDir = Path.Combine(Path.GetTempPath(), $"choco_nupkg_{appId}");
//                Directory.CreateDirectory(tempDir);

//                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
//                string resourceName = $"{assemblyName}.Packages.{appId}.nupkg";

//                string nupkgPath = Path.Combine(tempDir, $"{appId}.nupkg");

//                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
//                {
//                    if (stream == null)
//                    {
//                        ConsoleHelper.Error($"Không tìm thấy file {appId}.nupkg trong tool!");
//                        ConsoleHelper.Error($"Kiểm tra: Build Action của file phải là Embedded Resource, và tên file khớp với '{appId}.nupkg'");
//                        return false;
//                    }

//                    using (FileStream fs = new FileStream(nupkgPath, FileMode.Create))
//                    {
//                        stream.CopyTo(fs);
//                    }
//                }

//                Console.WriteLine($"Đã extract {appId}.nupkg từ tool...");
//                Console.WriteLine("Đang cài silent...\n");

//                string chocoArgs = $"install \"{appId}\" -y --force --source=\"{tempDir}\"";

//                bool success = RunChoco(chocoArgs);

//                try { Directory.Delete(tempDir, true); } catch { }

//                return success;
//            }
//            catch (Exception ex)
//            {
//                ConsoleHelper.Error($"Lỗi cài {appId} từ .nupkg trong tool: {ex.Message}");
//                return false;
//            }
//        }

//        private static bool RunChoco(string arguments)
//        {
//            try
//            {
//                ProcessStartInfo psi = new ProcessStartInfo("choco", arguments)
//                {
//                    UseShellExecute = false,
//                    RedirectStandardOutput = true,
//                    RedirectStandardError = true,
//                    CreateNoWindow = true
//                };

//                using Process process = Process.Start(psi);

//                process.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine($"  {e.Data}"); };
//                process.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine($"  {e.Data}"); };

//                process.BeginOutputReadLine();
//                process.BeginErrorReadLine();

//                process.WaitForExit(300000);

//                return process.ExitCode == 0;
//            }
//            catch (Exception ex)
//            {
//                ConsoleHelper.Error($"Lỗi chạy choco: {ex.Message}");
//                return false;
//            }
//        }

//        private static List<(string Name, string Id)> GetAppList()
//        {
//            return new List<(string Name, string Id)>
//    {
//        ("Google Chrome", "chrome.131.0.0"),
//        ("Cốc Cốc Browser", "coccoc.131.0.0"),
//        ("UltraViewer", "ultraviewer.6.6.124"),
//        ("Foxit Reader", "foxitreader.2025.3.1"),
//        ("WinRAR", "winrar.7.1.3"),
//        ("Zalo", "zalo.25.12.11"),
//        ("UniKey", "unikey.4.6.0"),
//        ("Zoom", "zoom.6.0.0")
//    };
//        }
//    }
//}