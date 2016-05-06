function init() {
    //any other initialization
    getLocation();
}

function getLocation() {
    if (navigator.geolocation) {   //geolocation is supported by the browser
        navigator.geolocation.getCurrentPosition(getPosition, getError);
    } else {   //geolocation is not supported by the browser
        document.forms['hiddenForm'].elements['error'].value = '1';
        treatError();
    }
}

function getPosition(position) { //set coordinates in hidden fields and submit
    if (position.coords.accuracy > 1000) {    //change to 1 km for better results on desktops
        alert(position.coords.accuracy);
        document.forms['hiddenForm'].elements['error'].value = 6;
        treatError();
    }
else{
    document.forms['hiddenForm'].elements['latitude'].value = position.coords.latitude;
    document.forms['hiddenForm'].elements['longitude'].value = position.coords.longitude;
    document.forms['hiddenForm'].elements['error'].value = 0; //no error
    document.forms['hiddenForm'].submit(); //submit form with geolocation info 
} 
}

function getError(error) {   //geolocation was not successful
    switch(error.code) {
        case error.PERMISSION_DENIED:
            document.forms['hiddenForm'].elements['error'].value = 2; //you can define specific error functionality if you like
            break;
        case error.POSITION_UNAVAILABLE:
            document.forms['hiddenForm'].elements['error'].value = 3;
            break;
        case error.TIMEOUT:
            document.forms['hiddenForm'].elements['error'].value = 4;
            break;
        case error.UNKNOWN_ERROR:
            document.forms['hiddenForm'].elements['error'].value = 5;
            break;
    }
    treatError();
}
function treatError() {   //geolocation was not successful, ask user for postal code instead
    document.getElementById('enterPostalCode').style.display='block';  //unhide div 
}

window.onload=init;