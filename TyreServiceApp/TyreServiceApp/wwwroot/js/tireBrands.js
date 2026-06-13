document.addEventListener('DOMContentLoaded', function () {
    var data = {
        "Michelin": ["Primacy", "Latitude", "Pilot", "Energy", "Agilis", "CrossClimate", "Alpin", "X-Ice"],
        "Bridgestone": ["Turanza", "Dueler", "Potenza", "Blizzak", "Ecopia", "Alenza"],
        "Continental": ["ContiPremium", "ContiSport", "ContiCross", "PremiumContact", "WinterContact", "ContiViking"],
        "Pirelli": ["Cinturato", "Scorpion", "P Zero", "Sottozero", "Winter", "Carrier"],
        "Goodyear": ["EfficientGrip", "Wrangler", "Eagle", "UltraGrip", "Vector", "Duratrac"],
        "Hankook": ["Ventus", "Kinergy", "Dynapro", "Winter i*cept"],
        "Nokian": ["Hakkapeliitta", "Nordman", "Hakka", "Rotiiva"],
        "Yokohama": ["Geolandar", "Advance", "BlueEarth", "IceGuard"],
        "Dunlop": ["SP Sport", "Enasave", "Grandtrek", "Winter Sport"],
        "Toyo": ["Proxes", "Open Country", "Observe", "NanoEnergy"],
        "Cooper": ["Discoverer", "Evolution", "Weather-Master"],
        "Kumho": ["Solus", "Ecsta", "Road Venture", "WinterCraft"],
        "Gislaved": ["Nord Frost", "Speed", "Ultra Frost"],
        "BFGoodrich": ["G-Force", "Long Trail", "Mud-Terrain", "All-Terrain"],
        "Maxxis": ["Victra", "Bravo", "Premitra", "AT"],
        "Cordiant": ["Polar", "Snow", "Comfort", "Road Runner"],
        "Viatti": ["Bosco", "Strada", "Brina", "Vettore"],
        "Matador": ["MP", "Hector", "Elite"],
        "Debica": ["D-Max", "Frigo", "Navigator"],
        "Sava": ["Intensa", "Eskimo", "Tempo"],
        "Firestone": ["Firehawk", "Destination", "Winterforce"],
        "Barum": ["Bravuris", "Polaris", "Quartaris"]
    };

    var brandInput = document.querySelector('[name="Manufacturer"]');
    var modelInput = document.getElementById('TireModel');
    var modelDatalist = document.getElementById('tireModelList');

    function fillModels(brand) {
        if (!modelDatalist) return;
        modelDatalist.innerHTML = '';
        var list = data[brand];
        if (list) {
            list.forEach(function (m) {
                var opt = document.createElement('option');
                opt.value = m;
                modelDatalist.appendChild(opt);
            });
            if (modelInput) modelInput.placeholder = 'Выберите модель';
        } else {
            if (modelInput) modelInput.placeholder = 'Введите модель';
        }
    }

    if (brandInput) {
        brandInput.addEventListener('change', function () {
            fillModels(this.value);
        });
        brandInput.addEventListener('input', function () {
            fillModels(this.value);
        });
    }

    fillModels(brandInput ? brandInput.value : '');
});
