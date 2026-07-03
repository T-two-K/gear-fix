const MAP_CONFIG = {
    defaultCenter: [53.9045, 27.5615],
    defaultZoom: 13,
    tileUrl: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
    tileAttribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
};

let map;
let userMarker = null;
let userCircle = null;
let shopMarkers = [];
let userLatLng = null;

function initMap() {
    map = L.map('map', {
        center: MAP_CONFIG.defaultCenter,
        zoom: MAP_CONFIG.defaultZoom,
        zoomControl: true,
        attributionControl: true
    });

    L.tileLayer(MAP_CONFIG.tileUrl, {
        attribution: MAP_CONFIG.tileAttribution,
        maxZoom: 19
    }).addTo(map);

    map.on('click', function (e) {
        var lat = e.latlng.lat;
        var lng = e.latlng.lng;

        clearShopMarkers();

        window.chrome.webview.postMessage(JSON.stringify({ lat: lat, lon: lng }))
    });
}

function showUserLocation(lat, lon, radius) {
    userLatLng = L.latLng(lat, lon);

    if (userMarker) {
        userMarker.setLatLng(userLatLng);
    } else {
        const icon = L.divIcon({
            className: '',
            html: '<div class="marker-user"></div>',
            iconSize: [18, 18],
            iconAnchor: [9, 9]
        });
        userMarker = L.marker(userLatLng, { icon }).addTo(map);
        userMarker.bindPopup(
            '<div class="popup-content">' +
            '<div class="popup-title">Вы здесь</div>' +
            '<div class="popup-address">' + lat.toFixed(5) + ', ' + lon.toFixed(5) + '</div>' +
            '</div>'
        );
    }

    drawRadiusCircle(userLatLng.lat, userLatLng.lng, radius);
    map.flyTo(userLatLng, 13, { duration: 0.5 });
}

function drawRadiusCircle(lat, lon, radiusKm) {
    if (userCircle) {
        map.removeLayer(userCircle);
    }
    userCircle = L.circle([lat, lon], {
        radius: radiusKm * 1000,
        className: 'radius-circle'
    }).addTo(map);
}

function placeCarServices(services) {
    clearShopMarkers();

    if (!services || !Array.isArray(services)) {
        console.error('placeCarServices: services не является массивом', services);
        return;
    }

    const icon = L.divIcon({
        className: '',
        html: '<div class="marker-shop"></div>',
        iconSize: [14, 14],
        iconAnchor: [7, 7]
    });

    services.forEach(shop => {
        if (!shop.point || shop.point.lat == null || shop.point.lon == null) {
            console.warn('placeCarServices: у сервиса нет координат', shop);
            return;
        }

        const marker = L.marker([shop.point.lat, shop.point.lon], { icon }).addTo(map);
        marker.bindPopup(buildPopup(shop), { maxWidth: 300 });
        shopMarkers.push(marker);
    });

    if (shopMarkers.length > 0) {
        const group = L.featureGroup(shopMarkers);
        map.flyToBounds(group.getBounds().pad(0.2), { duration: 1.0 });
    }
}

function buildPopup(shop) {
    const dayMap = { Mon: 'Пн', Tue: 'Вт', Wed: 'Ср', Thu: 'Чт', Fri: 'Пт', Sat: 'Сб', Sun: 'Вс' };
    const dayOrder = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

    const rubricsHtml = shop.rubrics && shop.rubrics.length > 0
        ? '<div class="popup-rubrics">' +
        shop.rubrics.map(r => '<span class="popup-rubric-tag">' + r.name + '</span>').join('') +
        '</div>'
        : '';

    let scheduleHtml = '';
    if (shop.schedule) {
        const rows = dayOrder
            .filter(day => shop.schedule[day])
            .map(day => {
                const hours = shop.schedule[day].working_hours;
                const timeStr = hours && hours.length > 0
                    ? hours[0].from + ' – ' + hours[0].to
                    : '—';
                return '<div class="popup-schedule-row">' +
                    '<span class="popup-day">' + dayMap[day] + '</span>' +
                    '<span class="popup-hours">' + timeStr + '</span>' +
                    '</div>';
            }).join('');

        scheduleHtml = '<div class="popup-schedule">' + rows + '</div>';
    }

    return '<div class="popup-content">' +
        '<div class="popup-title">' + (shop.name || 'Без названия') + '</div>' +
        (shop.full_name && shop.full_name !== shop.name
            ? '<div class="popup-full-name">' + shop.full_name + '</div>'
            : '') +
        '<div class="popup-address">📍 ' + (shop.address_name || '') + '</div>' +
        (shop.purpose_name
            ? '<div class="popup-purpose">🔧 ' + shop.purpose_name + '</div>'
            : '') +
        rubricsHtml +
        (scheduleHtml
            ? '<div class="popup-section-label">Часы работы</div>' + scheduleHtml
            : '') +
        '</div>';
}

function clearShopMarkers() {
    shopMarkers.forEach(m => map.removeLayer(m));
    shopMarkers = [];
}

function clearAll() {
    clearShopMarkers();
    if (userMarker) { map.removeLayer(userMarker); userMarker = null; }
    if (userCircle) { map.removeLayer(userCircle); userCircle = null; }
    userLatLng = null;
}

function getUserLocation(radius) {
    navigator.geolocation.getCurrentPosition(
        function (position) {
            var lat = position.coords.latitude;
            var lon = position.coords.longitude;

            map.flyTo([lat, lon], 13, {duration : 1.2});
            showUserLocation(lat, lon, radius);

            window.chrome.webview.postMessage(JSON.stringify({ lat: lat, lon: lon }));
        },
        function (error) {
            window.chrome.webview.postMessage(JSON.stringify({ error: error.code }));
        }
    );
}

initMap();