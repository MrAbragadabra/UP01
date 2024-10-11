using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using MewingLab.DB;


namespace MewingLab.Forms
{
    public partial class AccountantWindow : Window
    {
        private users currentUser;
        private MewingLabEntities4 db = new MewingLabEntities4();
        public AccountantWindow(users _currentUser)
        {
            InitializeComponent();
            currentUser = _currentUser;
        }

        private void accountantWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void accountantWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string savePath = System.IO.Path.GetFullPath(@"..\Avatars");
            savePath = savePath + "\\" + currentUser.avatar_path;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new System.Uri(savePath);
            bitmap.EndInit();
            userAvatar.Source = bitmap;

            userText.Text = $"Бухгалтер {currentUser.second_name}. Логин: {currentUser.login}";

            List<insurance_company> companies = new List<insurance_company>();
            companies = db.insurance_company.ToList();
            insuranceListView.ItemsSource = companies;

            List<services> services = new List<services>();
            services = db.services.ToList();
            servicesListView.ItemsSource = services;

        }
    }
}
