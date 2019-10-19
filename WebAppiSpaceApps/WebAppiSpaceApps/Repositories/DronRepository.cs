using WebAppiSpaceApps.Models;

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

                    shared.connection = DronConnection.Shared;
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
            simulator.Simulate(paramsDron);
            return simulator.GetOutput();
        }

        public void SetDronDir(DirectionDron directionDron)
        {
            connection.DronIp = directionDron.Ip;
            connection.Port = directionDron.Port;
        }

        public void StartFly()
        {
            simulator.InitVuelo();
        }
    }
}
