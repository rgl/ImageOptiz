using System.Reflection;
using System.Windows.Forms;

namespace ImageOptiz
{
    public partial class AboutForm : Form
    {
        private class About
        {
            public string Product { get; set; }
            public string Version { get; set; }
            public string Description { get; set; }
            public string Copyright { get; set; }
        };

        public AboutForm()
        {
            InitializeComponent();
        }

        protected override void  OnLoad(System.EventArgs e)
        {
 	        base.OnLoad(e);

            var about = new About();

            var assembly = Assembly.GetEntryAssembly();
            foreach (var attribute in assembly.GetCustomAttributes(false))
            {
                var typeName = attribute.GetType().FullName;
                switch (typeName)
                {
                    case "System.Reflection.AssemblyFileVersionAttribute":
                        var fileVersionAttribute = (AssemblyFileVersionAttribute)attribute;
                        about.Version = fileVersionAttribute.Version;
                        break;

                    case "System.Reflection.AssemblyProductAttribute":
                        var productAttribute = (AssemblyProductAttribute)attribute;
                        about.Product = productAttribute.Product;
                        break;

                    case "System.Reflection.AssemblyDescriptionAttribute":
                        var descriptionAttribute = (AssemblyDescriptionAttribute)attribute;
                        about.Description = descriptionAttribute.Description;
                        break;

                    case "System.Reflection.AssemblyCopyrightAttribute":
                        var copyrightAttribute = (AssemblyCopyrightAttribute)attribute;
                        about.Copyright = copyrightAttribute.Copyright;
                        break;

                    /*
                    case "System.Reflection.AssemblyTitleAttribute":
                        var titleAttribute = (AssemblyTitleAttribute)attribute;
                        var title = titleAttribute.Title;
                        break;

                    case "System.Reflection.AssemblyCompanyAttribute":
                        var companyAttribute = (AssemblyCompanyAttribute)attribute;
                        var company = companyAttribute.Company;
                        break;

                    case "System.Reflection.AssemblyTrademarkAttribute":
                        var trademarkAttribute = (AssemblyTrademarkAttribute)attribute;
                        var trademark = trademarkAttribute.Trademark;
                        break;
                    */
                }
            }

            Text = string.Format("{0} {1}", Text, about.Product);
            titleLabel.Text = string.Format("{0} v{1}", about.Product, about.Version);
            bodyLabel.Text = string.Format("{0}\n\n{1}", about.Description, about.Copyright);
        }
    }
}
