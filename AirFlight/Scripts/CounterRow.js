$('.table tr').each(function (i) {
   
    i && $(this).find('td:first').text(i + ".");

});