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
    public partial class UserForm : Form
    {
        public UserForm()
        {
            InitializeComponent();
        }

        public IUser Context
        {
            get => context;
            set
            {
                context = value;
                UpdateToContext();
            }
        }

        public IUser SetContext(IUser value)
        {
            context = value;
            return value;
        }

        private IUser context;
        private object telephoneBox;

        private void UpdateToContext()
        {
            emailBox.Text = Context.Email;
            nickNameBox.Text = Context.NickName;
            nameBox.Text = Context.Name;
            passwordBox.Text = "**********";
        }

        private void UserForm_Load(object sender, EventArgs e)
        {

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            // Console.WriteLine("Saving");
            Application.Context.Add(this.Context);
            Application.Context.SaveChanges();
            Context.GenerateHash(passwordBox.Text);
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void genButton_Click(object sender, EventArgs e)
        {
            passwordBox.Text = "783745683475fjks-df";
        }

        private void emailBox_TextChanged(object sender, EventArgs e)
        {
            Context.Email = emailBox.Text;
        }

        private void nickNameBox_TextChanged(object sender, EventArgs e)
        {
            Context.NickName = nickNameBox.Text;
        }

        private void nameBox_TextChanged(object sender, EventArgs e)
        {
            Context.Name = nameBox.Text;
        }
        private void telephoneBox_TextChanged(object sender, EventArgs e)
        {
            Context.Telephone = telephoneBox.Text;
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
