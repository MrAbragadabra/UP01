using System.Text;
using System;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;

namespace MewingLab.Classes
{
    /*
     Класс Helper будет работать как помощник, делая частые действия более просто, например вывод сообщений.
     */
    public static class Helper
    {
        /*
         функция Message позволяет выводить какую-либо информацию в виде MessageBox
         Принимает фукнция сам текст, который нужно отобразить и принимает тип сообщения (good, error)
         Если тип good, то будет выводить сообщение с иконкой Information, а если тип error, то будет 
         выведен MessageBox с иконкой ошибки
         */
        public static void Message(string messageText, string type)
        {
            switch (type)
            {
                case "good":
                    MessageBox.Show(messageText, "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "error":
                    MessageBox.Show(messageText, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    MessageBox.Show(messageText, "Сообщение", MessageBoxButton.OK);
                    break;
            }
        }

        public static string GenerateCaptcha(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder password = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }
        public static void SaveBlockTime()
        {
            RegistryKey currentUserKey = Registry.CurrentUser;

            RegistryKey blockTimeKey = currentUserKey.OpenSubKey("MewingLaba", true);

            if (blockTimeKey == null)
            {
                RegistryKey blockTimeKeyCreate = currentUserKey.CreateSubKey("MewingLaba");
                blockTimeKeyCreate.SetValue("time", DateTime.Now.ToString());
                blockTimeKey.Close();

                return;
            }

            blockTimeKey.SetValue("time", DateTime.Now.ToString());
            blockTimeKey.Close();
        }
    }
}
