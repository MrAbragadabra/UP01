using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using MewingLab.DB;


namespace MewingLab.Forms
{
    public partial class AdministratorWindow : Window
    {
        private users currentUser;
        private MewingLabEntities4 db = new MewingLabEntities4();
        List<users_history> allHistory;
        List<users> allLogin;
        private int sort = 2;
        public AdministratorWindow(users _currentUser)
        {
            InitializeComponent();
            currentUser = _currentUser;
        }

        private void administratorWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void administratorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Построение пути до фото
            string savePath = System.IO.Path.GetFullPath(@"..\Avatars");
            savePath = savePath + "\\" + currentUser.avatar_path;

            // Установка в элемент Image фото
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new System.Uri(savePath);
            bitmap.EndInit();
            userAvatar.Source = bitmap;

            // Установка имени
            userText.Text = $"Администратор {currentUser.second_name}. Логин: {currentUser.login}";

            // Добавление в ListView всей истории входа
            allHistory = db.users_history.ToList();
            historyListBox.ItemsSource = allHistory;

            allLogin = db.users.ToList();

            foreach (users i in allLogin)
            {
                loginCMB.Items.Add(i.login);
            }
        }

        private void sortingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sort % 2 == 0)
            {
                // Переворот истории
                List<users_history> allHistory = new List<users_history>();
                allHistory = db.users_history.ToList();
                allHistory.Reverse();
                historyListBox.ItemsSource = allHistory;
                sort++;
            }
            else
            {
                // Переворот истории
                List<users_history> allHistory = new List<users_history>();
                allHistory = db.users_history.ToList();
                historyListBox.ItemsSource = allHistory;
                sort++;
            }
        }

        private void createWorker_Click(object sender, RoutedEventArgs e)
        {
            // Открытие окна для создания рабочего
            CreateWorkerWindow createWorkerWindow = new CreateWorkerWindow();
            createWorkerWindow.ShowDialog();
        }

        private void loginCMB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            allHistory.Clear();
            historyListBox.ItemsSource = null;

            allHistory = db.users_history.Where(h => h.users.login == loginCMB.SelectedItem.ToString()).ToList();
            historyListBox.ItemsSource = allHistory;
        }
    }
}
