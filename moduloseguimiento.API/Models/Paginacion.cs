namespace moduloseguimiento.API.Models
{
    public class Paginacion<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalRegistros { get; set; } //Es el número total de registros que existen en la base de datos sin filtrar por página. Ejemplo: si tienes 95 monitores en total, este valor sería 95.
        public int TotalPaginas { get; set; } //Indica cuántas páginas hay en total, calculado con la fórmula: Ejemplo: si TotalRecords es 95 y PageSize es 10, entonces TotalPages sería 10.
        public int NumeroActualPagina { get; set; } //Es el número actual de la página que estás viendo. Por ejemplo, si pediste la página 2, este valor sería 2.
        public int TotalRegistrosXPagina { get; set; } //Es cuántos registros estás solicitando por página. Ejemplo: si estás mostrando 10 monitores por página, este valor sería 10.
    }


    // *************************************************

    public enum TipoFiltroFecha
    {
        Ninguno, //0
        UltimoDia, //1
        Ultimos7Dias, //2
        UltimoMes, //3
        RangoPersonalizado //4
    }


}
