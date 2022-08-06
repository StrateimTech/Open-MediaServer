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