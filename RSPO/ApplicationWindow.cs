using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSPO
{
    public partial class ApplicationWindow : Form
    {
        public ApplicationWindow()
        {
            InitializeComponent();
        }

        private void userButton_Click(object sender, EventArgs e)
        {
            IUser testUser = Application.Context.Users.Create();
            testUser.Email = "vano@gnail.ru";
            testUser.Name = "Цискаридзе Вано ибн Петро оглы";
            testUser.NickName = "hottubych";
            UserForm view = new UserForm
            {
                Context = testUser,
                Visible = false
            };
            view.ShowDialog();

            String msg = String.Format("<{0}:{1}> {2} #{3}",
                testUser.Name,
                testUser.NickName,
                testUser.Email,
                testUser.PasswordHash);
            Console.WriteLine(msg);
        }

        private void quitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void importMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory=".",
                Filter = "All files (*.*)|*.*|Yandex Zipped XML files (*.xml.zip)|*.xml.zip|Yandex XML files (*.xml)|*.xml",
                Title = "Select Yandex XML file or an archive containing it for import"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK) {

                Console.WriteLine(String.Format("Open File: '{0}'", openFileDialog.FileName));
                ImportFromAtlcomru import = new ImportFromAtlcomru()
                {
                    // FileName = openFileDialog.FileName
                    InputStream = openFileDialog.OpenFile()
                };
                import.Import();
            }
        }
    }
}
