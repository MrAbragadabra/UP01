using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MewingLab.Classes;
using MewingLab.DB;

namespace MewingLab.Forms
{
    public partial class AddPatientWindow : Window
    {
        private MewingLabEntities4 db = new MewingLabEntities4();
        private List<insurance_type> insuranceTypesList = new List<insurance_type>();
        private List<insurance_company> insuranceCompaniesList = new List<insurance_company>();
        public AddPatientWindow()
        {
            InitializeComponent();

            // Указание начала и конца в календаре, начало 100 лет от сегодня, конец 14 лет от сегодня
            patientBornDP.DisplayDateStart = DateTime.Now - TimeSpan.FromDays(36500);
            patientBornDP.DisplayDateEnd = DateTime.Now - TimeSpan.FromDays(5110);
            patientBornDP.SelectedDate = DateTime.Now - TimeSpan.FromDays(5110);

            insuranceTypesList = db.insurance_type.ToList();
            insuranceCompaniesList = db.insurance_company.ToList();

            insuranceCompanyCMB.ItemsSource = insuranceCompaniesList;
            insuranceTypeCMB.ItemsSource = insuranceTypesList;

            insuranceCompanyCMB.SelectedIndex = 0;
            insuranceTypeCMB.SelectedIndex = 0;
        }

        private void addPatientButton_Click(object sender, RoutedEventArgs e)
        {
            if (patientNameTB.Text == "")
            {
                Helper.Message("Поле ФИО не может быть пустым!", "error");
                return;
            }

            if (patientSeriesPassportTB.Text == "" || !patientSeriesPassportTB.Text.All(char.IsDigit) || patientSeriesPassportTB.Text.Length != 4)
            {
                Helper.Message("Поле серия паспорта не может быть пустым!", "error");
                return;
            }
            if (patientNumberPassportTB.Text == "" || !patientNumberPassportTB.Text.All(char.IsDigit) || patientNumberPassportTB.Text.Length != 6)
            {
                Helper.Message("Поле номер паспорта не может быть пустым!", "error");
                return;
            }

            if (patientEmailTB.Text == "" || !Mail_LIB.Validation.checkEmail(patientEmailTB.Text))
            {
                Helper.Message("Поле электронная почта не может быть пустым! А также должна быть валидна!", "error");
                return;
            }

            if (patientInsuranceNumbertTB.Text == "" || !patientInsuranceNumbertTB.Text.All(char.IsDigit) || patientInsuranceNumbertTB.Text.Length != 16)
            {
                Helper.Message("Поле номер полиса не может быть пустым! И должно содержать только цифры!", "error");
                return;
            }

            string[] userName = patientNameTB.Text.Trim().Split(' ');

            if (userName.Length != 3)
            {
                Helper.Message("В поле ФИО может быть только три слова", "error");
                return;
            }

            insurance_type selectedInsuranceType = (insurance_type)insuranceTypeCMB.SelectedItem;
            insurance_company selectedInsuranceCompany = (insurance_company)insuranceCompanyCMB.SelectedItem;

            patients checkPassportNumber = db.patients.Where(p => p.passport_number == patientNumberPassportTB.Text).FirstOrDefault();
            patients checkEmail = db.patients.Where(p => p.email == patientEmailTB.Text).FirstOrDefault();
            patients checkInsuranceNumber = db.patients.Where(p => p.email == patientInsuranceNumbertTB.Text).FirstOrDefault();

            if (checkPassportNumber != null)
            {
                Helper.Message("Такой номер паспорта занят!", "error");
                return;
            }

            if (checkEmail != null)
            {
                Helper.Message("Такой электро почта занят!", "error");
                return;
            }

            if (checkInsuranceNumber != null)
            {
                Helper.Message("Такой номер полиса занят!", "error");
                return;
            }

            patients addPatient = new patients 
            {
                name = userName[0],
                second_name = userName[1],
                last_name = userName[2],
                born_date = (DateTime)patientBornDP.SelectedDate,
                passport_series = patientSeriesPassportTB.Text,
                passport_number = patientNumberPassportTB.Text,
                email = patientEmailTB.Text,
                insurance_number = patientInsuranceNumbertTB.Text,
                id_insurance_type = selectedInsuranceType.id,
                id_insurance_company = selectedInsuranceCompany.id,
            };

            db.patients.Add(addPatient);
            db.SaveChanges();

            Helper.Message("Пациент добавлен!", "good");
            Close();
        }

        private void patientSeriesPassportTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true; // отменяем вставку
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // отмена пробела
            }
        }

        private void patientNumberPassportTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true; // отменяем вставку
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // отмена пробела
            }
        }

        private void patientEmailTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true; // отменяем вставку
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // отмена пробела
            }
        }

        private void patientInsuranceNumbertTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true; // отменяем вставку
            }

            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true; // отмена пробела
            }
        }
    }
}
