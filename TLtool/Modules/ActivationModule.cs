using System;
using System.Diagnostics;
using TLTool.Utils;

namespace TLTool.Modules
{
    public static class ActivationModule
    {
        public static void Run()
        {
            // Chạy thẳng MAS mà không hỏi gì cả
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"irm https://get.activated.win | iex\"",
                UseShellExecute = true,     // Mở cửa sổ PowerShell riêng
                Verb = "runas"              // Yêu cầu quyền Admin tự động (hỏi UAC nếu chưa có)
            };

            try
            {
                Process.Start(psi);

                // Thông báo ngắn gọn trong console tool (tùy chọn, mày có thể xóa nếu không muốn hiện gì)
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Đang chạy Microsoft Activation Scripts (MAS)...");
                Console.WriteLine("Cửa sổ PowerShell mới đã mở. Làm theo hướng dẫn bên đó.");
                Console.WriteLine("Sau khi xong, đóng cửa sổ MAS để tiếp tục dùng tool.");
                Console.ResetColor();

                // Không pause, tự về menu khi người dùng quay lại
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"Không thể chạy MAS: {ex.Message}");
            }
        }
    }
}