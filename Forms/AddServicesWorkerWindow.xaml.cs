using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MewingLab.Classes;
using MewingLab.DB;


namespace MewingLab.Forms
{
    public partial class AddServicesWorkerWindow : Window
    {
        private MewingLabEntities4 db = new MewingLabEntities4();
        private List<services> servicesList = new List<services>();
        private List<services> SelectedServicesList = new List<services>();
        public AddServicesWorkerWindow()
        {
            InitializeComponent();

            // Получение спискаиз БД
            servicesList = db.services.ToList();
        }

        private void addServicesWorkerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Привязка к ComboBox данных
            servicesCMB.ItemsSource = servicesList;
        }

        private void servicesCMB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // При нажатии на элемент из списка услуг, проходимся по
            // Списку выбранных услуг, если такой есть уже, то ничего не делаем
            // Если нет, то добавляем
            for (int i = 0; i < SelectedServicesList.Count; i++)
            {
                if (SelectedServicesList[i] == (services)servicesCMB.SelectedItem)
                {
                    SelectedServicesList.Remove((services)servicesCMB.SelectedItem);
                }
            }

            SelectedServicesList.Add((services)servicesCMB.SelectedItem);

            // Обновление списка
            selectedServicesCMB.ItemsSource = null;
            selectedServicesCMB.ItemsSource = SelectedServicesList;
        }

        private void addServicesToUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Если услуги есть в списке, то добавляем в базу все услуги в пользователя
            if (SelectedServicesList.Count != 0)
            {
                users last = db.users.OrderByDescending(u => u.id).FirstOrDefault();
                foreach (services i in selectedServicesCMB.Items)
                {
                    services_in_user add = new services_in_user
                    {
                        user_id = last.id,
                        id_service = i.id,
                    };

                    db.services_in_user.Add(add);
                }
                db.SaveChanges();
                Hide();
            }
            else
            {
                Helper.Message("Вы не выбрали услуги!!!", "error");
            }
        }

        private void selectedServicesCMB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Удаление элемента из выбранного 
            SelectedServicesList.Remove((services)selectedServicesCMB.SelectedItem);
            selectedServicesCMB.ItemsSource = null;
            selectedServicesCMB.ItemsSource = SelectedServicesList;
        }
    }
}