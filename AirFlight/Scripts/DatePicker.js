            $(function () {
                $("input[type='date']")
                    .datepicker({ dateFormat: 'yyyy-mm-dd' })
                    .get(0).setAttribute("type", "text");
            });

 
