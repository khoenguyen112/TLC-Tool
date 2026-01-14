using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace TLTool.Utils
{
    public static class ConsoleHelper
    {
        private static Random rand = new Random();

        // ================= MATRIX INTRO =================
        public static void MatrixRainIntro()
        {
            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();

            int width = Console.WindowWidth - 1;
            int height = Console.WindowHeight;

            int[] y = new int[width];
            for (int i = 0; i < width; i++)
                y[i] = rand.Next(height);

            DateTime start = DateTime.Now;
            int rainDurationMs = 20000; // 20 giây

            while ((DateTime.Now - start).TotalMilliseconds < rainDurationMs)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    break;
                }

                for (int x = 0; x < width; x++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.SetCursorPosition(x, y[x]);
                    Console.Write(MatrixChar());

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    int tail = y[x] - 5;
                    if (tail >= 0)
                    {
                        Console.SetCursorPosition(x, tail);
                        Console.Write(MatrixChar());
                    }

                    int clear = y[x] - 20;
                    if (clear >= 0)
                    {
                        Console.SetCursorPosition(x, clear);
                        Console.Write(' ');
                    }

                    y[x]++;
                    if (y[x] >= height) y[x] = 0;
                }

                // Cảnh báo ngẫu nhiên
                if (rand.Next(1000) < 6)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.SetCursorPosition(rand.Next(width - 20), rand.Next(height));
                    Console.Write("TRUY CẬP TRÁI PHÉP");
                }

                Thread.Sleep(50);
            }

            Console.Clear();
            Console.CursorVisible = true;
            Console.ForegroundColor = ConsoleColor.Green;
        }

        static char MatrixChar()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$%&";
            return chars[rand.Next(chars.Length)];
        }


        // ================= LOGO TRUNG TÂM =================
        public static void HackerWelcome()
        {
            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.DarkRed;

            string asciiArt = @"
 ⠀⠀⠀.o oOOOOOOOo                                            OOOo
   Ob.OOOOOOOo  OOOo.      oOOo.                      .adOOOOOOO
   OboO"""""""""""""""""""""""".OOo. .oOOOOOo.    OOOo.oOOOOOo..""""""""""""""""""'OO
   OOP.oOOOOOOOOOOO ""POOOOOOOOOOOo.   `""OOOOOOOOOP,OOOOOOOOOOOB'
   `O'OOOO'     `OOOOo""OOOOOOOOOOO` .adOOOOOOOOO""oOOO'    `OOOOo
   .OOOO'            `OOOOOOOOOOOOOOOOOOOOOOOOOO'            `OO
   OOOOO                 '""OOOOOOOOOOOOOOOO""`                oOO
  oOOOOOba.                .adOOOOOOOOOOba               .adOOOOo.
 oOOOOOOOOOOOOOba.    .adOOOOOOOOOO@^OOOOOOOba.     .adOOOOOOOOOOOO
OOOOOOOOOOOOOOOOO.OOOOOOOOOOOOOO""`  '""OOOOOOOOOOOOO.OOOOOOOOOOOOOO
""OOOO""       ""YOoOOOOMOIONODOO""`  .   '""OOROAOPOEOOOoOY""     ""OOO""
   Y           'OOOOOOOOOOOOOO: .oOOo. :OOOOOOOOOOO?'         :`
   :            .oO%OOOOOOOOOOo.OOOOOO.oOOOOOOOOOOOO?         .
   .            oOOP""%OOOOOOOOoOOOOOOO?oOOOOO?OOOO""OOo
                '%o  OOOO""%OOOO%""%OOOOO""OOOOOO""OOO':
                     `$""  `OOOO' `O""Y ' `OOOO'  o             .
   .                  .     OP""          : o     .
                             :
                             .

                 CHÀO MỪNG ĐẾN VỚI TLC TOOL
";

            string[] lines = asciiArt.Trim().Split('\n');

            int topPadding = (Console.WindowHeight - lines.Length) / 2;
            if (topPadding < 0) topPadding = 0;

            for (int i = 0; i < topPadding; i++)
                Console.WriteLine();

            int maxLength = lines.Max(l => l.Length);
            int leftPadding = (Console.WindowWidth - maxLength) / 2;
            if (leftPadding < 0) leftPadding = 0;

            foreach (string line in lines)
                Console.WriteLine(new string(' ', leftPadding) + line);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Beep(600, 200);
            Console.Beep(900, 200);
            Console.Beep(1200, 400);

            //Thread.Sleep(3000);
            DateTime waitStart = DateTime.Now;
            int waitMs = 5000;

            while ((DateTime.Now - waitStart).TotalMilliseconds < waitMs)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    break;
                }
                Thread.Sleep(50);
            }

        }

        // ================= GÕ CHẬM KIỂU HACKER =================
        public static void SlowType(string text, int delay = 50)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);
                if (rand.Next(10) < 2)
                    Console.Beep(700 + rand.Next(300), 20);
            }
            Console.WriteLine();
        }

        // ================= THANH GIẢ TIẾN TRÌNH =================
        public static void FakeProgress(string action)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(action);

            for (int i = 0; i <= 100; i += rand.Next(5, 12))
            {
                Console.Write($"\r{action} [{new string('#', i / 5)}{new string(' ', 20 - i / 5)}] {i}%");
                Thread.Sleep(rand.Next(80, 150));
            }

            Console.WriteLine($"\r{action} [####################] 100% HOÀN TẤT");
            Console.Beep(2000, 400);
            Console.ResetColor();
        }

        // ================= MENU CHÍNH =================
        public static void ShowMainMenu(bool isPremium = false)
        {
            Console.Clear();
            Banner();

            int width = Console.WindowWidth;
            int boxWidth = 46;
            int left = (width - boxWidth) / 2;
            if (left < 0) left = 0;

            void WriteBoxLine(string text, ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.SetCursorPosition(left, Console.CursorTop);
                Console.WriteLine(text);
                Console.ResetColor();
            }

            // ===== KHUNG FREE / PREMIUM (CĂN GIỮA CHUẨN) =====
            WriteBoxLine("╔══════════════════════════════════════════╗",
                isPremium ? ConsoleColor.Cyan : ConsoleColor.Yellow);

            WriteBoxLine(
                isPremium
                    ? "║          PHIÊN BẢN PREMIUM               ║"
                    : "║             CHẾ ĐỘ FREE                  ║",
                isPremium ? ConsoleColor.Cyan : ConsoleColor.Yellow
            );

            WriteBoxLine("╚══════════════════════════════════════════╝",
                isPremium ? ConsoleColor.Cyan : ConsoleColor.Yellow);

            Console.WriteLine();

            // ===== MENU CHỨC NĂNG =====
            Console.ForegroundColor = ConsoleColor.Green;

            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[1] Kiểm tra thông tin hệ thống");

            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[2] Kích hoạt Windows & Office");

            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[3] Tinh chỉnh nhanh");

            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[4] Chẩn đoán phần cứng");

            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[5] Quản lý máy in");

            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[6] Quản lý key");

            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[7] Cài app nhanh offline");


            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[8] Cài app nhanh online");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.SetCursorPosition(left, Console.CursorTop);
            Console.WriteLine("[0] >> Thoát");

            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(left, Console.CursorTop);
            Console.Write("> Nhập lựa chọn: ");
        }



        // ================= BANNER =================
        public static void Banner()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            string banner = @"
 _____  _   _ ___ _____ _   _   _     ___   ____   _____ ___   ___  _     
