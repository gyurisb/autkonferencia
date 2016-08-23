$(function () {
    $(".file-uploader").change(function (e) {
        var container = $(this).prev();
        var url = document.URL.replace('Upload', 'UploadFile');
        var data = getFileData(e);
        //if ($(this).hasClass('single-file-uploader')) {
        //    var deleteBtn = container.find('.fileDeleteBtn');
        //    if (deleteBtn.length > 0)
        //        fireDelete(deleteBtn);
        //}
        ajaxUploadFile(url, data, function (result) {
            container.append(result);
            undindDeleteListeners();
            bindDeleteListeners();
        });
    });
    bindDeleteListeners();
    $("#embed-html").change(function (e) {
        var data = $("#embed-html").val();
        var url = document.URL.replace('Upload', 'EmbedVideo');
        $.ajax({
            type: "POST",
            url: url,
            dataType: "text",
            contentType: false,
            data: data,
            success: function (data) {
                alert("success");
            }
        })
    });
});

function undindDeleteListeners() {
    $('.fileDeleteBtn').unbind('click');
}

function bindDeleteListeners() {
    $('.fileDeleteBtn').click(function () {
        fireDelete($(this));
        return false;
    });
}

function fireDelete(elem) {
    var url = document.URL.replace('Upload', 'DeleteFile');
    var fileUrl = elem.attr('href');
    url += '?name=' + fileUrl.substr(fileUrl.lastIndexOf('/') + 1);
    $.post(url, null, function () {
        elem.parent().remove();
    });
}