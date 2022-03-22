using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RestSharp;

namespace Biometrico
{
    public partial class Registro : Form
    {
        private DPFP.Template Template;

        private string urlApi = "https://localhost:44357/api/Empleados";
        public Registro()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnHuella_Click(object sender, EventArgs e)
        {
            CaptureForm captureForm = new CaptureForm();
            captureForm.ShowDialog();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            CapturarHuella capturarHuella = new CapturarHuella();
            capturarHuella.OnTemplate += this.OnTemplate;
            capturarHuella.ShowDialog();
        }

        private void OnTemplate(DPFP.Template template)
        {
            this.Invoke(new Function(delegate ()
            {
                Template = template;
                btnGuardarRegistro.Enabled = (Template != null);
                if (Template != null)
                {
                    
                  //  imgHuella.Image = System.Drawing.Image.FromFile("C:/Users/Ast/Downloads/huellaCorrecta.png");
                    MessageBox.Show("Huella Capturada Correctamente");
                }
                else
                {
                    MessageBox.Show("Algo ocurrio durante el registro de la huella, vuelva a repetir el proceso por favor");
                }
            }
                ));
        }

        private void btnGuardarRegistro_Click(object sender, EventArgs e)
        {
            try
            {
                var res = EnviarInformacion();

                if (!res.Equals(null))
                {
                    if (res.IsSuccessful)
                    {
                        MessageBox.Show("Información Guardada Correctamente");
                        txtNombre.Text = "";
                        Template = null;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Hubo un problema al guardar la información");
                        txtNombre.Text = "";
                        Template = null;

                    }
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("No es posible conectar con el sistema de inicio de sesión, intentelo más tarde " + ex);
            }
        }

        private IRestResponse EnviarInformacion()
        {
            try
            {
                var nombre = txtNombre.Text;
                byte[] streamHuella = Template.Bytes;
                string huella = Convert.ToBase64String(streamHuella);


                var client = new RestClient(urlApi);
                var request = new RestRequest("", Method.POST);

                request.RequestFormat = DataFormat.Json;

                request.AddJsonBody(new
                {
                    nombreCompleto = $"{nombre}",
                    huella = $"{huella}"

            });

                var response = client.Execute(request);
                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex);
                return null;
            }

        }

    }
}
