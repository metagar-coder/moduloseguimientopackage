using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        public Paginacion<GetListaEE_X_CSeguimiento> ObtenerCursos_X_CSeguimientoFiltrado(
        string idUsuarioMonitorArea,
        int pageNumber,
        int pageSize,
        string? busquedaGeneral,
        string? programaEducativo,
        string? area,
        string? region,
        out string salida,
        out int estado)
        {
            var result = new Paginacion<GetListaEE_X_CSeguimiento>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<GetListaEE_X_CSeguimiento> cursos = new List<GetListaEE_X_CSeguimiento>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_CursosMonitoreados_X_CSeguimiento", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@IdUsuarioMonitorArea", idUsuarioMonitorArea));

                command.Parameters.Add(new SqlParameter("@Salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Estatus", SqlDbType.Int) { Direction = ParameterDirection.Output });

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cursos.Add(new GetListaEE_X_CSeguimiento
                            {
                                idCurso = Convert.ToInt32(reader["IdCurso"]),
                                experienciaEducativa = reader["ExperienciaEducativa"].ToString(),
                                periodo = reader["Periodo"].ToString(),
                                idUsuarioDocente = reader["IdUsuarioDocente"].ToString(),
                                nombreDocente = reader["NombreDocente"].ToString(),
                                correoDocente = reader["CorreoDocente"].ToString(),
                                cve_PE = reader["Cve_PE"].ToString(),
                                programaEducativo = reader["ProgramaEducativo"].ToString(),
                                cve_Dependencia = reader["Cve_Dependencia"].ToString(),
                                dependencia = reader["Dependencia"].ToString(),
                                idUsuarioMonitorPE = reader["IdUsuarioMonitorPE"].ToString(),
                                nombreMonitorPE = reader["NombreMonitorPE"].ToString(),
                                area = reader["Area"].ToString(),
                                region = reader["Region"].ToString(),
                                estatusActividades = Convert.ToInt32(reader["EstatusActividades"]),
                                estatusForos = Convert.ToInt32(reader["EstatusForos"]),
                                estatusAsistencias = Convert.ToInt32(reader["EstatusAsistencias"])
                            });
                        }
                    }

                    salida = command.Parameters["@Salida"].Value?.ToString() ?? string.Empty;
                    estado = Convert.ToInt32(command.Parameters["@Estatus"].Value ?? 0);
                }
                catch (SqlException ex)
                {
                    salida = $"{ex.Number} - {ex.Message}";
                    estado = -1;
                    return result;
                }
            }

            // Filtros
            var query = cursos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busquedaGeneral))
            {
                var lowerBusqueda = busquedaGeneral.ToLower();
                query = query.Where(c =>
                    (c.experienciaEducativa ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.nombreDocente ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.programaEducativo ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.cve_PE ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.nombreMonitorPE ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.area ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.region ?? "").ToLower().Contains(lowerBusqueda));
            }
            if (!string.IsNullOrWhiteSpace(programaEducativo))
            {
                query = query.Where(c =>
                    c.programaEducativo.Contains(programaEducativo, StringComparison.OrdinalIgnoreCase) ||
                    c.cve_PE.Contains(programaEducativo, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(area))
                query = query.Where(c => c.area.Contains(area, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(region))
                query = query.Where(c => c.region.Equals(region, StringComparison.OrdinalIgnoreCase));

            // Paginación
            result.TotalRegistros = query.Count();
            result.TotalPaginas = (int)Math.Ceiling(result.TotalRegistros / (double)pageSize);
            result.Items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return result;
        }


    }
}
