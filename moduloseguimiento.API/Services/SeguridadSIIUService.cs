using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace moduloseguimiento.API.Services
{
    public class SeguridadSIIUService
    {
        private readonly IConfiguration _configuration;

        public SeguridadSIIUService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        private bool funORA64_AsignarPermisosEnConexion(ref OracleConnection tConexion, String sObjeto)
        {
            const long kSeed1 = 12345678;
            const long kSeed3 = 87651234;

            OracleCommand odbComm = new("G$_SECURITY.G$_VERIFY_PASSWORD1_PRD", tConexion);
            OracleCommand odbCommDecrip = new("G$_SECURITY.G$_DECRYPT_FNC", tConexion);
            String strVersion = "1.0";
            String strHoldComm = "";
            String strPassword = "";
            String strRoleName = "";
            Boolean blnProcessOK = true;

            //Obtener strPassword encriptado - Primera Parte del Procesamiento
            try
            {
                odbComm.CommandType = CommandType.StoredProcedure;
                odbComm.Parameters.Add(new("Objeto", sObjeto));
                odbComm.Parameters.Add(new("Version", strVersion));
                odbComm.Parameters.Add(new("Password", OracleDbType.NVarchar2, 40));
                odbComm.Parameters.Add(new("Otro", OracleDbType.NVarchar2, 30));
                odbComm.Parameters["Password"].Direction = ParameterDirection.InputOutput;
                odbComm.Parameters["Otro"].Direction = ParameterDirection.Output;
                odbComm.Parameters["Password"].Value = strPassword;
                odbComm.ExecuteNonQuery();
                strPassword = odbComm.Parameters["Password"].Value.ToString().Trim();
            }
            catch (OracleException)
            {
                var _sErrMsg = "El usuario no tiene permisos para acceder a la forma \"" + sObjeto.ToUpper() + "\".";
                blnProcessOK = false;
            }

            if (blnProcessOK)
                //Segunda fase de procesamiento
                try
                {
                    odbCommDecrip.CommandType = CommandType.StoredProcedure;
                    odbCommDecrip.Parameters.Add(new("Password", OracleDbType.NVarchar2, 40));
                    odbCommDecrip.Parameters.Add(new("Password2", OracleDbType.NVarchar2, 40));
                    odbCommDecrip.Parameters.Add(new("Semilla3", OracleDbType.NVarchar2));
                    odbCommDecrip.Parameters["Password"].Direction = ParameterDirection.ReturnValue;
                    odbCommDecrip.Parameters["Password2"].Value = strPassword;
                    odbCommDecrip.Parameters["Semilla3"].Value = kSeed3;
                    odbCommDecrip.ExecuteNonQuery();
                    strPassword = odbCommDecrip.Parameters["Password"].Value.ToString().Trim();
                }
                catch (OracleException)
                {
                    var _sErrMsg = "Segunda Fase: Verifique que cuenta con los permisos correspondientes.";
                    blnProcessOK = false;
                }

            if (blnProcessOK)
                //Tercera fase de procesamiento
                try
                {
                    odbComm.CommandText = "G$_SECURITY.G$_VERIFY_PASSWORD1_PRD";
                    odbComm.Parameters.Clear();
                    odbComm.Parameters.Add(new("Objeto", sObjeto));
                    odbComm.Parameters.Add(new("Version", strVersion));
                    odbComm.Parameters.Add(new("Password", OracleDbType.NVarchar2, 40));
                    odbComm.Parameters.Add(new("Rol", OracleDbType.NVarchar2, 30));
                    odbComm.Parameters["Password"].Direction = ParameterDirection.InputOutput;
                    odbComm.Parameters["Rol"].Direction = ParameterDirection.Output;
                    odbComm.Parameters["Password"].Value = strPassword;
                    odbComm.ExecuteNonQuery();
                    strPassword = odbComm.Parameters["Password"].Value.ToString().Trim();
                    strRoleName = odbComm.Parameters["Rol"].Value.ToString().Trim();
                }
                catch (OracleException)
                {
                    var _sErrMsg = "Tercera Fase: Verifique que cuenta con los permisos correspondientes.";
                    blnProcessOK = false;
                }

            if (blnProcessOK)
                //Cuarta fase de procesamiento
                try
                {
                    odbCommDecrip.Parameters["Password2"].Value = strPassword;
                    odbCommDecrip.Parameters["Semilla3"].Value = kSeed1;
                    odbCommDecrip.ExecuteNonQuery();
                    strPassword = "\"" + odbCommDecrip.Parameters["Password"].Value.ToString().Trim() + "\"";

                    odbComm.Parameters.Clear();
                    strHoldComm = strRoleName + " IDENTIFIED BY " + strPassword;
                    odbComm.CommandText = "DBMS_SESSION.SET_ROLE";
                    odbComm.Parameters.Add(new("ChecarRoles", strHoldComm.Trim()));
                    odbComm.ExecuteNonQuery();
                }
                catch (OracleException)
                {
                    var _sErrMsg = "Cuarta Fase: Verifique que cuenta con los permisos correspondientes.";
                    blnProcessOK = false;
                }

            // Aqui regresamos el resultado final
            return blnProcessOK;
        }

        private void funORA64_RevocarPermisosEnConexion(ref OracleConnection tConexion)
        {
            OracleCommand odbComm = new("DBMS_SESSION.SET_ROLE", tConexion);
            String strHoldComm = "NONE";

            try
            {
                OracleParameter P1 = new("RevocarPermiso", OracleDbType.NVarchar2, strHoldComm.Length);
                P1.Direction = ParameterDirection.Input;
                P1.Value = strHoldComm;
                odbComm.CommandType = CommandType.StoredProcedure;
                odbComm.Parameters.Add(P1);
                odbComm.ExecuteNonQuery();
            }
            catch (OracleException odbExcep)
            {
                var _sErrMsg = "No se pudieron revocar los roles: " + odbExcep.Message;
            }
        }


        #region Conexion Seguridad SIIU

        public OracleConnection ConectaSIIU(string pagina)
        {
            string cadinterna = _configuration.GetValue<string>("ConnectionStrings:OracleDb");
            OracleConnection conexion = new OracleConnection(cadinterna);

            try
            {
                conexion.Open();

                if (conexion.State == ConnectionState.Open)
                {

                    bool permisosOK = funORA64_AsignarPermisosEnConexion(ref conexion, "llave");

                    if (!permisosOK)
                    {
                        // Si no se asignaron permisos, cerramos la conexion
                        conexion.Dispose();
                        conexion = null;
                    }
                }
            }
            catch (Exception ex)
            {
                conexion?.Dispose();
                conexion = null;
            }

            return conexion;
        }

        public void DesconectaSIIU(OracleConnection conexion)
        {
            try
            {
                if (conexion != null)
                {
                    funORA64_RevocarPermisosEnConexion(ref conexion);
                    conexion.Close();
                    conexion.Dispose();
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        #endregion
    }

}
