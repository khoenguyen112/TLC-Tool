using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TLTool.Utils;

namespace TLTool
{
    public static class LicenseManager
    {
        private static readonly string licenseFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "tltool",
            "license.key");

        private static readonly string Prefix = "TL";
        private static readonly string SecretSalt = "KhoeNguyen112Secret2026"; // Đổi cái này thành chuỗi bí mật riêng

        public static void Manage()
        {
            while (true)
            {
                Console.Clear();
                ConsoleHelper.Header("QUẢN LÝ LICENSE KEY (OFFLINE)");

                string currentKey = ReadLicenseKey();
                if (!string.IsNullOrEmpty(currentKey))
                {
                    Console.WriteLine($"Key hiện tại: {currentKey}");
                    if (IsValidKey(currentKey))
                    {
                        ConsoleHelper.Success("Key hợp lệ và còn hạn!");
                    }
                    else
                    {
                        ConsoleHelper.Error("Key hết hạn hoặc không hợp lệ!");
                    }
                }
                else
                {
                    ConsoleHelper.Warning("Hiện chưa có key nào được lưu.");
                }
                Console.WriteLine();
                Console.WriteLine("1. Nhập/Đổi key mới");
                Console.WriteLine("2. Xóa key hiện tại");
                Console.WriteLine("0. Quay lại menu chính");
                Console.Write("\n➤ Chọn: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        Console.Write("Nhập key mới (dạng TL-YYYYMMDD-XXXXXX): ");
                        string newKey = Console.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(newKey))
                        {
                            if (IsValidKey(newKey))
                            {
                                SaveLicenseKey(newKey);
                                ConsoleHelper.Success("Key hợp lệ! Đã lưu và áp dụng ngay.");
                            }
                            else
                            {
                                ConsoleHelper.Error("Key không hợp lệ hoặc đã hết hạn!");
                            }
                        }
                        else
                        {
                            ConsoleHelper.Error("Key không được để trống!");
                        }
                        Thread.Sleep(2000);
                        break;

                    case "2":
                        if (File.Exists(licenseFile))
                        {
                            File.Delete(licenseFile);
                            ConsoleHelper.Success("Đã xóa key!");
                        }
                        else
                        {
                            ConsoleHelper.Warning("Không có key nào để xóa.");
                        }
                        Thread.Sleep(2000);
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

        // Làm public để Program gọi được
        public static bool IsPremium()
        {
            string key = ReadLicenseKey();
            return !string.IsNullOrEmpty(key) && IsValidKey(key);
        }

        public static bool IsValidKey(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length != 18 || !key.StartsWith(Prefix + "-"))
                return false;

            var parts = key.Split('-');
            if (parts.Length != 3) return false;

            if (parts[0] != Prefix) return false;

            if (!DateTime.TryParseExact(parts[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime expiry))
                return false;

            string signature = parts[2];
            string expectedSig = ComputeSignature(Prefix + parts[1]);

            if (signature != expectedSig) return false;

            return DateTime.Today <= expiry;
        }

        private static string ComputeSignature(string input)
        {
            string salted = input + SecretSalt;
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(salted));
                return Convert.ToBase64String(hash).Substring(0, 6).Replace("/", "X").ToUpper();
            }
        }

        public static void SaveLicenseKey(string key)
        {
            string dir = Path.GetDirectoryName(licenseFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Mã hóa đơn giản (KHÔNG reverse)
            string encrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
            File.WriteAllText(licenseFile, encrypted);
        }

        private static string ReadLicenseKey()
        {
            if (!File.Exists(licenseFile)) return null;

            string encrypted = File.ReadAllText(licenseFile);
            try
            {
                byte[] bytes = Convert.FromBase64String(encrypted);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }
    }
}