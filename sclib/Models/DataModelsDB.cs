using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace sclib.Models
{
    public class samsDB : DbContext
    {
        static samsDB()
        {
            //do not try to create a database 
            Database.SetInitializer<samsDB>(null);
        }

        public samsDB()
            : base("name=OPWContext")
        {
        }

        public DbSet<SamsClubItem> SamsItems { get; set; }
        public DbSet<SourceCategories> Categories { get; set; }


        public void RemoveRecords(int categoryId)
        {
            Database.ExecuteSqlCommand("delete from SamsClubItems where categoryId=" + categoryId.ToString());
        }

        public async Task ListingSave(SamsClubItem listing)
        {
            try
            {
                SamsItems.Add(listing);
                await this.SaveChangesAsync();
            }
            catch
            {
                SamsItems.Remove(listing);
                throw;
            }
        }

        public string getUrl(int categoryId)
        {
            string url = null;
            var r = this.Categories.Find(categoryId);
            if (r != null)
                url = r.URL;
            return url;
        }
    }
}
