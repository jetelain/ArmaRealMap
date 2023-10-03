// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function setupToggleMap(mapOSM) {
    var layerOSM = new L.TileLayer("http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        { attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors' });
    mapOSM.addLayer(layerOSM);
    var layerS2 = new L.TileLayer("https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2020_3857/default/GoogleMapsCompatible/{z}/{y}/{x}.jpg",
        { attribution: '<a xmlns:dct="http://purl.org/dc/terms/" href="https://s2maps.eu" property="dct:title">Sentinel-2 cloudless - https://s2maps.eu</a> by <a xmlns:cc="http://creativecommons.org/ns#" href="https://eox.at" property="cc:attributionName" rel="cc:attributionURL">EOX IT Services GmbH</a> (Contains modified Copernicus Sentinel data 2020) released under <a rel="license" href="https://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License</a>. For commercial usage please see <a href="https://cloudless.eox.at">https://cloudless.eox.at</a>' });
    $('#osm-btn').on('click', _ => {
        mapOSM.addLayer(layerOSM);
        mapOSM.removeLayer(layerS2);
        $('#osm-btn').removeClass('btn-outline-primary');
        $('#osm-btn').addClass('btn-primary');
        $('#s2-btn').removeClass('btn-primary');
        $('#s2-btn').addClass('btn-outline-primary');
        return false;
    });
    $('#s2-btn').on('click', _ => {
        mapOSM.addLayer(layerS2);
        mapOSM.removeLayer(layerOSM);
        $('#s2-btn').removeClass('btn-outline-primary');
        $('#s2-btn').addClass('btn-primary');
        $('#osm-btn').removeClass('btn-primary');
        $('#osm-btn').addClass('btn-outline-primary');
        return false;
    });
}

function setupSelectMap(mapOSM) {
    var rect = null;
    var firstPoint = null;
    mapOSM.on('click', function (e) {
        if (firstPoint) {
            $('#lat1').val(firstPoint.lat);
            $('#lon1').val(firstPoint.lng);
            $('#lat2').val(e.latlng.lat);
            $('#lon2').val(e.latlng.lng);
            $('#reload').submit();
        } else {
            firstPoint = e.latlng;
            rect = L.rectangle([firstPoint, firstPoint], { color: "#ff7800", weight: 1 }).addTo(mapOSM);
        }
    });
    mapOSM.on('mousemove', function (e) {
        if (rect) {
            rect.setBounds([firstPoint, e.latlng]);
        }
    });
    mapOSM.on('contextmenu', function (e) {
        if (firstPoint) {
            rect.remove();
            rect = null;
            firstPoint = null;
            return false;
        }
    });
}

$('.btn-copy').on('click', function () {
    navigator.clipboard.writeText($('#' + $(this).attr('data-copy')).text());
});