function updateQueryURL(query, window) {
    let url = window.location.href;

    if (url.includes("&" + query + "=true") || url.includes("&" + query + "=false") 
        || url.includes("?" + query + "=true") || url.includes("?" + query + "=false")) {
        url = url.replace("&" + query + "=true", '');
        url = url.replace("?" + query + "=true", '');
    } else {
        let addURL = ''
        if (url.indexOf('?') > -1) {
            addURL += '&' + query + '=true'
        } else {
            addURL += '?' + query + '=true'
        }
        url += addURL;
    }
    
    if(url.indexOf('?') <= 0) {
        url = url.replace("&", '?');
    }
    
    window.location.href = url;
}

function dragOverHandler(ev) {
    ev.preventDefault();
}

function dropHandler(ev, action) {
    console.log(action);
    ev.preventDefault();
    if (ev.dataTransfer.files) {
        let form = document.createElement("form");
        form.method = "POST";
        form.action = action;
        form.enctype = "multipart/form-data";
        for (let i = 0; i < ev.dataTransfer.files.length; i++) {
            let file = ev.dataTransfer.files[i];
            
            let fileInput = document.createElement("input");
            let fileName = document.createElement("input");
            let filePrivate = document.createElement("input");
            
            fileInput.type = "file";
            fileName.type = "text";
            filePrivate.type = "checkbox";
            
            fileInput.name = "File " + i;
            fileName.name = "Name " + i;
            filePrivate.name = "Private " + i;

            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(file);
            fileInput.files = dataTransfer.files;
            fileName.value = file.name;
            filePrivate.checked = false;

            fileInput.style.display = "none";
            fileName.style.display = "none";
            filePrivate.style.display = "none";

            form.appendChild(fileInput);
            form.appendChild(fileName);
            form.appendChild(filePrivate);
        }
        
        document.body.appendChild(form);
        form.submit();
    }
}