$("#date-input").datepicker({ dateFormat: "yy-mm-dd" }).datepicker('setDate', new Date());
//$.datepicker.regional["ru"];
$("#datepicker").click(function () {
    $("#date-input")
        .datepicker({ 
            showAnim: "puff"
           }).datepicker("show")
      
});


