using DPFP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json;

namespace Biometrico
{
    public partial class Asistencia : CaptureForm
    {
        private DPFP.Template Template;
        private DPFP.Verification.Verification  verification;
        private string urlApi = "https://localhost:44357/api/Empleados";

        public void Verify(DPFP.Template template)
        {
            Template = template;
            ShowDialog();
        }

        protected override void Init()
        {
            base.Init();
            base.Text = "Verificacion de Huella Digital";
            verification = new DPFP.Verification.Verification();
            UpdateStatus(0);
        }

        private void UpdateStatus(int FAR)
        {
            SetStatus(String.Format("False Accep Rate (RAT) = {0}", FAR));
        }

        protected override void Process(DPFP.Sample Sample)
        {
            base.Process(Sample);

            //Procese la muestra y cree un conjunto de características para fines de inscripción
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

            // Verifique la calidad de la muestra y comience la verificación si es buena
            // TODO: pasar a una tarea separada
            if (features != null)
            {
                // Compare the feature set with our template
                DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();

                DPFP.Template template = new DPFP.Template();
                Stream stream;

                try
                {
                    var res = GetEmpleados();
                    if (!res.Equals(null))
                    {

                        string var = res.Content;

                        Empleado empleado = JsonConvert.DeserializeObject<Empleado>(res.Content);

                      MessageBox.Show("");
                
                       
                    }
                    else
                    {
                        MessageBox.Show("Hubo un problema al consultar la información");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(""+ex);
                }
            }
        }


        private IRestResponse GetEmpleados()
        {
            try
            {
                var client = new RestClient(urlApi);
                var request = new RestRequest("", Method.GET);

              //  request.RequestFormat = DataFormat.Json;

                var response = client.Execute(request);
                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
                return null;
            }

        }


        public Asistencia()
        {
            InitializeComponent();
        }

    }

    public class Empleado
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Huella { get; set; }
    }
}
