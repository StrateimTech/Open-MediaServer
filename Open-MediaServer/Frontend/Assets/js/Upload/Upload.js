let id = 0;

function dragOverHandler(ev) {
    ev.preventDefault();
}
function dropHandler(ev) {
    ev.preventDefault();
    if (ev.dataTransfer.files) {
        for (let i = 0; i < ev.dataTransfer.files.length; i++) {
            let file = ev.dataTransfer.files[i];
            id++;
            addContainer(ev);
            document.getElementById("Name " + id).value = file.name;
            document.getElementById("File " + id).files = ev.dataTransfer.files;
            itemChange(ev.dataTransfer, i, id);
        }
    }
}

function addContainer(ev) {
    if (id >= 9) {
        return;
    }
    document.getElementById("file-drag-container").insertAdjacentHTML("beforebegin", '<div class="file-upload">\n<div>\n<video class="file-upload-preview" id="Video Preview ' + id + '" muted></video>\n<img class="file-upload-preview" id="Img Preview ' + id + '"/>\n<div class="file-no-preview" id="No Preview ' + id + '">No file preview</div>\n</div>\n<div class="file-input-area">\n<div>\n<input class="file-upload-input" type="file" id="File ' + id + '" name="File '+ id +'" onchange="itemChange(this, 0, '+ id +');">\n<label class="file-upload-input-label" for="File ' + id +'">\n<span>Change file</span>\n</label>\n</div>\n<div>\n<div class="file-name-input-label">\n<span>Name</span>\n</div>\n<input class="file-name-input" type="text" id="Name '+ id + '" name="Name '+ id +'">\n</div>\n<div>\n<div class="file-private-input-label">\n<span>Private</span>\n</div>\n<input class="file-private-input" type="checkbox" id="Private ' + id + '" name="Private '+ id +'">\n</div>\n</div>\n</div>');
}

function itemChange(input, fileId, id) {
    if (input.files) {
        let file = input.files[fileId];
        document.getElementById("Name " + id).value = file.name;
        let fileReader = new FileReader();
        fileReader.readAsDataURL(file);
        fileReader.onload = function(event) {
            let videoPreview =  document.getElementById("Video Preview " + id);
            let imgPreview =  document.getElementById("Img Preview " + id);
            let noPreview =  document.getElementById("No Preview " + id);
            noPreview.style.display = "none";
            if (file.type === "video/mp4" || file.type === "video/x-msvideo" || file.type === "video/quicktime" || file.type === "video/webm" || file.type === "video/x-ms-wmv") {
                videoPreview.src = event.target.result;
                videoPreview.style.display = "inline-block";
                imgPreview.style.display = "none";
                return;
            }
            imgPreview.src = event.target.result;
            imgPreview.style.display = "inline-block";
            videoPreview.style.display = "none";
        };
    }
}