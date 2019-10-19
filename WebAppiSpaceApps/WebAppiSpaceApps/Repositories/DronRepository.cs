using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppiSpaceApps.Models;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

namespace WebAppiSpaceApps.Repositories
{
    public class DronRepository
    {

        public static ResultsDron RefreshState(ParamsDron paramsDron) //ENVIA LOS DATOS, LOS SIMULA Y DEVUELVE LOS RESULTADOS. LLAMARA A LAS OTRAS FUNCIONES
        {

        }

        protected static class DronSimulator //SIMULA EL DRON
        {
            public static ResultsDron Simulate(ParamsDron paramsDron)
            {

            }
            
        }

        

        
    }
}
