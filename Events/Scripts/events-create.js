$(function () {
    $("#add-lecturer").click(addLecturerLine);
    $("#add-sponsor").click(addSponsorLine);
    setFormIds();

    $(".remove").click(removeClick);
    $(".image-canvas").click(imageClick);
    $(".file-uploader").change(showImage);
    if ($(".container-lecturer").length == 0)
        addLecturerLine();

    $(".digit-2x").uniqueId();
    $(".digit-2x").click(timeClickHandler);
    $(".digit-2x").keypress(timeKeyHandler);

    $(".date").datepicker();
    $(".date").change(function () {
        timeClickHandler(null, $(".hourFrom"))
    });

    initializeDigits();
});

tinymce.init({
    selector: "textarea[name='Description']",
    //inline: true,
    content_css: "/Content/bootstrap.css",
    menubar: false,
    statusbar: false,
    plugins: [
        "advlist autolink lists link image charmap print preview anchor",
        "searchreplace visualblocks code fullscreen",
        "insertdatetime media table contextmenu paste"
    ],
    toolbar: "insertfile | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link",
    force_br_newlines : false,
    force_p_newlines : false,
    forced_root_block: '',
});

function initializeDigits() {
    $(".digit-2x").each(function () {
        var e = $(this);
        if (e.val() == '') e.val("00");
        else if (e.val().length == 1)
            e.val('0' + e.val());
    });
}

function showImage(event) {
    var output = $(this).closest(".container").find(".image-canvas");
    output.attr('src', URL.createObjectURL(event.target.files[0]));
}
function imageClick() {
    $(this).closest(".container").find(".file-uploader").trigger("click");
}
function removeClick() {
    var cont = $(this).closest(".container");
    if (cont.hasClass("required-container") && $(".required-container").length == 1) {
        alert("Legalább egy előadó kell egy előadáshoz.");
        return false;
    }
    var parent = cont.parent();
    cont.remove();
    if (parent.hasClass("auto-index"))
        setFormIdsOf(parent);
    return false;
}

function addLecturerLine() {
    var line = $('#lecturer-template').html();
    var cont = $(".cont-lecturers");
    addLine(line, cont);
    return false;
}
function addSponsorLine() {
    var line = $('#sponsor-template').html();
    var cont = $(".cont-sponsors");
    addLine(line, cont);
    setFormIds();
    return false;
}

function addLine(line, cont) {
    cont.append(line);
    cont.find(".remove").last().click(removeClick);
    cont.find(".image-canvas").last().click(imageClick);
    cont.find(".file-uploader").last().change(showImage);
}

function setFormIds() {
    $(".auto-index").each(function () {
        setFormIdsOf($(this));
    });
}
function setFormIdsOf(cont) {
    cont.find(".container").each(function (i, e) {
        $(this).find("input[namepattern]").each(function () {
            $(this).attr("name", $(this).attr("namepattern").replace("%", i));
        });
    });
}


var boxStates = {};
function timeKeyHandler(event) {
    if (event.charCode >= 48 && event.charCode <= 57) {
        var box = $(this);
        var ch = String.fromCharCode(event.charCode);
        var state = boxStates[box.id];
        var newState = state;
        var maxHour = box.hasClass("hourFrom") || box.hasClass("hourTo") ? 2 : 5;
        if (state == 0) {
            box.val("0" + ch).change();
            if (parseInt(ch) > maxHour)
                newState = 2;
            else
                newState = 1;
        }
        else if (state == 1) {
            box.val(box.val().substr(1) + ch).change();
            newState = 2;
        }
        boxStates[box.id] = newState;

        if (newState == 2)
            timeClickHandler(null, box.next().next(".digit-2x"));
    }
    return false;
}

function timeClickHandler(event, source) {
    if (source == undefined) source = $(this);
    source.select();
    boxStates[$(this).id] = 0;
}