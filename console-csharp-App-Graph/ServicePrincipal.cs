using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2CAppGraph
{
    public interface IServicePrincipal
    {
        string GetId();

        string GetAppId();
    }

    public class MSGraphServicePrincipal : IServicePrincipal
    {
        public string Id { get; set; }

        public string AppId { get; set; }

        public string GetAppId()
        {
            return AppId;
        }

        public string GetId()
        {
            return Id;
        }
    }

    public class AADGraphServicePrincipal : IServicePrincipal
    {
        public string ObjectId { get; set; }

        public string AppId { get; set; }

        public string GetAppId()
        {
            return AppId;
        }

        public string GetId()
        {
            return ObjectId;
        }
    }
}
