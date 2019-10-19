using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppiSpaceApps.Models;

namespace WebAppiSpaceApps.Repositories
{
    public class DronSimulator
    {
        //singleton
        private DronSimulator() { }

        private static DronSimulator shared = null;
        public static DronSimulator Shared
        {
            get
            {
                shared = shared == null ? new DronSimulator() : shared;
                return shared;
            }
        }
        public ResultsDron Simulate(ParamsDron paramsDron)
        {
            return new ResultsDron();
        }
    }
}
