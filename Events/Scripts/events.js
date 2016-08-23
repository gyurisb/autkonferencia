$(function () {
    $("input[type='number']").keypress(function (event) {
        return event.charCode >= 48 && event.charCode <= 57;
    });
});

/* Hungarian initialisation for the jQuery UI date picker plugin. */
(function (factory) {
    if (typeof define === "function" && define.amd) {

        // AMD. Register as an anonymous module.
        define(["../datepicker"], factory);
    } else {

        // Browser globals
        factory(jQuery.datepicker);
    }
}(function (datepicker) {

    datepicker.regional['hu'] = {
        closeText: 'bezár',
        prevText: 'vissza',
        nextText: 'előre',
        currentText: 'ma',
        monthNames: ['Január', 'Február', 'Március', 'Április', 'Május', 'Június',
        'Július', 'Augusztus', 'Szeptember', 'Október', 'November', 'December'],
        monthNamesShort: ['Jan', 'Feb', 'Már', 'Ápr', 'Máj', 'Jún',
        'Júl', 'Aug', 'Szep', 'Okt', 'Nov', 'Dec'],
        dayNames: ['Vasárnap', 'Hétfő', 'Kedd', 'Szerda', 'Csütörtök', 'Péntek', 'Szombat'],
        dayNamesShort: ['Vas', 'Hét', 'Ked', 'Sze', 'Csü', 'Pén', 'Szo'],
        dayNamesMin: ['V', 'H', 'K', 'Sz', 'Cs', 'P', 'Sz'],
        weekHeader: 'Hét',
        dateFormat: 'yy.mm.dd.',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: true,
        yearSuffix: ''
    };
    datepicker.setDefaults(datepicker.regional['hu']);

    return datepicker.regional['hu'];

}));


function getFileData(e) {
    var files = e.target.files;
    if (files.length > 0) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            for (var x = 0; x < files.length; x++) {
                data.append("file" + x, files[x]);
            }
            return data;
        } else {
            alert("This browser doesn't support HTML5 file uploads!");
        }
    }
}

function ajaxUploadFile(url, data, onSuccess, onProgress) {
    $.ajax({
        type: "POST",
        url: url,
        contentType: false,
        processData: false,
        data: data,
        success: function (result) {
            onSuccess(result);
        },
        xhrFields: {
            onprogress: function (e) {
                if (e.lengthComputable) {
                    onProgress(e.loaded / e.total * 100)
                }
            }
        },
        error: function (xhr, status, p3, p4) {
            var err = "Error " + " " + status + " " + p3 + " " + p4;
            if (xhr.responseText && xhr.responseText[0] == "{")
                err = JSON.parse(xhr.responseText).Message;
            console.log(err);
        }
    });
}