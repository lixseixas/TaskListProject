// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$("#btnFindTasks").click(function () {

    FindTasks();

});

function FindTasks() {

    //limpar tabela
    $('#TableTasks').empty();

    dateReceivedInicial = $("#InitialDate").val();
    dateReceivedFinal = $("#FinalDate").val();

    //acessing the services
    $.post("http://localhost:50747/Task/ListHoursPerDay/",
        {
            InitialDate: dateReceivedInicial,
            FinalDate: dateReceivedFinal,
        }, function (dataReceived) {
           
            for (var i = 0; i < dataReceived.listTasksSummarized.length; i++) {

                $('#TableTasks').append('<tr><td>' + ConvertJsonDate(dataReceived.listTasksSummarized[i].date) +
                    '</td><td>' + ConvertSimpleHour(dataReceived.listTasksSummarized[i].hours) +
                    '</td><td>' + dataReceived.listTasksSummarized[i].totalTasks +
                    '</td><td>' + ConvertSimpleHour(dataReceived.listTasksSummarized[i].averageHours) +
                    '</td><td>' + dataReceived.listTasksSummarized[i].percentualConcludedTasks + "%" +
                    '</td></tr>');
            }

        });
}


function ConvertJsonDate(dateReceived) {

    if (dateReceived == null || dateReceived.length == 0) {
        return " /  /  "
    } else {
        var year = dateReceived.substring(0, 4);
        var month = dateReceived.substring(5, 7);
        var day = dateReceived.substring(8, 10);

        return day + "/" + month + "/" + year;

    }
}

function ConvertJsonHour(dateReceived) {

    if (dateReceived == null || dateReceived.length == 0) {
        return " : :  "
    } else {
        var hora = dateReceived.substring(11, 13);
        var minuto = dateReceived.substring(14, 16);

        return hora + ":" + minuto;

    }
}

function ConvertSimpleHour(dateReceived) {

    if (dateReceived == null || dateReceived.length == 0) {
        return " :  "
    } else {
        var hora = dateReceived.substring(0, 2);
        var minuto = dateReceived.substring(3, 5);

        return hora + ":" + minuto;

    }
}



$("#btnCallJs").click(function () {

    JsJsCalled();

});

function JsJsCalled() {

    inputA = $("#InputA").val();
    inputB = $("#InputB").val();

    var concat = inputA + " " + inputB

    alert("Js Called " );

    $("#OutPut").val(concat);
}