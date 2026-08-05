using System;
using System.Collections.Generic;

namespace TaskReportApi.Models;

public class SearchTaskModel
{
    public DateTime InitialDate { get; set; }
    public DateTime FinalDate { get; set; }
    public List<SummarizedTasksModel> ListTasksSummarized { get; set; } = new();
}
