using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebAppiSpaceApps.Models;

namespace WebAppiSpaceApps.Repositories
{
    public class DronConnection //ESTABLECER LA CONEXION Y ENVIAR LOS DATOS 
    {
        // Singleton
        static private DronConnection shared = null;
        public static DronConnection Shared { get { shared = shared == null ? new DronConnection() : shared; return shared;  } }
        private DronConnection() { }

        public string DronIp { get; set; }
        public int Port { get; set; }
        private Socket Client { get; set; }

        private bool isConnected = false;

        private String response = "";
        // Semaforos
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);


        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        #region StartClient
        /*
         * La función inciara una conexión con el cliente dada una Ip y un puerto 
         */
        private void StartClient()
        {
            // 1) ESTABLECEMOS LA DIRECCIÓN IP DE LA MÁQUINA 
            IPHostEntry ipHostInfo = Dns.GetHostEntry(DronIp);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

            // 2) CREAMOS EL SOCKET 
            Client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // 3) REALIZAMOS LA CONEXIÓN 
            Client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), Client);

            connectDone.WaitOne();
            isConnected = true;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                //Conseguir el socket en base al estado del objeto.  
                Socket client = (Socket)ar.AsyncState;

                // Completar la conexión
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Señal de que la conexión ha sido realizada  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion


        #region SendMessage
        public string SendMessage(ParamsDron paramsDron)
        {
            if (!isConnected)
            {
                StartClient();
            }

            // Generamos la data
            string data = $"x={paramsDron.HorizontalAxis.ToString()};y={paramsDron.VerticalAxis.ToString()}";
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Enviamos la data
            Client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), Client);
            sendDone.WaitOne();

            // Recibimos la data
            try
            {
                // Creamos el object stateObject
                StateObject state = new StateObject();
                state.workSocket = Client;

                // Begin receiving the data from the remote device.  
                Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            CloseClient();
            return response;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Recibimos el socket del estado del objeto
                Socket client = (Socket)ar.AsyncState;

                // Completamos el envio de datos
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Indicamos que los datos se han enviado  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion

        private void CloseClient()
        {
            Client.Shutdown(SocketShutdown.Both);
            Client.Close();
        }
    }
}
