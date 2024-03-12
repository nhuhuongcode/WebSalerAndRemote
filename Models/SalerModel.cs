using System.Data;

namespace remoteWebMos.Models
{
    public class SalerModel
    {
        //public int UId { get; set; }
        public List<CheckboxOption> CheckboxOptions { get; set; }
        public List<string> ListExamlimits { get; set; }
        public List<Brand> brands { get; set; }
        public SaleOrder saleOrder { get; set; }
        public AccountModel account { get; set; }
        public bool AtLeastOneCheckboxSelected => CheckboxOptions.Any(option => option.IsChecked);

        public SalerModel KhoiTao(string _secret)
        {
            DataTable examcat = new DBAccess().GetDataTable("select exam_limit_category from mos_exam_limit_cat", CommandType.Text);
            DataTable prefix = new DBAccess().GetDataTable("SELECT [uid],[loginkey_prefix],[note] FROM [MOS365].[dbo].[mos_prefix]", CommandType.Text);
            SalerModel salerModel = new SalerModel();
            int uid = int.Parse(new DBAccess().ExecuteScalar("Select uid from mos_user where login_key=@secret", CommandType.Text, 
            	new string[]{"@secret"}, new string[] {_secret}).ToString());
            List<CheckboxOption> options = new List<CheckboxOption>();
            foreach (DataRow row in examcat.Rows)
            {
                options.Add(new CheckboxOption()
                {
                    IsChecked = false,
                    value = row["exam_limit_category"].ToString()
                });
            }
            List<Brand> brands = new List<Brand>();
            foreach (DataRow row in prefix.Rows)
            {
                brands.Add(new Brand
                {
                    Id = int.Parse(row["uid"].ToString()),
                    loginkey_pref = row["loginkey_prefix"].ToString(),
                    Name = row["note"].ToString()

                });
            }
            salerModel.brands = brands;
            salerModel.CheckboxOptions = options;
            salerModel.saleOrder = new SaleOrder()
            {
                NgayCap = DateTime.Now,
                EmployID = uid
            };
            salerModel.account = new AccountModel()
            {
                Expired = DateTime.Now.AddDays(185)
            };
            return salerModel;
        }
    }
    public class Brand
    {
        public int Id { get; set; }
        public string loginkey_pref { get; set; }
        public string Name { get; set; }
    }


    public class CheckboxOption
    {
        public bool IsChecked { get; set; }
        public string value { get; set; }
    }

}
