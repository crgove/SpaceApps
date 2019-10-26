using System;

namespace WebAppiSpaceApps.Models
{
    public class ResultsDron   //PARA REPRESENTARLOS EN LA API
    {
        public double The { get; set; }
        public double Phi { get; set; }
        public double Alp { get; set; }
        public double Q { get; set; }
        public DateTime dateTime { get; set; }
    }
}
