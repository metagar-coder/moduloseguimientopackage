using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        //Lista de Estudiantes X Experiencias Educativa con Filtros
        public Paginacion<GetListaEstudiantes> ObtenerListaEstudiantes_X_EE(
        string idCurso, string idUsuarioDoc, int pageNumber, int pageSize,
        string? busquedaGeneral,
        out string salida, out int estado)
        {
            var result = new Paginacion<GetListaEstudiantes>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize,
                Items = new List<GetListaEstudiantes>()
            };

            salida = string.Empty;
            estado = 0;

            var experiencia = new GetListaEstudiantes
            {
                estudiantes = new List<Estudiante>()
            };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_AlumnosEE", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@pr_IdCurso", idCurso));
                command.Parameters.Add(new SqlParameter("@IdUsuarioDoc", idUsuarioDoc));
                command.Parameters.Add(new SqlParameter("@salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@estado", SqlDbType.Int) { Direction = ParameterDirection.Output });

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (experiencia.experienciaEducativa == null)
                            {
                                experiencia.experienciaEducativa = reader["ExperienciaEducativa"]?.ToString();
                                experiencia.idUsuarioDocente = reader["IdUsuarioDocente"]?.ToString();
                                experiencia.nombreDocente = reader["NombreDocente"]?.ToString(); // Asegúrate del nombre de campo
                            }

                            experiencia.estudiantes.Add(new Estudiante
                            {
                                idUsuarioEstudiante = reader["IdUsuarioEstudiante"]?.ToString(),
                                nombreEstudiante = reader["NombreEstudiante"]?.ToString()
                            });
                        }
                    }

                    salida = command.Parameters["@salida"].Value?.ToString() ?? string.Empty;
                    estado = Convert.ToInt32(command.Parameters["@estado"].Value ?? 0);
                }
                catch (SqlException ex)
                {
                    salida = $"{ex.Number} - {ex.Message}";
                    estado = -1;
                    return result;
                }
            }

            // Aplicar búsqueda sobre los estudiantes
            if (!string.IsNullOrWhiteSpace(busquedaGeneral))
            {
                var lower = busquedaGeneral.ToLower();
                experiencia.estudiantes = experiencia.estudiantes
                    .Where(e =>
                        (e.idUsuarioEstudiante ?? "").ToLower().Contains(lower) ||
                        (e.nombreEstudiante ?? "").ToLower().Contains(lower))
                    .ToList();
            }

            // Paginación sobre estudiantes
            result.TotalRegistros = experiencia.estudiantes.Count;
            result.TotalPaginas = (int)Math.Ceiling(result.TotalRegistros / (double)pageSize);
            experiencia.estudiantes = experiencia.estudiantes
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            result.Items.Add(experiencia);
            return result;
        }
        

        // Lista de datos especificos de un estudiante en un curso especifico.
        public List<GetDetallesEstudiante> DetallesEstudianteXCurso(int idCurso, string idUsuarioEstudiante)
        {
            List<GetDetallesEstudiante> detalles = new List<GetDetallesEstudiante>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_DetallesEstudiante_X_Curso", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@IdCurso", idCurso));
                    command.Parameters.Add(new SqlParameter("@IdUsuario", idUsuarioEstudiante));

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GetDetallesEstudiante d = new GetDetallesEstudiante
                                {
                                    idCurso = int.Parse(reader["IdCurso"].ToString()),
                                    idUsuario = reader["IdUsuario"].ToString(),
                                    experienciaEducativa = reader["ExperienciaEducativa"].ToString(),
                                    nombreDocente = reader["NombreDocente"].ToString(),
                                    nombreEstudiante = reader["NombreEstudiante"].ToString(),
                                    totalActividades = int.Parse(reader["TotalActividades"].ToString()),
                                    totalEntregadasATiempo = int.Parse(reader["TotalEntregadasATiempo"].ToString()),
                                    totalEntregadasConProrroga = int.Parse(reader["TotalEntregadasConProrroga"].ToString()),
                                    totalPorEntregar = int.Parse(reader["TotalPorEntregar"].ToString()),
                                    totalNoEntregadas = int.Parse(reader["TotalNoEntregadas"].ToString()),
                                    totalParticipacionesForo = int.Parse(reader["TotalParticipacionesForo"].ToString()),
                                    totalExamenes = int.Parse(reader["TotalExamenes"].ToString()),
                                    examenesNoPresentados = int.Parse(reader["ExamenesNoPresentados"].ToString()),
                                    examenesReprobados = int.Parse(reader["ExamenesReprobados"].ToString()),
                                };
                                detalles.Add(d);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        
                    }
                }
            }

            return detalles;
        }

    }
}
