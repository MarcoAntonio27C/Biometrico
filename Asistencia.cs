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
        private DPFP.Verification.Verification  verificator;
        private string urlApiEmpleados = "https://localhost:44357/api/Empleados";
        private string urlApiAsistencia = "https://localhost:44357/api/Asistencias";

        public void Verify(DPFP.Template template)
        {
            Template = template;
            ShowDialog();
        }

        protected override void Init()
        {
            base.Init();
            base.Text = "Verificacion de Huella Digital";
            verificator = new DPFP.Verification.Verification();
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

                        var empleados = JsonConvert.DeserializeObject<List<Empleado>>(res.Content);
                        bool found = false;
                        foreach (var empl in empleados)
                        {
                            byte[] huella = Convert.FromBase64String(empl.huella);
                            stream = new MemoryStream(huella);
                            template = new DPFP.Template(stream);
                            verificator.Verify(features, template, ref result);
                            UpdateStatus(result.FARAchieved);

                            if (result.Verified)
                            {
                                found = true;
                                var asistencia = RegistrarAsistencia(empl);
                                if (!asistencia.Equals(null) && (!asistencia.IsSuccessful.Equals(false)))
                                {
                                    MessageBox.Show("" + empl.nombreCompleto+" Se Registro tu asistencia a las: "+ DateTime.Now.ToString());
                                    break;
                                }
                                else
                                {
                                    MessageBox.Show("Ocurrio algo innesperado: " + asistencia.StatusCode.ToString()+ " ");
                                }
                                break;
                            }

                        }

                        if(found == false)
                        {
                            MessageBox.Show("Usuario no encontrado, intentelo de nuevo por favor");
                        }
                       
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
                var client = new RestClient(urlApiEmpleados);
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

        private IRestResponse RegistrarAsistencia(Empleado empleado)
        {
            try
            {

              //  string hora = "2019-07-26T00:00:00";

                var hora = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                var client = new RestClient(urlApiAsistencia);
                var request = new RestRequest("",Method.POST);
                request.RequestFormat = DataFormat.Json;

                request.AddJsonBody(new
                {
                    fechaHora = $"{hora}",
                    empleadoId = $"{empleado.Id}"

                });

                var response = client.Execute(request);
                return response;
            }
            catch(Exception ex)
            {
                MessageBox.Show("" + ex);
                return null;
            }
        }

    }

    [Serializable]
    public class Empleado
    {
        public Guid Id { get; set; }
        public string nombreCompleto { get; set; }
        public string huella { get; set; }
    }
}
