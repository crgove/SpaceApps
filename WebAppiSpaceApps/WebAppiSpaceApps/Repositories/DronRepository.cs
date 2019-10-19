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
        // Singleton
        private static DronRepository shared = null;
        public static DronRepository Shared {
            get {
                if (shared == null) {
                    shared = new DronRepository();

                    shared.connection.DronIp = "";
                    shared.connection.Port = 9182;

                    shared.simulator = DronSimulator.Shared;
                }
                return shared;
            }
        }

        // Props
        private DronConnection connection;
        private DronSimulator simulator;


        public ResultsDron RefreshState(ParamsDron paramsDron) //ENVIA LOS DATOS, LOS SIMULA Y DEVUELVE LOS RESULTADOS. LLAMARA A LAS OTRAS FUNCIONES
        {
            connection.SendMessage(paramsDron);
            return simulator.Simulate(paramsDron);
        }
    }
}
