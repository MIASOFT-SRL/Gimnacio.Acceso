using System;
using System.Collections.Generic;using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gimnacio.Acceso
{
    public partial class MainAccesControl : Form
    {
        public zkemkeeper.CZKEM dispositivo;
        private int iMachineNumber;
        private int Port { set; get; }
        private string IP { set; get; }
        private bool IsConected { get; set; }
        private string RutaDB { set; get; }

        #region"Events"

        private void SPA2_OnAttTransactionEx(string sEnrollNumber, int iIsInValid, int iAttState, int iVerifyMethod, int iYear, int iMonth, int iDay, int iHour, int iMinute, int iSecond, int iWorkCode)
        {
            //Codigo del evento que se ejecutara cuando se active una hella en el dispositivo
            Data.dbControlAccesoEntities db = new Data.dbControlAccesoEntities();
            lblCodigo.Text = sEnrollNumber;
            Data.USERINFO ui = db.USERINFO.FirstOrDefault(u => u.BADGENUMBER == sEnrollNumber);

            if (ui != null)
            {
                lblNombre.Text = ui.NAME;
                DateTime fechaFin = Convert.ToDateTime(ui.HIREDDAY);
                DateTime fechaHoy = DateTime.Now;
                TimeSpan ts = fechaFin - fechaHoy;
                int difereciaDias = ts.Days;
                lblDiasFaltantes.Text = difereciaDias.ToString();
                lblFechaFin.Text = fechaFin.ToLongDateString();
                DateTime fechaInicio = Convert.ToDateTime(ui.BIRTHDAY);
                lblFechaInicio.Text = fechaInicio.ToLongDateString();

                if (difereciaDias <= 5)
                {
                    if (difereciaDias >= 0)
                    {
                        lblDiasFaltantes.ForeColor = Color.Red;
                    }
                    else
                    {
                        lblDiasFaltantes.ForeColor = Color.Red;
                        lblDiasFaltantes.Text = "La fecha de expiración es menor a la fecha atual";
                    }
                }
                else
                {
                    lblDiasFaltantes.ForeColor = Color.Black;
                }
            }
            else
            {
                MessageBox.Show("No existe en la base de datos","Control de Acceso", MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }
        }

        #endregion

        #region"Metodos"

        private void Limpiar()
        {
            lblCodigo.Text = string.Empty;
            lblDiasFaltantes.Text = "0";
            lblDiasFaltantes.ForeColor = Color.Black;
            lblFechaFin.Text = "No definido";
            lblFechaInicio.Text = "No definido";
            lblNombre.Text = string.Empty ;
        }
        private void Desconectar()
        {
            Cursor = Cursors.WaitCursor;
            if (IsConected)
            {
                dispositivo.Disconnect();
                this.dispositivo.OnAttTransactionEx -= new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(SPA2_OnAttTransactionEx);
                IsConected = false;
                Cursor = Cursors.Default;
            }
        }

        private bool Conectar()
        {
            int idwErrorCode = 0;
            Cursor = Cursors.WaitCursor;
            IsConected = dispositivo.Connect_Net(IP, Port);
            if (IsConected)
            {   
                iMachineNumber = 1;

                if (dispositivo.RegEvent(iMachineNumber, 65535))//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)
                {
                    this.dispositivo.OnAttTransactionEx += new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(SPA2_OnAttTransactionEx);
                    Cursor = Cursors.Default;
                    return true;   
                }
            }
            else
            {
                dispositivo.GetLastError(ref idwErrorCode);
            }
            Cursor = Cursors.Default;
            return false;
        }

        #endregion

        #region "Acceso a Datos"

        public System.Data.OleDb.OleDbConnection ConnectToAccess()
        {
            System.Data.OleDb.OleDbConnection conn = new
                System.Data.OleDb.OleDbConnection();
            // TODO: Modificar la cadena de conexion
            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data source=" + RutaDB;
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to data source {0}" + ex.Message);
            }
            return conn;
        }

        public System.Data.DataTable TraerDataTabla(string sql)
        {
            System.Data.DataTable tabla = new System.Data.DataTable();
            using (System.Data.OleDb.OleDbConnection conn = ConnectToAccess())
            {
                System.Data.OleDb.OleDbCommand com = new System.Data.OleDb.OleDbCommand(conn.ConnectionString);
                com.Connection = conn;
                com.CommandText = sql;//consulta sql
                System.Data.OleDb.OleDbDataAdapter adaptador = new System.Data.OleDb.OleDbDataAdapter(com.CommandText,com.Connection.ConnectionString);
                adaptador.Fill(tabla);
            }
            return tabla;
        }

        #endregion

        public MainAccesControl()
        {
            InitializeComponent();
            dispositivo = new zkemkeeper.CZKEM();
            IsConected = false;
            Port = 4370;
            IP = "192.168.1.201";
            RutaDB = @"C:\Program Files (x86)\Att\att2000.mdb";
            //RutaDB = @"C:\Program Files\ZKTime5.0\att2000.mdb";
        }

        private void MainAccesControl_Load(object sender, EventArgs e)
        {
            if (Conectar())
            {
                timerDispositivo.Start();
                MessageBox.Show("Se conecto correctamente dispositivo", "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            }
            else
            {
                MessageBox.Show("No se conecto correctamente dispositivo", "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void MainAccesControl_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (MessageBox.Show("Desea cerrar el programa?", "Conexión", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                timerDispositivo.Stop();
                Desconectar();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void timerDispositivo_Tick(object sender, EventArgs e)
        {
            Limpiar();
            timerDispositivo.Stop();
            if (IsConected)
            {
                Desconectar();
                Conectar();
            }
            else
            {
                Conectar();
            }
            timerDispositivo.Start();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    //Codigo del evento que se ejecutara cuando se active una hella en el dispositivo
        //    string sEnrollNumber = "2";
        //    Data.dbControlAccesoEntities db = new Data.dbControlAccesoEntities();
        //    lblCodigo.Text = sEnrollNumber;
        //    Data.USERINFO ui = db.USERINFO.FirstOrDefault(u => u.BADGENUMBER == sEnrollNumber);

        //    if (ui != null)
        //    {
        //        lblNombre.Text = ui.NAME;
        //        DateTime fechaFin = Convert.ToDateTime(ui.HIREDDAY);
        //        DateTime fechaHoy = DateTime.Now;
        //        TimeSpan ts = fechaFin - fechaHoy;
        //        int difereciaDias = ts.Days;
        //        lblDiasFaltantes.Text = difereciaDias.ToString();
        //        lblFechaFin.Text = fechaFin.ToLongDateString();

        //        if (difereciaDias <= 5)
        //        {
        //            lblDiasFaltantes.ForeColor = Color.Red;
        //        }
        //        else
        //        {
        //            if (difereciaDias <= 10)
        //            {
        //                lblDiasFaltantes.ForeColor = Color.Yellow;
        //            }
        //            else
        //            {
        //                lblDiasFaltantes.ForeColor = Color.Green;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("No existe en la base de datos", "Control de Acceso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    string sEnrollNumber = "1";
        //    lblCodigo.Text = sEnrollNumber;
        //    string sql = "select * from USERINFO where BADGENUMBER = '" + sEnrollNumber + "'";
        //    System.Data.DataTable tabla = TraerDataTabla(sql);
        //    if (tabla.Rows.Count > 0)
        //    {
        //        lblNombre.Text = tabla.Rows[0]["Name"].ToString();
        //        string fecha = tabla.Rows[0]["HIREDDAY"].ToString();
        //        DateTime fechaFin = Convert.ToDateTime(fecha);
        //        DateTime fechaHoy = DateTime.Now;
        //        TimeSpan ts = fechaFin - fechaHoy;
        //        int difereciaDias = ts.Days;
        //        lblDiasFaltantes.Text = difereciaDias.ToString();
        //        lblFechaFin.Text = fechaFin.ToLongDateString();

        //        if (difereciaDias <= 5)
        //        {
        //            lblDiasFaltantes.ForeColor = Color.Red;
        //        }
        //        else
        //        {
        //            if (difereciaDias <= 10)
        //            {
        //                lblDiasFaltantes.ForeColor = Color.Yellow;
        //            }
        //            else
        //            {
        //                lblDiasFaltantes.ForeColor = Color.Green;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("No existe en la base de datos", "Control de Acceso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}
    }
}