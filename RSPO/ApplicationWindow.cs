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
            testUser.Name = "Цискаридзе Вано ибн Петро аглы";
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
    }
}
