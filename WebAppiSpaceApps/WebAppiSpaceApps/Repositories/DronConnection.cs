using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebAppiSpaceApps.Models;

namespace WebAppiSpaceApps.Repositories
{
    public class DronConnection // Establecer la conexión y enviar los datos
    {
        #region Vars
        // Singleton
        static private DronConnection shared = null;
        public static DronConnection Shared { get { shared = shared == null ? new DronConnection() : shared; return shared;  } }
        private DronConnection() { }

        // Props
        public string DronIp { get; set; }
        public int Port { get; set; }

        // Private attributes
        private bool isConnected = false;
        private string response = "";

        // Semaphors
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        #endregion


        #region StateObject
        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }
        #endregion

        #region StartClient
        /*
         * La función inciara una conexión con el cliente dada una Ip y un puerto 
         */
        private Socket StartClient()
        {
            // 1) ESTABLECEMOS LA DIRECCIÓN IP DE LA MÁQUINA 
            IPHostEntry ipHostInfo = Dns.GetHostEntry(DronIp);
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

            // 2) CREAMOS EL SOCKET 
            Socket client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // 3) REALIZAMOS LA CONEXIÓN 
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);

            connectDone.WaitOne();
            isConnected = true;

            return client;
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
            Socket client = StartClient();

            // Generamos la data
            string data = $"alp={paramsDron.Alp.ToString()};bet={paramsDron.Bet.ToString()}";
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Enviamos la data
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
            sendDone.WaitOne();

            // Recibimos la data
            try
            {
                // Creamos el object stateObject
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            CloseClient(client);
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

        private void CloseClient(Socket client)
        {
            try {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
