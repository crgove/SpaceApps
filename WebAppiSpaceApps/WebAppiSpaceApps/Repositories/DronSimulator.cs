using System;
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
                shared = shared ?? new DronSimulator();
                return shared;
            }
        }


        #region Calculos
        // vector de estado con v, p, q, r, phi, psi, chi, alp, bet, the
        private double[] vectorEstado = new double[10];
        private double[] vectorControl = new double[2];
        private double[] time = new double[2];
        private double[,] derivadasEstabilidad = new double[10,6];
        private double[,] derivadasControl = new double[2,6];

        private const double alp0 = -4.95673;
        private const double e0 = 0.0005;
        private const double CD0 = 0.0015;
        private const double AR = 5.1;
        private const double S = 5.1;
        private const double rho = 1;
        private const double g = 1.3;
        private const double c = 1.0;
        private const double b = 0.1;
        private const double mass = 100;
        private const double Ixx = 150;
        private const double Iyy = 100;
        private const double Izz = 50.0;
        private const double Ixz = 12.5;
        private const double CL0 = 0.41617;

        public enum nombreVariables
        {
            v, p, q, r, phi, psi, the, alp, bet
        }

        public void Simulate(ParamsDron paramsDron)
        {
            double u_alp = paramsDron.Alp;
            double u_bet = paramsDron.Bet;

            time[1] = time[0];
            time[0] = DateTime.Now.Millisecond;
            double dt = (time[0] > time[1]) ? 0.001 * (time[0] - time[1]) : 1 + (0.001 * (time[0] - time[1]));
            double CL = CL0;
            double CD = 0;
            double CY = 0;
            double Cm = 0;
            double Cn = 0;
            double Cl = 0;
            double e = e0;
            // Ley de control
            vectorControl[0] = u_alp;
            vectorControl[1] = u_bet;
            // calculo de los coeficientes - 
            for (int i = 1; i < 9; i++)
            {
                CL = CL + vectorEstado[i] * derivadasEstabilidad[i, 0];
                CY = CY + vectorEstado[i] * derivadasEstabilidad[i, 1];
                Cl = Cl + vectorEstado[i] * derivadasEstabilidad[i, 2];
                Cm = Cm + vectorEstado[i] * derivadasEstabilidad[i, 3];
                Cn = Cn + vectorEstado[i] * derivadasEstabilidad[i, 4];
            }
            for (int i = 0; i < 2; i++)
            {
                CL = CL + vectorControl[i] * derivadasControl[i, 0];
                CY = CY + vectorControl[i] * derivadasControl[i, 1];
                Cl = Cl + vectorControl[i] * derivadasControl[i, 2];
                Cm = Cm + vectorControl[i] * derivadasControl[i, 3];
                Cn = Cn + vectorControl[i] * derivadasControl[i, 4];
                e = e + vectorControl[i] * derivadasControl[i, 5];
            }
            CD = CD0 + CL * CL / (3.141592 * e * AR);

            // Mecanica de vuelo
            double q_dyn = 0.5 * rho * S * vectorEstado[0] * vectorEstado[0];
            double L = q_dyn * CL;
            double D = q_dyn * CD;
            double Y = q_dyn * CY;
            double l = q_dyn * b * Cl;
            double m = q_dyn * c * Cm;
            double n = q_dyn * b * Cn;

            // Fuerzas

            double X = -Math.Cos(vectorEstado[(int)nombreVariables.alp]) / Math.Cos(vectorEstado[(int)nombreVariables.bet]) * D - Math.Cos(vectorEstado[(int)nombreVariables.alp]) * Math.Tan(vectorEstado[(int)nombreVariables.bet]) * Y + Math.Sin(vectorEstado[(int)nombreVariables.alp]) * L;
            double Z = -Math.Sin(vectorEstado[(int)nombreVariables.alp]) / Math.Cos(vectorEstado[(int)nombreVariables.bet]) * D - Math.Sin(vectorEstado[(int)nombreVariables.alp]) * Math.Tan(vectorEstado[(int)nombreVariables.bet]) * Y - Math.Cos(vectorEstado[(int)nombreVariables.alp]) * L;

            // Derivadas
            double u = Math.Cos(vectorEstado[(int)nombreVariables.alp]) * Math.Cos(vectorEstado[(int)nombreVariables.bet]) * vectorEstado[(int)nombreVariables.v];
            double v = Math.Cos(vectorEstado[(int)nombreVariables.alp]) * Math.Sin(vectorEstado[(int)nombreVariables.v]);
            double w = Math.Sin(vectorEstado[(int)nombreVariables.alp]) * vectorEstado[(int)nombreVariables.v];

            double roll = vectorEstado[(int)nombreVariables.phi];
            double pitch = vectorEstado[(int)nombreVariables.the];
            double yaw = vectorEstado[(int)nombreVariables.psi];
            double p = vectorEstado[(int)nombreVariables.p];
            double q = vectorEstado[(int)nombreVariables.q];
            double r = vectorEstado[(int)nombreVariables.r];

            double du = (X - g * mass * Math.Sin(pitch) - q * w + r * v) / mass;
            double dv = (Y - g * mass * Math.Cos(pitch) * Math.Sin(roll) - r * u + p * w) / mass;
            double dw = (Z - g * mass * Math.Cos(pitch) * Math.Cos(roll) - p * v + q * u) / mass;

            double dp = Izz / (Ixx * Izz - Ixz * Ixz) * l + Ixz / (Ixx * Izz - Ixz * Ixz) * n + (Ixz * (Ixx - Iyy + Izz) / (Ixx * Izz - Ixz * Ixz)) * p * q + ((Izz * (Iyy - Izz) - Ixz * Ixz) / (Ixx * Izz - Ixz * Ixz)) * r * q;
            double dq = m / Iyy;//+ (Izz - Ixx) / Iyy * p * r + Ixz / Iyy * (r*r - p*p);
            double dr = Ixx / (Ixx * Izz - Ixz * Ixz) * n + Ixz / (Ixx * Izz - Ixz * Ixz) * l + ((Ixx * (Ixx - Iyy) + Ixz * Ixz) / (Ixx * Izz - Ixz * Ixz)) * p * q + (Ixz * (Iyy - Ixx - Izz) / (Ixx * Izz - Ixz * Ixz)) * r * q;

            double droll = p + (q * Math.Sin(roll) + r * Math.Cos(roll)) * Math.Tan(pitch);
            double dpitch = q / Math.Cos(roll);  // - r * Math.Sin(roll);
            double dyaw = (q * Math.Sin(roll) + r * Math.Cos(roll)) / Math.Cos(pitch);

            /*double dx = u * Math.Cos(yaw) * Math.Cos(pitch) + v * (Math.Cos(yaw) * Math.Sin(pitch) * Math.Sin(roll) - Math.Cos(roll) * Math.Sin(yaw)) + w * (Math.Sin(pitch) * Math.Cos(roll) * Math.Cos(yaw) + Math.Sin(roll) * Math.Sin(yaw));
            double dy = u * Math.Sin(yaw) * Math.Cos(pitch) + v * (Math.Sin(yaw) * Math.Sin(pitch) * Math.Sin(roll) + Math.Cos(roll) * Math.Cos(yaw)) + w * (Math.Sin(pitch) * Math.Cos(roll) * Math.Sin(yaw) - Math.Sin(roll) * Math.Cos(yaw));
            double dz = -u * Math.Sin(pitch) + v * Math.Cos(pitch) * Math.Sin(roll) + w * Math.Cos(pitch) * Math.Cos(roll);*/

            // Actualizamos
            vectorEstado[(int)nombreVariables.v] = vectorEstado[(int)nombreVariables.v] + dt * Math.Sqrt(du * du + dv * dv + dw * dw);
            vectorEstado[(int)nombreVariables.p] = vectorEstado[(int)nombreVariables.p] + dt * dp;
            vectorEstado[(int)nombreVariables.q] = vectorEstado[(int)nombreVariables.q] + dt * dq;
            vectorEstado[(int)nombreVariables.r] = vectorEstado[(int)nombreVariables.r] + dt * dr;
            vectorEstado[(int)nombreVariables.phi] = vectorEstado[(int)nombreVariables.phi] + dt * droll;
            vectorEstado[(int)nombreVariables.psi] = vectorEstado[(int)nombreVariables.psi] + dt * dyaw;
            vectorEstado[(int)nombreVariables.the] = vectorEstado[(int)nombreVariables.the] + dt * dpitch;
            vectorEstado[(int)nombreVariables.alp] = vectorEstado[(int)nombreVariables.alp] - dt * Math.Atan2(dw, Math.Sqrt(du * du + dv * dv));
            vectorEstado[(int)nombreVariables.bet] = vectorEstado[(int)nombreVariables.bet] + dt * Math.Atan2(dv, du);

        }
        public ResultsDron GetOutput()
        {
            return new ResultsDron {
                The = vectorEstado[(int)nombreVariables.the],
                Phi = vectorEstado[(int)nombreVariables.phi],
                Alp = vectorEstado[(int)nombreVariables.alp],
                Q = vectorEstado[(int)nombreVariables.q],
            };
        }

        public void InitVuelo()
        {
            // crea el stopwatch
            time[0] = DateTime.Now.Millisecond;
            // valores iniciales
            vectorEstado[(int)nombreVariables.v] = 10.0;
            for (int i = 1; i < 7; i++)
            {
                vectorEstado[i] = 0.0;
            }

            derivadasEstabilidad[0, 0] = 0.0;
            derivadasEstabilidad[0, 1] = 0.000000;
            derivadasEstabilidad[0, 2] = 0.000000;
            derivadasEstabilidad[0, 3] = 0.0;
            derivadasEstabilidad[0, 4] = 0.000000;
            derivadasEstabilidad[0, 5] = 0.000000;

            derivadasEstabilidad[1, 0] = -0.000000;
            derivadasEstabilidad[1, 1] = -0.855707;
            derivadasEstabilidad[1, 2] = -0.340108;
            derivadasEstabilidad[1, 3] = -0.000000;
            derivadasEstabilidad[1, 4] = 0.028242;
            derivadasEstabilidad[1, 5] = 0.000000;

            derivadasEstabilidad[2, 0] = 2.442555;
            derivadasEstabilidad[2, 1] = -0.000000;
            derivadasEstabilidad[2, 2] = -0.000000;
            derivadasEstabilidad[2, 3] = -0.516606;
            derivadasEstabilidad[2, 4] = -0.000000;
            derivadasEstabilidad[2, 5] = 0.000000;

            derivadasEstabilidad[3, 0] = 0.000000;
            derivadasEstabilidad[3, 1] = 0.030203;
            derivadasEstabilidad[3, 2] = 0.012741;
            derivadasEstabilidad[3, 3] = 0.0;
            derivadasEstabilidad[3, 4] = -0.008172;
            derivadasEstabilidad[3, 5] = 0.000000;

            derivadasEstabilidad[4, 0] = 0.0;
            derivadasEstabilidad[4, 1] = 0.000000;
            derivadasEstabilidad[4, 2] = 0.000000;
            derivadasEstabilidad[4, 3] = 0.0;
            derivadasEstabilidad[4, 4] = 0.000000;
            derivadasEstabilidad[4, 5] = 0.000000;

            derivadasEstabilidad[5, 0] = 0.0;
            derivadasEstabilidad[5, 1] = 0.000000;
            derivadasEstabilidad[5, 2] = 0.000000;
            derivadasEstabilidad[5, 3] = 0.0;
            derivadasEstabilidad[5, 4] = 0.000000;
            derivadasEstabilidad[5, 5] = 0.000000;

            derivadasEstabilidad[6, 0] = 0.0;
            derivadasEstabilidad[6, 1] = 0.000000;
            derivadasEstabilidad[6, 2] = 0.000000;
            derivadasEstabilidad[6, 3] = 0.0;
            derivadasEstabilidad[6, 4] = 0.000000;
            derivadasEstabilidad[6, 5] = 0.000000;

            derivadasEstabilidad[7, 0] = 3.634085;
            derivadasEstabilidad[7, 1] = 0.000000;
            derivadasEstabilidad[7, 2] = 0.000000;
            derivadasEstabilidad[7, 3] = -1.422702;
            derivadasEstabilidad[7, 4] = 0.000000;
            derivadasEstabilidad[7, 5] = 0.000000;

            derivadasEstabilidad[8, 0] = -0.000000;
            derivadasEstabilidad[8, 1] = -1.385843;
            derivadasEstabilidad[8, 2] = -0.528024;
            derivadasEstabilidad[8, 3] = -0.000000;
            derivadasEstabilidad[8, 4] = 0.043876;
            derivadasEstabilidad[8, 5] = 0.000000;

            derivadasControl[0, 0] = 0.051937;
            derivadasControl[0, 1] = 0.0;
            derivadasControl[0, 2] = 0.0;
            derivadasControl[0, 3] = 0.0;
            derivadasControl[0, 4] = 0.018433;
            derivadasControl[0, 5] = 0.031042;

            derivadasControl[1, 0] = 0.0;
            derivadasControl[1, 1] = -1.415529;
            derivadasControl[1, 2] = 0.242913;
            derivadasControl[1, 3] = 0.0;
            derivadasControl[1, 4] = 0.038828;
            derivadasControl[1, 5] = 0.000000;
        }
        #endregion
    }
}
