//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SyncData.TableCreation
//{
//    public class syncMasterContextFactory : IDesignTimeDbContextFactory<syncMasterPublisherContext>
//    {
//        public syncMasterPublisherContext CreateDbContext(string[] args)
//        {
//            var optionsBuilder = new DbContextOptionsBuilder<syncMasterPublisherContext>();
//            optionsBuilder.UseSqlServer("Server=SYSLP1945;Database=simple_cSync;Integrated Security=true;TrustServerCertificate=true;");

//            return new syncMasterPublisherContext(optionsBuilder.Options);
//        }
//    }
//}
