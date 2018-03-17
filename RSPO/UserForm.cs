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
        private void UpdateToContext()
        {
            Console.WriteLine("Updated");
        }
    }
}
