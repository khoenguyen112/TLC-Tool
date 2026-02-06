using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TLTool.Modules
{
    public static class QuickOptimizeModule
    {
        public static void Run()
        {
            Console.Clear();
            Console.WriteLine("=== QUICK OPTIMIZE SYSTEM ===\n");
            SetupOfficeTemplate();
            DisableBitLocker();
            SetVietnamTimezone();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nHoàn tất Quick Optimize!");
            Console.ResetColor();
            Console.WriteLine("Nhấn phím bất kỳ để thoát...");
            Console.ReadKey(true);
        }

        private static void SetupOfficeTemplate()
        {
            Console.WriteLine("[1/3] Thiết lập Office");
            try
            {
                Console.WriteLine("\n → Đóng tất cả tiến trình Office...");
                KillOfficeProcesses();
                System.Threading.Thread.Sleep(800);

                var versions = DetectAllOfficeVersions();
                if (versions.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ Không phát hiện Office nào - bỏ qua bước này");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine($"\n → Phát hiện {versions.Count} phiên bản Office:");
                foreach (var ver in versions)
                {
                    string name = ver switch
                    {
                        "16.0" => "Office 2016/2019/2021/365",
                        "15.0" => "Office 2013",
                        "14.0" => "Office 2010",
                        "12.0" => "Office 2007",
                        _ => $"Office {ver}"
                    };
                    Console.WriteLine($"   • {name} ({ver})");
                }

                Console.WriteLine("\n → Xóa Normal.dotm cũ nếu tồn tại...");
                DeleteOldNormalDotm();
                System.Threading.Thread.Sleep(400);

                Console.WriteLine("\n → Áp dụng cấu hình Registry...");
                foreach (var version in versions)
                {
                    Console.Write($"   • Phiên bản {version}: ");
                    ApplyFullOfficeConfiguration(version);
                }

                Console.WriteLine("\n → Tạo và cài đặt Normal.dotm mới (font/lề/giãn dòng mặc định)...");
                CreateAndInstallNormalDotm();

                Console.WriteLine("\n → Dọn cache Office...");
                CleanOfficeCache();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓ Hoàn tất thiết lập Office");
                Console.WriteLine("   Mọi tài liệu Word mới sẽ tự động dùng:");
                Console.WriteLine("   • Font: Times New Roman 14");
                Console.WriteLine("   • Giãn dòng: 1.15");
                Console.WriteLine("   • Khoảng cách đoạn: Trước/Sau 3pt");
                Console.WriteLine("   • Lề: Trên 2cm, Dưới 2cm, Trái 3cm, Phải 1.5cm");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Lỗi trong quá trình tối ưu Office: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static List<string> DetectAllOfficeVersions()
        {
            var found = new List<string>();
            string[] possibleVersions = { "16.0", "15.0", "14.0", "12.0" };

            foreach (string ver in possibleVersions)
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\Office\{ver}\Word");
                    if (key != null)
                    {
                        found.Add(ver);
                    }
                }
                catch { }
            }
            return found;
        }

        private static void KillOfficeProcesses()
        {
            string[] processNames = { "WINWORD", "EXCEL", "POWERPNT", "OUTLOOK", "ONENOTE" };
            bool killed = false;

            foreach (var procName in processNames)
            {
                var processes = Process.GetProcessesByName(procName);
                foreach (var proc in processes)
                {
                    try
                    {
                        proc.Kill();
                        proc.WaitForExit(2000);
                        Console.WriteLine($"   ✓ Đóng {procName} (PID: {proc.Id})");
                        killed = true;
                    }
                    catch { }
                }
            }

            if (!killed)
            {
                Console.WriteLine("   • Không có tiến trình Office nào đang chạy");
            }
        }

        private static void DeleteOldNormalDotm()
        {
            string[] possiblePaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Templates"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Word\STARTUP"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Word"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Word")
            };

            bool deleted = false;

            foreach (string basePath in possiblePaths)
            {
                if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath)) continue;

                try
                {
                    var files = Directory.GetFiles(basePath, "Normal.dotm", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                            Console.WriteLine($"   ✓ Xóa: {file}");
                            deleted = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"   ✗ Không xóa được {file}: {ex.Message}");
                        }
                    }
                }
                catch { }
            }

            if (!deleted)
            {
                Console.WriteLine("   • Không tìm thấy Normal.dotm nào");
            }
        }

        private static void CreateAndInstallNormalDotm()
        {
            string templatesFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Microsoft\Templates");

            Directory.CreateDirectory(templatesFolder); // đảm bảo thư mục tồn tại

            string normalDotmPath = Path.Combine(templatesFolder, "Normal.dotm");

            // Xóa file cũ nếu tồn tại
            if (File.Exists(normalDotmPath))
            {
                try
                {
                    File.SetAttributes(normalDotmPath, FileAttributes.Normal);
                    File.Delete(normalDotmPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   Cảnh báo: Không xóa được file cũ → {ex.Message}");
                }
            }

            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(
                    normalDotmPath, WordprocessingDocumentType.MacroEnabledTemplate))
                {
                    MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());

                    // Thêm styles part
                    StyleDefinitionsPart stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
                    stylesPart.Styles = new Styles();

                    // Tạo style Normal
                    Style normalStyle = new Style(
                        new StyleName { Val = "Normal" },
                        new BasedOn { Val = "" },
                        new StyleParagraphProperties(
                            new SpacingBetweenLines
                            {
                                Before = "60",     // 3 pt = 60 half-points
                                After = "60",
                                Line = "276",      // ≈1.15 lines (240 = 1.0, 276 ≈ 1.15)
                                LineRule = LineSpacingRuleValues.Auto
                            }
                        ),
                        new StyleRunProperties(
                            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", EastAsia = "Times New Roman" },
                            new FontSize { Val = "28" }  // 14 pt = 28 half-points
                        )
                    )
                    {
                        Type = StyleValues.Paragraph,
                        Default = true,
                        StyleId = "Normal"
                    };

                    stylesPart.Styles.Append(normalStyle);

                    // Set page margins mặc định cho template
                    SectionProperties sectionProps = new SectionProperties(
                        new PageMargin
                        {
                            Top = 1134,     // 2 cm
                            Bottom = 1134,  // 2 cm
                            Left = 1701,    // 3 cm
                            Right = 850     // 1.5 cm
                        }
                    );

                    mainPart.Document.Body.Append(sectionProps);

                    mainPart.Document.Save();
                }

                Console.WriteLine($"   ✓ Đã tạo Normal.dotm thành công tại: {normalDotmPath}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   ⚠ Không tạo được Normal.dotm: {ex.Message}");
                Console.WriteLine("   → Word sẽ tự tạo file mặc định khi mở lần đầu.");
                Console.ResetColor();
            }
        }

        private static void ApplyFullOfficeConfiguration(string version)
        {
            try
            {
                // Common
                using (var common = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{version}\Common\General"))
                {
                    common.SetValue("ShownFirstRunOptin", 1, RegistryValueKind.DWord);
                    common.SetValue("DisableBootToOfficeStart", 1, RegistryValueKind.DWord);
                }

                // Word
                using (var pv = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{version}\Word\Security\ProtectedView"))
                {
                    pv.SetValue("DisableInternetFilesInPV", 1, RegistryValueKind.DWord);
                    pv.SetValue("DisableUnsafeLocationsInPV", 1, RegistryValueKind.DWord);
                    pv.SetValue("DisableAttachmentsInPV", 1, RegistryValueKind.DWord);
                }

                using (var opt = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{version}\Word\Options"))
                {
                    opt.SetValue("DisableBootToOfficeStart", 1, RegistryValueKind.DWord);
                    opt.SetValue("DisableAutoRecover", 1, RegistryValueKind.DWord);
                    opt.SetValue("NoCapitalization", 1, RegistryValueKind.DWord);
                    opt.SetValue("NoTableAutoFormat", 1, RegistryValueKind.DWord);

                    opt.SetValue("DefaultTopMargin", 1134, RegistryValueKind.DWord);
                    opt.SetValue("DefaultBottomMargin", 1134, RegistryValueKind.DWord);
                    opt.SetValue("DefaultLeftMargin", 1701, RegistryValueKind.DWord);
                    opt.SetValue("DefaultRightMargin", 850, RegistryValueKind.DWord);
                }

                // Excel & PowerPoint
                foreach (var app in new[] { "Excel", "PowerPoint" })
                {
                    using (var pv = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{version}\{app}\Security\ProtectedView"))
                    {
                        pv.SetValue("DisableInternetFilesInPV", 1, RegistryValueKind.DWord);
                        pv.SetValue("DisableUnsafeLocationsInPV", 1, RegistryValueKind.DWord);
                        pv.SetValue("DisableAttachmentsInPV", 1, RegistryValueKind.DWord);
                    }

                    using (var opt = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{version}\{app}\Options"))
                    {
                        opt.SetValue("DisableBootToOfficeStart", 1, RegistryValueKind.DWord);
                        opt.SetValue("DisableAutoRecover", 1, RegistryValueKind.DWord);
                    }
                }

                Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }
        }

        private static void CleanOfficeCache()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] versions = { "16.0", "15.0", "14.0" };

            int deletedCount = 0;

            foreach (var ver in versions)
            {
                string cacheDir = Path.Combine(basePath, $@"Microsoft\Office\{ver}");
                if (!Directory.Exists(cacheDir)) continue;

                try
                {
                    var files = Directory.GetFiles(cacheDir, "*.tmp", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            deletedCount++;
                        }
                        catch { }
                    }
                }
                catch { }
            }

            Console.WriteLine($"   ✓ Đã xóa {deletedCount} file cache");
        }

        private static void DisableBitLocker()
        {
            Console.WriteLine("\n[2/3] Tắt BitLocker");
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "manage-bde",
                    Arguments = "-status C:",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                using var proc = Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                if (output.Contains("Protection On"))
                {
                    Console.WriteLine(" → BitLocker đang bật → Yêu cầu tắt...");
                    var offPsi = new ProcessStartInfo
                    {
                        FileName = "manage-bde",
                        Arguments = "-off C:",
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(offPsi)?.WaitForExit(5000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Đã yêu cầu tắt BitLocker (decrypt chạy nền)");
                }
                else if (output.Contains("Protection Off"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ BitLocker đã tắt");
                }
                else
                {
                    Console.WriteLine(" → Không phát hiện BitLocker trên ổ C:");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ Không xử lý được BitLocker: {ex.Message}");
            }
            Console.ResetColor();
        }

        private static void SetVietnamTimezone()
        {
            Console.WriteLine("\n[3/3] Thiết lập múi giờ Việt Nam");
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "tzutil",
                    Arguments = "/s \"SE Asia Standard Time\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                using var proc = Process.Start(psi);
                proc.WaitForExit();

                if (proc.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Đã đặt múi giờ Việt Nam (GMT+7)");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ Lệnh tzutil thất bại");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Lỗi: {ex.Message}");
            }
            Console.ResetColor();
        }
    }
}