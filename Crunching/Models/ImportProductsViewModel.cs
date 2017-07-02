using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Crunching.Models
{
    public class ImportProductsViewModel
    {
        public HttpPostedFileBase File { get; set; }
    }
    public class ImportResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }

        public int SuccessCount { get; set; }
        public int FailCount { get; set; }

        public List<ImportResultRow> result = new List<ImportResultRow>();

    }
    public class ImportResultRow
    {
        public string RefNumber { get; set; }
        public string Message { get; set; }
    }
}