|_   _| | | |_ _| ____| \ | | | |   / _ \ / ___| |_   _/ _ \ / _ \| |    
  | | | |_| || ||  _| |  \| | | |  | | | | |       | || | | | | | | |    
  | | |  _  || || |___| |\  | | |__| |_| | |___    | || |_| | |_| | |___ 
  |_| |_| |_|___|_____|_| \_| |_____\___/ \____|   |_| \___/ \___/|_____|";

            string[] lines = banner.Trim().Split('\n');
            int maxLen = lines.Max(l => l.Length);
            int left = (Console.WindowWidth - maxLen) / 2;
            if (left < 0) left = 0;

            foreach (string line in lines)
            {
                Console.SetCursorPosition(left, Console.CursorTop);
                Console.WriteLine(line);
            }

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;

            string version = "Phiên bản DEMO ";
            int vLeft = (Console.WindowWidth - version.Length) / 2;
            if (vLeft < 0) vLeft = 0;

            Console.SetCursorPosition(vLeft, Console.CursorTop);
            Console.WriteLine(version);

            Console.ResetColor();
            Console.WriteLine();

        }


        // ================= TIÊU ĐỀ CHỨC NĂNG =================
        public static void Header(string text)
        {
            Console.Clear();
            Banner();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"=== {text.ToUpper()} ===\n");
            Console.ResetColor();
        }

        public static void Success(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
            Pause();
        }

        public static void Error(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
            Pause();
        }

        public static void Warning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void Pause()
        {
            Console.WriteLine("\nNhấn phím bất kỳ để tiếp tục...");
            Console.ReadKey(true);
        }

        // ================= CHẠY LỆNH =================
        public static void RunCommand(string command, bool usePowerShell = true)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = usePowerShell ? "powershell" : "cmd",
                Arguments = usePowerShell ? $"-Command \"{command}\"" : $"/c {command}",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = Process.Start(psi)!;
            process.WaitForExit();

            if (process.ExitCode != 0)
                Warning("Cảnh báo: lệnh có thể không thực thi thành công.");
        }

        public static void SetTitle(string title)
        {
            Console.Title = title;
        }

        internal static void Info(string v)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(v);
            Console.ResetColor();
        }

        internal static void PressAnyKeyToContinue()
        {
            throw new NotImplementedException();
        }
    }
}
