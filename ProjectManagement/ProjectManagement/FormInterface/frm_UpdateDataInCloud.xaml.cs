using ProjectManagement.Commun;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectManagement.FormInterface
{
    /// <summary>
    /// frm_UpdateDataInCloud.xaml的交互逻辑
    /// </summary>
    public partial class frm_UpdateDataInCloud : Window
    {
        public string _comment;

        public frm_UpdateDataInCloud()
        {
            InitializeComponent();
            auteur.Text = UserData.user.email;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
                string sent = comment.Text;
                string[] stringArray = comment.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                string commentToSend = "";
                int count = stringArray.Length;
                for (int i = 0; i < count - 2; i++)
                {
                    commentToSend += stringArray[i] + "\\n";
                }
                commentToSend += stringArray[count - 1];

                _comment = commentToSend;

                Close();
            }
            catch
            {
                return;
            }
        }
    }
}